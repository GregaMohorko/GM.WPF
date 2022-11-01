/*
MIT License

Copyright (c) 2022 Gregor Mohorko

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
Created: 2022-11-1
Author: Gregor Mohorko
*/

using System.Windows.Controls;
using System.Windows.Markup;

namespace GM.WPF.Controls
{
	/// <summary>
	/// A tab control that has tab items on the left side.
	/// </summary>
	public class TabControlLeft : TabControl
	{
		/// <summary>
		/// Creates a new instance of <see cref="TabControlLeft"/>.
		/// </summary>
		public TabControlLeft()
		{
			TabStripPlacement = Dock.Left;

			string templateXamlString = TabControlLeftResource.TabControlLeftTemplate;
			Template = (ControlTemplate)XamlReader.Parse(templateXamlString);
		}
	}
}
