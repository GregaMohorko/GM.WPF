/*
MIT License

Copyright (c) 2019 Gregor Mohorko

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
Author: Gregor Mohorko
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
using GM.Windows.Utility;
using GM.WPF.Utility;

namespace GM.WPF.Controls
{
	/// <summary>
	/// A <see cref="DataGrid"/> with:
	/// <para>- ability to cut cell values</para>
	/// <para>- ability to paste cell values</para>
	/// <para>- ability to delete cell values (only works when <see cref="DataGrid.CanUserDeleteRows"/> is false)</para>
	/// <para>- ability to copy/paste comma separated values data (even directly from/to excel)</para>
	/// <para>- property <see cref="ScrollViewer"/></para>
	/// <para>- property <see cref="SelectedCellContent"/></para>
	/// <para>- property <see cref="SelectedCellsWithDataCount"/></para>
	/// </summary>
	public class GMDataGrid : DataGrid
	{
		static GMDataGrid()
		{
			RegisterCommandCut();
			RegisterCommandPaste();
		}

		/// <summary>
		/// Provides handling for the <see cref="CommandBinding.Executed"/> event associated with the <see cref="ApplicationCommands.Copy"/> command.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		/// <exception cref="NotSupportedException"><see cref="DataGrid.ClipboardCopyMode"/> is set to <see cref="DataGridClipboardCopyMode.None"/>.</exception>
		protected override void OnExecutedCopy(ExecutedRoutedEventArgs e)
		{
			OnExecutedCopyPrivate(e);
		}

		/// <summary>
		/// Provides handling for the <see cref="CommandBinding.CanExecute"/> event associated with the <see cref="DataGrid.DeleteCommand"/> command.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected override void OnCanExecuteDelete(CanExecuteRoutedEventArgs e)
		{
			OnCanExecuteDeletePrivate(e);
		}

		/// <summary>
		/// Provides handling for the <see cref="CommandBinding.Executed"/> event associated with the <see cref="DataGrid.DeleteCommand"/> command.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected override void OnExecutedDelete(ExecutedRoutedEventArgs e)
		{
			OnExecutedDeletePrivate(e);
		}

		/// <summary>
		/// Sets the <see cref="SelectedCellContent"/> and <see cref="SelectedCellsWithDataCount"/> properties and then calls <see cref="DataGrid.OnSelectedCellsChanged(SelectedCellsChangedEventArgs)"/>.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
		{
			UpdateSelectedCellContent();
			UpdateSelectedCellsWithDataCount();
			base.OnSelectedCellsChanged(e);
		}

		#region CUT
		private static void RegisterCommandCut()
		{
			var cutCommandBinding = new CommandBinding(ApplicationCommands.Cut, new ExecutedRoutedEventHandler(OnExecutedCutStatic), new CanExecuteRoutedEventHandler(OnCanExecuteCutStatic));
			CommandManager.RegisterClassCommandBinding(typeof(GMDataGrid), cutCommandBinding);
		}

		private static void OnCanExecuteCutStatic(object sender, CanExecuteRoutedEventArgs e)
		{
			((GMDataGrid)sender).OnCanExecuteCut(e);
		}

		private static void OnExecutedCutStatic(object sender, ExecutedRoutedEventArgs e)
		{
			((GMDataGrid)sender).OnExecutedCut(e);
		}

		/// <summary>
		/// DependencyProperty for <see cref="AutoScrollToSelectedCellOnCut"/>.
		/// </summary>
		public static readonly DependencyProperty AutoScrollToSelectedCellOnCutProperty = DependencyProperty.Register(nameof(AutoScrollToSelectedCellOnCut), typeof(bool), typeof(GMDataGrid), new FrameworkPropertyMetadata(false, null, null));

		/// <summary>
		/// Determines whether it should auto-scroll to the currently selected cell after the Cut operation.
		/// </summary>
		public bool AutoScrollToSelectedCellOnCut
		{
			get => (bool)GetValue(AutoScrollToSelectedCellOnCutProperty);
			set => SetValue(AutoScrollToSelectedCellOnCutProperty, value);
		}

		/// <summary>
		/// Provides handling for the <see cref="CommandBinding.CanExecute"/> event associated with the <see cref="ApplicationCommands.Cut"/> command.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected virtual void OnCanExecuteCut(CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SelectedCells.Count > 0;
			e.Handled = true;
		}

		/// <summary>
		/// Provides handling for the <see cref="CommandBinding.Executed"/> event associated with the <see cref="ApplicationCommands.Cut"/> command.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected virtual void OnExecutedCut(ExecutedRoutedEventArgs e)
		{
			bool keepCurrentVerticalOffset = !AutoScrollToSelectedCellOnCut;

			int? scrollKey = null;
			if(keepCurrentVerticalOffset) {
				scrollKey = SaveCurrentVerticalOffset();
			}

			ExecuteCut(e);

			if(keepCurrentVerticalOffset) {
				RestoreSavedVerticalOffset(scrollKey.Value);
			}
		}

		/// <summary>
		/// This method is called when <see cref="ApplicationCommands.Cut"/> command is executed.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		private void ExecuteCut(ExecutedRoutedEventArgs e)
		{
			Debug.Assert(!e.Handled);
			ExecuteCopy(e, DataGridClipboardCopyMode.ExcludeHeader);
			if(e.Handled) {
				// copying was successful
				// now delete
				e.Handled = false;
				ExecuteDelete(e);
				// it doesn't matter if delete was successful or not, keep the data on the clipboard ...
			}
		}
		#endregion CUT

		#region COPY/PASTE
		private static void RegisterCommandPaste()
		{
			var pasteCommandBinding = new CommandBinding(ApplicationCommands.Paste, new ExecutedRoutedEventHandler(OnExecutedPasteStatic), new CanExecuteRoutedEventHandler(OnCanExecutePasteStatic));
			CommandManager.RegisterClassCommandBinding(typeof(GMDataGrid), pasteCommandBinding);
		}

		private static void OnCanExecutePasteStatic(object sender, CanExecuteRoutedEventArgs e)
		{
			((GMDataGrid)sender).OnCanExecutePaste(e);
		}

		private static void OnExecutedPasteStatic(object sender, ExecutedRoutedEventArgs e)
		{
			((GMDataGrid)sender).OnExecutedPaste(e);
		}

		/// <summary>
		/// DependencyProperty for <see cref="AutoScrollToSelectedCellOnPaste"/>.
		/// </summary>
		public static readonly DependencyProperty AutoScrollToSelectedCellOnPasteProperty = DependencyProperty.Register(nameof(AutoScrollToSelectedCellOnPaste), typeof(bool), typeof(GMDataGrid), new FrameworkPropertyMetadata(false, null, null));
		/// <summary>
		/// DependencyProperty for <see cref="CanUserPasteToNewRows"/>.
		/// </summary>
		public static readonly DependencyProperty CanUserPasteToNewRowsProperty = DependencyProperty.Register(nameof(CanUserPasteToNewRows), typeof(bool), typeof(GMDataGrid), new FrameworkPropertyMetadata(true, null, null));

		/// <summary>
		/// Determines whether it should auto-scroll to the currently selected cell after the Paste operation.
		/// </summary>
		public bool AutoScrollToSelectedCellOnPaste
		{
			get => (bool)GetValue(AutoScrollToSelectedCellOnPasteProperty);
			set => SetValue(AutoScrollToSelectedCellOnPasteProperty, value);
		}
		/// <summary>
		/// Determines whether the end-user can add new rows to the ItemsSource.
		/// </summary>
		public bool CanUserPasteToNewRows
		{
			get => (bool)GetValue(CanUserPasteToNewRowsProperty);
			set => SetValue(CanUserPasteToNewRowsProperty, value);
		}

		private void OnExecutedCopyPrivate(ExecutedRoutedEventArgs e)
		{
			DataGridClipboardCopyMode clipboardCopyMode = ClipboardCopyMode;

			if(clipboardCopyMode != DataGridClipboardCopyMode.None) {
				ExecuteCopy(e, clipboardCopyMode);
			} else {
				base.OnExecutedCopy(e);
			}
		}

		/// <summary>
		/// Provides handling for the <see cref="CommandBinding.CanExecute"/> event associated with the <see cref="ApplicationCommands.Paste"/> command.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected virtual void OnCanExecutePaste(CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SelectedCells.Count > 0;
			e.Handled = true;
		}

		/// <summary>
		/// Provides handling for the <see cref="CommandBinding.Executed"/> event associated with the <see cref="ApplicationCommands.Paste"/> command.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected virtual void OnExecutedPaste(ExecutedRoutedEventArgs e)
		{
			bool keepCurrentVerticalOffset = !AutoScrollToSelectedCellOnPaste;

			int? scrollKey = null;
			if(keepCurrentVerticalOffset) {
				scrollKey = SaveCurrentVerticalOffset();
			}

			ExecutePaste(e);

			if(keepCurrentVerticalOffset) {
				RestoreSavedVerticalOffset(scrollKey.Value);
			}
		}

		/// <summary>
		/// This method is called when <see cref="ApplicationCommands.Copy"/> command (or <see cref="ApplicationCommands.Cut"/>) is executed.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		/// <param name="clipboardCopyMode">Copy mode.</param>
		private void ExecuteCopy(ExecutedRoutedEventArgs e, DataGridClipboardCopyMode clipboardCopyMode)
		{
			// copy all selected cells as a tab-delimited text ONLY (without style, formatting, etc.)

			IList<DataGridCellInfo> selectedCells = SelectedCells;

			var selectedCellsByItem = selectedCells.GroupBy(c => c.Item);

			var tsvLines = new List<string>();

			if(clipboardCopyMode == DataGridClipboardCopyMode.IncludeHeader) {
				// include the column headers in the 1st line
				var columns = selectedCells.Select(c => c.Column).Distinct().ToList();
				var columnHeaders = columns.Select(c => c.Header?.ToString());
				tsvLines.Add(string.Join("\t", columnHeaders));
			}

			foreach(var itemAndCells in selectedCellsByItem) {
				object item = itemAndCells.Key;
				var tsvLineParts = new List<string>();
				foreach(DataGridCellInfo cell in itemAndCells) {
					object value = cell.Column.ClipboardContentBinding?.GetValueFor(item);
					string text = value?.ToString();
					tsvLineParts.Add($"\"{text}\"");
				}
				string tsvLine = string.Join("\t", tsvLineParts);
				tsvLines.Add(tsvLine);
			}

			string tsvText = string.Join(Environment.NewLine, tsvLines);
			ClipboardUtility.SetText(tsvText);
			e.Handled = true;
		}

		/// <summary>
		/// This method is called when <see cref="ApplicationCommands.Paste"/> command is executed.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		private void ExecutePaste(ExecutedRoutedEventArgs e)
		{
			IList<DataGridCellInfo> selectedCells = SelectedCells;

			// parse the clipboard data [row][column]
			List<string[]> clipboardData = ClipboardUtility.ParseClipboardData();

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

				if(gridRow >= Items.Count) {
					// hmm, what to do here?
					// how is that even possible?
					Debug.Fail("What to do here?");
					break;
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

					// if a new row has to be added, it will be added on BeginEdit ...
					BeginEditCommand.Execute(null, this);
					// ... access the item after BeginEdit, so that the item has been created
					object item = Items[gridRow];
					object cellContent = clipboardData[clipboardRow][clipboardColumn];
					column.OnPastingCellClipboardContent(item, cellContent);
					CommitEditCommand.Execute(null, this);
				}
			}

			e.Handled = true;
		}
		#endregion COPY/PASTE

		#region DELETE
		/// <summary>
		/// DependencyProperty for <see cref="AutoScrollToSelectedCellOnDelete"/>.
		/// </summary>
		public static readonly DependencyProperty AutoScrollToSelectedCellOnDeleteProperty = DependencyProperty.Register(nameof(AutoScrollToSelectedCellOnDelete), typeof(bool), typeof(GMDataGrid), new FrameworkPropertyMetadata(false, null, null));

		/// <summary>
		/// Determines whether it should auto-scroll to the currently selected cell after the Delete operation.
		/// </summary>
		public bool AutoScrollToSelectedCellOnDelete
		{
			get => (bool)GetValue(AutoScrollToSelectedCellOnDeleteProperty);
			set => SetValue(AutoScrollToSelectedCellOnDeleteProperty, value);
		}

		private void OnCanExecuteDeletePrivate(CanExecuteRoutedEventArgs e)
		{
			if(CanUserDeleteRows == false) {
				e.CanExecute = true;
				e.Handled = true;
				return;
			} else {
				base.OnCanExecuteDelete(e);
			}
		}

		private void OnExecutedDeletePrivate(ExecutedRoutedEventArgs e)
		{
			if(CanUserDeleteRows) {
				base.OnExecutedDelete(e);
				return;
			}

			bool keepCurrentVerticalOffset = !AutoScrollToSelectedCellOnDelete;

			int? scrollKey = null;
			if(keepCurrentVerticalOffset) {
				scrollKey = SaveCurrentVerticalOffset();
			}

			ExecuteDelete(e);

			if(keepCurrentVerticalOffset) {
				RestoreSavedVerticalOffset(scrollKey.Value);
			}
		}

		/// <summary>
		/// This method is called when <see cref="ApplicationCommands.Delete"/> command (or <see cref="ApplicationCommands.Cut"/>) is executed.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		private void ExecuteDelete(ExecutedRoutedEventArgs e)
		{
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
				bool? bindingSuccessful = column.ClipboardContentBinding?.TrySetValueFor(item, cellContent);
				if(bindingSuccessful != true) {
					column.OnPastingCellClipboardContent(item, cellContent);
				}
				CommitEditCommand.Execute(null, this);
			}

			e.Handled = true;
		}
		#endregion DELETE

		#region SCROLL
		private ScrollViewer _scrollViewer;
		/// <summary>
		/// Gets the <see cref="ScrollViewer"/> inside of this <see cref="DataGrid"/>.
		/// </summary>
		public ScrollViewer ScrollViewer
		{
			get
			{
				if(_scrollViewer == null) {
					List<ScrollViewer> scrollViewers = VisualUtility.GetVisualChildCollection<ScrollViewer>(this, VisualUtility.TreeTraverseStrategy.BreadthFirst).ToList();
					_scrollViewer = scrollViewers.FirstOrDefault();
				}
				return _scrollViewer;
			}
		}

		private readonly Lazy<Dictionary<int, double?>> savedVerticalOffsets = new Lazy<Dictionary<int, double?>>();
		/// <summary>
		/// Saves the current vertical offset of <see cref="ScrollViewer"/> and returns a key with which it can be restored using <see cref="RestoreSavedVerticalOffset(int)"/>.
		/// </summary>
		private int SaveCurrentVerticalOffset()
		{
			lock(savedVerticalOffsets) {
				int newKey = savedVerticalOffsets.Value.Count;
				double? currentVerticalOffset = ScrollViewer?.VerticalOffset;
				savedVerticalOffsets.Value.Add(newKey, currentVerticalOffset);
				return newKey;
			}
		}

		/// <summary>
		/// Restored the previously saved vertical offset of <see cref="ScrollViewer"/> with <see cref="SaveCurrentVerticalOffset"/>.
		/// </summary>
		private void RestoreSavedVerticalOffset(int key)
		{
			lock(savedVerticalOffsets) {
				double? savedVerticalOffset = savedVerticalOffsets.Value[key];
				if(savedVerticalOffset != ScrollViewer?.VerticalOffset) {
					ScrollViewer.ScrollToVerticalOffset(savedVerticalOffset.Value);
				}
			}
		}
		#endregion SCROLL

		#region SELECTED CELL CONTENT
		/// <summary>
		/// A common event for all <see cref="GMDataGrid"/> controls that occurs when the content of the current cell in any of the <see cref="GMDataGrid"/> controls changes.
		/// </summary>
		public static event EventHandler<object> StaticSelectedCellContentChanged;
		/// <summary>
		/// Occurs when the content of the current cell changes.
		/// </summary>
		public event EventHandler<object> SelectedCellContentChanged;

		//private static readonly DependencyPropertyKey SelectedCellContentPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SelectedCellContent), typeof(object), typeof(GMDataGrid), new PropertyMetadata());
		//public static readonly DependencyProperty SelectedCellContentProperty = SelectedCellContentPropertyKey.DependencyProperty;
		/// <summary>
		/// DependencyProperty for <see cref="SelectedCellContent"/>.
		/// </summary>
		public static readonly DependencyProperty SelectedCellContentProperty = DependencyProperty.Register(nameof(SelectedCellContent), typeof(object), typeof(GMDataGrid), new PropertyMetadata());

		/// <summary>
		/// Gets the content of the current cell.
		/// <para>Do not set this property.</para>
		/// </summary>
		public object SelectedCellContent
		{
			get => GetValue(SelectedCellContentProperty);
			//set => SetValue(SelectedCellContentPropertyKey, value);
			set
			{
				SetValue(SelectedCellContentProperty, value);
				SelectedCellContentChanged?.Invoke(this, value);
				StaticSelectedCellContentChanged?.Invoke(this, value);
			}
		}

		private void UpdateSelectedCellContent()
		{
			IList<DataGridCellInfo> selectedCells = SelectedCells;
			if(selectedCells.Count != 1) {
				SelectedCellContent = null;
				return;
			}
			var cell = selectedCells.Single();
			if(cell.Column.ClipboardContentBinding == null) {
				SelectedCellContent = null;
				return;
			}
			(bool success, object value) = cell.Column.ClipboardContentBinding.TryGetValueFor(cell.Item);
			if(!success) {
				SelectedCellContent = null;
				return;
			}
			SelectedCellContent = value;
		}
		#endregion SELECTED CELL CONTENT

		#region NUMBER OF SELECTED CELLS WITH DATA
		/// <summary>
		/// A common event for all <see cref="GMDataGrid"/> controls that occurs when the number of selected cells that contain data in any of the <see cref="GMDataGrid"/> controls changes.
		/// </summary>
		public static event EventHandler<int> StaticSelectedCellsWithDataCountChanged;
		/// <summary>
		/// Occurs when the number of selected cells that contain data changes.
		/// </summary>
		public event EventHandler<int> SelectedCellsWithDataCountChanged;

		/// <summary>
		/// DependencyProperty for <see cref="SelectedCellsWithDataCount"/>.
		/// </summary>
		public static readonly DependencyProperty SelectedCellsWithDataCountProperty = DependencyProperty.Register(nameof(SelectedCellsWithDataCount), typeof(int), typeof(GMDataGrid));

		/// <summary>
		/// Gets the number of selected cells that contain data.
		/// <para>Do not set this property.</para>
		/// </summary>
		public int SelectedCellsWithDataCount
		{
			get => (int)GetValue(SelectedCellsWithDataCountProperty);
			set
			{
				SetValue(SelectedCellsWithDataCountProperty, value);
				SelectedCellsWithDataCountChanged?.Invoke(this, value);
				StaticSelectedCellsWithDataCountChanged?.Invoke(this, value);
			}
		}

		private void UpdateSelectedCellsWithDataCount()
		{
			// count the number of selected cells that contain data
			int count = 0;
			IList<DataGridCellInfo> selectedCells = SelectedCells;
			foreach(DataGridCellInfo cell in selectedCells) {
				DataGridColumn column = cell.Column;
				if(column.ClipboardContentBinding == null) {
					continue;
				}
				(bool success, object value) = column.ClipboardContentBinding.TryGetValueFor(cell.Item);
				if(success && value != null) {
					if((value is string stringValue) && string.IsNullOrWhiteSpace(stringValue)) {
						// empty strings don't count
						continue;
					}
					++count;
				}
			}
			if(SelectedCellsWithDataCount != count) {
				SelectedCellsWithDataCount = count;
			}
		}
		#endregion NUMBER OF SELECTED CELLS WITH DATA
	}
}
