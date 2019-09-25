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
Created: 2019-09-25
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GM.WPF
{
	/// <summary>
	/// This class can be used for updating progress from a background thread.
	/// </summary>
	public class ProgressUpdater
	{
		private readonly Action<object> titleContentSetter;
		private readonly Action<object> messageContentSetter;
		private readonly Action<double?> progressSetter;

		/// <summary>
		/// Creates a new instance of <see cref="ProgressUpdater"/>. All the setters will be invoked on the UI thread.
		/// <para>At least one parameter must not be null.</para>
		/// </summary>
		/// <param name="titleContentSetter">A method that sets the content of the title.</param>
		/// <param name="messageContentSetter">A method that sets the content of the message.</param>
		/// <param name="progressSetter">A method that sets the progress. The values will be between 0 and 100.</param>
		public ProgressUpdater(Action<object> titleContentSetter, Action<object> messageContentSetter, Action<double?> progressSetter)
		{
			if(titleContentSetter == null && messageContentSetter == null && progressSetter == null) {
				throw new ArgumentNullException("At least one parameter must not be null.", (Exception)null);
			}
			this.titleContentSetter = titleContentSetter;
			this.messageContentSetter = messageContentSetter;
			this.progressSetter = progressSetter;
		}

		/// <summary>
		/// Determines whether a setter for the title content exists.
		/// </summary>
		public bool IsTitleEnabled => titleContentSetter != null;
		/// <summary>
		/// Determines whether a setter for the message content exists.
		/// </summary>
		public bool IsMessageEnabled => messageContentSetter != null;
		/// <summary>
		/// Determines whether a setter for the progress exists.
		/// </summary>
		public bool IsProgressEnabled => progressSetter != null;

		/// <summary>
		/// Updates the title content, message content and progress. The progress value should be between 0 and 100.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		/// <param name="messageContent">The message content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set(object titleContent, object messageContent, double? progress)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				titleContentSetter?.Invoke(titleContent);
				messageContentSetter?.Invoke(messageContent);
				progressSetter?.Invoke(progress);
			});
		}

		/// <summary>
		/// Updates the title content, message content and progress. The progress value should be between 0 and 1.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		/// <param name="messageContent">The message content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set2(object titleContent, object messageContent, double? progress)
		{
			Set(titleContent, messageContent, progress * 100);
		}

		/// <summary>
		/// Updates the message content and progress. The progress value should be between 0 and 100.
		/// </summary>
		/// <param name="messageContent">The message content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set(object messageContent, double? progress)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				messageContentSetter?.Invoke(messageContent);
				progressSetter?.Invoke(progress);
			});
		}

		/// <summary>
		/// Updates the message content and progress. The progress value should be between 0 and 1.
		/// </summary>
		/// <param name="messageContent">The message content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set2(object messageContent, double? progress)
		{
			Set(messageContent, progress * 100);
		}

		/// <summary>
		/// Updates the title content and progress. The progress value should be between 0 and 100.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set3(object titleContent, double? progress)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				titleContentSetter?.Invoke(titleContent);
				progressSetter?.Invoke(progress);
			});
		}

		/// <summary>
		/// Updates the title content and progress. The progress value should be between 0 and 1.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set4(object titleContent, double? progress)
		{
			Set3(titleContent, progress * 100);
		}

		/// <summary>
		/// Updates the title content.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		public void SetTitle(object titleContent)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				titleContentSetter?.Invoke(titleContent);
			});
		}

		/// <summary>
		/// Updates the message content.
		/// </summary>
		/// <param name="messageContent">The message content to set.</param>
		public void SetMessage(object messageContent)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				messageContentSetter?.Invoke(messageContent);
			});
		}

		/// <summary>
		/// Updates the progress. The value should be between 0 and 100.
		/// </summary>
		/// <param name="progress">The progress to set.</param>
		public void SetProgress(double? progress)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				progressSetter?.Invoke(progress);
			});
		}

		/// <summary>
		/// Updates the progress. The value should be between 0 and 1.
		/// </summary>
		/// <param name="progress">The progress to set.</param>
		public void SetProgress2(double? progress)
		{
			SetProgress(progress * 100);
		}
	}
}
