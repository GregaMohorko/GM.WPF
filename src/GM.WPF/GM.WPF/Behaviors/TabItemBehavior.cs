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
Created: 2021-02-07
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GM.WPF.Utility;

namespace GM.WPF.Behaviors
{
	/// <summary>
	/// A behavior for <see cref="TabItem"/>.
	/// </summary>
	public static class TabItemBehavior
	{
		#region NullifyDataContextWhenInactive
		/// <summary>
		/// If set to true, it sets the <see cref="FrameworkElement.DataContext"/> of this <see cref="TabItem"/> to null when it is not active (not selected in the parent <see cref="TabControl"/>).
		/// </summary>
		public static readonly DependencyProperty NullifyDataContextWhenInactiveProperty = DependencyProperty.RegisterAttached("NullifyDataContextWhenInactive", typeof(bool), typeof(TabItemBehavior), new FrameworkPropertyMetadata(false, OnNullifyDataContextWhenInactiveChanged));

		/// <summary>
		/// Gets the current effective value of <see cref="NullifyDataContextWhenInactiveProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		public static bool GetNullifyDataContextWhenInactive(DependencyObject target)
		{
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
			return (bool)target.GetValue(NullifyDataContextWhenInactiveProperty);
		}

		/// <summary>
		/// Sets the local value of <see cref="NullifyDataContextWhenInactiveProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetNullifyDataContextWhenInactive(DependencyObject target, bool value)
		{
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
			target.SetValue(NullifyDataContextWhenInactiveProperty, value);
		}

		private static void OnNullifyDataContextWhenInactiveChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			var tabItem = (TabItem)target;

			if(!(tabItem.GetParent() is TabControl tabControl)) {
				// What is going on? Where is the TabControl? The TabItem should be in the TabControl?
				return;
			}

			if((bool)e.NewValue) {
				FrameworkElementBehavior.RegisterNullificationAndNullifyDataContext(tabControl, tabItem, tabItem);
			} else {
				FrameworkElementBehavior.UnregisterNullificationAndRestoreDataContext(tabControl, tabItem, tabItem);
			}
		}
		#endregion NullifyDataContextWhenInactive
	}
}
