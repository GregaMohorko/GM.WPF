/*
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GM.WPF.Utility;

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// The base class for dialogs.
	/// </summary>
	public class Dialog:BaseControl
	{
		/// <summary>
		/// Default dialog background.
		/// </summary>
		public static Brush DefaultBackground = new SolidColorBrush(Color.FromRgb(26, 117, 207));
		/// <summary>
		/// Default dialog foreground.
		/// </summary>
		public static Brush DefaultForeground = Brushes.White;
		/// <summary>
		/// Default dialog border brush.
		/// </summary>
		public static Brush DefaultBorderBrush = Brushes.White;
		/// <summary>
		/// Default dialog border thickness.
		/// </summary>
		public static Thickness DefaultBorderThickness = new Thickness(1);

		/// <summary>
		/// Initializes a new instance of <see cref="Dialog"/>.
		/// </summary>
		public Dialog()
		{
			if(IsInDesignMode) {
				return;
			}

			Hide();
		}

		/// <summary>
		/// Shows this dialog and attempts to focus it.
		/// </summary>
		public void Show()
		{
			Visibility = Visibility.Visible;
			Focus();
		}

		/// <summary>
		/// Hides (collapses) this dialog.
		/// </summary>
		public void Hide()
		{
			Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// Sets a new background of this dialog.
		/// </summary>
		/// <param name="background">A new background to set.</param>
		public void SetBackground(Brush background)
		{
			var contentWrapper = Content as DialogContentWrapper;
			contentWrapper._Border.Background = background;
		}

		/// <summary>
		/// Injects the <see cref="DialogContentWrapper"/> and passes on the brushes. Overrides <see cref="FrameworkElement.OnInitialized(EventArgs)"/>.
		/// </summary>
		/// <param name="e">The RoutedEventArgs that contains the event data.</param>
		protected override void OnInitialized(EventArgs e)
		{
			var contentWrapper = new DialogContentWrapper();
			contentWrapper._ContentPresenter.Content = Content;
			
			// if any of the default values were manually set, pass them on
			if(!this.IsSet(ForegroundProperty)) {
				Foreground = DefaultForeground;
			}
			if(this.IsSet(BackgroundProperty)) {
				contentWrapper._Border.Background = Background;
				Background = null;
			}
			if(this.IsSet(BorderBrushProperty)) {
				contentWrapper._Border.BorderBrush = BorderBrush;
				BorderBrush = null;
			}
			if(this.IsSet(BorderThicknessProperty)) {
				contentWrapper._Border.BorderThickness = BorderThickness;
				BorderThickness = new Thickness();
			}
			Brush foreground = this.IsSet(ForegroundProperty) ? Foreground : DefaultForeground;
			IEnumerable<Visual> allLabelsAndTextBoxes = ((DependencyObject)Content).GetVisualChildCollection<Label,TextBlock>();
			foreach(Visual labelOrTextBlock in allLabelsAndTextBoxes) {
				if(labelOrTextBlock is Label label) {
					if(!label.IsSet(ForegroundProperty)) {
						label.Foreground = foreground;
					}
				} else if(labelOrTextBlock is TextBlock textBlock) {
					if(!textBlock.IsSet(TextBlock.ForegroundProperty)) {
						textBlock.Foreground = foreground;
					}
				}
			}

			Content = contentWrapper;

			base.OnInitialized(e);
		}
	}
}
