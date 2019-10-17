/*
MIT License

Copyright (c) 2019 Grega Mohorko

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
Created: 2019-10-03
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace GM.WPF.Windows
{
	/// <summary>
	/// A base class for a splash screen.
	/// <para>Can be moved by dragging it.</para>
	/// <para>Is not resizable.</para>
	/// </summary>
	public abstract class SplashWindow : BaseWindow, IDisposable
	{
		/// <summary>
		/// A cancellation token source used to cancel the <see cref="ExecuteTasks"/> operation.
		/// <para>Is automatically cancelled when this window is manually closed.</para>
		/// </summary>
		public CancellationTokenSource CancellationTokenSource { get; private set; }

		/// <summary>
		/// Gets or sets the view model. If setting, the current view model is first disposed.
		/// </summary>
		public new SplashWindowViewModel ViewModel
		{
			get => (SplashWindowViewModel)base.ViewModel;
			set => base.ViewModel = value;
		}

		private bool isDisposed;

		/// <summary>
		/// Creates a new instance of <see cref="SplashWindow"/>.
		/// </summary>
		public SplashWindow()
		{
			WindowStyle = WindowStyle.None;
			ResizeMode = ResizeMode.NoResize;
			WindowStartupLocation = WindowStartupLocation.CenterScreen;
			ShowInTaskbar = true;
			Topmost = false;
			_ = SetBinding(TitleProperty, new Binding
			{
				Path = new PropertyPath(nameof(SplashWindowViewModel.Title)),
				Mode = BindingMode.OneWay
			});

			CancellationTokenSource = new CancellationTokenSource();
			MouseDown += Window_MouseDown;
		}

		/// <summary>
		/// Releases all resources.
		/// </summary>
		public void Dispose()
		{
			if(!isDisposed) {
				CancellationTokenSource.Dispose();
				isDisposed = true;
			}
		}

		/// <summary>
		/// Adds an action to the task list. The action will be executed on a background thread when <see cref="ExecuteTasks"/> is called.
		/// </summary>
		/// <param name="description">The description of the task to be displayed to the user.</param>
		/// <param name="work">The action that represents the task.</param>
		public void AddTask(string description, Action work)
		{
			ViewModel.AddTask(description, work);
		}

		/// <summary>
		/// Executes all the actions that were set using the <see cref="AddTask(string, Action)"/> on a background thread and in the same order that they were added.
		/// <para>To cancel, use <see cref="CancellationTokenSource"/>.</para>
		/// <para>Returns true, if it was cancelled.</para>
		/// </summary>
		public async Task<bool> ExecuteTasks()
		{
			// https://johnthiriet.com/cancel-asynchronous-operation-in-csharp

			var taskCompletionSource = new TaskCompletionSource<object>();

			_ = CancellationTokenSource.Token.Register(delegate
			{
				_ = taskCompletionSource.TrySetCanceled();
			});

			Task completedTask = await Task.WhenAny(ViewModel.ExecuteTasks(CancellationTokenSource.Token), taskCompletionSource.Task);

			try {
				// the task is finished and the await will return immediately (the point of this is to throw exception if it occured)
				await completedTask;
			} catch(OperationCanceledException) {
				// do nothing ...
			}

			return CancellationTokenSource.IsCancellationRequested;
		}

		/// <summary>
		/// Manually closes a <see cref="Window"/>.
		/// </summary>
		public new void Close()
		{
			if(!isDisposed) {
				CancellationTokenSource.Cancel();
			}
			base.Close();
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if(e.ChangedButton == MouseButton.Left) {
				DragMove();
			}
		}
	}
}
