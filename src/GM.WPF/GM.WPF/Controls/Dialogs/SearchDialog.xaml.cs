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
		/// <param name="search">The method that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeader">The header of the only column in the <see cref="DataGrid"/> that displays the items.</param>
		/// <param name="loadingMessage">The text to show to the user while load is in process.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="msTimeout">The amount of milliseconds to wait before loading is executed after the user inputs a new character into the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public Task<List<T>> Show<T>(string title, Func<string, Task<List<T>>> search, string columnHeader = "Items", string loadingMessage = "Loading ...", string watermark = "Search text ...", int msTimeout = 300, int minSearchTextLength = 4, string defaultSearchText = null)
		{
			// create the binding out of the column header
			var columnHeadersAndBindings = new List<Tuple<string, Binding>> { Tuple.Create(columnHeader, new Binding { Mode=BindingMode.OneWay }) };

			return Show(title, search, columnHeadersAndBindings, loadingMessage, watermark, msTimeout, minSearchTextLength, defaultSearchText);
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The method that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeadersAndBindingPaths">A collection of tuples with a column header and a path for the binding (the name of the property in the item) for that column.</param>
		/// <param name="loadingMessage">The text to show to the user while load is in process.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="msTimeout">The amount of milliseconds to wait before loading is executed after the user inputs a new character into the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public Task<List<T>> Show<T>(string title, Func<string, Task<List<T>>> search, ICollection<Tuple<string, string>> columnHeadersAndBindingPaths, string loadingMessage = "Loading ...", string watermark = "Search text ...", int msTimeout = 300, int minSearchTextLength = 4, string defaultSearchText = null)
		{
			// create bindings out of the paths
			var columnHeadersAndBindings = columnHeadersAndBindingPaths.Select(ct => Tuple.Create(ct.Item1, new Binding
			{
				Path = new PropertyPath(ct.Item2),
				Mode = BindingMode.OneWay
			})).ToList();

			return Show(title, search, columnHeadersAndBindings, loadingMessage, watermark, msTimeout, minSearchTextLength, defaultSearchText);
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The method that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeadersAndBindings">A collection of tuples with a column header and the binding for that column.</param>
		/// <param name="loadingMessage">The text to show to the user while load is in process.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="msTimeout">The amount of milliseconds to wait before loading is executed after the user inputs a new character into the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public async Task<List<T>> Show<T>(string title, Func<string, Task<List<T>>> search, ICollection<Tuple<string, Binding>> columnHeadersAndBindings, string loadingMessage = "Loading ...", string watermark = "Search text ...", int msTimeout = 300, int minSearchTextLength = 4, string defaultSearchText = null)
		{
			bool wasCancelled = await ShowAndWait(title, search, columnHeadersAndBindings, DataGridSelectionMode.Extended, loadingMessage, watermark, msTimeout, minSearchTextLength, defaultSearchText);
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
		/// <param name="search">The method that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeader">The header of the only column in the <see cref="DataGrid"/> that displays the items.</param>
		/// <param name="loadingMessage">The text to show to the user while load is in process.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="msTimeout">The amount of milliseconds to wait before loading is executed after the user inputs a new character into the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public Task<T> ShowSingle<T>(string title, Func<string, Task<List<T>>> search, string columnHeader = "Items", string loadingMessage = "Loading ...", string watermark = "Search text ...", int msTimeout = 300, int minSearchTextLength = 4, string defaultSearchText = null)
		{
			// create the binding out of the column header
			var columnHeadersAndBindings = new List<Tuple<string, Binding>> { Tuple.Create(columnHeader, new Binding { Mode = BindingMode.OneWay }) };

			return ShowSingle(title, search, columnHeadersAndBindings, loadingMessage, watermark, msTimeout, minSearchTextLength, defaultSearchText);
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select a single item and confirm. If the user cancels the dialog, this method returns default value.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The method that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeadersAndBindingPaths">A collection of tuples with a column header and a path for the binding (the name of the property in the item) for that column.</param>
		/// <param name="loadingMessage">The text to show to the user while load is in process.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="msTimeout">The amount of milliseconds to wait before loading is executed after the user inputs a new character into the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public Task<T> ShowSingle<T>(string title, Func<string, Task<List<T>>> search, ICollection<Tuple<string, string>> columnHeadersAndBindingPaths, string loadingMessage = "Loading ...", string watermark = "Search text ...", int msTimeout = 300, int minSearchTextLength = 4, string defaultSearchText = null)
		{
			// create bindings out of the paths
			var columnHeadersAndBindings = columnHeadersAndBindingPaths.Select(ct => Tuple.Create(ct.Item1, new Binding
			{
				Path = new PropertyPath(ct.Item2),
				Mode = BindingMode.OneWay
			})).ToList();

			return ShowSingle(title, search, columnHeadersAndBindings, loadingMessage, watermark, msTimeout, minSearchTextLength, defaultSearchText);
		}

		/// <summary>
		/// Shows the search dialog and waits for the user to search and select a single item and confirm. If the user cancels the dialog, this method returns default value.
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="title">The title text.</param>
		/// <param name="search">The method that gets the search text and returns the results asynchronously.</param>
		/// <param name="columnHeadersAndBindings">A collection of tuples with a column header and the binding for that column.</param>
		/// <param name="loadingMessage">The text to show to the user while load is in process.</param>
		/// <param name="watermark">The text to show in the search box.</param>
		/// <param name="msTimeout">The amount of milliseconds to wait before loading is executed after the user inputs a new character into the search box.</param>
		/// <param name="minSearchTextLength">The minimum length of the search text for which the loading will execute.</param>
		/// <param name="defaultSearchText">Default search text to set when this dialog shows.</param>
		public async Task<T> ShowSingle<T>(string title, Func<string, Task<List<T>>> search, ICollection<Tuple<string, Binding>> columnHeadersAndBindings, string loadingMessage = "Loading ...", string watermark = "Search text ...", int msTimeout = 300, int minSearchTextLength = 4, string defaultSearchText = null)
		{
			bool wasCancelled = await ShowAndWait(title, search, columnHeadersAndBindings, DataGridSelectionMode.Single, loadingMessage, watermark, msTimeout, minSearchTextLength, defaultSearchText);
			if(wasCancelled) {
				return default(T);
			}
			return (T)_DataGrid.SelectedItem;
		}

		private async Task<bool> ShowAndWait<T>(string title, Func<string, Task<List<T>>> search, ICollection<Tuple<string, Binding>> columnHeadersAndBindings, DataGridSelectionMode selectionMode, string loadingMessage, string watermark, int msTimeout, int minSearchTextLength, string defaultSearchText)
		{
			_DataGrid.SelectionMode = selectionMode;

			// create columns
			_DataGrid.Columns.Clear();
			foreach(Tuple<string, Binding> columnTuple in columnHeadersAndBindings) {
				_DataGrid.Columns.Add(new DataGridTextColumn
				{
					Header = columnTuple.Item1,
					Binding = columnTuple.Item2
				});
			}

			var vm = new SearchDialogViewModel<T>(title, search, loadingMessage, watermark, msTimeout, minSearchTextLength, defaultSearchText);
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
