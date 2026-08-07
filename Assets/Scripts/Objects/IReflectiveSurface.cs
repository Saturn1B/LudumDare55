using UnityEngine;

public interface IReflectiveSurface
{
	public void GenerateReflection(LaserEmitter source, Vector3 hitPosition, Vector3 hitNormal, Vector3 incomingDirection, Vector3 direction);
	public void RemoveReflection(LaserEmitter source);
}
