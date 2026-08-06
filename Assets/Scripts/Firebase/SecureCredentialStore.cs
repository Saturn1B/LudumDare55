using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

/// Stores the Google refresh token in the OS credential vault instead of PlayerPrefs.
/// On Windows this is the Windows Credential Manager - encrypted at rest by Windows
/// itself and scoped to the current user account. Never touches PlayerPrefs, the
/// registry, or a plaintext file.
public static class SecureCredentialStore
{
	private const string TargetName = "MATLAB_GoogleRefreshToken"; // shown in Windows Credential Manager

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
	public static bool TryGet(out string value)
	{
		value = null;

		if (!CredRead(TargetName, CredType.GENERIC, 0, out IntPtr credPtr))
			return false;

		try
		{
			var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);

			if (cred.CredentialBlobSize == 0)
				return false;

			byte[] bytes = new byte[cred.CredentialBlobSize];
			Marshal.Copy(cred.CredentialBlob, bytes, 0, bytes.Length);
			value = Encoding.UTF8.GetString(bytes);
			return true;
		}
		finally
		{
			CredFree(credPtr);
		}
	}

	public static void Set(string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);

		var cred = new CREDENTIAL
		{
			Type = CredType.GENERIC,
			TargetName = TargetName,
			CredentialBlobSize = (uint)bytes.Length,
			CredentialBlob = Marshal.AllocHGlobal(bytes.Length),
			Persist = CredPersist.LOCAL_MACHINE,
			AttributeCount = 0,
			UserName = Environment.UserName
		};

		Marshal.Copy(bytes, 0, cred.CredentialBlob, bytes.Length);

		try
		{
			if (!CredWrite(ref cred, 0))
				Debug.LogError($"Failed to write credential: {Marshal.GetLastWin32Error()}");
		}
		finally
		{
			Marshal.FreeHGlobal(cred.CredentialBlob);
		}
	}

	public static void Delete()
	{
		CredDelete(TargetName, CredType.GENERIC, 0);
	}

	private enum CredType : uint { GENERIC = 1 }
	private enum CredPersist : uint { LOCAL_MACHINE = 2 }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct CREDENTIAL
	{
		public uint Flags;
		public CredType Type;
		public string TargetName;
		public string Comment;
		public long LastWritten;
		public uint CredentialBlobSize;
		public IntPtr CredentialBlob;
		public CredPersist Persist;
		public uint AttributeCount;
		public IntPtr Attributes;
		public string TargetAlias;
		public string UserName;
	}

	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern bool CredRead(string target, CredType type, int reservedFlag, out IntPtr credentialPtr);

	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern bool CredWrite(ref CREDENTIAL credential, uint flags);

	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern bool CredDelete(string target, CredType type, int reservedFlag);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern void CredFree(IntPtr cred);

#else
	// Non-Windows fallback (Mac/Linux Editor, or if you ever ship those platforms).
	// This is NOT secure storage - just here so the project still compiles off-Windows.
	// Swap in macOS Keychain / libsecret if you ever target those platforms for real.
	public static bool TryGet(out string value)
	{
		value = PlayerPrefs.GetString(TargetName, null);
		return !string.IsNullOrEmpty(value);
	}

	public static void Set(string value)
	{
		PlayerPrefs.SetString(TargetName, value);
		PlayerPrefs.Save();
	}

	public static void Delete()
	{
		PlayerPrefs.DeleteKey(TargetName);
		PlayerPrefs.Save();
	}
#endif
}