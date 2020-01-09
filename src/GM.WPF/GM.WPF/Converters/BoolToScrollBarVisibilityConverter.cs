/*
MIT License

Copyright (c) 2020 Gregor Mohorko

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
Created: 2020-01-09
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using GM.Utility;

namespace GM.WPF.Converters
{
	/// <summary>
	/// Converter from <see cref="bool"/> to <see cref="ScrollBarVisibility"/>.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(ScrollBarVisibility))]
	public class BoolToScrollBarVisibilityConverter : BaseConverter
	{
		/// <summary>
		/// Inverts the value.
		/// </summary>
		public const string PARAM_INVERT = BoolToBoolConverter.PARAM_INVERT;
		/// <summary>
		/// Causes true to represent <see cref="ScrollBarVisibility.Hidden"/> instead of <see cref="ScrollBarVisibility.Visible"/>.
		/// </summary>
		public const string PARAM_HIDDEN = "hidden";
		/// <summary>
		/// Causes true to represent <see cref="ScrollBarVisibility.Auto"/> instead of <see cref="ScrollBarVisibility.Visible"/>.
		/// </summary>
		public const string PARAM_AUTO = "auto";

		/// <summary>
		/// Converts the provided value with the specified parameter to <see cref="ScrollBarVisibility"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="options">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static ScrollBarVisibility? Convert(object value, ref string options)
		{
			bool? boolValue = BoolToBoolConverter.Convert(value, ref options);
			if(boolValue == null) {
				return null;
			}

			if(!boolValue.Value) {
				return ScrollBarVisibility.Disabled;
			} else {
				// options is already lowered in the BoolToBoolConverter
				var trueEquivalent = GetTrueEquivalent(ref options, true);
				return trueEquivalent;
			}
		}

		/// <summary>
		/// Converts the provided value with the specified parameter back to <see cref="bool"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="options">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static bool? ConvertBack(object value, ref string options)
		{
			if(!(value is ScrollBarVisibility scrollBarVisibilityValue)) {
				return null;
			}

			var trueEquivalent = GetTrueEquivalent(ref options);
			bool boolValue;
			if(scrollBarVisibilityValue == ScrollBarVisibility.Disabled) {
				boolValue = false;
			} else {
				if(scrollBarVisibilityValue != trueEquivalent) {
					throw new Exception($"Value is not '{ScrollBarVisibility.Disabled}', but it is not a true equivalent either. The true equivalent is '{trueEquivalent}', but the provided value to convert to bool is '{scrollBarVisibilityValue}'.");
				}
				boolValue = true;
			}

			boolValue = (bool)BoolToBoolConverter.Convert(boolValue, ref options);

			return boolValue;
		}

		private static ScrollBarVisibility GetTrueEquivalent(ref string options, bool alreadyConvertedToLower = false)
		{
			if(options != null) {
				if(!alreadyConvertedToLower) {
					options = options.ToLowerInvariant();
				}

				if(options.Contains(PARAM_HIDDEN)) {
					if(options.Contains(PARAM_AUTO)) {
						throw new Exception($"Converter parameters '{PARAM_HIDDEN}' and '{PARAM_AUTO}' are exclusive. Both were provided: '{options}'.");
					}
					options = StringUtility.RemoveFirstOf(options, PARAM_HIDDEN);
					return ScrollBarVisibility.Hidden;
				} else if(options.Contains(PARAM_AUTO)) {
					options = StringUtility.RemoveFirstOf(options, PARAM_AUTO);
					return ScrollBarVisibility.Auto;
				}
			}

			return ScrollBarVisibility.Visible;
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
			return ConvertBack(value, ref options);
		}
	}
}
