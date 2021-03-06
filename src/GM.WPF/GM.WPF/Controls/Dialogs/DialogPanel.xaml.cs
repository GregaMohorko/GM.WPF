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
Created: 2017-12-4
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// A convenient panel for all dialogs. Simply place this where you want the dialogs to appear.
	/// </summary>
	public partial class DialogPanel : BaseControl, IDisposable
	{
		/// <summary>
		/// Creates a new instance of <see cref="DialogPanel"/>.
		/// </summary>
		public DialogPanel()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Creates a new dialog of the specified type and returns it.
		/// <para>The dialog is then automatically removed when it closes.</para>
		/// </summary>
		/// <typeparam name="T">The type of the dialog to create.</typeparam>
		public T Create<T>() where T : Dialog, new()
		{
			T newDialog = new T();
			newDialog.DialogPanel = this;
			_Grid.Children.Add(newDialog);

			return newDialog;
		}

		internal void Remove(Dialog dialog)
		{
			Debug.Assert(dialog.DialogPanel == this);
			_Grid.Children.Remove(dialog);

			dialog.DisposeBaseControl();
		}

		/// <summary>
		/// Disposes all dialogs.
		/// </summary>
		public void Dispose()
		{
			foreach(Dialog dialog in _Grid.Children) {
				dialog.DisposeBaseControl();
			}
		}
	}
}
