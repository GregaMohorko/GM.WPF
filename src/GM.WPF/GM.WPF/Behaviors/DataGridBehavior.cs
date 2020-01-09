/*
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
Created: 2017-10-30
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GM.WPF.Utility;

namespace GM.WPF.Behaviors
{
	/// <summary>
	/// A behavior for <see cref="DataGrid"/>.
	/// </summary>
	public static class DataGridBehavior
	{
		#region DisplayRowNumber
		// implementation from: http://stackoverflow.com/questions/4663771/wpf-4-datagrid-getting-the-row-number-into-the-rowheader/4663799#4663799

		/// <summary>
		/// Determines whether or not to display the row numbers.
		/// </summary>
		public static readonly DependencyProperty DisplayRowNumberProperty = DependencyProperty.RegisterAttached("DisplayRowNumber", typeof(bool), typeof(DataGridBehavior), new FrameworkPropertyMetadata(false, OnDisplayRowNumberChanged));

		/// <summary>
		/// Gets the current effective value of <see cref="DisplayRowNumberProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		public static bool GetDisplayRowNumber(DependencyObject target)
		{
			return (bool)target.GetValue(DisplayRowNumberProperty);
		}

		/// <summary>
		/// Sets the local value of <see cref="DisplayRowNumberProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetDisplayRowNumber(DependencyObject target, bool value)
		{
			target.SetValue(DisplayRowNumberProperty, value);
		}

		private static void OnDisplayRowNumberChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			var dataGrid = (DataGrid)target;

			void loadedRowHandler(object sender, DataGridRowEventArgs ea)
			{
				if(!GetDisplayRowNumber(dataGrid)) {
					dataGrid.LoadingRow -= loadedRowHandler;
					return;
				}
				ea.Row.Header = ea.Row.GetIndex() + 1;
			}

			void itemsChangedHandler(object sender, ItemsChangedEventArgs ea)
			{
				if(!GetDisplayRowNumber(dataGrid)) {
					dataGrid.ItemContainerGenerator.ItemsChanged -= itemsChangedHandler;
					return;
				}
				foreach(DataGridRow row in dataGrid.GetVisualChildCollection<DataGridRow>()) {
					row.Header = row.GetIndex() + 1;
				}
			}

			if(!(bool)e.NewValue) {
				dataGrid.LoadingRow -= loadedRowHandler;
				dataGrid.ItemContainerGenerator.ItemsChanged -= itemsChangedHandler;
			} else {
				dataGrid.LoadingRow += loadedRowHandler;
				dataGrid.ItemContainerGenerator.ItemsChanged += itemsChangedHandler;
			}
		}
		#endregion // DisplayRowNumber

		#region MouseWheelProperties
		// implementation from: https://stackoverflow.com/questions/1585462/bubbling-scroll-events-from-a-listview-to-its-parent
		/// <summary>
		/// Determines whether or not the vertical scroll is ignored.
		/// </summary>
		public static readonly DependencyProperty IgnoreScrollProperty = DependencyProperty.RegisterAttached("IgnoreScroll", typeof(bool), typeof(DataGridBehavior), new FrameworkPropertyMetadata(false, OnIgnoreScrollChanged));
		// implementation from: https://stackoverflow.com/questions/3727439/how-to-enable-horizontal-scrolling-with-mouse
		/// <summary>
		/// Determines whether or not it should scroll horizontally on mouse wheel event if a shift is being pressed.
		/// </summary>
		public static readonly DependencyProperty ShiftWheelScrollsHorizontallyProperty = DependencyProperty.RegisterAttached("ShiftWheelScrollsHorizontally", typeof(bool), typeof(DataGridBehavior), new FrameworkPropertyMetadata(false, OnShiftWheelScrollsHorizontallyChanged));

		/// <summary>
		/// Gets the current effective value of <see cref="IgnoreScrollProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		public static bool GetIgnoreScroll(DependencyObject target)
		{
			return (bool)target.GetValue(IgnoreScrollProperty);
		}

		/// <summary>
		/// Sets the local value of <see cref="IgnoreScrollProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetIgnoreScroll(DependencyObject target, bool value)
		{
			target.SetValue(IgnoreScrollProperty, value);
		}

		/// <summary>
		/// Gets the current effective value of <see cref="ShiftWheelScrollsHorizontallyProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		public static bool GetShiftWheelScrollsHorizontally(DependencyObject target)
		{
			return (bool)target.GetValue(ShiftWheelScrollsHorizontallyProperty);
		}

		/// <summary>
		/// Sets the local value of <see cref="ShiftWheelScrollsHorizontallyProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetShiftWheelScrollsHorizontally(DependencyObject target, bool value)
		{
			target.SetValue(ShiftWheelScrollsHorizontallyProperty, value);
		}

		private static void OnIgnoreScrollChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			OnMouseWheelPropertyChanged(target);
		}

		private static void OnShiftWheelScrollsHorizontallyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			OnMouseWheelPropertyChanged(target);
		}

		private static void OnMouseWheelPropertyChanged(DependencyObject target)
		{
			var dataGrid = (DataGrid)target;
			bool ignoreScroll = GetIgnoreScroll(target);
			bool shiftWheelScrollsHorizontally = GetShiftWheelScrollsHorizontally(target);

			dataGrid.PreviewMouseWheel -= PreviewMouseWheel;

			if(ignoreScroll || shiftWheelScrollsHorizontally) {
				dataGrid.PreviewMouseWheel += PreviewMouseWheel;
			}
		}

		private static void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if(!(sender is DataGrid dataGrid)) {
				return;
			}

			bool shiftWheelScrollsHorizontally = GetShiftWheelScrollsHorizontally(dataGrid);
			if(shiftWheelScrollsHorizontally) {
				if(Keyboard.Modifiers == ModifierKeys.Shift) {
					var childScrollViewer = VisualUtility.GetVisualChildCollection<ScrollViewer>(dataGrid).FirstOrDefault();
					// scroll by 6 lines
					for(int i = 6; i > 0; --i) {
						if(e.Delta < 0) {
							childScrollViewer?.LineLeft();
						} else {
							childScrollViewer?.LineRight();
						}
					}
					e.Handled = true;
					return;
				}
			}

			bool ignoreScroll = GetIgnoreScroll(dataGrid);
			if(ignoreScroll) {
				var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
				{
					RoutedEvent = UIElement.MouseWheelEvent
				};
				dataGrid.RaiseEvent(e2);
				e.Handled = true;
				return;
			}
		}
		#endregion // MouseWheelProperties
	}
}
