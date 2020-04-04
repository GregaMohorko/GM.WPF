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
Created: 2019-10-03
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GM.WPF.MVVM;

namespace GM.WPF.Windows
{
	/// <summary>
	/// The view model for <see cref="SplashWindow"/>. Can be extended to add custom properties.
	/// </summary>
	public class SplashWindowViewModel : ViewModel
	{
		/// <summary>
		/// The title of the window.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// The description of the task currently processing.
		/// </summary>
		public string CurrentTaskDescription { get; private set; }
		/// <summary>
		/// The index of the task currently processing.
		/// </summary>
		public int CurrentTaskIndex { get; private set; }
		/// <summary>
		/// Gets the total count of tasks.
		/// </summary>
		public int TotalTasks { get; private set; }

		private readonly List<(Func<string> DescriptionGeter, Action Work)> tasks;

		/// <summary>
		/// Creates a new instance of <see cref="SplashWindowViewModel"/>.
		/// </summary>
		public SplashWindowViewModel() : this(null) { }

		/// <summary>
		/// Creates a new instance of <see cref="SplashWindowViewModel"/>.
		/// </summary>
		/// <param name="appName">The name of the application. Will only be used for the window title.</param>
		public SplashWindowViewModel(string appName)
		{
			if(IsInDesignMode) {
				Title = "Opening - My Application";
				CurrentTaskDescription = "Connecting with the database";
				CurrentTaskIndex = 3;
				TotalTasks = 42;
				return;
			}

			tasks = new List<(Func<string> DescriptionGeter, Action Work)>();

			Title = "Opening";
			if(appName != null) {
				Title += $" - {appName}";
			}
		}

		internal void AddTask(Func<string> descriptionGetter, Action work)
		{
			tasks.Add((descriptionGetter, work));
			++TotalTasks;
		}

		internal Task ExecuteTasks(CancellationToken ct)
		{
			return Task.Run(delegate
			{
				for(int i = 0; i < tasks.Count; ++i) {
					(Func<string> taskDescriptionGetter, Action taskAction) = tasks[i];

					// set the progress data
					CurrentTaskIndex = i + 1;
					CurrentTaskDescription = taskDescriptionGetter();

					ct.ThrowIfCancellationRequested();

					// execute the task
					taskAction.Invoke();

					ct.ThrowIfCancellationRequested();
				}
			}, ct);
		}
	}
}
