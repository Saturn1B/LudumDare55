using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HystoryCommand : MonoBehaviour
{
	public static HystoryCommand Instance { get; private set; }

	private Stack<ICommand> undoStack = new Stack<ICommand>();
	private Stack<ICommand> redoStack = new Stack<ICommand>();

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	public void ExecuteCommand(ICommand command)
	{
		Debug.Log("Add command");

		command.Execute(false);
		undoStack.Push(command);
		redoStack.Clear();
	}

	[ContextMenu("Undo")]
	public void Undo()
	{
		if(undoStack.Count > 0)
		{
			ICommand command = undoStack.Pop();
			command.Undo();
			redoStack.Push(command);
		}
	}

	[ContextMenu("Redo")]
	public void Redo()
	{
		if(redoStack.Count > 0)
		{
			ICommand command = redoStack.Pop();
			command.Execute(true);
			undoStack.Push(command);
		}
	}
}

public interface ICommand
{
	void Execute(bool isRedo);
	void Undo();
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
