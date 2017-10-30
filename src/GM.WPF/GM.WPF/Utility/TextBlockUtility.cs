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
		/// Measures the provided text block and returns it's size when drawn in WPF application.
		/// </summary>
		/// <param name="textBlock">The text block to measure.</param>
		public static Size MeasureText(TextBlock textBlock)
		{
			return MeasureText(textBlock.Text, textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch, textBlock.FontSize);
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
		public static Size MeasureText(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
		{
			FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(fontFamily, fontStyle, fontWeight, fontStretch), fontSize, Brushes.Black);

			return new Size(formattedText.Width, formattedText.Height);
		}
	}
}
