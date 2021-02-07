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
Created: 2020-02-19
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
				RegisterNullificationOfDataContext(tabControl, tabItem);
				NullifyDataContext(tabItem);
			} else {
				UnregisterNullificationOfDataContext(tabControl, tabItem);
				TrySetDataContextBack(tabItem);
			}
		}

		// no need to have concurrent dictionaries since everything is run on the main UI thread
		private static Dictionary<TabControl, List<TabItem>> registeredTabControlsAndItems;
		private static Dictionary<TabItem, (BindingBase Binding, object Value)> nullifiedDataContexts;

		private static void RegisterNullificationOfDataContext(TabControl tabControl, TabItem tabItem)
		{
			if(registeredTabControlsAndItems == null) {
				// init
				registeredTabControlsAndItems = new Dictionary<TabControl, List<TabItem>>();
				nullifiedDataContexts = new Dictionary<TabItem, (BindingBase Binding, object Value)>();
			}

			List<TabItem> registeredItemsOfThisTabControl;
			{
				// check if this TabItem is already registered
				TabControl alreadyRegisteredTabControl = registeredTabControlsAndItems
					.SingleOrDefault(kvp => kvp.Value.Contains(tabItem))
					.Key;
				if(alreadyRegisteredTabControl != null) {
					// already registered
					if(alreadyRegisteredTabControl == tabControl) {
						// already registered to the same TabControl
						// nothing to do here
						return;
					} else {
						// already registered to a different TabControl
						// maybe this TabItem was moved to another TabControl?
						// remove the old TabControl
						UnregisterNullificationOfDataContext(alreadyRegisteredTabControl, tabItem);
					}
				}

				if(!registeredTabControlsAndItems.TryGetValue(tabControl, out registeredItemsOfThisTabControl)) {
					// register this TabControl
					registeredItemsOfThisTabControl = new List<TabItem>();
					registeredTabControlsAndItems.Add(tabControl, registeredItemsOfThisTabControl);

					tabControl.SelectionChanged += RegisteredTabControl_SelectionChanged;
				}
			}

			// register this TabItem
			registeredItemsOfThisTabControl.Add(tabItem);
		}

		private static void UnregisterNullificationOfDataContext(TabControl tabControl, TabItem tabItem)
		{
			if(registeredTabControlsAndItems == null) {
				// nothing was registered yet
				return;
			}

			if(!registeredTabControlsAndItems.TryGetValue(tabControl, out List<TabItem> registeredItemsOfThisTabControl)) {
				// nothing to do here
				return;
			}

			// unregister this TabItem
			_ = registeredItemsOfThisTabControl.Remove(tabItem);

			if(registeredItemsOfThisTabControl.Count == 0) {
				// unregister this TabControl
				tabControl.SelectionChanged -= RegisteredTabControl_SelectionChanged;
				_ = registeredTabControlsAndItems.Remove(tabControl);
			}
		}

		private static void RegisteredTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!(e.Source is TabControl tabControl)) {
				return;
			}

			List<TabItem> registeredTabItemsOfThisTabControl = registeredTabControlsAndItems[tabControl];

			// add datacontext to newly selected tab items
			foreach(TabItem selectedTabItem in e.AddedItems.OfType<TabItem>()) {
				TrySetDataContextBack(selectedTabItem);
			}

			// remove datacontext from newly deselected tab items
			foreach(TabItem unselectedTabItem in e.RemovedItems.OfType<TabItem>()) {
				if(registeredTabItemsOfThisTabControl.Contains(unselectedTabItem)) {
					NullifyDataContext(unselectedTabItem);
				}
			}
		}

		private static void TrySetDataContextBack(TabItem tabItem)
		{
			if(nullifiedDataContexts.TryGetValue(tabItem, out (BindingBase Binding, object Value) nullification)) {
				// set DataContext back

				// check that it wasn't set to anything else while being nullified by this behavior
				// if it was, don't overwrite it
				if(tabItem.DataContext == null) {
					// it wasn't set to anything else

					if(nullification.Binding != null) {
						// set the binding back
						_ = tabItem.SetBinding(FrameworkElement.DataContextProperty, nullification.Binding);
					} else {
						// apparently there was no binding
						// set the datacontext value back
						tabItem.DataContext = nullification.Value;
					}
				}

				_ = nullifiedDataContexts.Remove(tabItem);
			}
		}

		private static void NullifyDataContext(TabItem tabItem)
		{
			BindingBase binding;
			object value = default;

			// try to find the binding first
			{
				FrameworkElement dataContextSource = tabItem;
				bool wasPreviousSourceContentPresenter = false;
				while(true) {
					binding = BindingOperations.GetBindingBase(dataContextSource, FrameworkElement.DataContextProperty);
					if(binding != null) {
						// binding found
						break;
					}
					if(wasPreviousSourceContentPresenter && dataContextSource is ContentControl) {
						binding = BindingOperations.GetBindingBase(dataContextSource, ContentControl.ContentProperty);
						if(binding != null) {
							// binding found
							break;
						}
					}
					wasPreviousSourceContentPresenter = dataContextSource is ContentPresenter;
					dataContextSource = dataContextSource.GetParent() as FrameworkElement;
					if(dataContextSource == null) {
						// there is no binding on DataContext
						break;
					}
				}
				if(binding != null && dataContextSource != tabItem) {
					// we will be setting the binding to the TabItem, so just create a dummy binding that will use the same value as the parent
					binding = new Binding
					{
						Mode = BindingMode.OneWay
					};
				}
			}
			if(binding == null) {
				// use the value
				value = tabItem.DataContext;
			}

			nullifiedDataContexts.Add(tabItem, (binding, value));
			tabItem.DataContext = null;
		}
		#endregion NullifyDataContextWhenInactive
	}
}
