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
	/// A behavior for <see cref="FrameworkElement"/>.
	/// </summary>
	public static class FrameworkElementBehavior
	{
		#region NullifyDataContextWhenParentTabItemInactive
		/// <summary>
		/// If set to true, it sets the <see cref="FrameworkElement.DataContext"/> to null when the first parent <see cref="TabItem"/> is not active (not selected in the parent <see cref="TabControl"/>).
		/// </summary>
		public static readonly DependencyProperty NullifyDataContextWhenParentTabItemInactiveProperty = DependencyProperty.RegisterAttached("NullifyDataContextWhenParentTabItemInactive", typeof(bool), typeof(FrameworkElementBehavior), new FrameworkPropertyMetadata(false, OnNullifyDataContextWhenParentTabItemInactiveChanged));

		/// <summary>
		/// Gets the current effective value of <see cref="NullifyDataContextWhenParentTabItemInactiveProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		public static bool GetNullifyDataContextWhenParentTabItemInactive(DependencyObject target)
		{
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
			return (bool)target.GetValue(NullifyDataContextWhenParentTabItemInactiveProperty);
		}

		/// <summary>
		/// Sets the local value of <see cref="NullifyDataContextWhenParentTabItemInactiveProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetNullifyDataContextWhenParentTabItemInactive(DependencyObject target, bool value)
		{
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
			target.SetValue(NullifyDataContextWhenParentTabItemInactiveProperty, value);
		}

		private static void OnNullifyDataContextWhenParentTabItemInactiveChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			var frameworkElement = (FrameworkElement)target;

			TabItem tabItem = frameworkElement.TryGetParent<TabItem>();
			if(tabItem == null) {
				// this element is not inside a TabItem
				// nothing to do
				return;
			}

			TabControl tabControl = tabItem.TryGetParent<TabControl>();
			if(tabControl == null) {
				// this TabItem is not inside a TabControl
				// nothing to do
				return;
			}

			if((bool)e.NewValue) {
				RegisterNullificationAndNullifyDataContext(tabControl, tabItem, frameworkElement);
			} else {
				UnregisterNullificationAndRestoreDataContext(tabControl, tabItem, frameworkElement);
			}
		}

		// no need to have concurrent dictionaries since everything is run on the main UI thread
		private static Dictionary<TabControl, Dictionary<TabItem, List<FrameworkElement>>> registeredTabControlsAndItems;
		private static Dictionary<FrameworkElement, (BindingBase Binding, object Value)> nullifiedDataContexts;

		internal static void RegisterNullificationAndNullifyDataContext(TabControl tabControl, TabItem tabItem, FrameworkElement frameworkElement)
		{
			if(registeredTabControlsAndItems == null) {
				registeredTabControlsAndItems = new Dictionary<TabControl, Dictionary<TabItem, List<FrameworkElement>>>();
				nullifiedDataContexts = new Dictionary<FrameworkElement, (BindingBase Binding, object Value)>();
			}

			List<FrameworkElement> registeredElementsOfThisTabItem;
			{
				Dictionary<TabItem, List<FrameworkElement>> registeredTabItemsOfThisTabControl;
				{
					// check if this TabItem is already registered
					TabControl alreadyRegisteredTabControl = registeredTabControlsAndItems
						.SingleOrDefault(kvp => kvp.Value.Any(kvp2 => kvp2.Value.Contains(frameworkElement)))
						.Key;
					if(alreadyRegisteredTabControl != null) {
						// already registered
						if(alreadyRegisteredTabControl == tabControl) {
							// already registered to the same TabControl
							// nothing to do here
							return;
						} else {
							// already registered to a different TabControl
							// maybe this element/TabItem was moved to another TabItem/TabControl?
							// remove the old TabControl
							UnregisterNullificationAndRestoreDataContext(alreadyRegisteredTabControl, tabItem, frameworkElement);
						}
					}

					if(!registeredTabControlsAndItems.TryGetValue(tabControl, out registeredTabItemsOfThisTabControl)) {
						// register this TabControl
						registeredTabItemsOfThisTabControl = new Dictionary<TabItem, List<FrameworkElement>>();
						registeredTabControlsAndItems.Add(tabControl, registeredTabItemsOfThisTabControl);

						tabControl.SelectionChanged += RegisteredTabControl_SelectionChanged;
					}
				}

				if(!registeredTabItemsOfThisTabControl.TryGetValue(tabItem, out registeredElementsOfThisTabItem)) {
					// register this TabItem
					registeredElementsOfThisTabItem = new List<FrameworkElement>();
					registeredTabItemsOfThisTabControl.Add(tabItem, registeredElementsOfThisTabItem);
				}
			}

			// register this element
			registeredElementsOfThisTabItem.Add(frameworkElement);

			// nullify
			if(!tabItem.IsSelected) {
				NullifyDataContext(frameworkElement, tabItem);
			}
		}

		internal static void UnregisterNullificationAndRestoreDataContext(TabControl tabControl, TabItem tabItem, FrameworkElement frameworkElement)
		{
			if(registeredTabControlsAndItems == null) {
				// nothing was registered yet
				return;
			}

			if(!registeredTabControlsAndItems.TryGetValue(tabControl, out Dictionary<TabItem, List<FrameworkElement>> registeredTabItemsOfThisTabControl)) {
				// nothing to do here
				return;
			}

			if(!registeredTabItemsOfThisTabControl.TryGetValue(tabItem, out List<FrameworkElement> registeredElementsOfThisTabItem)) {
				// nothing to do here
				return;
			}

			// unregister this element
			_ = registeredElementsOfThisTabItem.Remove(frameworkElement);

			if(registeredElementsOfThisTabItem.Count == 0) {
				// unregister this TabItem
				_ = registeredTabItemsOfThisTabControl.Remove(tabItem);
			}

			if(registeredTabItemsOfThisTabControl.Count == 0) {
				// unregister this TabControl
				tabControl.SelectionChanged -= RegisteredTabControl_SelectionChanged;
				_ = registeredTabControlsAndItems.Remove(tabControl);
			}

			// restore DataContext
			TrySetDataContextBack(frameworkElement);
		}

		private static void RegisteredTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!(e.Source is TabControl tabControl)) {
				return;
			}

			if(!registeredTabControlsAndItems.TryGetValue(tabControl, out Dictionary<TabItem, List<FrameworkElement>> registeredTabItemsOfThisTabControl)) {
				// probably the sender was the registered TabControl, but the source is not
				// this happens when a TabControl is an immediate child of a TabItem
				// simply skip
				return;
			}

			// add DataContext to newly selected TabItems
			foreach(TabItem selectedTabItem in e.AddedItems.OfType<TabItem>()) {
				if(registeredTabItemsOfThisTabControl.TryGetValue(selectedTabItem, out List<FrameworkElement> registeredElementsOfThisTabItem)) {
					foreach(FrameworkElement element in registeredElementsOfThisTabItem) {
						TrySetDataContextBack(element);
					}
				}
			}

			// remove DataContext from newly deselected TabItems
			foreach(TabItem unselectedTabItem in e.RemovedItems.OfType<TabItem>()) {
				if(registeredTabItemsOfThisTabControl.TryGetValue(unselectedTabItem, out List<FrameworkElement> registeredElementsOfThisTabItem)) {
					foreach(FrameworkElement element in registeredElementsOfThisTabItem) {
						NullifyDataContext(element);
					}
				}
			}
		}

		private static void NullifyDataContext(FrameworkElement frameworkElement, TabItem tabItem = null)
		{
			BindingBase binding;
			object value = default;

			// try to find the binding first
			{
				FrameworkElement dataContextSource = frameworkElement;
				bool wasPreviousSourceContentPresenter = false;
				while(true) {
					binding = BindingOperations.GetBindingBase(dataContextSource, FrameworkElement.DataContextProperty);
					if(binding != null) {
						// binding found
						break;
					}
					if(wasPreviousSourceContentPresenter && dataContextSource is ContentControl) {
						// if the previous control was ContentPresenter and current is ContentControl ...
						// ... then it's probably inside a DataTemplate and the DataContext si taken from the Content property of the ContentControl

						binding = BindingOperations.GetBindingBase(dataContextSource, ContentControl.ContentProperty);
						if(binding != null) {
							// binding found
							break;
						}
					}
					FrameworkElement parent = dataContextSource.GetParent() as FrameworkElement;
					if(parent == null) {
						// there currently is no binding on DataContext
						if(!dataContextSource.IsLoaded && tabItem != null) {
							// this was probably called from a UserControl, from constructor, from InitializeComponent()
							// try again when it is initialized
							void InitializedHandler(object sender, RoutedEventArgs e)
							{
								dataContextSource.Loaded -= InitializedHandler;
								if(!tabItem.IsSelected) {
									NullifyDataContext(frameworkElement);
								}
							}
							dataContextSource.Loaded += InitializedHandler;
							return;
						}
						break;
					}
					wasPreviousSourceContentPresenter = dataContextSource is ContentPresenter;
					dataContextSource = parent;
				}
				if(binding != null && dataContextSource != frameworkElement) {
					// we will be setting the binding to the original element, so just create a dummy binding that will use the same value as the parent
					binding = new Binding
					{
						Mode = BindingMode.OneWay
					};
				}
			}
			if(binding == null) {
				// use the value
				value = frameworkElement.DataContext;
			}

			nullifiedDataContexts[frameworkElement] = (binding, value);
			frameworkElement.DataContext = null;
		}

		private static void TrySetDataContextBack(FrameworkElement frameworkElement)
		{
			if(nullifiedDataContexts.TryGetValue(frameworkElement, out (BindingBase Binding, object Value) nullification)) {
				// set DataContext back

				// check that it wasn't set to anything else while being nullified by this behavior
				// if it was, don't overwrite it
				if(frameworkElement.DataContext == null) {
					// it wasn't set to anything else

					if(nullification.Binding != null) {
						// set the binding back
						_ = frameworkElement.SetBinding(FrameworkElement.DataContextProperty, nullification.Binding);
					} else {
						// apparently there was no binding
						// set the datacontext value back
						frameworkElement.DataContext = nullification.Value;
					}
				}

				_ = nullifiedDataContexts.Remove(frameworkElement);
			}
		}
		#endregion NullifyDataContextWhenParentTabItemInactive
	}
}
