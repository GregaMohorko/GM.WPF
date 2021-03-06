﻿/*
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM.WPF.Windows.Model.SettingsWindow
{
	/// <summary>
	/// Represents a user interface control in the settings window.
	/// </summary>
	public interface ISettingsUI : INotifyPropertyChanged
	{
		/// <summary>
		/// The name of this setting control.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Determines whether this setting has changed.
		/// </summary>
		bool IsDirty { get; }

		/// <summary>
		/// Determines whether this setting is read-only.
		/// </summary>
		bool IsReadOnly { get; }

		/// <summary>
		/// Applies the current value.
		/// </summary>
		void Apply();
	}
}
