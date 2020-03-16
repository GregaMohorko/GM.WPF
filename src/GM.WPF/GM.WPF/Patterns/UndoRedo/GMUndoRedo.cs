/*
MIT License

Copyright (c) 2020 Gregor Mohorko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Project: GM.WPF
Created: 2020-03-16
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace GM.WPF.Patterns.UndoRedo
{
	/// <summary>
	/// Represents an Undo/Redo manager.
	/// <para>You need to register all the window types for which you want to use this instance of undo/redo manager. You can create a new undo/redo manager for each of your windows or you can use one undo/redo manager for all of them. One window (or window type) can only be bound to one undo/redo manager.</para>
	/// <para>A classical approach to implement undo/redo is to allow changes on the model only through commands. And every command should be invertible. The user then executes an action, the application creates a command, executes it and puts an inverted command on the undo-stack. When the user clicks on undo, the application executes the top-most (inverse) command on the undo-stack, inverts it again (to get the original command again) and puts it on the redo-stack. That's it.</para>
	/// </summary>
	public class GMUndoRedo : ObservableObject
	{
		private static readonly List<GMUndoRedo> instances = new List<GMUndoRedo>();

		/// <summary>
		/// Tries to find the undo/redo manager associated with the <see cref="Window"/> object that hosts the content tree within which the specified dependency object is located.
		/// <para>Uses <see cref="Window.GetWindow(DependencyObject)"/>.</para>
		/// </summary>
		/// <param name="dependencyObject">The dependency object that is in the content tree of a window.</param>
		public static GMUndoRedo GetInstance(DependencyObject dependencyObject)
		{
			// try to find the window
			Window window = Window.GetWindow(dependencyObject);
			if(window == null) {
				return null;
			}
			// find the undo/redo instance that holds this window type or window instance
			Type currentWindowType = window.GetType();
			return instances.SingleOrDefault(ur => ur.registeredWindowTypes.Contains(currentWindowType) || ur.registeredWindows.Contains(window));
		}

		private readonly List<Window> registeredWindows;
		private readonly List<Type> registeredWindowTypes;
		private readonly Stack<InvertibleCommand> undoStack;
		private readonly Stack<InvertibleCommand> redoStack;

		/// <summary>
		/// Creates a new instance of <see cref="GMUndoRedo"/>.
		/// </summary>
		public GMUndoRedo()
		{
			registeredWindows = new List<Window>();
			registeredWindowTypes = new List<Type>();
			undoStack = new Stack<InvertibleCommand>();
			redoStack = new Stack<InvertibleCommand>();

			instances.Add(this);
		}

		private CommandBinding UndoCommandBinding => new CommandBinding(ApplicationCommands.Undo, new ExecutedRoutedEventHandler(OnExecutedUndo), new CanExecuteRoutedEventHandler(OnCanExecuteUndo));
		private CommandBinding RedoCommandBinding => new CommandBinding(ApplicationCommands.Redo, new ExecutedRoutedEventHandler(OnExecutedRedo), new CanExecuteRoutedEventHandler(OnCanExecuteRedo));

		/// <summary>
		/// Register the provided window instance into this undo/redo manager.
		/// </summary>
		/// <param name="window">The window to register into this undo/redo manager.</param>
		public void RegisterWindow(Window window)
		{
			if(window == null) {
				throw new ArgumentNullException(nameof(window));
			}
			// check if this window is already registered
			if(registeredWindows.Contains(window)) {
				throw new InvalidOperationException("The specified window is already registered in this instance of undo/redo manager.");
			}
			// check if the class of this window is already registered
			if(registeredWindowTypes.Contains(window.GetType())) {
				throw new InvalidOperationException("The window type of the specified window is already registered in this instance of undo/redo manager.");
			}

			// register
			_ = window.CommandBindings.Add(UndoCommandBinding);
			_ = window.CommandBindings.Add(RedoCommandBinding);
			registeredWindows.Add(window);
		}

		/// <summary>
		/// Register on the class level means that all instances of <typeparamref name="T"/> will share the same undo/redo manager.
		/// </summary>
		public void RegisterWindowType<T>() where T : Window
		{
			RegisterWindowType(typeof(T));
		}

		/// <summary>
		/// Register on the class level means that all instances of this window type will share the same undo/redo manager.
		/// </summary>
		/// <param name="windowType">The type to register in this instance of undo/redo manager.</param>
		public void RegisterWindowType(Type windowType)
		{
			if(windowType == null) {
				throw new ArgumentNullException(nameof(windowType));
			}
			// make sure that the type is of subclass Window
			if(!windowType.IsSubclassOf(typeof(Window))) {
				throw new ArgumentException($"The provided window type must be a subclass of {nameof(Window)}.", nameof(windowType));
			}
			// check if the class of this window is already registered
			if(registeredWindowTypes.Any(rwt => rwt == windowType || windowType.IsSubclassOf(rwt) || rwt.IsSubclassOf(windowType))) {
				throw new InvalidOperationException("The specified window type (or it's subclass or parentclass) is already registered in this instance of undo/redo manager.");
			}

			// register
			CommandManager.RegisterClassCommandBinding(windowType, UndoCommandBinding);
			CommandManager.RegisterClassCommandBinding(windowType, RedoCommandBinding);
			registeredWindowTypes.Add(windowType);
		}

		/// <summary>
		/// Gets a list of descriptions of available undo commands.
		/// </summary>
		public List<string> AvailableUndoCommands => undoStack
			.Select(ic => ic.Description)
			.ToList();
		/// <summary>
		/// Gets a list of descriptions of available redo commands.
		/// </summary>
		public List<string> AvailableRedoCommands => redoStack
			.Select(ic => ic.Description)
			.ToList();

		/// <summary>
		/// Clears the undo/redo stacks.
		/// </summary>
		public void Clear()
		{
			undoStack.Clear();
			redoStack.Clear();
			RaisePropertyChanged(nameof(AvailableUndoCommands));
			RaisePropertyChanged(nameof(AvailableRedoCommands));
		}

		/// <summary>
		/// Creates an <see cref="InvertibleCommand"/> that, when executed, will automatically put the inverted command to the undo stack.
		/// </summary>
		/// <param name="description">The description of the command.</param>
		/// <param name="execute">The action.</param>
		/// <param name="undoExecute">The undo action.</param>
		public InvertibleCommand CreateAction(string description, Action execute, Action undoExecute)
		{
			var newAction = new InvertibleCommand(description, execute, undoExecute);
			newAction.Executed += Action_Executed;
			return newAction;
		}

		/// <summary>
		/// Creates an <see cref="InvertibleCommand"/> that, when executed, will automatically put the inverted command to the undo stack.
		/// </summary>
		/// <param name="description">The description of the command.</param>
		/// <param name="execute">The action.</param>
		/// <param name="canExecute">The execution status logic.</param>
		/// <param name="undoExecute">The undo action.</param>
		/// <param name="canUndoExecute">The undo execution status logic.</param>
		public InvertibleCommand CreateAction(string description, Action execute, Func<bool> canExecute, Action undoExecute, Func<bool> canUndoExecute)
		{
			var newAction = new InvertibleCommand(description, execute, canExecute, undoExecute, canUndoExecute);
			newAction.Executed += Action_Executed;
			return newAction;
		}

		private void Action_Executed(object sender, EventArgs e)
		{
			var ic = (InvertibleCommand)sender;
			// get the inverted command that represents the undo command
			InvertibleCommand undoCommand = ic.GetInvertedCommand();
			Add(undoCommand);
		}

		/// <summary>
		/// Adds the specified undo/redo actions to the undo/redo stack as a single action.
		/// <para>If there are currently any actions on the redo stack, they will be removed.</para>
		/// </summary>
		/// <param name="description">The description of the action.</param>
		/// <param name="undoRedoActions">The undo/redo actions.</param>
		public void Add(string description, List<(Action undo, Action redo)> undoRedoActions)
		{
			if(undoRedoActions == null) {
				throw new ArgumentNullException(nameof(undoRedoActions));
			}
			if(undoRedoActions.Count == 0) {
				return;
			}
			List<Action> undoActions = undoRedoActions
				.Select(t => t.undo)
				.ToList();
			List<Action> redoActions = undoRedoActions
				.Select(t => t.redo)
				.ToList();

			void undo()
			{
				foreach(Action undoAction in undoActions) {
					undoAction();
				}
			}
			void redo()
			{
				foreach(Action redoAction in redoActions) {
					redoAction();
				}
			}
			Add(description, undo, redo);
		}

		/// <summary>
		/// Adds the specified undo/redo action to the undo/redo stack.
		/// <para>If there are currently any actions on the redo stack, they will be removed.</para>
		/// </summary>
		/// <param name="description">The description of the action.</param>
		/// <param name="undo">The undo action.</param>
		/// <param name="redo">The redo action.</param>
		public void Add(string description, Action undo, Action redo)
		{
			var undoCommand = new InvertibleCommand(description, undo, redo);
			Add(undoCommand);
		}

		/// <summary>
		/// Adds the provided undo command to the undo stack.
		/// <para>If there are currently any actions on the redo stack, they will be removed.</para>
		/// </summary>
		/// <param name="undoCommand">The undo command.</param>
		public void Add(InvertibleCommand undoCommand)
		{
			undoStack.Push(undoCommand);
			RaisePropertyChanged(nameof(AvailableUndoCommands));
			if(redoStack.Count > 0) {
				redoStack.Clear();
				RaisePropertyChanged(nameof(AvailableRedoCommands));
			}
		}

		private void OnCanExecuteUndo(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = undoStack.Count > 0;
			e.Handled = true;
		}

		private void OnCanExecuteRedo(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = redoStack.Count > 0;
			e.Handled = true;
		}

		private void OnExecutedUndo(object sender, ExecutedRoutedEventArgs e)
		{
			UndoRedo(true);
			e.Handled = true;
		}

		private void OnExecutedRedo(object sender, ExecutedRoutedEventArgs e)
		{
			UndoRedo(false);
			e.Handled = true;
		}

		private void UndoRedo(bool undo)
		{
			Stack<InvertibleCommand> executingStack = undo ? undoStack : redoStack;
			Stack<InvertibleCommand> oppositeStack = undo ? redoStack : undoStack;

			if(executingStack.Count == 0) {
				// how did this happen? hmm ...
				return;
			}
			// take the topmost command from the executing stack
			InvertibleCommand commandToExecute = executingStack.Pop();
			if(!commandToExecute.CanExecute(null)) {
				// if this happens, it is a bad design from the programmer using this pattern, but just in case
				return;
			}
			// execute the command
			commandToExecute.Execute(null);
			// push the inverted command to the opposite stack
			InvertibleCommand invertedCommand = commandToExecute.GetInvertedCommand();
			oppositeStack.Push(invertedCommand);

			RaisePropertyChanged(nameof(AvailableUndoCommands));
			RaisePropertyChanged(nameof(AvailableRedoCommands));
		}
	}
}
