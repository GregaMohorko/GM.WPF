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
Created: 2020-01-15
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
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
	/// Converter from <see cref="string"/> to <see cref="string"/>.
	/// <para>Look at constants starting with PARAM_ for parameter options.</para>
	/// </summary>
	[ValueConversion(typeof(string), typeof(string))]
	public class StringToStringConverter : BaseConverter
	{
		/// <summary>
		/// Transforms into lowercase.
		/// </summary>
		public const string PARAM_LOWERCASE = "lowercase";
		/// <summary>
		/// Transforms into UPPERCASE.
		/// </summary>
		public const string PARAM_UPPERCASE = "uppercase";
		/// <summary>
		/// Transforms into PascalCase.
		/// </summary>
		public const string PARAM_PASCALCASE = "pascalcase";
		/// <summary>
		/// Transforms into Title Case.
		/// </summary>
		public const string PARAM_TITLECASE = "titlecase";

		/// <summary>
		/// Converts the provided value with the specified parameter to <see cref="string"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="options">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static string Convert(object value, ref string options)
		{
			if(!(value is string stringValue)) {
				return null;
			}

			if(options != null) {
				options = options.ToLowerInvariant();

				if(options.Contains(PARAM_LOWERCASE)) {
					stringValue = stringValue.ToLowerInvariant();
				} else if(options.Contains(PARAM_UPPERCASE)) {
					stringValue = stringValue.ToUpperInvariant();
				} else if(options.Contains(PARAM_PASCALCASE)) {
					stringValue = stringValue.ToPascalCase();
				} else if(options.Contains(PARAM_TITLECASE)) {
					stringValue = stringValue.ToTitleCase();
				}
			}

			return stringValue;
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
			if(value is null) {
				return null;
			}
			// format of the original text is lost, so let's just return the string as it currently is
			if(!(value is string valueS)) {
				return DependencyProperty.UnsetValue;
			}
			return valueS;
		}
	}
}
