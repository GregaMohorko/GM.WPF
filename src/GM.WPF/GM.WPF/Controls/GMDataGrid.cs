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
Created: 2018-12-13
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GM.Utility;

namespace GM.WPF.Controls
{
	/// <summary>
	/// A <see cref="DataGrid"/> with:
	/// <para>- ability to paste comma separated values data (even directly from excel)</para>
	/// </summary>
	public class GMDataGrid : DataGrid
	{
		static GMDataGrid()
		{
			var pasteCommandBinding = new CommandBinding(ApplicationCommands.Paste, new ExecutedRoutedEventHandler(OnExecutedPasteInternal), new CanExecuteRoutedEventHandler(OnCanExecutePasteInternal));
			var deleteCommandBinding = new CommandBinding(ApplicationCommands.Delete, new ExecutedRoutedEventHandler(OnExecutedDeleteInternal), new CanExecuteRoutedEventHandler(OnCanExecuteDeleteInternal));
			CommandManager.RegisterClassCommandBinding(typeof(GMDataGrid), pasteCommandBinding);
			CommandManager.RegisterClassCommandBinding(typeof(GMDataGrid), deleteCommandBinding);
		}

		private static void OnExecutedPasteInternal(object sender, ExecutedRoutedEventArgs e)
		{
			((GMDataGrid)sender).OnExecutePaste(sender, e);
		}

		private static void OnCanExecutePasteInternal(object sender, CanExecuteRoutedEventArgs e)
		{
			((GMDataGrid)sender).OnCanExecutePaste(sender, e);
		}

		private static void OnExecutedDeleteInternal(object sender, ExecutedRoutedEventArgs e)
		{
			((GMDataGrid)sender).OnExecuteDelete(sender, e);
		}

		private static void OnCanExecuteDeleteInternal(object sender, CanExecuteRoutedEventArgs e)
		{
			((GMDataGrid)sender).OnCanExecuteDelete(sender, e);
		}

		/// <summary>
		/// Occurs when pasting.
		/// </summary>
		public event ExecutedRoutedEventHandler ExecutePasteEvent;
		/// <summary>
		/// Occurs when determining whether paste event can be executed.
		/// </summary>
		public event CanExecuteRoutedEventHandler CanExecutePasteEvent;
		/// <summary>
		/// Occurs when deleting.
		/// </summary>
		public event ExecutedRoutedEventHandler ExecuteDeleteEvent;
		/// <summary>
		/// Occurs when determining whether delete event can be executed.
		/// </summary>
		public event CanExecuteRoutedEventHandler CanExecuteDeleteEvent;

		/// <summary>
		/// DependencyProperty for CanUserAddRows.
		/// </summary>
		public static readonly DependencyProperty CanUserPasteToNewRowsProperty = DependencyProperty.Register(nameof(CanUserPasteToNewRows), typeof(bool), typeof(GMDataGrid), new FrameworkPropertyMetadata(true, null, null));

		/// <summary>
		///     Whether the end-user can add new rows to the ItemsSource.
		/// </summary>
		public bool CanUserPasteToNewRows
		{
			get => (bool)GetValue(CanUserPasteToNewRowsProperty);
			set => SetValue(CanUserPasteToNewRowsProperty, value);
		}

		/// <summary>
		/// This virtual method is called when ApplicationCommands.Paste command query its state.
		/// </summary>
		/// <param name="sender">The target.</param>
		/// <param name="e">Arguments.</param>
		protected virtual void OnCanExecutePaste(object sender, CanExecuteRoutedEventArgs e)
		{
			if(CanExecutePasteEvent != null) {
				CanExecutePasteEvent(sender, e);
				if(e.Handled) {
					return;
				}
			}

			e.CanExecute = SelectedCells.Count > 0;
			e.Handled = true;
		}

		/// <summary>
		/// This virtual method is called when ApplicationCommands.Delete command is executed.
		/// </summary>
		/// <param name="sender">The target.</param>
		/// <param name="e">Arguments.</param>
		private void OnExecuteDelete(object sender, ExecutedRoutedEventArgs e)
		{
			if(ExecuteDeleteEvent != null) {
				ExecuteDeleteEvent(sender, e);
				if(e.Handled) {
					return;
				}
			}

			IList<DataGridCellInfo> selectedCells = SelectedCells;
			foreach(DataGridCellInfo cell in selectedCells) {
				DataGridColumn column = cell.Column;
				if(column.IsReadOnly) {
					continue;
				}
				object item = cell.Item;
				object cellContent = null;

				BeginEditCommand.Execute(null, this);
				// using column.OnPastingCellClipboardContent did not paste on all cells sometimes ... which is weird!
				// that's why I'm first trying to set it manually using binding
				bool bindingSuccessful = column.ClipboardContentBinding.TrySetValueFor(item, cellContent);
				if(!bindingSuccessful) {
					column.OnPastingCellClipboardContent(item, cellContent);
				}
				CommitEditCommand.Execute(null, this);
			}
		}

