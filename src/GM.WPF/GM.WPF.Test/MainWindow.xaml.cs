﻿/*
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

Project: GM.WPF.Test
Created: 2017-11-10
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using GM.WPF.Controls;
using GM.WPF.Controls.Dialogs;
using GM.WPF.Windows;

namespace GM.WPF.Test
{
	public partial class MainWindow : BaseWindow, IDisposable
	{
		public MainWindow()
		{
			InitializeComponent();

			var vm = new MainWindowViewModel();
			ViewModel = vm;
		}

		public bool IsDisposed { get; private set; }
		public void Dispose()
		{
			Debug.Assert(!IsDisposed);
			IsDisposed = true;
		}

		#region MAINMENU

		#region DIALOGS
		private async void MenuItem_Dialogs_Progress_Click(object sender, RoutedEventArgs e)
		{
			const int msTime = 3000;
			_ProgressDialog.Show($"Showing for {msTime} milliseconds", null, 0);
			ProgressUpdater progressUpdater = _ProgressDialog.Updater;
			await Task.Run(async delegate
			{
				int progressSteps = 100;
				int msStep = (int)Math.Round(msTime / (double)progressSteps);
				progressUpdater.StartNewLoop(progressSteps);
				for(int i = 0; i < progressSteps; ++i) {
					progressUpdater.SetForLoop(i, true);
					await Task.Delay(msStep);
				}
			});
			_ProgressDialog.Hide();
		}

		private async void MenuItem_Dialogs_Message_Normal_Click(object sender, RoutedEventArgs e)
		{
			// using the DialogPanel
			MessageDialog messageDialog = _DialogPanel.Create<MessageDialog>();
			await messageDialog.Show("This is a normal message dialog created using the MessageDialog.");

			// using the manual reusable dialog
			// await _MessageDialog.Show("This is a normal message dialog created using the manual reusable dialog.");
		}

		private async void MenuItem_Dialogs_Message_Warning_Click(object sender, RoutedEventArgs e)
		{
			await _MessageDialog.Show("This is a warning message created using the manual reusable dialog.", MessageType.WARNING);
		}

		private async void MenuItem_Dialogs_Message_Error_Click(object sender, RoutedEventArgs e)
		{
			await _MessageDialog.Show("This is an error message.", MessageType.ERROR);
		}

		private void MenuItem_Dialogs_Input_Empty_Click(object sender, RoutedEventArgs e)
		{
			ShowAndCheckInputDialog(null, null, null);
		}

		private void MenuItem_Dialogs_Input_Message_Click(object sender, RoutedEventArgs e)
		{
			ShowAndCheckInputDialog("This is an input dialog with a message.", null, null);
		}

		private void MenuItem_Dialogs_Input_MessageWatermark_Click(object sender, RoutedEventArgs e)
		{
			ShowAndCheckInputDialog("This is an input dialog with a message and a watermark.", "This is the watermark ...", null);
		}

		private void MenuItem_Dialogs_Input_MessageWatermarkDefaulttext_Click(object sender, RoutedEventArgs e)
		{
			ShowAndCheckInputDialog("This is an input dialog with a message, a watermark and some default text.", "This is the watermark ...", "Default text");
		}

		private void MenuItem_Dialogs_Input_Watermark_Click(object sender, RoutedEventArgs e)
		{
			ShowAndCheckInputDialog(null, "This is the watermark ...", null);
		}

		private void MenuItem_Dialogs_Input_WatermarkDefaulttext_Click(object sender, RoutedEventArgs e)
		{
			ShowAndCheckInputDialog(null, "This is the watermark ...", "Default text");
		}

		private void MenuItem_Dialogs_Input_Defaulttext_Click(object sender, RoutedEventArgs e)
		{
			ShowAndCheckInputDialog(null, null, "Default text");
		}

		private async void ShowAndCheckInputDialog(string message, string watermark, string defaultText)
		{
			string input = await _InputDialog.Show(message, watermark, defaultText);
			if(input == null) {
				MessageBox.Show("Cancelled.");
				return;
			}

			MessageBox.Show($"Your input: '{input}'");
		}

		private async void MenuItem_Dialogs_Choose_Click(object sender, RoutedEventArgs e)
		{
			string[] possibleAnswers = { "Star trek", "42", "Bazinga", "Artjom" };
			int? answer = await _ChooseDialog.Show("Choose your answer:",possibleAnswers);
			if(answer == null) {
				MessageBox.Show("Cancelled.");
				return;
			}

			MessageBox.Show($"You chose a button with the index '{answer}', which is '{possibleAnswers[(int)answer]}'.");
		}

		private async void MenuItem_Dialogs_Select_Multiple_Click(object sender, RoutedEventArgs e)
		{
			string[] items = { "Item 1", "Item 02", "Item 003", "Item 0004", "Item 00005", "Item 000006", "Item 0000007" };
			List<string> selectedItems = (await _SelectDialog.Show("Select some items below:", items))?.ToList();
			if(selectedItems == null) {
				MessageBox.Show("Cancelled.");
				return;
			}

			MessageBox.Show($"You selected {selectedItems.Count} items.{Environment.NewLine}Selected items:{Environment.NewLine}{string.Join(Environment.NewLine, selectedItems)}");
		}

		private async void MenuItem_Dialogs_Select_Single_Click(object sender, RoutedEventArgs e)
		{
			string[] items = { "Item 1", "Item 02", "Item 003", "Item 0004", "Item 00005", "Item 000006", "Item 0000007" };
			string selectedItem = await _SelectDialog.ShowSingle("Select an item below:", items);
			if(selectedItem == null) {
				MessageBox.Show("Cancelled.");
				return;
			}

			MessageBox.Show($"You selected '{selectedItem}'.");
		}

		private async void MenuItem_Dialogs_Search_Multiple_Click(object sender, RoutedEventArgs e)
		{
			List<string> selectedItems = await _DialogPanel.Create<SearchDialog>().Show("Search and select multiple items (default settings):", SearchDialogLoadingFunction);
			if(selectedItems == null) {
				MessageBox.Show("Cancelled.");
				return;
			}

			MessageBox.Show($"You selected {selectedItems.Count} items.{Environment.NewLine}Selected items:{Environment.NewLine}{string.Join(Environment.NewLine, selectedItems)}");
		}

		private async void MenuItem_Dialogs_Search_Single_Click(object sender, RoutedEventArgs e)
		{
			string selectedItem = await _DialogPanel.Create<SearchDialog>().ShowSingle("Search and select a single item (default settings):", SearchDialogLoadingFunction);
			if(selectedItem == null) {
				MessageBox.Show("Cancelled.");
				return;
			}

			MessageBox.Show($"You selected '{selectedItem}'.");
		}

		private Random rand = new Random();

		private async Task<List<string>> SearchDialogLoadingFunction(string searchText, CancellationToken ct, ProgressUpdater progressUpdater)
		{
			// simulate one second of loading
			await Task.Delay(1000, ct);

			// generate some random items
			int count = rand.Next(2, 30);
			var items = new List<string>(count);
			progressUpdater?.StartNewLoop(count);
			for(int i = count - 1; i >= 0; --i) {
				progressUpdater?.SetForLoop(i);
				items.Add($"{rand.Next(100)}-{searchText}");
				ct.ThrowIfCancellationRequested();
			}
			return items;
		}
		#endregion // Dialogs

		#endregion // MainMenu
	}
}
