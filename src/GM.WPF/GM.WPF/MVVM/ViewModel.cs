﻿/*
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
Created: 2017-10-29
Author: Gregor Mohorko
*/

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GM.WPF.MVVM
{
	/// <summary>
	/// Base class for the ViewModel in the MVVM pattern.
	/// <para>For automatic injection of <see cref="INotifyPropertyChanged"/> code into properties at compile time, use PropertyChanged.Fody project from NuGet.</para>
	/// <para>For commands, use <see cref="RelayCommand"/> or <see cref="AsyncRelayCommand"/>.</para>
	/// </summary>
	public abstract class ViewModel : ObservableRecipient
	{
		/// <summary>
		/// Gets a value that indicates whether a debugger is attached to the process.
		/// </summary>
		public bool IsDebuggerAttached => Debugger.IsAttached;
		/// <summary>
		/// Determines whether the control is in design mode.
		/// </summary>
		public bool IsInDesignMode => IsInDesignModeStatic;
		/// <summary>
		/// Determines whether the control is in design mode.
		/// </summary>
		public static bool IsInDesignModeStatic { get; }

		static ViewModel()
		{
			IsInDesignModeStatic = DesignerProperties.GetIsInDesignMode(new DependencyObject());
		}
	}
}
