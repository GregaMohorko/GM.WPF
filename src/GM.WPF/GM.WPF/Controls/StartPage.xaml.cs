/*
MIT License

Copyright (c) 2019 Grega Mohorko

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
Created: 2019-09-05
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GM.WPF.Controls
{
	/// <summary>
	/// A starting page for your applications. Tries to mimic the visual effect of the Start page of Windows 8.
	/// </summary>
	public partial class StartPage : BaseControl
	{
		/// <summary>
		/// Represents the <see cref="UserName1"/> property.
		/// </summary>
		public static readonly DependencyProperty UserName1Property = DependencyVMProperty<StartPage, StartPageViewModel>(nameof(UserName1));
		/// <summary>
		/// Represents the <see cref="UserName2"/> property.
		/// </summary>
		public static readonly DependencyProperty UserName2Property = DependencyVMProperty<StartPage, StartPageViewModel>(nameof(UserName2));
		/// <summary>
		/// Represents the <see cref="Columns"/> property.
		/// </summary>
		public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(ICollection<StartPageColumn>), typeof(StartPage), new PropertyMetadata(ColumnsChanged));

		private static void ColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((StartPage)d).OnColumnsChanged();
		}

		/// <summary>
		/// The top user name (above).
		/// </summary>
		public string UserName1
		{
			get => (string)GetValue(UserName1Property);
			set => SetValue(UserName1Property, value);
		}

		/// <summary>
		/// The bottom user name (below).
		/// </summary>
		public string UserName2
		{
			get => (string)GetValue(UserName1Property);
			set => SetValue(UserName1Property, value);
		}

		/// <summary>
		/// A collection of columns that are displayed on this start page.
		/// </summary>
		public ICollection<StartPageColumn> Columns
		{
			get => (ICollection<StartPageColumn>)GetValue(ColumnsProperty);
			set => SetValue(ColumnsProperty, value);
		}

		/// <summary>
		/// Creates a new instance of <see cref="StartPage"/>.
		/// </summary>
		public StartPage()
		{
			InitializeComponent();

			var vm = new StartPageViewModel();
			ViewModel = vm;

			ClearTiles();
		}

		private void ClearTiles()
		{
			// remove all previous
			_Grid_Columns.Children.Clear();
			_Grid_Columns.ColumnDefinitions.Clear();
		}

		private void OnColumnsChanged()
		{
			// remove all previous
			ClearTiles();

			// add each column
			foreach(StartPageColumn column in Columns) {
				if(_Grid_Columns.Children.Count > 1) {
					// add the column in between
					_Grid_Columns.ColumnDefinitions.Add(new ColumnDefinition { Style = (Style)_Grid_Columns.Resources["CD_Inbetween"] });
				}
				// add the new column definition
				_Grid_Columns.ColumnDefinitions.Add(new ColumnDefinition { Style = (Style)_Grid_Columns.Resources["CD_Content"] });

				var columnStackPanel = new StackPanel();
				_Grid_Columns.Children.Add(columnStackPanel);

				// add each row to this column
				foreach(StartPageRow row in column.Rows) {
					var rowStackPanel = new StackPanel();
					if(columnStackPanel.Children.Count > 1) {
						rowStackPanel.Style = (Style)_Grid_Columns.Resources["Row_NotFirst"];
					}
					columnStackPanel.Children.Add(rowStackPanel);

					// row title
					var rowTitle = new TextBlock
					{
						Text = row.Title,
						Style = (Style)_Grid_Columns.Resources["Row_Title"]
					};
					rowStackPanel.Children.Add(rowTitle);

					var tilesWrapPanel = new WrapPanel();
					rowStackPanel.Children.Add(tilesWrapPanel);

					// add each tile
					foreach(StartPageTile tile in row.Tiles) {
						var tileButton = new Button();
						tilesWrapPanel.Children.Add(tileButton);

						// set the command
						tileButton.Command = tile.Command;

						// set the style
						switch(tile.Size) {
							case StartPageTileSize._1x1:
								tileButton.Style = (Style)_Grid_Columns.Resources["Tile_1x1"];
								break;
							case StartPageTileSize._2x1:
								tileButton.Style = (Style)_Grid_Columns.Resources["Tile_2x1"];
								break;
							case StartPageTileSize._2x2:
								tileButton.Style = (Style)_Grid_Columns.Resources["Tile_2x2"];
								break;
							default:
								throw new NotImplementedException();
						}

						var tileDockPanel = new DockPanel();
						tileButton.Content = tileDockPanel;

						// tile title
						if(tile.Title != null) {
							var tileTitle = new TextBlock
							{
								Text = tile.Title,
								Style = (Style)_Grid_Columns.Resources["Tile_title"]
							};
							tileDockPanel.Children.Add(tileTitle);
						}

						// tile description
						if(tile.Description != null) {
							tileButton.ToolTip = tile.Description;
						}

						// tile icon
						var tileIconGrid = new Grid();
						tileDockPanel.Children.Add(tileIconGrid);
						if(tile.Icon != null) {
							tileIconGrid.ColumnDefinitions.Add(new ColumnDefinition());
							tileIconGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
							tileIconGrid.ColumnDefinitions.Add(new ColumnDefinition());
							tileIconGrid.RowDefinitions.Add(new RowDefinition());
							tileIconGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });
							tileIconGrid.RowDefinitions.Add(new RowDefinition());
							var tileIconRectangle = new Rectangle();
							tileIconGrid.Children.Add(tileIconRectangle);
							Grid.SetColumn(tileIconRectangle, 1);
							Grid.SetRow(tileIconRectangle, 1);
							tileIconRectangle.Fill = new VisualBrush
							{
								Stretch = Stretch.Uniform,
								Visual = tile.Icon
							};
						}
					}
				}
			}
		}
	}
}
