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
using System.ComponentModel;
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
using GM.Utility;
using GM.WPF.Utility;

namespace GM.WPF.Controls
{
	/// <summary>
	/// A control that holds information about the time. Hours and minutes are editable.
	/// </summary>
	public partial class TimeControl : BaseControl
	{
		/// <summary>
		/// Represents the <see cref="Time"/> property.
		/// </summary>
		public static readonly DependencyProperty TimeProperty = BindableVMProperty(nameof(Time), typeof(TimeControl), typeof(TimeControlViewModel));

		/// <summary>
		/// Represents the <see cref="TimeChanged"/> event.
		/// </summary>
		public static readonly RoutedEvent TimeChangedEvent = EventManager.RegisterRoutedEvent(nameof(TimeChanged), RoutingStrategy.Bubble, typeof(EventHandler<TimeSpan>), typeof(TimeControl));

		/// <summary>
		/// Occurs whenever the <see cref="Time"/> changes.
		/// </summary>
		public event EventHandler<TimeSpan> TimeChanged;

		/// <summary>
		/// The current time value of this time control.
		/// </summary>
		public TimeSpan Time
		{
			get => (TimeSpan)GetValue(TimeProperty);
			set => SetValue(TimeProperty, value);
		}

		/// <summary>
		/// Creates a new instance of <see cref="TimeControl"/>.
		/// </summary>
		public TimeControl()
		{
			InitializeComponent();

			var vm = new TimeControlViewModel();
			vm.PropertyChanged += ViewModel_PropertyChanged;

			ViewModel = vm;
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (TimeControlViewModel)ViewModel;

			switch(e.PropertyName) {
				case nameof(TimeControlViewModel.Time):
					if(Time == vm.Time) {
						return;
					}
					Time = vm.Time;
					TimeChanged?.Invoke(this, Time);
					break;
			}
		}

		private void Control_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			// 5% of margin on left and right
			Size allowedSize = new Size(_TextBox_Minutes.ActualWidth * 0.9, _TextBox_Minutes.ActualHeight * 0.9);

			double fontSize = 8;
			while(true) {
				Size sizeOf00 = TextBlockUtility.MeasureText("00", FontFamily, FontStyle, FontWeight, FontStretch, fontSize);
				if(!sizeOf00.FitsInside(allowedSize)) {
					break;
				}

				++fontSize;
			}

			// get previous size that fitted
			--fontSize;

			FontSize = fontSize;

			Size spaceSize = TextBlockUtility.MeasureText(" ", FontFamily, FontStyle, FontWeight, FontStretch, fontSize);

			double colonAppear_HorizMargin = spaceSize.Width * 0.5;
			_TextBlock_ColonAppear.Margin = new Thickness(colonAppear_HorizMargin, 0, colonAppear_HorizMargin, 0);
		}
	}
}
