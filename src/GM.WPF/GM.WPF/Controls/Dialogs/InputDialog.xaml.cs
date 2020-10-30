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
Created: 2017-11-28
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
using GM.WPF.Converters;

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// A dialog with an input box.
	/// </summary>
	public partial class InputDialog : TaskDialog
	{
		/// <summary>
		/// Creates a new instance of <see cref="InputDialog"/>.
		/// </summary>
		public InputDialog()
		{
			InitializeComponent();

			_TextBox.TextChanged += TextBox_TextChanged;
		}

		private const string TEXTBOX_TEXTCHANGED_CONVERTERPARAMETER = StringToVisibilityConverter.PARAM_EMPTY + "_" + StringToVisibilityConverter.PARAM_INVERT;

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			// this should be done in XAML using Binding, but it doesn't seem to work ...
			string options = TEXTBOX_TEXTCHANGED_CONVERTERPARAMETER;
			_Label_Watermark.Visibility = (Visibility)StringToVisibilityConverter.Convert(_TextBox.Text, ref options);
		}

		/// <summary>
		/// Shows the input dialog and waits for the users response. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <param name="message">The message to show above the input box.</param>
		/// <param name="watermark">The text to show in the input box.</param>
		/// <param name="defaultText">The default text that will already be in the input box.</param>
		/// <param name="acceptsTab">Determines whether the textbox will accept tabulator as an input.</param>
		/// <param name="acceptsReturn">Determines whether the textbox will accept return as an input (meaning it's probably multiline).</param>
		public Task<string> Show(string message = null, string watermark = null, string defaultText = null, bool acceptsTab = false, bool acceptsReturn = false)
		{
			return Show<string>(message, watermark, defaultText, acceptsTab, acceptsReturn);
		}

		/// <summary>
		/// Shows the input dialog and waits for the users response. If the user cancels the dialog, this method will return the default value.
		/// </summary>
		/// <typeparam name="T">The type of the input.</typeparam>
		/// <param name="message">The message to show above the input box.</param>
		/// <param name="watermark">The text to show in the input box.</param>
		/// <param name="defaultValue">The default value that will already be in the input box.</param>
		public Task<T> Show<T>(string message = null, string watermark = null, T defaultValue = default)
		{
			return Show(message, watermark, defaultValue, false, false);
		}

		private async Task<T> Show<T>(string message, string watermark, T defaultValue, bool acceptsTab, bool acceptsReturn)
		{
			var vm = new InputDialogViewModel<T>
			{
				Message = message,
				Watermark = watermark,
				Text = defaultValue?.ToString(),
				AcceptsTab = acceptsTab,
				AcceptsReturn = acceptsReturn
			};
			if(vm.Text == null && typeof(T) == typeof(string)) {
				// this is to distinguish between clicking 'cancel' and clicking 'ok' with an empty input
				vm.Text = "";
			}
			ViewModel = vm;

			bool wasCancelled = await WaitDialog();
			Close();

			if(wasCancelled) {
				return default;
			}

			return vm.Value;
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
