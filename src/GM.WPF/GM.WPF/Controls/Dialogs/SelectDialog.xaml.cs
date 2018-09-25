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
Created: 2017-11-29
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
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
	/// Interaction logic for SelectDialog.xaml
	/// </summary>
	public partial class SelectDialog : TaskDialog
	{
		/// <summary>
		/// Creates a new instance of <see cref="SelectDialog"/>.
		/// </summary>
		public SelectDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Shows the select dialog and waits for the user to select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="message">The message text.</param>
		/// <param name="items">Items to put in the <see cref="ListView"/> control.</param>
		public async Task<IEnumerable<T>> Show<T>(string message, IEnumerable<T> items)
		{
			bool wasCancelled = await ShowAndWait(message, items, SelectionMode.Multiple);
			if(wasCancelled) {
				return null;
			}

			return _ListView.SelectedItems.Cast<T>();
		}

		/// <summary>
		/// Shows the select dialog and waits for the user to select a single item and confirm. If the user cancels the dialog, this method returns null.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="message">The message text.</param>
		/// <param name="items">Items to put in the <see cref="ListView"/> control.</param>
		public async Task<T> ShowSingle<T>(string message, IEnumerable<T> items)
		{
			bool wasCancelled = await ShowAndWait(message, items, SelectionMode.Single);
			if(wasCancelled) {
				return default(T);
			}

			return (T)_ListView.SelectedItem;
		}

		private async Task<bool> ShowAndWait<T>(string message, IEnumerable<T> items, SelectionMode selectionMode)
		{
			var vm = new SelectDialogViewModel()
			{
				Message=message,
				Items=items,
				SelectionMode=selectionMode
			};
			ViewModel = vm;

			bool wasCancelled = await WaitDialog();
			Close();
			return wasCancelled;
		}

		private void Button_OK_Click(object sender, RoutedEventArgs e)
		{
			EndDialog();
		}

		private void Button_Cancel_Click(object sender, RoutedEventArgs e)
		{
			EndDialog(true);
		}
	}
}
