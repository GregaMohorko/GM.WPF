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
Created: 2018-11-27
Author: Gregor Mohorko
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GM.Utility;

namespace GM.WPF.Converters
{
	/// <summary>
	/// Converter from <see cref="ICollection"/> to <see cref="bool"/>.
	/// <para>Is considered true, when the collection is not empty.</para>
	/// </summary>
	[ValueConversion(typeof(ICollection), typeof(bool))]
	public class ICollectionToBoolConverter : BaseConverter
	{
		/// <summary>
		/// Inverts the value.
		/// </summary>
		public const string PARAM_INVERT = BoolToBoolConverter.PARAM_INVERT;

		/// <summary>
		/// Determines how many items must there be in the collection to be considered as true. Default is 1.
		/// <para>Usage: atleast(*), where * is replaced with the number of items.</para>
		/// </summary>
		public const string PARAM_ATLEAST = "atleast";

		/// <summary>
		/// Converts the provided value with the specified parameter to <see cref="bool"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="options">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static bool? Convert(object value, ref string options)
		{
			if(!(value is ICollection collection)) {
				return null;
			}

			bool boolValue = collection.Count >= 1;

			if(options != null) {
				options = options.ToLower();

				boolValue = AtLeast(collection, ref options) ?? boolValue;
				if(options.Contains(PARAM_INVERT)) {
					boolValue = !boolValue;
					options = StringUtility.RemoveFirstOf(options, PARAM_INVERT);
				}
			}

			return boolValue;
		}

		private readonly static Regex regex_atLeast = new Regex($@"{PARAM_ATLEAST}\((\d+)\)", RegexOptions.Compiled);
		private static bool? AtLeast(ICollection collection, ref string options)
		{
			MatchCollection matches = regex_atLeast.Matches(options);
			if(matches.Count == 0) {
				return null;
			}
			if(matches.Count > 1) {
				throw new ArgumentException($"The provided parameter '{options}' for the converter is invalid: only one '{PARAM_ATLEAST}' criteria is allowed.");
			}

			options = StringUtility.RemoveAllOf(options, $"{PARAM_ATLEAST}(");

			string atLeastParameter = matches[0].Groups[1].Value;
			int atLeastValue = int.Parse(atLeastParameter);

			return collection.Count >= atLeastValue;
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string options = parameter as string;
			return Convert(value, ref options);
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// converting back is disabled because the information about the collection is lost
			return DependencyProperty.UnsetValue;
		}
	}
}
