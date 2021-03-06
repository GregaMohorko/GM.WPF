﻿/*
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
Created: 2017-11-26
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
	/// A dialog with a message and an OK button.
	/// </summary>
	public partial class MessageDialog : TaskDialog
	{
		/// <summary>
		/// Background for messages of type <see cref="MessageType.NORMAL"/>.
		/// </summary>
		public static Brush NormalBackground = DefaultBackground;
		/// <summary>
		/// Backgroudn for messages of type <see cref="MessageType.SUCCESS"/>.
		/// </summary>
		public static Brush SuccessBackground = Brushes.Green;
		/// <summary>
		/// Background for messages of type <see cref="MessageType.WARNING"/>.
		/// </summary>
		public static Brush WarningBackground = Brushes.Orange;
		/// <summary>
		/// Background for messages of type <see cref="MessageType.ERROR"/>.
		/// </summary>
		public static Brush ErrorBackground = Brushes.Red;

		/// <summary>
		/// Creates a new instance of <see cref="MessageDialog"/>.
		/// </summary>
		public MessageDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Shows the dialog with the specified message.
		/// </summary>
		/// <param name="message">The message to show.</param>
		/// <param name="type">The type of the message.</param>
		public async Task Show(string message, MessageType type = MessageType.NORMAL)
		{
			Brush background;
			switch(type) {
				case MessageType.NORMAL:
					background = NormalBackground;
					break;
				case MessageType.SUCCESS:
					background = SuccessBackground;
					break;
				case MessageType.WARNING:
					background = WarningBackground;
					break;
				case MessageType.ERROR:
					background = ErrorBackground;
					break;
				default:
					throw new NotImplementedException();
			}

			SetBackground(background);
			_TextBlock.Text = message;

			_ = await WaitDialog();
			Close();
		}

		private void Button_OK_Click(object sender, RoutedEventArgs e)
		{
			EndDialog();
		}
	}
}
