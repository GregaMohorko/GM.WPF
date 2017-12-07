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
Created: 2017-12-4
Author: Grega Mohorko
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

namespace GM.WPF.Controls
{
	/// <summary>
	/// A control that has a transparent dark background, a message and a progress bar.
	/// </summary>
	public partial class ProgressOverlay : BaseControl
	{
		/// <summary>
		/// Represents the <see cref="Message"/> property.
		/// </summary>
		public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(ProgressOverlay), new PropertyMetadata(OnMessageChanged));
		/// <summary>
		/// Represents the <see cref="ProgressValue"/> property.
		/// </summary>
		public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(nameof(ProgressValue), typeof(double?), typeof(ProgressOverlay), new PropertyMetadata(OnProgressValueChanged));

		private static void OnMessageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			var c = (ProgressOverlay)o;
			var vm = (ProgressOverlayViewModel)c.ViewModel;
			vm.Message = c.Message;
		}

		private static void OnProgressValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			var c = (ProgressOverlay)o;
			var vm = (ProgressOverlayViewModel)c.ViewModel;
			vm.ProgressValue = c.ProgressValue;
		}

		/// <summary>
		/// Get or set the message.
		/// </summary>
		public string Message
		{
			get => (string)GetValue(MessageProperty);
			set => SetValue(MessageProperty, value);
		}

		/// <summary>
		/// Get or set the progress value. Should be between 0 and 100. If null, the progress bar will be set to intederminate state.
		/// </summary>
		public double? ProgressValue
		{
			get => (double?)GetValue(ProgressValueProperty);
			set => SetValue(ProgressValueProperty, value);
		}

		/// <summary>
		/// Creates a new instance of <see cref="ProgressOverlay"/>.
		/// </summary>
		public ProgressOverlay()
		{
			InitializeComponent();

			var vm = new ProgressOverlayViewModel();
			ViewModel = vm;
		}
	}
}
