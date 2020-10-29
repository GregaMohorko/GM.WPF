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
Created: 2018-12-13
Author: Gregor Mohorko
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GM.Utility;
using GM.Windows.Utility;
using GM.WPF.Patterns.UndoRedo;
using GM.WPF.Utility;

namespace GM.WPF.Controls
{
	/// <summary>
	/// A <see cref="DataGrid"/> with:
	/// <para>- undo/redo functionality (if the owning window is registered with <see cref="GMWPFUndoRedo"/>)</para>
	/// <para>- cell by cell editing (and not row by row as is default)</para>
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
		/// Initializes a new instance of <see cref="GMDataGrid"/> class.
		/// </summary>
		public GMDataGrid()
		{
			Loaded += GMDataGrid_Loaded_ForUndoRedo;
		}

		/// <summary>
		/// Captures the sort event in undo/redo manager (if it exists) and raises the <see cref="DataGrid.Sorting"/> event.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected override void OnSorting(DataGridSortingEventArgs e)
		{
			List<SortDescription> sortDescriptionsBefore = null;
			if(undoRedo != null) {
				sortDescriptionsBefore = Items.SortDescriptions
					.Select(sd => new SortDescription(sd.PropertyName, sd.Direction))
					.ToList();
			}

			base.OnSorting(e);

			if(undoRedo != null) {
				List<SortDescription> sortDescriptionsAfter = Items.SortDescriptions
					.Select(sd => new SortDescription(sd.PropertyName, sd.Direction))
					.ToList();
				CaptureSortingInUndoRedo(sortDescriptionsBefore, sortDescriptionsAfter);
			}
		}

		/// <summary>
		/// Calls the <see cref="DataGrid.OnCellEditEnding(DataGridCellEditEndingEventArgs)"/> and then manually commits the edit and adds the undo command to the associated undo/redo manager.
		/// </summary>
		/// <param name="e">The data for the event.</param>
		protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
		{
			base.OnCellEditEnding(e);
			CommitEditAndHandleUndoRedo(e);
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

		#region UNDO/REDO
		private GMWPFUndoRedo undoRedo;
		private Dictionary<string, DataGridColumn[]> propertyNameToColumns;
		private bool isManuallyCommittingEdit;

		private void GMDataGrid_Loaded_ForUndoRedo(object sender, RoutedEventArgs e)
		{
			// find the undoredo associated with the window in which this datagrid is in
			if(undoRedo != null) {
				throw new NotImplementedException("How is this possible? This handler is registered only at beginning, and then removed when loaded for the first time.");
			}
			undoRedo = GMWPFUndoRedo.GetInstance(this);
			Loaded -= GMDataGrid_Loaded_ForUndoRedo;

			if(undoRedo != null) {
				Columns.CollectionChanged += Columns_CollectionChanged_ForUndoRedo;
			}
		}

		private void Columns_CollectionChanged_ForUndoRedo(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action) {
				case NotifyCollectionChangedAction.Move:
					// do nothing
					break;
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					// reset the cache of columns
					propertyNameToColumns = null;
					break;
				default:
					throw new NotImplementedException($"Unsupported notify collection changed action: '{e.Action}'.");
			}
		}

		private void CaptureSortingInUndoRedo(List<SortDescription> sortsbefore, List<SortDescription> sortsAfter)
		{
			if(undoRedo == null) {
				return;
			}

			if(propertyNameToColumns == null) {
				propertyNameToColumns = new Dictionary<string, DataGridColumn[]>();
			}

			List<(SortDescription sort, DataGridColumn[] columns)> findAssociatedColumns(List<SortDescription> sorts)
			{
				var associatedColumns = new List<(SortDescription sort, DataGridColumn[] columns)>();
				foreach(SortDescription sd in sorts) {
					if(!propertyNameToColumns.TryGetValue(sd.PropertyName, out DataGridColumn[] columnsWithSameSortMemberPath)) {
						// search for and add
						columnsWithSameSortMemberPath = Columns.Where(c => c.SortMemberPath == sd.PropertyName).ToArray();
						propertyNameToColumns.Add(sd.PropertyName, columnsWithSameSortMemberPath);
					}
					associatedColumns.Add((sd, columnsWithSameSortMemberPath));
				}
				return associatedColumns;
			}

			List<(SortDescription sort, DataGridColumn[] columns)> before = findAssociatedColumns(sortsbefore);
			List<(SortDescription sort, DataGridColumn[] columns)> after = findAssociatedColumns(sortsAfter);

			var sortDescriptions = new List<string>();
			foreach((SortDescription sd, DataGridColumn[] columns) in after) {
				string columnsS = string.Join("/", columns.Select(c => c.Header)).Replace("\n", " ").RemoveAllOf("\r");
				string sortDescription = $"{(sd.Direction == ListSortDirection.Ascending ? "ascending" : "descending")} by {columnsS}";
				sortDescriptions.Add(sortDescription);
			}
			string description = $"Sort {string.Join(", ", sortDescriptions)}";

			void sort(List<(SortDescription sort, DataGridColumn[] columns)> sorts)
			{
				Items.SortDescriptions.Clear();
				foreach(DataGridColumn c in Columns) {
					c.SortDirection = null;
				}
				foreach((SortDescription sd, DataGridColumn[] columns) in sorts) {
					Items.SortDescriptions.Add(sd);
					foreach(DataGridColumn c in columns) {
						c.SortDirection = sd.Direction;
					}
				}
				Items.Refresh();
			}
			void undo()
			{
				sort(before);
			};
			void redo()
			{
				sort(after);
			};
			undoRedo.Add(description, undo, redo);
		}

