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
Created: 2021-02-17
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GM.WPF.Converters
{
	/// <summary>
	/// Converter from <see cref="Enum"/> to a collecion of all values in the enum and their descriptions from <see cref="DescriptionAttribute"/>.
	/// <para>You can set a static enum example value to the converter parameter and it will be used.</para>
	/// </summary>
	[ValueConversion(typeof(Enum), typeof(List<ValueDescription>))]
	public class EnumToCollectionConverter : BaseConverter
	{
		/// <summary>
		/// A class with an enum value and description.
		/// </summary>
		public class ValueDescription
		{
			/// <summary>
			/// Enum value.
			/// </summary>
			public Enum Value { get; set; }
			/// <summary>
			/// Description.
			/// </summary>
			public string Description { get; set; }
		}

		/// <summary>
		/// Converts the provided value to a collection of all enum values.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		public static List<ValueDescription> Convert(object value)
		{
			if(value?.GetType()?.IsEnum != true) {
				return null;
			}

			string Description(Enum @enum)
			{
				var descAttributes = @enum
					.GetType()
					.GetField(@enum.ToString())
					.GetCustomAttributes(typeof(DescriptionAttribute), false);
				if(!descAttributes.Any()) {
					return @enum.ToString();
				}
				return ((DescriptionAttribute)descAttributes.First()).Description;

			}

			return Enum.GetValues(value.GetType())
				.Cast<Enum>()
				.Select(e => new ValueDescription { Value = e, Description = Description(e) })
				.ToList();
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
			if(parameter?.GetType()?.IsEnum == true) {
				return Convert(parameter);
			} else {
				return Convert(value);
			}
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
			// we don't know what was the original value ...
			return DependencyProperty.UnsetValue;
		}
	}
}