		/// <summary>
		/// This virtual method is called when ApplicationCommands.Paste command is executed.
		/// </summary>
		/// <param name="sender">The target.</param>
		/// <param name="e">Arguments.</param>
		protected virtual void OnExecutePaste(object sender, ExecutedRoutedEventArgs e)
		{
			if(ExecutePasteEvent != null) {
				ExecutePasteEvent(sender, e);
				if(e.Handled) {
					return;
				}
			}

			IList<DataGridCellInfo> selectedCells = SelectedCells;

			// parse the clipboard data [row][column]
			List<string[]> clipboardData = clipboardData = ClipboardUtility.ParseClipboardData();

			// inspect selected cells ...
			bool shouldPasteDuplicates;
			int topmostRow;
			int leftTopmostColumn;
			int bottomRow;
			int rightBottomColumn;
			while(true) {
				List<DataGridCellInfo> topMostCells = selectedCells.WhereMin(c => Items.IndexOf(c.Item));
				List<DataGridCellInfo> bottomCells = selectedCells.WhereMax(c => Items.IndexOf(c.Item));

				DataGridCellInfo leftTopmostCell = topMostCells.FirstMin(c => c.Column.DisplayIndex);
				DataGridCellInfo rightTopmostCell = topMostCells.FirstMax(c => c.Column.DisplayIndex);
				DataGridCellInfo leftBottomCell = bottomCells.FirstMin(c => c.Column.DisplayIndex);
				DataGridCellInfo rightBottomCell = bottomCells.FirstMax(c => c.Column.DisplayIndex);

				topmostRow = Items.IndexOf(rightTopmostCell.Item);
				leftTopmostColumn = leftTopmostCell.Column.DisplayIndex;
				bottomRow = Items.IndexOf(rightBottomCell.Item);
				rightBottomColumn = rightBottomCell.Column.DisplayIndex;

				if(clipboardData.Count == 1 && clipboardData[0].Length == 1) {
					// there is only one cell being pasted
					shouldPasteDuplicates = true;
					break;
				}

				if(!clipboardData.AllSame(l => l.Length)) {
					// pasted data is not rectangular
					shouldPasteDuplicates = false;
					break;
				}

				int numberOfSelectedRows = bottomRow - topmostRow + 1;
				int numberOfSelectedColumns = rightBottomColumn - leftTopmostColumn + 1;

				// determine if selected cells are all together
				bool areAllSelectedCellsTogether;
				while(true) {
					bool[][] cellPresences = ArrayUtility.Create<bool>(numberOfSelectedRows, numberOfSelectedColumns);
					foreach(DataGridCellInfo cell in selectedCells) {
						int cellRow = Items.IndexOf(cell.Item) - topmostRow;
						int cellColumn = cell.Column.DisplayIndex - leftTopmostColumn;
						if(cellRow >= numberOfSelectedRows || cellColumn >= numberOfSelectedColumns) {
							areAllSelectedCellsTogether = false;
							break;
						}
						cellPresences[cellRow][cellColumn] = true;
					}
					// were all set to true?
					areAllSelectedCellsTogether = cellPresences.All(cp => cp.All(cpp => cpp));
					break;
				}
				if(!areAllSelectedCellsTogether) {
					shouldPasteDuplicates = false;
					break;
				}
				int numberOfClipboardColumns = clipboardData[0].Length;
				int numberOfClipboardRows = clipboardData.Count;

				shouldPasteDuplicates =
					// selected column count is biggger or equal to clipboard column count
					numberOfSelectedColumns >= numberOfClipboardColumns
					// selected column count is dividable by clipboard column count
					&& numberOfSelectedColumns % numberOfClipboardColumns == 0
					// selected row count is bigger or equal to clipboard row count
					&& numberOfSelectedRows >= numberOfClipboardRows
					// selected row count is dividable by clipboard row count
					&& numberOfSelectedRows % numberOfClipboardRows == 0;
				break;
			}

			// iterate through all the rows
			int maxGridRowIndex = Items.Count - 1;
			int startIndexOfDisplayColumn = (SelectionUnit != DataGridSelectionUnit.FullRow) ? leftTopmostColumn : 0;
			int clipboardRow = 0;
			for(int gridRow = topmostRow; ; ++gridRow, ++clipboardRow) {
				if(gridRow >= Items.Count) {
					// hmm, what to do here?
					// how is that even possible?
					Debug.Fail("What to do here?");
					break;
				}

				if(clipboardRow == clipboardData.Count) {
					// all clipboard rows have already been pasted
					if(!shouldPasteDuplicates) {
						break;
					}
					if(gridRow > bottomRow) {
						// is out of the selected cells
						break;
					}
					// should duplicate, so start again
					clipboardRow = 0;
				}

				if(gridRow == maxGridRowIndex) {
					if(!CanUserPasteToNewRows) {
						break;
					} else {
						// new row will automatically be added
						++maxGridRowIndex;
					}
				}

				CurrentItem = Items[gridRow];

				// iterate through all the columns for this row and paste the data
				int clipboardRowLength = clipboardData[clipboardRow].Length;
				int clipboardColumn = 0;
				for(int gridColumn = startIndexOfDisplayColumn; ; ++gridColumn, ++clipboardColumn) {
					if(clipboardColumn == clipboardRowLength) {
						// all clipboard columns have already been pasted
						if(!shouldPasteDuplicates) {
							break;
						}
						if(gridColumn > rightBottomColumn) {
							// is out of the selected cells
							break;
						}
						// should duplicate, so start again
						clipboardColumn = 0;
					}

					DataGridColumn column = Columns.FirstOrDefault(c => c.DisplayIndex == gridColumn);
					if(column == null || column.IsReadOnly) {
						continue;
					}
					object item = Items[gridRow];
					object cellContent = clipboardData[clipboardRow][clipboardColumn];

					BeginEditCommand.Execute(null, this);
					column.OnPastingCellClipboardContent(item, cellContent);
					CommitEditCommand.Execute(null, this);
				}
			}
		}

		/// <summary>
		/// This virtual method is called when ApplicationCommands.Delete command query its state.
		/// </summary>
		/// <param name="sender">The target.</param>
		/// <param name="e">Arguments.</param>
		private void OnCanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
		{
			if(CanExecuteDeleteEvent != null) {
				CanExecuteDeleteEvent(sender, e);
				if(e.Handled) {
					return;
				}
			}

			e.CanExecute = CurrentItem != null;
			e.Handled = true;
		}
	}
}
