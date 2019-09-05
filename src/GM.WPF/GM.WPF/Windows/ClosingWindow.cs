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
Created: 2019-09-05
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GM.WPF.Windows
{
	/// <summary>
	/// A window that invokes the <see cref="CanClose"/> before closing to make sure that the window can close.
	/// </summary>
	public abstract class ClosingWindow : BaseWindow, IClosingControl
	{
		/// <summary>
		/// If false, the cancelling process will be cancelled because apparently some action is still required from the user.
		/// </summary>
		public abstract Task<bool> CanClose();

		private bool shouldClose;

		/// <summary>
		/// Invokes the <see cref="CanClose"/> property and only invokes the <see cref="Window.OnClosing(CancelEventArgs)"/> if it returns true.
		/// </summary>
		/// <param name="e">A <see cref="CancelEventArgs"/> that contains the event data.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			if(!shouldClose) {
				e.Cancel = true;

				Application.Current.Dispatcher.InvokeAsync(async delegate
				{
					shouldClose = await CanClose();
					if(shouldClose) {
						Close();
					}
				});

				return;
			}

			base.OnClosing(e);
		}
	}
}