		private void CommitEditAndHandleUndoRedo(DataGridCellEditEndingEventArgs e)
		{
			if(undoRedo == null) {
				return;
			}
			if(isManuallyCommittingEdit) {
				return;
			}
			if(e.Cancel || e.EditAction != DataGridEditAction.Commit) {
				// only add to the stack if it's a non-cancelled commit
				return;
			}

			object editedItem = e.Row.Item;
			string columnHeader = e.Column.Header as string;
			BindingBase binding = e.Column.ClipboardContentBinding;
			if(binding == null) {
				throw new NotImplementedException("How is this possible? How does the DataGrid know which property to edit then?");
			}
			object oldValue = binding.GetValueFor(editedItem);

			// manually commit the edit so that we can get the new value
			isManuallyCommittingEdit = true;
			_ = CommitEdit(DataGridEditingUnit.Row, false);
			isManuallyCommittingEdit = false;

			object newValue = binding.GetValueFor(editedItem);

			if(oldValue != null) {
				if(oldValue.Equals(newValue)) {
					// nothing was changed, no need to mark undo/redo command
					return;
				}
			} else if(newValue == null) {
				// both are null, nothing was changed, no need to mark undo/redo command
				return;
			}

			// add the undo command to the undo/redo manager
			string oldValueDescription = oldValue?.ToString()?.ShortenWith3Dots(10);
			string newValueDescription = newValue?.ToString()?.ShortenWith3Dots(10);
			string description = $"Edit {columnHeader} from '{oldValueDescription}' to '{newValueDescription}'";
			void undo() => binding.SetValueFor(editedItem, oldValue);
			void redo() => binding.SetValueFor(editedItem, newValue);
			undoRedo.Add(description, undo, redo);
		}

		private void UndoRedo_HandlePaste(List<(Action undo, Action redo)> undoRedoActions, int cellsPasted, int newItemsAdded)
		{
			if(undoRedoActions.Count == 0) {
				return;
			}
			if(undoRedoActions.Count > (cellsPasted + newItemsAdded)) {
				throw new NotImplementedException("The count of undo/redo actions should be less (in case there were any new items added) or equal to the sum of cells pasted and new items added.");
			}
			string description = $"Paste {cellsPasted} cell{(cellsPasted > 1 ? "s" : "")}";
			if(newItemsAdded > 0) {
				description += $", add {newItemsAdded} items";
			} else if(undoRedoActions.Count != cellsPasted) {
				throw new NotImplementedException("The count of undo/redo actions should match the number of cells pasted, because no new items were added.");
			}
			// force refresh after edits
			undoRedoActions.Add((Items.Refresh, Items.Refresh));
			undoRedo.Add(description, undoRedoActions);
		}

		private void UndoRedo_HandleDelete(List<(Action undo, Action redo)> undoRedoActions)
		{
			string description = $"Delete {undoRedoActions.Count} cell{(undoRedoActions.Count > 1 ? "s" : "")}";
			// force refresh after edits
			undoRedoActions.Add((Items.Refresh, Items.Refresh));
			undoRedo.Add(description, undoRedoActions);
		}
		#endregion UNDO/REDO

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
			const char SEPARATOR_CHAR = '\t';
			string SEPARATOR_STRING = SEPARATOR_CHAR.ToString();

			IList<DataGridCellInfo> selectedCells = SelectedCells;

			var selectedCellsByItem = selectedCells.GroupBy(c => c.Item);

			var tsvLines = new List<string>();

			if(clipboardCopyMode == DataGridClipboardCopyMode.IncludeHeader) {
				// include the column headers in the 1st line
				var columns = selectedCells.Select(c => c.Column).Distinct().ToList();
				var columnHeaders = columns.Select(c => c.Header?.ToString());
				tsvLines.Add(string.Join(SEPARATOR_STRING, columnHeaders));
			}

			foreach(var itemAndCells in selectedCellsByItem) {
				object item = itemAndCells.Key;
				var tsvLineParts = new List<string>();
				foreach(DataGridCellInfo cell in itemAndCells) {
					object value = cell.Column.ClipboardContentBinding?.GetValueFor(item);
					string text = value?.ToString();
					// only surround with double quotes if text is null/empty or if it contains tab character
					string newLinePart = CsvUtility.SurroundWithDoubleQuotes(text, true, false, SEPARATOR_CHAR);
					tsvLineParts.Add(newLinePart);
				}
				string tsvLine = string.Join(SEPARATOR_STRING, tsvLineParts);
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
					(numberOfSelectedColumns >= numberOfClipboardColumns)
					// selected column count is dividable by clipboard column count
					&& ((numberOfSelectedColumns % numberOfClipboardColumns) == 0)
					// selected row count is bigger or equal to clipboard row count
					&& (numberOfSelectedRows >= numberOfClipboardRows)
					// selected row count is dividable by clipboard row count
					&& ((numberOfSelectedRows % numberOfClipboardRows) == 0);
				break;
			}

