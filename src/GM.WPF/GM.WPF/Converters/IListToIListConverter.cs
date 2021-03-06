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
Created: 2017-10-30
Author: Grega Mohorko
*/

using System;
using System.Collections;
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
	/// Converter from <see cref="IList"/> to <see cref="IList"/>.
	/// </summary>
	[ValueConversion(typeof(IList), typeof(IList))]
	public class IListToIListConverter : BaseConverter
	{
		/// <summary>
		/// Ignores the selected zero-based index. Usage: ignore(*), where * is replaced with the index.
		/// </summary>
		public const string PARAM_IGNORE = "ignore";

		/// <summary>
		/// Moves the start by the specified number of elements to the right. Can be negative. Usage: rotate(*), where * is replaced with the number of elements to move.
		/// </summary>
		public const string PARAM_ROTATE = "rotate";

		/// <summary>
		/// Converts the provided value with the specified parameter to <see cref="IList"/>.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="options">The parameter, usually a string. For supported options, check the class constants starting with PARAM_.</param>
		public static IList Convert(object value, ref string options)
		{
			var ilistValue = value as IList;
			if(ilistValue == null) {
				return null;
			}
			if(ilistValue.Count == 0) {
				return ilistValue;
			}

			if(options == null) {
				return ilistValue;
			}
			options = options.ToLower();

			ilistValue = Ignore(ilistValue, ref options);
			ilistValue = Rotate(ilistValue, ref options);

			return ilistValue;
		}

		private readonly static Regex regex_ignore = new Regex($@"{PARAM_IGNORE}\((\d+)\)", RegexOptions.Compiled);
		private static IList Ignore(IList list, ref string options)
		{
			MatchCollection matches = regex_ignore.Matches(options);
			if(matches.Count == 0) {
				return list;
			}

			options = StringUtility.RemoveAllOf(options, $"{PARAM_IGNORE}(");

			List<int> indexesToIgnore = new List<int>(matches.Count);
			foreach(Match match in matches) {
				string ignoreParameter = match.Groups[1].Value;
				int ignoreIndex = int.Parse(ignoreParameter);
				indexesToIgnore.Add(ignoreIndex);
			}

			indexesToIgnore.Sort();

			if(list.IsFixedSize) {
				list = new ArrayList(list);
			}

			for(int i = indexesToIgnore.Count - 1; i >= 0; i--) {
				list.RemoveAt(i);
			}

			return list;
		}

		private readonly static Regex regex_rotate = new Regex($@"{PARAM_ROTATE}\((\d+)\)", RegexOptions.Compiled);
		private static IList Rotate(IList list, ref string options)
		{
			MatchCollection matches = regex_rotate.Matches(options);
			if(matches.Count == 0) {
				return list;
			}
			if(matches.Count > 1) {
				throw new ArgumentException($"The provided parameter '{options}' for the converter is invalid: only one '{PARAM_ROTATE}' criteria is allowed.");
			}

			options = StringUtility.RemoveFirstOf(options, $"{PARAM_ROTATE}(");

			string rotateParameter = matches[0].Groups[1].Value;
			int rotateValue = int.Parse(rotateParameter);
			if(rotateValue < 0) {
				rotateValue = (rotateValue % list.Count) + list.Count;
			}
			if(rotateValue >= list.Count) {
				rotateValue %= list.Count;
			}
			if(rotateValue == 0) {
				return list;
			}

			IList sortedList = new ArrayList(list.Count);

			for(int i = rotateValue; i < list.Count; ++i) {
				sortedList.Add(list[i]);
			}
			for(int i = 0; i < rotateValue; ++i) {
				sortedList.Add(list[i]);
			}

			return sortedList;
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
			// due to possible loss of elements (parameter ignore), converting back is disabled
			return DependencyProperty.UnsetValue;
		}
	}
}
