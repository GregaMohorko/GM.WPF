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
using GalaSoft.MvvmLight.CommandWpf;
using GM.WPF.MVVM;

namespace GM.WPF.Controls.Dialogs
{
	[Obsolete("Design only.", true)]
	class SearchDialogViewModel:SearchDialogViewModel<string> { }

	class SearchDialogViewModel<T>:ViewModel
	{
		public event EventHandler Submit;

		public RelayCommand Command_Ok { get; private set; }

		public string Title { get; private set; }
		public string SearchText { get; set; }
		public string SearchWatermark { get; private set; }
		public T Selected { get; set; }
		public string OverlayMessage { get; set; }
		public List<T> Items { get; private set; }

		private int lastSearchCounter;

		private readonly string watermark;
		private readonly Func<string, Task<List<T>>> search;
		private readonly string loadingMessage;
		private readonly int msTimeout;
		private readonly int minSearchTextLength;

		[Obsolete("Design only.", true)]
		protected SearchDialogViewModel()
		{
			Title = "Search and select:";
			SearchWatermark = "Search something ...";
			OverlayMessage = "";
		}

		public SearchDialogViewModel(string title, Func<string, Task<List<T>>> search, string loadingMessage, string watermark, int msTimeout, int minSearchTextLength, string defaultSearchText)
		{
			Title = title;
			this.search = search;
			this.loadingMessage = loadingMessage;
			this.watermark = watermark;
			this.msTimeout = msTimeout;
			this.minSearchTextLength = minSearchTextLength;

			Command_Ok = new RelayCommand(Ok, () => Selected != null);

			SearchWatermark = watermark;
			SearchText = defaultSearchText;
		}

		private async void OnSearchTextChanged()
		{
			// take care of the watermark
			if(string.IsNullOrEmpty(SearchText)) {
				SearchWatermark = watermark;
			} else {
				SearchWatermark = null;
			}

			int myCounter = ++lastSearchCounter;
			string mySearchText = SearchText;

			// search text length?
			if(SearchText.Length < minSearchTextLength) {
				Items = null;
				OverlayMessage = null;
				return;
			}

			OverlayMessage = loadingMessage;

			// timeout?
			if(msTimeout > 0) {
				await Task.Delay(msTimeout);
				if(myCounter != lastSearchCounter) {
					// search text has changed in the meantime
					return;
				}
			}

			// do the search
			List<T> items = await search(mySearchText);
			if(myCounter != lastSearchCounter) {
				// search text has changed in the meantime
				return;
			}
			Items = items;
			OverlayMessage = null;
		}

		private void Ok()
		{
			Submit?.Invoke(this, EventArgs.Empty);
		}
	}
}
