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
using System.Windows.Media;
using GalaSoft.MvvmLight.CommandWpf;

namespace GM.WPF.Controls
{
	/// <summary>
	/// Represents a column.
	/// </summary>
	public class StartPageColumn
	{
		/// <summary>
		/// Rows in this column.
		/// </summary>
		public ICollection<StartPageRow> Rows { get; set; }
	}

	/// <summary>
	/// Represents a row in a column.
	/// </summary>
	public class StartPageRow
	{
		/// <summary>
		/// The title of this row.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// The tiles in this row.
		/// </summary>
		public ICollection<StartPageTile> Tiles { get; set; }
	}

	/// <summary>
	/// Represents a 1x1 tile.
	/// </summary>
	public class StartPageTile
	{
		/// <summary>
		/// Tile size.
		/// </summary>
		public StartPageTileSize Size { get; set; }
		/// <summary>
		/// Tile icon.
		/// </summary>
		public Visual Icon { get; set; }
		/// <summary>
		/// Tile title.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Tile description.
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Tile command.
		/// </summary>
		public RelayCommand Command { get; set; }
	}

	/// <summary>
	/// Size of the start page tile.
	/// </summary>
	public enum StartPageTileSize
	{
		/// <summary>
		/// 1 x 1.
		/// </summary>
		_1x1,
		/// <summary>
		/// 2 x 1.
		/// </summary>
		_2x1,
		/// <summary>
		/// 2 x 2.
		/// </summary>
		_2x2
	}
}
