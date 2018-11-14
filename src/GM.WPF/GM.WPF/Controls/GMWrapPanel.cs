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
using System.Windows.Media;
using GM.WPF.Behaviors;

namespace GM.WPF.Controls
{
	/// <summary>
	/// A <see cref="WrapPanel"/> with:
	/// <para>- ability to set space between elements (use property <see cref="Spacing"/>)</para>
	/// </summary>
	public class GMWrapPanel : WrapPanel
	{
		/// <summary>
		/// The property for <see cref="Spacing"/>.
		/// </summary>
		public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(nameof(Spacing), typeof(Thickness), typeof(GMWrapPanel), new UIPropertyMetadata(OnSpacingChanged));

		private static void OnSpacingChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			var panel = (GMStackPanel)target;
			PanelBehavior.ApplySpacing(panel, panel.Spacing);
		}

		/// <summary>
		/// The inner margin between elements.
		/// </summary>
		public Thickness Spacing
		{
			get => (Thickness)GetValue(SpacingProperty);
			set => SetValue(SpacingProperty, value);
		}

		/// <summary>
		/// Invoked when the System.Windows.Media.VisualCollection of a visual object is modified.
		/// </summary>
		/// <param name="visualAdded">The <see cref="Visual"/> that was added to the collection.</param>
		/// <param name="visualRemoved">The <see cref="Visual"/> that was removed from the collection.</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);

			PanelBehavior.ApplySpacing(this, Spacing);
		}
	}
}
