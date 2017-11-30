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

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// Interaction logic for ChooseDialog.xaml
	/// </summary>
	public partial class ChooseDialog : TaskDialog
	{
		/// <summary>
		/// Creates a new instance of <see cref="ChooseDialog"/>.
		/// </summary>
		public ChooseDialog()
		{
			InitializeComponent();

			_WrapPanel_Buttons.Children.Clear();
		}

		/// <summary>
		/// Shows the choose dialog and waits for the users response. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <param name="question">Question text.</param>
		/// <param name="answers">The answers to choose from.</param>
		public async Task<int?> Show(string question, params string[] answers)
		{
			_TextBlock_Question.Text = question;
			_WrapPanel_Buttons.Children.Clear();

			int? response = null;

			for(int i = 0; i < answers.Length; i++) {
				var answerButton = new Button();
				answerButton.Content = answers[i];
				answerButton.Tag = i;
				answerButton.Click += delegate
				{
					response = (int)answerButton.Tag;
					EndDialog();
				};

				_WrapPanel_Buttons.Children.Add(answerButton);
			}

			await WaitDialog();
			Hide();

			return response;
		}

		private void Button_Cancel_Click(object sender, RoutedEventArgs e)
		{
			EndDialog(true);
		}
	}
}
