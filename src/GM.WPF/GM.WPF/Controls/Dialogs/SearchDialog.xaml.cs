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
Created: 2019-5-1
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// A dialog with a search box where the user can search for certain items. <see cref="DataGrid"/> is used for presenting items.
	/// <para>Everything is done on the UI thread.</para>
	/// </summary>
	public partial class SearchDialog : TaskDialog
	{
		/// <summary>
		/// Creates a new instance of <see cref="SearchDialog"/>.
		/// </summary>
		public SearchDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The function that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeader">The header of the only column in the <see cref="DataGrid"/> that displays the items.</param>
		/// <param name="defaultLoadingMessage">The text to show to the user while load is in process. Can be overriden by the progress updater in the search function.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute. Can be zero.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public Task<List<T>> Show<T>(string title, Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search, string columnHeader = "Items", string defaultLoadingMessage = "Loading ...", string watermark = "Search text ...", int minSearchTextLength = 4, string defaultSearchText = null)
		{
			// create the binding out of the column header
			var columns = new List<(string Header, Binding)> { (columnHeader, new Binding { Mode = BindingMode.OneWay }) };

			return Show(title, search, columns, defaultLoadingMessage, watermark, minSearchTextLength, defaultSearchText);
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The function that gets the search text and returns the results asynchronously.</param>
		/// <param name="columns">A collection of tuples with a column header and a path for the binding (the name of the property in the item) for that column.</param>
		/// <param name="defaultLoadingMessage">The text to show to the user while load is in process. Can be overriden by the progress updater in the search function.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute. Can be zero.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public Task<List<T>> Show<T>(string title, Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search, ICollection<(string Header, string Path)> columns, string defaultLoadingMessage = "Loading ...", string watermark = "Search text ...", int minSearchTextLength = 4, string defaultSearchText = null)
		{
			// create bindings out of the paths
			var columnsWithBindings = columns.Select(ct => (ct.Header, new Binding
			{
				Path = new PropertyPath(ct.Path),
				Mode = BindingMode.OneWay
			})).ToList();

			return Show(title, search, columnsWithBindings, defaultLoadingMessage, watermark, minSearchTextLength, defaultSearchText);
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The function that gets the search text and returns the results asynchronously.</param>
		/// <param name="columns">A collection of tuples with a column header and the binding for that column.</param>
		/// <param name="defaultLoadingMessage">The text to show to the user while load is in process. Can be overriden by the progress updater in the search function.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute. Can be zero.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public async Task<List<T>> Show<T>(string title, Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search, ICollection<(string Header, Binding)> columns, string defaultLoadingMessage = "Loading ...", string watermark = "Search text ...", int minSearchTextLength = 4, string defaultSearchText = null)
		{
			bool wasCancelled = await ShowAndWait(title, search, columns, DataGridSelectionMode.Extended, defaultLoadingMessage, watermark, minSearchTextLength, defaultSearchText);
			if(wasCancelled) {
				return null;
			}

			return _DataGrid.SelectedItems.Cast<T>().ToList();
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select a single item and confirm. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The function that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeader">The header of the only column in the <see cref="DataGrid"/> that displays the items.</param>
		/// <param name="defaultLoadingMessage">The text to show to the user while load is in process. Can be overriden by the progress updater in the search function.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute. Can be zero.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public Task<T> ShowSingle<T>(string title, Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search, string columnHeader = "Items", string defaultLoadingMessage = "Loading ...", string watermark = "Search text ...", int minSearchTextLength = 4, string defaultSearchText = null)
		{
			// create the binding out of the column header
			var columns = new List<(string Header, Binding)> { (columnHeader, new Binding { Mode = BindingMode.OneWay }) };

			return ShowSingle(title, search, columns, defaultLoadingMessage, watermark, minSearchTextLength, defaultSearchText);
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select a single item and confirm. If the user cancels the dialog, this method returns default value.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The function that gets the search text and returns the results asynchronously.</param>
		/// <param name="columns">A collection of tuples with a column header and a path for the binding (the name of the property in the item) for that column.</param>
		/// <param name="defaultLoadingMessage">The text to show to the user while load is in process. Can be overriden by the progress updater in the search function.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute. Can be zero.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public Task<T> ShowSingle<T>(string title, Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search, ICollection<(string Header, string Path)> columns, string defaultLoadingMessage = "Loading ...", string watermark = "Search text ...", int minSearchTextLength = 4, string defaultSearchText = null)
		{
			// create bindings out of the paths
			var columnsWithBindings = columns.Select(ct => (ct.Header, new Binding
			{
				Path = new PropertyPath(ct.Path),
				Mode = BindingMode.OneWay
			})).ToList();

			return ShowSingle(title, search, columnsWithBindings, defaultLoadingMessage, watermark, minSearchTextLength, defaultSearchText);
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select a single item and confirm. If the user cancels the dialog, this method returns default value.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The function that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeadersAndBindings">A collection of tuples with a column header and the binding for that column.</param>
		/// <param name="defaultLoadingMessage">The text to show to the user while load is in process. Can be overriden by the progress updater in the search function.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute. Can be zero.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public async Task<T> ShowSingle<T>(string title, Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search, ICollection<(string Header, Binding)> columnHeadersAndBindings, string defaultLoadingMessage = "Loading ...", string watermark = "Search text ...", int minSearchTextLength = 4, string defaultSearchText = null)
		{
			bool wasCancelled = await ShowAndWait(title, search, columnHeadersAndBindings, DataGridSelectionMode.Single, defaultLoadingMessage, watermark, minSearchTextLength, defaultSearchText);
			if(wasCancelled) {
				return default;
			}
			return (T)_DataGrid.SelectedItem;
		}

		private async Task<bool> ShowAndWait<T>(string title, Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search, ICollection<(string Header, Binding)> columnHeadersAndBindings, DataGridSelectionMode selectionMode, string defaultLoadingMessage, string watermark, int minSearchTextLength, string defaultSearchText)
		{
			_DataGrid.SelectionMode = selectionMode;

			// create columns
			_DataGrid.Columns.Clear();
			foreach((string header, Binding binding) in columnHeadersAndBindings) {
				_DataGrid.Columns.Add(new DataGridTextColumn
				{
					Header = header,
					Binding = binding
				});
			}

			var vm = new SearchDialogViewModel<T>(title, search, defaultLoadingMessage, watermark, minSearchTextLength, defaultSearchText, _ProgressOverlay.Updater);
			vm.Submit += delegate
			{
				EndDialog();
			};
			ViewModel = vm;

			bool wasCancelled = await WaitDialog();
			Close();
			return wasCancelled;
		}

		private void Button_Cancel_Click(object sender, RoutedEventArgs e)
		{
			EndDialog(true);
		}
	}
}
