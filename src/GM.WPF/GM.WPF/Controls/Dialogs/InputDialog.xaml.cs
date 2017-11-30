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
Created: 2017-11-28
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
using GM.WPF.Converters;

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// Interaction logic for InputDialog.xaml
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
		
		private static readonly string TextBox_TextChanged_ConverterParameter = $"{StringToVisibilityConverter.PARAM_EMPTY}_{StringToVisibilityConverter.PARAM_INVERT}";

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			// this should be done in XAML using Binding, but it doesn't seem to work ...
			string options = TextBox_TextChanged_ConverterParameter;
			_Label_Watermark.Visibility = (Visibility)StringToVisibilityConverter.Convert(_TextBox.Text, ref options);
		}

		/// <summary>
		/// Shows the input dialog and waits for the users reponse. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <param name="message">The message to show above the input box.</param>
		/// <param name="watermark">The text to show in the input box.</param>
		/// <param name="defaultText">The default text that will already be in the input box.</param>
		public async Task<string> Show(string message = null, string watermark = null, string defaultText = null)
		{
			_TextBlock_Message.Text = message;
			_Label_Watermark.Content = watermark;
			_TextBox.Text = defaultText;
			
			await WaitDialog();
			Hide();

			if(WasCancelled) {
				return null;
			}

			return _TextBox.Text;
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
