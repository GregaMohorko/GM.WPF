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
Created: 2017-11-29
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GM.WPF.Converters;

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// A dialog with a <see cref="ListBox"/> or <see cref="DataGrid"/> where the user can select items from it.
	/// </summary>
	public partial class SelectDialog : TaskDialog
	{
		/// <summary>
		/// Creates a new instance of <see cref="SelectDialog"/>.
		/// </summary>
		public SelectDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Shows the select dialog and waits for the user to select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// <para>This method will create a <see cref="ListBox"/>.</para>
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="message">The message text.</param>
		/// <param name="items">Items to put in the <see cref="ListBox"/> control.</param>
		/// <param name="convertToString">The function that converts every item to a string representation. If null, ToString is used.</param>
		public async Task<List<T>> Show<T>(string message, IEnumerable<T> items, Func<T, string> convertToString = null)
		{
			ListBox listBox = await ShowAndWait(message, items, SelectionMode.Multiple, convertToString);
			if(listBox == null) {
				return null;
			}

			return listBox.SelectedItems.Cast<T>().ToList();
		}

		/// <summary>
		/// Shows the select dialog and waits for the user to select a single item and confirm. If the user cancels the dialog, this method returns default value.
		/// <para>This method will create a <see cref="ListBox"/>.</para>
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="message">The message text.</param>
		/// <param name="items">Items to put in the <see cref="ListBox"/> control.</param>
		/// <param name="convertToString">The function that converts every item to a string representation. If null, ToString is used.</param>
		public async Task<T> ShowSingle<T>(string message, IEnumerable<T> items, Func<T, string> convertToString = null)
		{
			ListBox listBox = await ShowAndWait(message, items, SelectionMode.Single, convertToString);
			if(listBox == null) {
				return default(T);
			}

			return (T)listBox.SelectedItem;
		}

		/// <summary>
		/// Shows the select dialog and waits for the user to select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// <para>This method will create a <see cref="DataGrid"/> with the specified columns.</para>
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="message">The message text.</param>
		/// <param name="items">Items to put in the <see cref="DataGrid"/> control.</param>
		/// <param name="columnHeadersAndBindingPaths">A collection of tuples with a column header and a path for the binding (the name of the property in the item) for that column.</param>
		public async Task<List<T>> Show<T>(string message, IEnumerable<T> items, IEnumerable<Tuple<string, string>> columnHeadersAndBindingPaths)
		{
			// create bindings out of the paths
			var columnHeadersAndBindings = columnHeadersAndBindingPaths.Select(ct => Tuple.Create(ct.Item1, new Binding
			{
				Path = new PropertyPath(ct.Item2),
				Mode = BindingMode.OneWay
			}));

			return await Show(message, items, columnHeadersAndBindings);
		}

		/// <summary>
		/// Shows the select dialog and waits for the user to select possibly multiple items and confirm. If the user cancels the dialog, this method will return null.
		/// <para>This method will create a <see cref="DataGrid"/> with the specified columns.</para>
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="message">The message text.</param>
		/// <param name="items">Items to put in the <see cref="DataGrid"/> control.</param>
		/// <param name="columnHeadersAndBindings">A collection of tuples with a column header and the binding for that column.</param>
		public async Task<List<T>> Show<T>(string message, IEnumerable<T> items, IEnumerable<Tuple<string, Binding>> columnHeadersAndBindings)
		{
			DataGrid dataGrid = await ShowAndWait(message, items, columnHeadersAndBindings, DataGridSelectionMode.Extended);
			if(dataGrid == null) {
				return null;
			}

			return dataGrid.SelectedItems.Cast<T>().ToList();
		}

		/// <summary>
		/// Shows the select dialog and waits for the user to select a single item and confirm. If the user cancels the dialog, this method returns default value.
		/// <para>This method will create a <see cref="DataGrid"/> with the specified columns.</para>
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="message">The message text.</param>
		/// <param name="items">Items to put in the <see cref="DataGrid"/> control.</param>
		/// <param name="columnHeadersAndBindingPaths">A collection of tuples with a column header and a path for the binding (the name of the property in the item) for that column.</param>
		public async Task<T> ShowSingle<T>(string message, IEnumerable<T> items, IEnumerable<Tuple<string, string>> columnHeadersAndBindingPaths)
		{
			// create bindings out of the paths
			var columnHeadersAndBindings = columnHeadersAndBindingPaths.Select(ct => Tuple.Create(ct.Item1, new Binding
			{
				Path = new PropertyPath(ct.Item2),
				Mode = BindingMode.OneWay
			}));

			return await ShowSingle(message, items, columnHeadersAndBindings);
		}

		/// <summary>
		/// Shows the select dialog and waits for the user to select a single item and confirm. If the user cancels the dialog, this method returns default value.
		/// <para>This method will create a <see cref="DataGrid"/> with the specified columns.</para>
		/// </summary>
		/// <typeparam name="T">The type of items.</typeparam>
		/// <param name="message">The message text.</param>
		/// <param name="items">Items to put in the <see cref="DataGrid"/> control.</param>
		/// <param name="columnHeadersAndBindings">A collection of tuples with a column header and the binding for that column.</param>
		public async Task<T> ShowSingle<T>(string message, IEnumerable<T> items, IEnumerable<Tuple<string, Binding>> columnHeadersAndBindings)
		{
			DataGrid dataGrid = await ShowAndWait(message, items, columnHeadersAndBindings, DataGridSelectionMode.Extended);
			if(dataGrid == null) {
				return default(T);
			}

			return (T)dataGrid.SelectedItem;
		}

		private async Task<DataGrid> ShowAndWait<T>(string message, IEnumerable<T> items, IEnumerable<Tuple<string, Binding>> columnHeadersAndBindings, DataGridSelectionMode selectionMode)
		{
			var dataGrid = new DataGrid
			{
				ItemsSource = items,
				SelectionMode = selectionMode,
				AutoGenerateColumns = false,
				IsReadOnly = true
			};
			// create columns
			foreach(Tuple<string, Binding> columnTuple in columnHeadersAndBindings) {
				dataGrid.Columns.Add(new DataGridTextColumn
				{
					Header = columnTuple.Item1,
					Binding = columnTuple.Item2
				});
			}

			bool wasCancelled = await ShowAndWait(message, dataGrid);
			if(wasCancelled) {
				return null;
			}

			return dataGrid;
		}

		private async Task<ListBox> ShowAndWait<T>(string message, IEnumerable<T> items, SelectionMode selectionMode, Func<T, string> convertToString)
		{
			if(convertToString == null) {
				convertToString = (item) => item?.ToString();
			}

			// create the binding for the text
			var textBinding = new Binding
			{
				Converter = new FunctionToStringConverter<T>(convertToString),
				Mode = BindingMode.OneWay
			};

			// create the factory for the data template
			var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
			tbFactory.SetValue(TextBlock.TextProperty, textBinding);

			var listBox = new ListBox
			{
				ItemsSource = items,
				ItemTemplate = new DataTemplate
				{
					DataType = typeof(T),
					VisualTree = tbFactory
				},
				SelectionMode = selectionMode
			};

			bool wasCancelled = await ShowAndWait(message, listBox);
			if(wasCancelled) {
				return null;
			}

			return listBox;
		}

		private async Task<bool> ShowAndWait(string message, Selector content)
		{
			// set binding for the selected item
			content.SetBinding(Selector.SelectedItemProperty, new Binding
			{
				Path = new PropertyPath(nameof(SelectDialogViewModel.SelectedItem)),
				Mode = BindingMode.TwoWay
			});

			_ContentControl.Content = content;

			var vm = new SelectDialogViewModel
			{
				Message = message
			};
			ViewModel = vm;

			bool wasCancelled = await WaitDialog();
			Close();
			return wasCancelled;
		}

		private void Button_OK_Click(object sender, RoutedEventArgs e)
		{
			EndDialog();
		}

		private void Button_Cancel_Click(object sender, RoutedEventArgs e)
		{
			EndDialog(true);
		}
	}
}
