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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GM.WPF.Utility
{
	/// <summary>
	/// Utilities for <see cref="FrameworkElement"/>.
	/// </summary>
	public static class FrameworkElementUtility
	{
		/// <summary>
		/// Moves the keyboard focus to another focusable element downwards from the currently focused element.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static void MoveFocusDown(this FrameworkElement element)
		{
			element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
		}

		/// <summary>
		/// Moves the keyboard focus to the first focusable element in tab order.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static void MoveFocusFirst(this FrameworkElement element)
		{
			element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}

		/// <summary>
		/// Moves the keyboard focus to the last focusable element in tab order.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static void MoveFocusLast(this FrameworkElement element)
		{
			element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
		}

		/// <summary>
		/// Moves the keyboard focus to another focusable element to the left of currently focused element.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static void MoveFocusLeft(this FrameworkElement element)
		{
			element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
		}

		/// <summary>
		/// Moves the keyboard focus to the next focusable element in tab order.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static void MoveFocusNext(this FrameworkElement element)
		{
			element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}

		/// <summary>
		/// Moves the keyboard focus to the previous focusable element in tab order.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static void MoveFocusPrevious(this FrameworkElement element)
		{
			element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
		}

		/// <summary>
		/// Moves the keyboard focus to another focusable element to the right of currently focused element.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static void MoveFocusRight(this FrameworkElement element)
		{
			element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
		}

		/// <summary>
		/// Moves the keyboard focus to another focusable element upwards from the currently focused element.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static void MoveFocusUp(this FrameworkElement element)
		{
			element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
		}
	}
}