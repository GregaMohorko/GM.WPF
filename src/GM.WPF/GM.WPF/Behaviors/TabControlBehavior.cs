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
Created: 2021-4-2
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GM.WPF.Utility;

namespace GM.WPF.Behaviors
{
	/// <summary>
	/// A behavior for <see cref="TabControl"/>
	/// </summary>
	public static class TabControlBehavior
	{
		#region RememberSelectedTab
		/// <summary>
		/// If set to true, it will use the <see cref="FrameworkElementUtility.GetUniqueControlKeyByVisualTree(FrameworkElement)"/> to save the selected tab and set it again when the same control is instantiated again.
		/// <para>Useful when you have tab controls inside a template.</para>
		/// </summary>
		public static readonly DependencyProperty RememberSelectedTabProperty = DependencyProperty.RegisterAttached("RememberSelectedTab", typeof(bool), typeof(TabControlBehavior), new FrameworkPropertyMetadata(false, OnRememberSelectedTabChanged));

		/// <summary>
		/// Gets the current effective value of <see cref="RememberSelectedTabProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		public static bool GetRememberSelectedTab(DependencyObject target)
		{
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
			return (bool)target.GetValue(RememberSelectedTabProperty);
		}

		/// <summary>
		/// Sets the local value of <see cref="RememberSelectedTabProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetRememberSelectedTab(DependencyObject target, bool value)
		{
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
			target.SetValue(RememberSelectedTabProperty, value);
		}

		private static Dictionary<string, (TabControl RegisteredTabControl, int SelectedTabIndex)> savedSelectedTabs;

		private static void OnRememberSelectedTabChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			var tabControl = (TabControl)target;

			if(savedSelectedTabs == null) {
				savedSelectedTabs = new Dictionary<string, (TabControl RegisteredTabControl, int SelectedTabIndex)>();
			}

			if(tabControl.IsLoaded) {
				ProcessRememberSelectedTabChanged(tabControl);
			} else {
				// because this control is not yet loaded ...
				// ... we need to wait for it to be loaded and then register and set
				tabControl.Loaded += TabControl_Loaded;
			}
		}

		private static void TabControl_Loaded(object sender, RoutedEventArgs e)
		{
			if(!(e.Source is TabControl tabControl)) {
				return;
			}
			tabControl.Loaded -= TabControl_Loaded;
			ProcessRememberSelectedTabChanged(tabControl);
		}

		private static void ProcessRememberSelectedTabChanged(TabControl tabControl)
		{
			string key = tabControl.GetUniqueControlKeyByVisualTree();

			if(GetRememberSelectedTab(tabControl)) {
				// register
				if(!savedSelectedTabs.TryGetValue(key, out (TabControl RegisteredTabControl, int SelectedTabIndex) saved)) {
					saved = (tabControl, tabControl.SelectedIndex);
					savedSelectedTabs.Add(key, saved);
				} else {
					if(saved.RegisteredTabControl != null) {
						// remove previous tab control with the same key
						saved.RegisteredTabControl.SelectionChanged -= RememberSelectedTab_TabControl_SelectionChanged;
					}
					saved.RegisteredTabControl = tabControl;
					savedSelectedTabs[key] = saved;
				}
				tabControl.SelectionChanged += RememberSelectedTab_TabControl_SelectionChanged;
				// select saved tab
				if(tabControl.SelectedIndex != saved.SelectedTabIndex) {
					tabControl.SelectedIndex = saved.SelectedTabIndex;
				}
			} else {
				if(!savedSelectedTabs.TryGetValue(key, out (TabControl RegisteredTabControl, int SelectedTabIndex) saved)) {
					// this should never happen
					// but do nothing anyway
				} else {
					// unregister
					saved.RegisteredTabControl.SelectionChanged -= RememberSelectedTab_TabControl_SelectionChanged;
					saved.RegisteredTabControl = null;
					// leave the saved value
					savedSelectedTabs[key] = saved;
				}
			}
		}

		private static void RememberSelectedTab_TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!(e.Source is TabControl tabControl)) {
				return;
			}

			string key = tabControl.GetUniqueControlKeyByVisualTree();

			var saved = savedSelectedTabs[key];
			saved.SelectedTabIndex = tabControl.SelectedIndex;
			savedSelectedTabs[key] = saved;
		}
		#endregion
	}
}
