/*
MIT License

Copyright (c) 2018 Grega Mohorko

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
Created: 2018-11-14
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GM.WPF.Behaviors
{
	/// <summary>
	/// A behavior for all controls that derive from <see cref="Panel"/>.
	/// </summary>
	public static class PanelBehavior
	{
		#region Spacing
		// idea for implementation taken from: http://blogs.microsoft.co.il/eladkatz/2011/05/29/what-is-the-easiest-way-to-set-spacing-between-items-in-stackpanel/

		/// <summary>
		/// The inner margin between elements in this panel. Note that this will only set the margins when the panel is loaded. It will not do anything when you add/remove children.
		/// </summary>
		public static readonly DependencyProperty SpacingProperty;

		private static readonly bool spacingRegistered;
		static PanelBehavior()
		{
			// this prevents the "Property already registered" error in design
			if(!spacingRegistered) {
				spacingRegistered = true;
				if(SpacingProperty == null) {
					SpacingProperty = DependencyProperty.RegisterAttached("Spacing", typeof(Thickness), typeof(Panel), new UIPropertyMetadata(OnSpacingChanged));
				}
			}
		}

		/// <summary>
		/// Gets the current effective value of <see cref="SpacingProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		public static Thickness GetSpacing(DependencyObject target)
		{
			return (Thickness)target.GetValue(SpacingProperty);
		}

		/// <summary>
		/// Sets the local value of <see cref="SpacingProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetSpacing(DependencyObject target, Thickness value)
		{
			target.SetValue(SpacingProperty, value);
		}

		private static void OnSpacingChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			var panel = (Panel)target;
			panel.Loaded += Panel_Loaded;
		}

		private static void Panel_Loaded(object sender, RoutedEventArgs e)
		{
			if(sender == null) {
				return;
			}
			var panel = (Panel)sender;
			Thickness spacing = GetSpacing(panel);
			ApplySpacing(panel, spacing);
		}

		/// <summary>
		/// Apply the specified spacing to the provided panel.
		/// </summary>
		/// <param name="panel">The panel.</param>
		/// <param name="spacing">The spacing.</param>
		public static void ApplySpacing(Panel panel, Thickness spacing)
		{
			if(panel.Children.Count < 2) {
				// no need to do anything
				return;
			}

			double horizontalSpacing = Math.Max(spacing.Left, spacing.Right);
			double verticalSpacing = Math.Max(spacing.Top, spacing.Bottom);

			if(panel is StackPanel stackPanel) {
				ApplySpacingInStackPanel(stackPanel, horizontalSpacing, verticalSpacing);
			} else if(panel is WrapPanel wrapPanel) {
				ApplySpacingInWrapPanel(wrapPanel, horizontalSpacing, verticalSpacing);
			} else {
				throw new Exception($"Spacing is only supported for {nameof(StackPanel)} and {nameof(WrapPanel)}.");
			}
		}

		private static void ApplySpacingInStackPanel(StackPanel stackPanel, double horizontalSpacing, double verticalSpacing)
		{
			Thickness marginFirst;
			Thickness marginMiddle;
			Thickness marginLast;
			switch(stackPanel.Orientation) {
				case Orientation.Horizontal:
					double halfHorizSpacing = horizontalSpacing / 2;
					marginFirst = new Thickness(0, 0, halfHorizSpacing, 0);
					marginMiddle = new Thickness(halfHorizSpacing, 0, halfHorizSpacing, 0);
					marginLast = new Thickness(halfHorizSpacing, 0, 0, 0);
					break;
				case Orientation.Vertical:
					double halfVertSpacing = verticalSpacing / 2;
					marginFirst = new Thickness(0, 0, 0, halfVertSpacing);
					marginMiddle = new Thickness(0, halfVertSpacing, 0, halfVertSpacing);
					marginLast = new Thickness(0, halfVertSpacing, 0, 0);
					break;
				default:
					throw new NotImplementedException($"Unknown StackPanel orientation: {stackPanel.Orientation}.");
			}

			if(stackPanel.Children[0] is FrameworkElement firstChild) {
				firstChild.Margin = marginFirst;
			}
			if(stackPanel.Children[stackPanel.Children.Count - 1] is FrameworkElement lastChild) {
				lastChild.Margin = marginLast;
			}
			for(int i = stackPanel.Children.Count - 2; i > 0; --i) {
				if(stackPanel.Children[i] is FrameworkElement feChild) {
					feChild.Margin = marginMiddle;
				}
			}
		}

		private static void ApplySpacingInWrapPanel(WrapPanel wrapPanel, double horizontalSpacing, double verticalSpacing)
		{
			double halfHorizSpacing = horizontalSpacing / 2;
			double halfVertSpacing = verticalSpacing / 2;
			var thickness = new Thickness(halfHorizSpacing, halfVertSpacing, halfHorizSpacing, halfVertSpacing);
			foreach(FrameworkElement feChild in wrapPanel.Children) {
				feChild.Margin = thickness;
			}
		}
		#endregion Spacing
	}
}
