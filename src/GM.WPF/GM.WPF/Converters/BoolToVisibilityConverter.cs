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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GM.Utility;

namespace GM.WPF.Converters
{
	/// <summary>
	/// Converter from <see cref="bool"/> to <see cref="Visibility"/>.
	/// </summary>
	[ValueConversion(typeof(bool),typeof(Visibility))]
	public class BoolToVisibilityConverter:BaseConverter
	{
		/// <summary>
		/// Inverts the value.
		/// </summary>
		public const string PARAM_INVERT = BoolToBoolConverter.PARAM_INVERT;

		/// <summary>
		/// Causes false to represent <see cref="Visibility.Collapsed"/> instead of <see cref="Visibility.Hidden"/>.
		/// </summary>
		public const string PARAM_COLLAPSE = "collapse";

		/// <summary>
		/// Converts the provided value with the specified parameter to <see cref="Visibility"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="options">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static Visibility? Convert(object value, ref string options)
		{
			bool? boolValue = BoolToBoolConverter.Convert(value, ref options);
			if(boolValue == null) {
				return null;
			}

			var falseEquivalent = Visibility.Hidden;
			
			if(options!=null) {
				// is already lowered in the BoolToBoolConverter
				//options = options.ToLower();

				if(options.Contains(PARAM_COLLAPSE)) {
					falseEquivalent = Visibility.Collapsed;
					options = StringUtility.RemoveFirstOf(options, PARAM_COLLAPSE);
				}
			}

			return boolValue.Value ? Visibility.Visible : falseEquivalent;
		}

		/// <summary>
		/// Converts the provided value with the specified parameter back to <see cref="bool"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="options">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static bool? ConvertBack(object value,ref string options)
		{
			if(!(value is Visibility)) {
				return null;
			}

			var visibilityValue = (Visibility)value;

			var invert = false;
			var falseEquivalent = Visibility.Hidden;

			if(options!=null) {
				options = options.ToLower();

				if(options.Contains(PARAM_COLLAPSE)) {
					options = StringUtility.RemoveFirstOf(options, PARAM_COLLAPSE);
					if(visibilityValue == Visibility.Hidden) {
						return null;
					}
					falseEquivalent = Visibility.Collapsed;
				} else if(visibilityValue == Visibility.Collapsed) {
					options = StringUtility.RemoveFirstOf(options, PARAM_COLLAPSE);
					return null;
				}
				if(options.Contains(PARAM_INVERT)) {
					invert = true;
					options = StringUtility.RemoveFirstOf(options, PARAM_INVERT);
				}
			}

			bool boolValue = visibilityValue != falseEquivalent;

			if(invert) {
				boolValue = !boolValue;
			}

			return boolValue;
		}

		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string options = parameter as string;
			return Convert(value, ref options);
		}

		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string options = parameter as string;
			return Convert(value, ref options);
		}
	}
}
