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

namespace GM.WPF.Converters
{
	/// <summary>
	/// Converter from <see cref="string"/> to <see cref="bool"/>.
	/// <para>
	/// Is considered true, when the string is not null.
	/// </para>
	/// </summary>
	[SuppressMessage(null, "CS1591")]
	[ValueConversion(typeof(string), typeof(bool))]
	public class StringToBoolConverter : BaseConverter
	{
		/// <summary>
		/// Causes empty strings to be considered as false.
		/// </summary>
		public const string PARAM_EMPTY = "empty";

		/// <summary>
		/// Inverts the value.
		/// </summary>
		public const string PARAM_INVERT = BoolToBoolConverter.PARAM_INVERT;

		/// <summary>
		/// Converts the provided value with the specified parameter to <see cref="bool"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="parameter">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static bool? Convert(object value,object parameter)
		{
			string stringValue = value as string;

			bool boolValue = stringValue != null;

			if(parameter is string options) {
				options = options.ToLower();

				if(options.Contains(PARAM_EMPTY)) {
					boolValue = boolValue && stringValue.Length > 0;
				}
				if(options.Contains(PARAM_INVERT)) {
					boolValue = !boolValue;
				}
			}

			return boolValue;
		}

		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(value, parameter);
		}

		public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// converting back is disabled because the information about the string is lost
			return DependencyProperty.UnsetValue;
		}
	}
}
