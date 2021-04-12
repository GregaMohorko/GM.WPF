/*
MIT License

Copyright (c) 2021 Gregor Mohorko

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
Created: 2021-03-06
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GM.WPF.Utility;

namespace GM.WPF.Controls
{
	/// <summary>
	/// A <see cref="TextBox"/> with a <see cref="Watermark"/>.
	/// </summary>
	public partial class WatermarkTextBox : BaseControl
	{
		/// <summary>
		/// The property for <see cref="AcceptsReturn"/>.
		/// </summary>
		public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool), typeof(WatermarkTextBox), new PropertyMetadata(TextBoxBase.AcceptsReturnProperty.GetDefaultValue<bool>()));
		/// <summary>
		/// Gets or sets a value that indicates how the text editing control responds when the user presses the ENTER key.
		/// </summary>
		public bool AcceptsReturn
		{
			get => (bool)GetValue(AcceptsReturnProperty);
			set => SetValue(AcceptsReturnProperty, value);
		}

		/// <summary>
		/// The property for <see cref="AcceptsTab"/>.
		/// </summary>
		public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty.Register(nameof(AcceptsTab), typeof(bool), typeof(WatermarkTextBox), new PropertyMetadata(TextBoxBase.AcceptsTabProperty.GetDefaultValue<bool>()));
		/// <summary>
		/// Gets or sets a value that indicates how the text editing control responds when the user presses the TAB key.
		/// </summary>
		public bool AcceptsTab
		{
			get => (bool)GetValue(AcceptsTabProperty);
			set => SetValue(AcceptsTabProperty, value);
		}

		/// <summary>
		/// The property for <see cref="Text"/>.
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(WatermarkTextBox));
		/// <summary>
		/// Gets or sets the text contents of the text box.
		/// </summary>
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		/// <summary>
		/// The property for <see cref="Watermark"/>.
		/// </summary>
		public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(WatermarkTextBox));
		/// <summary>
		/// The watermark text that will be visible only when there is no text.
		/// </summary>
		public string Watermark
		{
			get => (string)GetValue(WatermarkProperty);
			set => SetValue(WatermarkProperty, value);
		}

		/// <summary>
		/// The property for <see cref="WatermarkForeground"/>.
		/// </summary>
		public static readonly DependencyProperty WatermarkForegroundProperty = DependencyProperty.Register(nameof(WatermarkForeground), typeof(Brush), typeof(WatermarkTextBox), new PropertyMetadata(Brushes.Gray));
		/// <summary>
		/// The foreground color of the watermark.
		/// </summary>
		public Brush WatermarkForeground
		{
			get => (Brush)GetValue(WatermarkForegroundProperty);
			set => SetValue(WatermarkForegroundProperty, value);
		}

		/// <summary>
		/// Creates a new instance of <see cref="WatermarkTextBox"/>.
		/// </summary>
		public WatermarkTextBox()
		{
			InitializeComponent();
		}
	}
}
