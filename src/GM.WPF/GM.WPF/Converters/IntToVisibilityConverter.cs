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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GM.Utility;

namespace GM.WPF.Converters
{
	/// <summary>
	/// Converter from <see cref="int"/> to <see cref="Visibility"/>.
	/// <para>
	/// Only 0 is considered as a false value (false = hidden/collapsed).
	/// </para>
	/// </summary>
	[SuppressMessage(null, "CS1591")]
	[ValueConversion(typeof(int),typeof(Visibility))]
	public class IntToVisibilityConverter : BaseConverter
	{
		/// <summary>
		/// Causes false to represent <see cref="Visibility.Collapsed"/> instead of <see cref="Visibility.Hidden"/>.
		/// </summary>
		public const string PARAM_COLLAPSE = BoolToVisibilityConverter.PARAM_COLLAPSE;

		/// <summary>
		/// Checks if the value is below the specified non-inclusive top border. Usage: below(*), where * is replaced with the border.
		/// </summary>
		public const string PARAM_BELOW = "below";

		/// <summary>
		/// Checks if the value is above the specified non-inclusive bottom border. Usage: above(*), where * is replaced with the border.
		/// </summary>
		public const string PARAM_ABOVE = "above";

		/// <summary>
		/// Checks if the value is between the specified non-inclusive borders. There can be multiple between criterias. If it has a ! before it (!between), it means that it is mandatory, otherwise it is optional. Usage: between(a-b), where a and b are the borders.
		/// </summary>
		public const string PARAM_BETWEEN = "between";

		/// <summary>
		/// Converts the provided value with the specified parameter to <see cref="Visibility"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="parameter">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static Visibility? Convert(object value,object parameter)
		{
			int? intValue = value as int?;
			if(intValue == null) {
				return null;
			}

			bool boolValue = intValue.Value != 0;

			var falseEquivalent = Visibility.Hidden;
			
			if(parameter is string options) {
				options = options.ToLower();

				if(options.Contains(PARAM_COLLAPSE)) {
					falseEquivalent = Visibility.Collapsed;
				}

				boolValue = IsBelow(intValue.Value, options) && IsAbove(intValue.Value,options) && IsBetween(intValue.Value,options);
			}

			return boolValue ? Visibility.Visible : falseEquivalent;
		}

		private static bool IsBelow(int value,string options)
		{
			var regex = new Regex($@"{PARAM_BELOW}\((\d+)\)");
			MatchCollection matches = regex.Matches(options);
			if(matches.Count == 0) {
				return true;
			}
			if(matches.Count > 1) {
				throw new ArgumentException($"The provided parameter '{options}' for the converter is invalid: only one '{PARAM_BELOW}' criteria is allowed.", "parameter");
			}

			string belowParameter = matches[0].Groups[1].Value;
			int belowValue = int.Parse(belowParameter);

			return value < belowValue;
		}

		private static bool IsAbove(int value,string options)
		{
			var regex = new Regex($@"{PARAM_ABOVE}\((\d+)\)");
			MatchCollection matches = regex.Matches(options);
			if(matches.Count == 0) {
				return true;
			}
			if(matches.Count > 1) {
				throw new ArgumentException($"The provided parameter '{options}' for the converter is invalid: only one '{PARAM_ABOVE}' criteria is allowed.", "parameter");
			}

			string aboveParameter = matches[0].Groups[1].Value;
			int aboveValue = int.Parse(aboveParameter);

			return value > aboveValue;
		}

		private static bool IsBetween(int value,string options)
		{
			var regex = new Regex($@"!?{PARAM_BETWEEN}\((\d+)-(\d+)\)");
			MatchCollection matches = regex.Matches(options);
			if(matches.Count == 0) {
				return true;
			}

			bool isAtLeastOneTrue = false;
			foreach(Match match in matches) {
				bool isMandatory = match.Value[0] == '!';

				string firstParameter = match.Groups[1].Value;
				string secondParameter = match.Groups[2].Value;
				int bottomBorder = int.Parse(firstParameter);
				int topBorder = int.Parse(secondParameter);
				if(bottomBorder > topBorder) {
					Util.Swap(ref bottomBorder, ref topBorder);
				}

				if(value>bottomBorder && value < topBorder) {
					// do not return true! there can still be a mandatory between that is not satisfied
					isAtLeastOneTrue = true;
				} else {
					// is not between
					if(isMandatory) {
						// if mandatory, the whole converter values is absolutelly false, no need to check other criterias
						return false;
					}
				}
			}

			return isAtLeastOneTrue;
		}

		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(value, parameter);
		}

		public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// converting back is disabled because the information about the int value is lost
			return DependencyProperty.UnsetValue;
		}
	}
}
