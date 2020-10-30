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
Created: 2019-5-1
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using GM.WPF.MVVM;

namespace GM.WPF.Controls.Dialogs
{
	[Obsolete("Design only.", true)]
	class SearchDialogViewModel : SearchDialogViewModel<string> { }

	class SearchDialogViewModel<T> : ViewModel, IDisposable
	{
		public event EventHandler Submit;

		public RelayCommand Command_Ok { get; private set; }

		public string Title { get; private set; }
		public string SearchText { get; set; }
		public string SearchWatermark { get; private set; }
		public T Selected { get; set; }
		public List<T> Items { get; private set; }

		private readonly string watermark;
		private readonly Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search;
		private readonly AsyncRequestLoader loader;
		private readonly ProgressUpdater progressUpdater;
		private readonly string defaultLoadingMessage;
		private readonly int minSearchTextLength;

		[Obsolete("Design only.", true)]
		protected SearchDialogViewModel()
		{
			Title = "Search and select:";
			SearchWatermark = "Search something ...";
		}

		public SearchDialogViewModel(string title, Func<string, CancellationToken, ProgressUpdater, Task<List<T>>> search, string defaultLoadingMessage, string watermark, int minSearchTextLength, string defaultSearchText, ProgressUpdater progressUpdater)
		{
			Title = title;
			this.search = search;
			this.defaultLoadingMessage = defaultLoadingMessage;
			this.watermark = watermark;
			this.minSearchTextLength = minSearchTextLength;
			this.progressUpdater = progressUpdater;

			Command_Ok = new RelayCommand(Ok, () => Selected != null);

			loader = new AsyncRequestLoader();
			loader.IsLoadingChanged += Loader_IsLoadingChanged;

			SearchWatermark = watermark;
			SearchText = defaultSearchText ?? string.Empty;
		}

		public void Dispose()
		{
			loader.Dispose();
		}

		private void Loader_IsLoadingChanged(object sender, bool isLoading)
		{
			string message = isLoading ? defaultLoadingMessage : null;
			progressUpdater.SetMessage(message);
			progressUpdater.SetProgress(null);
		}

#pragma warning disable IDE0051 // Remove unused private members
		private async void OnSearchTextChanged()
#pragma warning restore IDE0051 // Remove unused private members
		{
			// take care of the watermark
			if(string.IsNullOrEmpty(SearchText)) {
				SearchWatermark = watermark;
			} else {
				SearchWatermark = null;
			}

			await loader.InvokeWhenIfLast(async (CancellationToken ct) =>
			{
				// search text length?
				if(SearchText.Length < minSearchTextLength) {
					Items = null;
				} else {
					Items = await search(SearchText, ct, progressUpdater);
				}
			});
		}

		private void Ok()
		{
			if(Selected == null) {
				return;
			}
			Submit?.Invoke(this, EventArgs.Empty);
		}
	}
}
