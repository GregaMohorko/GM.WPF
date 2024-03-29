﻿/*
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
using CommunityToolkit.Mvvm.Input;
using GM.WPF.MVVM;

namespace GM.WPF.Test.Controls.TimePickerTests
{
    class TimePickerBindingTestViewModel:ViewModel
    {
		public RelayCommand Command_SetValueToCurrentTime { get; private set; }
		public RelayCommand Command_SetValueToNull { get; private set; }

		public TimeSpan? SelectedTime { get; set; }

		public TimePickerBindingTestViewModel()
		{
			if(IsInDesignMode) {
				SelectedTime = DateTime.Now.TimeOfDay;
				return;
			}

			Command_SetValueToCurrentTime = new RelayCommand(SetValueToCurrentTime);
			Command_SetValueToNull = new RelayCommand(SetValueToNull);
		}

		private void SetValueToCurrentTime()
		{
			SelectedTime = DateTime.Now.TimeOfDay;
		}

		private void SetValueToNull()
		{
			SelectedTime = null;
		}
    }
}
