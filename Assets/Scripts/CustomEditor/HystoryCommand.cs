using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HystoryCommand : MonoBehaviour
{
	public static HystoryCommand Instance { get; private set; }

	private Stack<ICommandBase> undoStack = new Stack<ICommandBase>();
	private Stack<ICommandBase> redoStack = new Stack<ICommandBase>();

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	public TResult ExecuteCommand<TResult>(ICommand<TResult> command)
	{
		Debug.Log("Add command");

		TResult result = command.Execute(false);
		undoStack.Push(command);
		redoStack.Clear();

		return result;
	}

	[ContextMenu("Undo")]
	public string Undo()
	{
		if(undoStack.Count > 0)
		{
			ICommandBase command = undoStack.Pop();
			command.Undo();
			redoStack.Push(command);
			return command.actionDescription;
		}
		return null;
	}

	[ContextMenu("Redo")]
	public string Redo()
	{
		if(redoStack.Count > 0)
		{
			ICommandBase command = redoStack.Pop();
			command.Execute(true);
			undoStack.Push(command);
			return command.actionDescription;
		}
		return null;
	}
}

public interface ICommandBase
{
	string actionDescription { get; }
	void Execute(bool isRedo);
	void Undo();
}

public interface ICommand<TResult> : ICommandBase
{
	new TResult Execute(bool isRedo);
}

public static class ListCloner
{
	public static List<T> CloneMonoBehaviourListReference<T>(List<T> originalList) where T : MonoBehaviour
	{
		List<T> clonedList = new List<T>(originalList.Count);

		foreach (T item in originalList)
		{
			clonedList.Add(item);
		}

		return clonedList;
	}
}