			List<(Action undo, Action redo)> undoRedoActions = null;
			if(undoRedo != null) {
				undoRedoActions = new List<(Action undo, Action redo)>();
			}

			int newItemsAdded = 0;
			int cellsPasted = 0;

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

				bool isNewItem = false;
				bool newItemAddedToUndoRedoActions = false;
				if(gridRow == maxGridRowIndex) {
					if(!CanUserPasteToNewRows) {
						break;
					} else {
						// new row will automatically be added
						++maxGridRowIndex;
						isNewItem = true;
						++newItemsAdded;
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

					object cellContent = clipboardData[clipboardRow][clipboardColumn];
					BindingBase binding = column.ClipboardContentBinding;
					if(binding == null) {
						throw new NotImplementedException($"Column '{column.Header}' is missing '{nameof(column.ClipboardContentBinding)}'.");
					}

					isManuallyCommittingEdit = true;
					// if a new row has to be added, it will be added on BeginEdit ...
					BeginEditCommand.Execute(null, this);
					// ... access the item after BeginEdit, so that the item has been created
					// before BeginEdit, the CurrentItem is of type NewItemPlaceholder
					object item = Items[gridRow];
					object oldValue = binding.GetValueFor(item, true);
					if(oldValue == cellContent) {
						// nothing to edit
						CancelEditCommand.Execute(null, this);
						isManuallyCommittingEdit = false;
						continue;
					}
					void pasteValue()
					{
						column.OnPastingCellClipboardContent(item, cellContent);
					}
					if(undoRedo != null) {
						if(isNewItem) {
							if(!newItemAddedToUndoRedoActions) {
								// add action that removes/adds this item
								void undoAdd()
								{
									if(ItemsSource is IList listSource) {
										listSource.Remove(item);
									} else {
										Items.Remove(item);
									}
								}
								void redoAdd()
								{
									if(ItemsSource is IList listSource) {
										_ = listSource.Add(item);
									} else {
										_ = Items.Add(item);
									}
								}
								undoRedoActions.Add((undoAdd, redoAdd));
								newItemAddedToUndoRedoActions = true;
							}
						} else {
							// editing columns of new items is not needed, when undoing/redoing, the whole object and it's state will be added/removed
							void undo() => binding.SetValueFor(item, oldValue);
							void redo()
							{
								// this is done because column.OnPastingCellClipboardContent triggers editing of cells
								CurrentItem = item;
								isManuallyCommittingEdit = true;
								BeginEditCommand.Execute(null, this);
								pasteValue();
								CommitEditCommand.Execute(null, this);
								isManuallyCommittingEdit = false;
							}
							undoRedoActions.Add((undo, redo));
						}
					}
					pasteValue();
					CommitEditCommand.Execute(null, this);
					isManuallyCommittingEdit = false;

					++cellsPasted;
				}
			}
			if(undoRedo != null) {
				UndoRedo_HandlePaste(undoRedoActions, cellsPasted, newItemsAdded);
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

			List<(Action undo, Action redo)> undoRedoActions = null;
			if(undoRedo != null) {
				undoRedoActions = new List<(Action undo, Action redo)>();
			}

			foreach(DataGridCellInfo cell in selectedCells) {
				DataGridColumn column = cell.Column;
				if(column.IsReadOnly) {
					continue;
				}
				object item = cell.Item;
				object cellContent = null;
				BindingBase binding = column.ClipboardContentBinding;
				if(binding == null) {
					throw new InvalidOperationException($"Column '{column.Header}' is missing '{nameof(column.ClipboardContentBinding)}'.");
				}
				object oldValue = binding.GetValueFor(item, true);
				if(oldValue == cellContent) {
					// nothing to delete
					continue;
				}

				void deleteValue()
				{
					CurrentItem = item;
					isManuallyCommittingEdit = true;
					BeginEditCommand.Execute(null, this);
					// using column.OnPastingCellClipboardContent did not paste on all cells sometimes ... which is weird!
					// that's why I'm setting it manually using binding
					// column.OnPastingCellClipboardContent(item, cellContent);
					binding.SetValueFor(item, cellContent);
					// commit edit 2 times, otherwise there were exceptions being thrown saying that it is still inside a commit transaction ... I don't know why
					CommitEditCommand.Execute(null, this);
					CommitEditCommand.Execute(null, this);
					isManuallyCommittingEdit = false;
				}

				if(undoRedo != null) {
					void undo() => binding.SetValueFor(item, oldValue);
					undoRedoActions.Add((undo, deleteValue));
				}

				deleteValue();
			}
			if(undoRedo != null) {
				UndoRedo_HandleDelete(undoRedoActions);
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
			DataGridCellInfo cell = selectedCells.Single();
			if(cell.Column?.ClipboardContentBinding == null) {
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
				if(column?.ClipboardContentBinding == null) {
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
