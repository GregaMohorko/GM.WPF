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
Created: 2017-10-30
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GM.WPF.Utility
{
	/// <summary>
	/// Utilities for <see cref="TextBlock"/>.
	/// </summary>
	public static class TextBlockUtility
	{
		/// <summary>
		/// Measures this text block and returns it's size when drawn in WPF application.
		/// </summary>
		/// <param name="textBlock">The text block to measure.</param>
		public static Size MeasureText(this TextBlock textBlock)
		{
			return MeasureText(textBlock.Text, textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch, textBlock.FontSize, textBlock);
		}

		/// <summary>
		/// Measures the provided text with the specified parameters and returns it's size when drawn in WPF application.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <param name="fontFamily">The font family.</param>
		/// <param name="fontStyle">The font style.</param>
		/// <param name="fontWeight">The font weight.</param>
		/// <param name="fontStretch">The font stretch.</param>
		/// <param name="fontSize">The font size.</param>
		/// <param name="visual">The visual target object where this text will be drawn. Used for getting the DPI information. If not provided, it will use the main window of the current application.</param>
		public static Size MeasureText(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize, Visual visual = null)
		{
			if(visual == null) {
				visual = Application.Current.MainWindow;
			}
			if(visual == null) {
				throw new ArgumentNullException(nameof(visual));
			}
			DpiScale dpiScale = VisualTreeHelper.GetDpi(visual);
			var formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(fontFamily, fontStyle, fontWeight, fontStretch), fontSize, Brushes.Black, dpiScale.PixelsPerDip);

			return new Size(formattedText.Width, formattedText.Height);
		}
	}
}
