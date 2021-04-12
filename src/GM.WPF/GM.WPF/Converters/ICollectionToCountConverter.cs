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
Created: 2021-03-15
Author: Gregor Mohorko
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GM.WPF.Converters
{
	/// <summary>
	/// Converter from <see cref="ICollection"/> to the count of elements in this collection.
	/// </summary>
	[ValueConversion(typeof(ICollection), typeof(int))]
	public class ICollectionToCountConverter : BaseConverter
	{
		/// <summary>
		/// Converts the provided value to the count of collection.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static int? Convert(object value)
		{
			if(!(value is ICollection collection)) {
				return null;
			}
			return collection.Count;
		}

		/// <summary>
		/// <inheritdoc />
		/// </summary>
		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(value);
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
