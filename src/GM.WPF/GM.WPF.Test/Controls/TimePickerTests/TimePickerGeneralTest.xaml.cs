/*
MIT License

Copyright (c) 2018 Grega Mohorko

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

Project: GM.WPF.Test
Created: 2018-12-26
Author: GregaMohorko
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
using GM.WPF.Controls;

namespace GM.WPF.Test.Controls.TimePickerTests
{
	public partial class TimePickerGeneralTest : BaseControl
	{
		public TimePickerGeneralTest()
		{
			InitializeComponent();
		}

		private void Left_SelectedTimeChanged(object sender, TimeSpan e)
		{
			EventOccured(nameof(TimePicker.SelectedTimeChanged), (TimePicker)sender, _Event_TextBox_Left);
		}

		private void Right_SelectedTimeChanged(object sender, TimeSpan e)
		{
			EventOccured(nameof(TimePicker.SelectedTimeChanged), (TimePicker)sender, _Event_TextBox_Right);
		}

		private void Left_SelectedTimeSelected(object sender, TimeSpan e)
		{
			EventOccured(nameof(TimePicker.SelectedTimeSelected), (TimePicker)sender, _Event_TextBox_Left);
		}

		private void Right_SelectedTimeSelected(object sender, TimeSpan e)
		{
			EventOccured(nameof(TimePicker.SelectedTimeSelected), (TimePicker)sender, _Event_TextBox_Right);
		}

		private void Left_PopupOpened(object sender, EventArgs e)
		{
			EventOccured(nameof(TimePicker.PopupOpened), (TimePicker)sender, _Event_TextBox_Left);
		}

		private void Right_PopupOpened(object sender, EventArgs e)
		{
			EventOccured(nameof(TimePicker.PopupOpened), (TimePicker)sender, _Event_TextBox_Right);
		}

		private void EventOccured(string eventName, TimePicker timePicker, TextBox associatedTextBox)
		{
			string text = $"{eventName}: {timePicker.SelectedTime}";
			if(!string.IsNullOrEmpty(associatedTextBox.Text)) {
				text = Environment.NewLine + text;
			}

			associatedTextBox.AppendText(text);
			associatedTextBox.ScrollToEnd();
		}
	}
}
