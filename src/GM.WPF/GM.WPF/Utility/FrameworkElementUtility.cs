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
Created: 2017-10-30
Author: Gregor Mohorko
*/

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GM.Utility;

namespace GM.WPF.Utility
{
	/// <summary>
	/// Utilities for <see cref="FrameworkElement"/>.
	/// </summary>
	public static class FrameworkElementUtility
	{
		/// <summary>
		/// Tries to find the first focusable child (depth-first) and set focus to it.
		/// </summary>
		/// <param name="element">The element whose first focusable child to focus.</param>
		public static bool FocusFirstFocusableChild(this FrameworkElement element)
		{
			bool predicate(Visual v) => (v is UIElement uie) && uie.Focusable;
			var firstFocusable = element.GetVisualChildCollection(predicate, VisualUtility.TreeTraverseStrategy.DepthFirst).FirstOrDefault() as UIElement;
			return firstFocusable?.Focus() ?? false;
		}

		/// <summary>
		/// Returns the <see cref="FrameworkElement.Parent"/> property of this element. If it is null, it returns <see cref="FrameworkElement.TemplatedParent"/>.
		/// <para>In cases where you don't know exactly the elements tree, this method should be preferred over simply using the <see cref="FrameworkElement.Parent"/>, because if this element is in a <see cref="FrameworkTemplate"/>, the <see cref="FrameworkElement.Parent"/> returns null, whereas the <see cref="FrameworkElement.TemplatedParent"/> returns the data template and you can then continue to move upward the elements tree.</para>
		/// </summary>
		/// <param name="element">The element whose parent to return.</param>
		public static DependencyObject GetParent(this FrameworkElement element)
		{
			if(element.Parent != null) {
				return element.Parent;
			}
			if(element.TemplatedParent != null) {
				return element.TemplatedParent;
			}
			return ((Visual)element).GetPropertyValue("VisualParent", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty) as DependencyObject;
		}

		/// <summary>
		/// Gets the key by which to identify this control within the visual tree.
		/// <para>Uses the control tree path to determine the key.</para>
		/// <para>Useful when you want to save settings for a control within the visual tree that is instantiated dynamically using templates.</para>
		/// </summary>
		public static string GetUniqueControlKeyByVisualTree(this FrameworkElement element)
		{
			var keyBuilder = new StringBuilder();

			FrameworkElement current = element;
			while(true) {
				if(!(GetParent(current) is FrameworkElement parent)) {
					break;
				}
				int? index = null;
				if(parent is Panel parentPanel) {
					index = parentPanel.Children.IndexOf(current);
				} else if(parent is ItemsControl parentItemsControl) {
					index = parentItemsControl.Items.IndexOf(current);
				}
				if(index != null) {
					_ = keyBuilder.Insert(0, $"[{index}]");
				}
				_ = keyBuilder.Insert(0, $".{parent.GetType().Name}");

				current = parent;
			}
			if(keyBuilder.Length > 1) {
				keyBuilder.Remove(0, 1);
			}
			return keyBuilder.ToString();
		}

		/// <summary>
		/// Moves the keyboard focus to another focusable element downwards from the currently focused element.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static bool MoveFocusDown(this FrameworkElement element)
		{
			return element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
		}

		/// <summary>
		/// Moves the keyboard focus to the first focusable element in tab order.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static bool MoveFocusFirst(this FrameworkElement element)
		{
			return element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}

		/// <summary>
		/// Moves the keyboard focus to the last focusable element in tab order.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static bool MoveFocusLast(this FrameworkElement element)
		{
			return element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
		}

		/// <summary>
		/// Moves the keyboard focus to another focusable element to the left of currently focused element.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static bool MoveFocusLeft(this FrameworkElement element)
		{
			return element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
		}

		/// <summary>
		/// Moves the keyboard focus to the next focusable element in tab order.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static bool MoveFocusNext(this FrameworkElement element)
		{
			return element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}

		/// <summary>
		/// Moves the keyboard focus to the previous focusable element in tab order.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static bool MoveFocusPrevious(this FrameworkElement element)
		{
			return element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
		}

		/// <summary>
		/// Moves the keyboard focus to another focusable element to the right of currently focused element.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static bool MoveFocusRight(this FrameworkElement element)
		{
			return element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
		}

		/// <summary>
		/// Moves the keyboard focus to another focusable element upwards from the currently focused element.
		/// </summary>
		/// <param name="element">The element from which to move focus.</param>
		public static bool MoveFocusUp(this FrameworkElement element)
		{
			return element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
		}

		/// <summary>
		/// Removes this element from the parent element.
		/// </summary>
		/// <param name="frameworkElement">The element to remove from parent.</param>
		public static void RemoveFromParent(this FrameworkElement frameworkElement)
		{
			DependencyObject parent = frameworkElement.GetParent();
			if(parent is ContentControl parentContentControl) {
				parentContentControl.Content = null;
			} else if(parent is TabControl parentTabControl) {
				parentTabControl.Items.Remove(frameworkElement);
			} else if(parent is ContextMenu parentContextMenu) {
				parentContextMenu.Items.Remove(frameworkElement);
			} else if(parent is Decorator parentDecorator) {
				parentDecorator.Child = null;
			} else if(parent is Panel parentPanel) {
				try {
					parentPanel.Children.Remove(frameworkElement);
				} catch(InvalidOperationException e) when(e.Message == "Cannot explicitly modify Children collection of Panel used as ItemsPanel for ItemsControl. ItemsControl generates child elements for Panel.") {
					// this panel is simply an ItemsPanel of ItemsControl
					if(frameworkElement is ContentPresenter contentPresenter) {
						// it needs to be done AFTER the current rendering (the content is shown for a very short duration (10ms?) and then removed)
						// if in the future you find a cleaner way to do it ... please do so
						_ = contentPresenter.Dispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(() =>
						{
							contentPresenter.ContentTemplate = null;
							contentPresenter.Content = null;
						}));
					} else {
						throw new NotImplementedException();
					}
				}
			} else if(parent is ContentPresenter parentContentPresenter) {
				if(parentContentPresenter.Content == frameworkElement) {
					parentContentPresenter.Content = null;
				} else {
					RemoveFromParent(parentContentPresenter);
				}
			} else {
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Returns the first parent of type <typeparamref name="TControl"/> of this element, or null if this control is not inside a <typeparamref name="TControl"/>.
		/// </summary>
		/// <typeparam name="TControl">The type of parent to search for.</typeparam>
		/// <param name="element">The element of which to search for parent <typeparamref name="TControl"/>.</param>
		public static TControl TryGetParent<TControl>(this FrameworkElement element) where TControl : FrameworkElement
		{
			Type tControlType = typeof(TControl);
			var current = element.GetParent() as FrameworkElement;
			while(current != null) {
				if(tControlType.IsAssignableFrom(current.GetType())) {
					return (TControl)current;
				}
				current = current.GetParent() as FrameworkElement;
			}
			return null;
		}
	}
}
