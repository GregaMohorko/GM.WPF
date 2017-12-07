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

Project: GM.WPF.Test
Created: 2017-11-26
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GM.WPF.MVVM;

namespace GM.WPF.Test
{
	class MainWindowViewModel : ViewModel
	{
		public bool IsDialogProgressShown { get; set; }

		#region PROGRESS OVERLAY
		public string ProgressOverlay_Message { get; set; }
		private double? _progressOverlay_Value;
		public double? ProgressOverlay_Value
		{
			get => _progressOverlay_Value;
			set
			{
				_progressOverlay_Value = value;
				if(value != null) {
					ProgressOverlay_IsIndeterminate = false;
				}
			}
		}
		private bool _progressOverlay_IsIndeterminate;
		public bool ProgressOverlay_IsIndeterminate
		{
			get => _progressOverlay_IsIndeterminate;
			set
			{
				_progressOverlay_IsIndeterminate = value;
				if(value) {
					ProgressOverlay_Value = null;
				} else if(ProgressOverlay_Value==null) {
					ProgressOverlay_Value = 0;
				}
			}
		}
		#endregion // ProgressOverlay

		public MainWindowViewModel()
		{
			ProgressOverlay_Message = "Loading ...";
		}
	}
}
