/*
MIT License

Copyright (c) 2019 Grega Mohorko

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
Created: 2019-09-17
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GM.WPF.Utility
{
	/// <summary>
	/// Utilities for <see cref="Brush"/>.
	/// </summary>
	public static class BrushUtility
	{
		/// <summary>
		/// Converts this <see cref="System.Drawing.Color"/> into a <see cref="SolidColorBrush"/>.
		/// </summary>
		/// <param name="drawingColor">The <see cref="System.Drawing.Color"/> to convert to <see cref="SolidColorBrush"/>.</param>
		public static SolidColorBrush ToBrush(this System.Drawing.Color drawingColor)
		{
			var color = new Color
			{
				A = drawingColor.A,
				R = drawingColor.R,
				G = drawingColor.G,
				B = drawingColor.B
			};
			return new SolidColorBrush(color);
		}

		/// <summary>
		/// Converts this <see cref="Brush"/> into a <see cref="System.Drawing.Color"/>.
		/// <para>Currently, only implemented for <see cref="SolidColorBrush"/>.</para>
		/// </summary>
		/// <param name="brush">The <see cref="Brush"/> to convert to <see cref="System.Drawing.Color"/>.</param>
		public static System.Drawing.Color ToDrawingColor(this Brush brush)
		{
			if(brush is SolidColorBrush solidBrush) {
				Color color = solidBrush.Color;
				return System.Drawing.Color.FromArgb(color.R, color.G, color.B);
			} else {
				throw new NotImplementedException($"Only implemented for {nameof(SolidColorBrush)}, sorry.");
			}
		}
	}
}
