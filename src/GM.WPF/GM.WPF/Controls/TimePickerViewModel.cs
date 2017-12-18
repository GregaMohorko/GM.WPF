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
Created: 2017-12-18
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using GM.WPF.MVVM;
using PropertyChanged;

namespace GM.WPF.Controls
{
	class TimePickerViewModel : ViewModel
	{
		public RelayCommand Command_OpenPopup { get; private set; }

		//public TimeSpan SelectedTime { get; set; }
		private TimeSpan _selectedTime;
		public TimeSpan SelectedTime
		{
			get => _selectedTime;
			set
			{
				if(_selectedTime == value) {
					return;
				}
				_selectedTime = value;
			}
		}
		public bool IsPopupOpen { get; set; }

		public string ButtonContent => SelectedTime.ToString("hh':'mm");

		public TimePickerViewModel()
		{
			SelectedTime = new TimeSpan(17, 0, 0);

			if(IsInDesignMode) {
				return;
			}

			Command_OpenPopup = new RelayCommand(OpenPopup);
		}

		private void OpenPopup()
		{
			IsPopupOpen = true;
		}
	}
}
