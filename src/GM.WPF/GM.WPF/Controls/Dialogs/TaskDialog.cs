﻿/*
MIT License

Copyright (c) 2017 Grega Mohorko

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
Created: 2017-10-30
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GM.WPF.Utility;

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// A dialog that has an awaitable task.
	/// <para>Use method <see cref="WaitDialog"/> for asynchronous waiting and <see cref="EndDialog"/> for continuing.</para>
	/// </summary>
	public class TaskDialog : Dialog
	{
		private TaskCompletionSource<bool> dialogAwaiter;

		/// <summary>
		/// Shows the dialog and then waits until the <see cref="EndDialog"/> is called.
		/// <para>The returned value determines whether the dialog was supposedly cancelled.</para>
		/// </summary>
		protected Task<bool> WaitDialog()
		{
			Show();
			dialogAwaiter = new TaskCompletionSource<bool>();
			return dialogAwaiter.Task;
		}

		/// <summary>
		/// Resumes the waiting thread from where the <see cref="WaitDialog"/> was called.
		/// </summary>
		/// <param name="wasCancelled">Determines whether the dialog was cancelled.</param>
		protected void EndDialog(bool wasCancelled = false)
		{
			dialogAwaiter.SetResult(wasCancelled);
		}
	}
}
