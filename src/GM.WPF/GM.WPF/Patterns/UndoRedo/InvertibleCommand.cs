/*
MIT License

Copyright (c) 2022 Gregor Mohorko

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
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GM.Utility.Patterns.UndoRedo;

namespace GM.WPF.Patterns.UndoRedo
{
	/// <summary>
	/// A command that has an invertible command to it. It relays it's functionality to other objects by invoking delegates.
	/// </summary>
	public class InvertibleCommand : IRelayCommand
	{
		/// <summary>
		/// Invoked when this command has just been executed.
		/// </summary>
		public event EventHandler Executed;
		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		internal UndoRedoAction UndoRedoAction { get; }

		private readonly RelayCommand _command;
		private readonly Func<bool> _canExecute;
		private readonly Func<bool> _canInvertedExecute;

		/// <summary>
		/// Creates a new instance of <see cref="InvertibleCommand"/>.
		/// </summary>
		/// <param name="description">The description of the command.</param>
		/// <param name="execute">The action.</param>
		/// <param name="invertedExecute">The inverted action.</param>
		public InvertibleCommand(string description, Action execute, Action invertedExecute)
			: this(new UndoRedoAction(description, execute, invertedExecute))
		{ }

		/// <summary>
		/// Creates a new instance of <see cref="InvertibleCommand"/>.
		/// </summary>
		/// <param name="description">The description of the command.</param>
		/// <param name="execute">The action.</param>
		/// <param name="canExecute">The execution status logic.</param>
		/// <param name="invertedExecute">The inverted action.</param>
		/// <param name="canInvertedExecute">The inverted execution status logic.</param>
		public InvertibleCommand(string description, Action execute, Func<bool> canExecute, Action invertedExecute, Func<bool> canInvertedExecute)
			: this(new UndoRedoAction(description, execute, invertedExecute), canExecute, canInvertedExecute)
		{ }

		private InvertibleCommand(UndoRedoAction undoRedoAction)
		{
			UndoRedoAction = undoRedoAction;
			_command = InitializeCommand();
		}

		private InvertibleCommand(UndoRedoAction undoRedoAction, Func<bool> canExecute, Func<bool> canInvertedExecute)
		{
			_canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
			_canInvertedExecute = canInvertedExecute ?? throw new ArgumentNullException(nameof(canInvertedExecute));

			UndoRedoAction = undoRedoAction;
			_command = InitializeCommand();
		}

		private RelayCommand InitializeCommand()
		{
			RelayCommand command;
			if(_canExecute == null) {
				command = new RelayCommand(UndoRedoAction.Action);
			} else {
				command = new RelayCommand(UndoRedoAction.Action, _canExecute);
			}
			command.CanExecuteChanged += (sender, e) => CanExecuteChanged?.Invoke(sender, e);
			return command;
		}

		/// <summary>
		/// The description of this command.
		/// </summary>
		public string Description => UndoRedoAction.Description;

		/// <summary>
		/// Returns a command that inverts the action of this command.
		/// </summary>
		public InvertibleCommand GetInvertedCommand()
		{
			UndoRedoAction invertedUndoRedoAction = UndoRedoAction.GetInvertedUndoRedoAction();
			if(_canExecute == null) {
				return new InvertibleCommand(invertedUndoRedoAction);
			}
			return new InvertibleCommand(invertedUndoRedoAction, _canInvertedExecute, _canExecute);
		}

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		public bool CanExecute(object parameter)
		{
			return _command.CanExecute(parameter);
		}

		/// <summary>
		/// Executes this command and invokes the <see cref="Executed"/> event.
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		public void Execute(object parameter)
		{
			_command.Execute(parameter);
			Executed?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Notifies that the <see cref="ICommand.CanExecute"/> property has changed.
		/// </summary>
		public void NotifyCanExecuteChanged()
		{
			_command.NotifyCanExecuteChanged();
		}
	}
}
