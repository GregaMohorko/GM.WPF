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
Created: 2020-10-30
Author: Gregor Mohorko
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
	/// A dialog with a <see cref="DatePicker"/>.
	/// </summary>
	public partial class DateDialog : TaskDialog
	{
		/// <summary>
		/// Creates a new instance of <see cref="DateDialog"/>.
		/// </summary>
		public DateDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Shows the date dialog and waits for the users response. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <param name="message">The message to show above the <see cref="DatePicker"/>.</param>
		/// <param name="defaultValue">The default date that will already be selected.</param>
		public async Task<DateTime?> Show(string message = null, DateTime? defaultValue = null)
		{
			var vm = new DateDialogViewModel
			{
				Message = message,
				Date = defaultValue
			};
			ViewModel = vm;

			bool wasCancelled = await WaitDialog();
			Close();

			if(wasCancelled) {
				return default;
			}

			return vm.Date;
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
