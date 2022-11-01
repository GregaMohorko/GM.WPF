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
using GM.Utility.Patterns.UndoRedo;

namespace GM.WPF.Patterns.UndoRedo
{
	/// <summary>
	/// Represents an Undo/Redo manager that can be used in a WPF application.
	/// <para>You need to register all the window types for which you want to use this instance of undo/redo manager. You can create a new undo/redo manager for each of your windows or you can use one undo/redo manager for all of them. One window (or window type) can only be bound to one undo/redo manager.</para>
	/// <seealso cref="InvertibleCommand"/>.
	/// </summary>
	public class GMWPFUndoRedo : GMUndoRedo
	{
		private static readonly List<GMWPFUndoRedo> instances = new List<GMWPFUndoRedo>();

		/// <summary>
		/// Tries to find the undo/redo manager associated with the <see cref="Window"/> object that hosts the content tree within which the specified dependency object is located.
		/// <para>Uses <see cref="Window.GetWindow(DependencyObject)"/>.</para>
		/// </summary>
		/// <param name="dependencyObject">The dependency object that is in the content tree of a window.</param>
		public static GMWPFUndoRedo GetInstance(DependencyObject dependencyObject)
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

		/// <summary>
		/// Creates a new instance of <see cref="GMWPFUndoRedo"/>.
		/// </summary>
		public GMWPFUndoRedo()
		{
			registeredWindows = new List<Window>();
			registeredWindowTypes = new List<Type>();

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
			UndoRedoAction undoAction = ic.UndoRedoAction.GetInvertedUndoRedoAction();
			Add(undoAction);
		}

		private void OnCanExecuteUndo(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CanUndo;
			e.Handled = true;
		}

		private void OnCanExecuteRedo(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CanRedo;
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
	}
}
