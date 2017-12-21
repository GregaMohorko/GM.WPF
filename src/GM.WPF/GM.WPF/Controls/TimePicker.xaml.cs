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
using System.Diagnostics;
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

namespace GM.WPF.Controls
{
	/// <summary>
	/// A control that enables you to pick a time using a pop-out.
	/// </summary>
	public partial class TimePicker : BaseControl
	{
		/// <summary>
		/// Represents the <see cref="SelectedTime"/> property.
		/// </summary>
		public static readonly DependencyProperty SelectedTimeProperty = DependencyVMProperty(nameof(SelectedTime), typeof(TimePicker), typeof(TimePickerViewModel));

		/// <summary>
		/// Represents the <see cref="SelectedTimeChanged"/> event.
		/// </summary>
		public static readonly RoutedEvent SelectedTimeChangedEvent = EventManager.RegisterRoutedEvent(nameof(SelectedTimeChanged), RoutingStrategy.Bubble, typeof(EventHandler<TimeSpan>), typeof(TimePicker));
		/// <summary>
		/// Represents the <see cref="SelectedTimeSelected"/> event.
		/// </summary>
		public static readonly RoutedEvent SelectedTimeSelectedEvent = EventManager.RegisterRoutedEvent(nameof(SelectedTimeSelected), RoutingStrategy.Bubble, typeof(EventHandler<TimeSpan>), typeof(TimePicker));
		/// <summary>
		/// Represents the <see cref="PopupOpened"/> event.
		/// </summary>
		public static readonly RoutedEvent PopupOpenedEvent = EventManager.RegisterRoutedEvent(nameof(PopupOpened), RoutingStrategy.Bubble, typeof(EventHandler), typeof(TimePicker));

		/// <summary>
		/// Occurs when the <see cref="SelectedTime"/> changes.
		/// </summary>
		public event EventHandler<TimeSpan> SelectedTimeChanged;
		/// <summary>
		/// Occurs when a new time was selected (the popup closes).
		/// </summary>
		public event EventHandler<TimeSpan> SelectedTimeSelected;
		/// <summary>
		/// Occurs when the popup opens.
		/// </summary>
		public event EventHandler PopupOpened;

		/// <summary>
		/// The currently selected time in this time picker.
		/// </summary>
		public TimeSpan SelectedTime
		{
			get => (TimeSpan)GetValue(SelectedTimeProperty);
			set => SetValue(SelectedTimeProperty, value);
		}

		/// <summary>
		/// Creates a new instance of <see cref="TimePicker"/>.
		/// </summary>
		public TimePicker()
		{
			InitializeComponent();

			var vm = new TimePickerViewModel();
			vm.PropertyChanged += ViewModel_PropertyChanged;

			ViewModel = vm;

			SelectedTime = vm.SelectedTime;
		}

		/// <summary>
		/// If the pop-up is open, prevents the focus to leave.
		/// </summary>
		protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			var vm = (TimePickerViewModel)ViewModel;
			
			if(vm.IsPopupOpen) {
				// don't allow focus to leave if the popup is open
				e.Handled = true;
				return;
			}

			base.OnPreviewLostKeyboardFocus(e);
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (TimePickerViewModel)ViewModel;

			switch(e.PropertyName) {
				case nameof(TimePickerViewModel.SelectedTime):
					if(SelectedTime == vm.SelectedTime) {
						return;
					}

					SelectedTime = vm.SelectedTime;

					SelectedTimeChanged?.Invoke(this, SelectedTime);
					break;
				case nameof(TimePickerViewModel.IsPopupOpen):
					if(vm.IsPopupOpen) {
						PopupOpened?.Invoke(this, EventArgs.Empty);
					} else {
						SelectedTimeSelected?.Invoke(this, SelectedTime);
					}
					break;
			}
		}

		/// <summary>
		/// Sets the size of the pop-up.
		/// </summary>
		private void Popup_Opened(object sender, EventArgs e)
		{
			double width = ActualWidth;

			if(width > 200) {
				width = 200;
			} else if(width < 100) {
				width = 100;
			}

			_Popup.Width = width;
			_Popup.Height = width * 0.8;
		}
	}
}
