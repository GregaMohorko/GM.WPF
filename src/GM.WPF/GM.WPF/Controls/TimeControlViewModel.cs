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
Created: 2017-12-17
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GM.Utility;
using GM.WPF.MVVM;

namespace GM.WPF.Controls
{
	class TimeControlViewModel : ViewModel
	{
		public RelayCommand Command_IncreaseHours { get; private set; }
		public RelayCommand Command_IncreaseMinutes { get; private set; }
		public RelayCommand Command_DecreaseHours { get; private set; }
		public RelayCommand Command_DecreaseMinutes { get; private set; }

		public TimeSpan Time { get; set; }

		public string HoursText
		{
			get => Time.Hours.ToString("D2");
			set
			{
				int? intValue = IntUtility.ParseNullable(value);
				if(intValue == null || intValue > 24 || intValue < 0) {
					return;
				}

				if(intValue == 24) {
					intValue = 0;
				}

				Time = new TimeSpan(intValue.Value, Time.Minutes, 0);
			}
		}

		public string MinutesText
		{
			get => Time.Minutes.ToString("D2");
			set
			{
				int? intValue = IntUtility.ParseNullable(value);
				if(intValue == null || intValue > 60 || intValue < 0) {
					return;
				}

				if(intValue == 60) {
					intValue = 0;
				}

				Time = new TimeSpan(Time.Hours, intValue.Value, 0);
			}
		}

		public TimeControlViewModel()
		{
			if(IsInDesignMode) {
				return;
			}

			Command_IncreaseHours = new RelayCommand(IncreaseHours);
			Command_IncreaseMinutes = new RelayCommand(IncreaseMinutes);
			Command_DecreaseHours = new RelayCommand(DecreaseHours);
			Command_DecreaseMinutes = new RelayCommand(DecreaseMinutes);
		}

		private void IncreaseHours()
		{
			int increasedHour = Time.Hours + 1;
			if(increasedHour >= 24) {
				increasedHour = 0;
			}

			Time = new TimeSpan(increasedHour, Time.Minutes, 0);
		}

		private void IncreaseMinutes()
		{
			int increasedMinute = Time.Minutes + 1;
			if(increasedMinute >= 60) {
				increasedMinute = 0;
			}

			Time = new TimeSpan(Time.Hours, increasedMinute, 0);
		}

		private void DecreaseHours()
		{
			int decreasedHour = Time.Hours - 1;
			if(decreasedHour < 0) {
				decreasedHour = 23;
			}

			Time = new TimeSpan(decreasedHour, Time.Minutes, 0);
		}

		private void DecreaseMinutes()
		{
			int decreasedMinute = Time.Minutes - 1;
			if(decreasedMinute < 0) {
				decreasedMinute = 59;
			}

			Time = new TimeSpan(Time.Hours, decreasedMinute, 0);
		}
	}
}
