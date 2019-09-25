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
Created: 2017-10-30
Author: Grega Mohorko
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
	/// A dialog with a title, message and a progress bar.
	/// <para>Use the <see cref="Updater"/> to update the progress in a background thread.</para>
	/// </summary>
	public partial class ProgressDialog : Dialog
	{
		private readonly Lazy<ProgressUpdater> updater;

		/// <summary>
		/// Initializes a new instance of <see cref="ProgressDialog"/>.
		/// </summary>
		public ProgressDialog()
		{
			InitializeComponent();

			updater = new Lazy<ProgressUpdater>(() => new ProgressUpdater(SetTitle, SetMessage, SetProgress));
		}

		/// <summary>
		/// Gets the <see cref="ProgressUpdater"/> that can be used for updating the values of this progress dialog.
		/// </summary>
		public ProgressUpdater Updater => updater.Value;

		/// <summary>
		/// Shows this progress bar and sets the information accordingly.
		/// </summary>
		/// <param name="titleContent">The title content.</param>
		/// <param name="messageContent">The message content.</param>
		/// <param name="progress">The progress value.</param>
		public void Show(object titleContent, object messageContent = null, double? progress = null)
		{
			SetTitle(titleContent);
			SetMessage(messageContent);
			SetProgress(progress);
			Show();
		}

		/// <summary>
		/// Sets the title to the provided content.
		/// </summary>
		/// <param name="titleContent">The new content of the title.</param>
		public void SetTitle(object titleContent)
		{
			_Label_Title.Content = titleContent;
		}

		/// <summary>
		/// Sets the message to the provided content.
		/// </summary>
		/// <param name="messageContent">The new content of the message.</param>
		public void SetMessage(object messageContent)
		{
			_Label_Message.Content = messageContent;
		}

		/// <summary>
		/// Sets the progress to the specified value. If null, below 0 or above 100, it is put in indeterminate mode.
		/// </summary>
		/// <param name="progress">The new progress value. Should either be null or in the [0-100] range.</param>
		public void SetProgress(double? progress = null)
		{
			if(progress == null || progress < 0 || progress > 100) {
				_ProgressBar.IsIndeterminate = true;
			} else {
				_ProgressBar.IsIndeterminate = false;
				_ProgressBar.Value = progress.Value;
			}
		}

		/// <summary>
		/// Sets the progress to the specified value. If null, below 0 or above 1, it is put in indeterminate mode.
		/// </summary>
		/// <param name="progress">The new progress value. Should either be null or in the [0-1] range.</param>
		public void SetProgress2(double? progress = null)
		{
			SetProgress(progress * 100);
		}
	}
}
