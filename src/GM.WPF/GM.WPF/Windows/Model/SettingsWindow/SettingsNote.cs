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
Created: 2019-10-08
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GM.WPF.Windows.Model.SettingsWindow
{
	/// <summary>
	/// A simple text note with an optional title and a text.
	/// </summary>
	public class SettingsNote : ObservableObject, ISettingsUI
	{
		/// <summary>
		/// The name of this setting. Will be displayed as a title.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The text to display.
		/// </summary>
		public string Text { get; private set; }
		/// <summary>
		/// Always false.
		/// </summary>
		public bool IsDirty => false;
		/// <summary>
		/// Always true.
		/// </summary>
		public bool IsReadOnly => true;

		internal SettingsNote(string title, string text)
		{
			Name = title;
			Text = text;
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Apply()
		{
			// do nothing ...
		}
	}
}
