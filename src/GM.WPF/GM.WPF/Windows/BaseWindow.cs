/*
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
using System.Threading.Tasks;
using System.Windows;
using GM.WPF.Controls;
using GM.WPF.MVVM;
using GM.WPF.Utility;

namespace GM.WPF.Windows
{
	/// <summary>
	/// The base class for windows. Will always dispose the view model (if disposable) when closed.
	/// <para>For view model, use <see cref="ViewModel"/> property.</para>
	/// <para>For design time view model data, use 'd:DataContext="{d:DesignInstance Type=local:MainWindowViewModel,IsDesignTimeCreatable=True}"'.</para>
	/// </summary>
	public class BaseWindow:Window
	{
		/// <summary>
		/// Gets or sets the view model. If setting, the current view model is first disposed.
		/// </summary>
		protected ViewModel ViewModel
		{
			get => DataContext as ViewModel;
			set
			{
				DisposeViewModel();
				DataContext = value;
			}
		}
		
		private void DisposeViewModel()
		{
			if(ViewModel is IDisposable vmDisposable) {
				vmDisposable.Dispose();
			}
		}

		/// <summary>
		/// Disposes the view model (if disposable) and raises the <see cref="Window.Closed"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnClosed(EventArgs e)
		{
			// dispose inner controls
			var controls = this.GetVisualChildCollection<BaseControl>();
			foreach(BaseControl control in controls) {
				control.DisposeBaseControl();
			}

			// dispose the window, if disposable
			if(this is IDisposable thisDisposable) {
				thisDisposable.Dispose();
			}

			// dispose the viewmodel of this window
			if(DataContext is IDisposable vmDisposable) {
				vmDisposable.Dispose();
			}

			base.OnClosed(e);
		}
	}
}
