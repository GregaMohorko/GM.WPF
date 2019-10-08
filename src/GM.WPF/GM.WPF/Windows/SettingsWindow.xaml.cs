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
Created: 2019-10-08
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
using GM.WPF.Controls.Dialogs;
using GM.WPF.Windows.Model.SettingsWindow;

namespace GM.WPF.Windows
{
	/// <summary>
	/// A window with configurable settings.
	/// </summary>
	public partial class SettingsWindow : ClosingWindow
	{
		/// <summary>
		/// Creates a new instance of <see cref="SettingsWindow"/>.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public SettingsWindow(Settings settings)
		{
			InitializeComponent();

			var vm = new SettingsWindowViewModel(settings);
			ViewModel = vm;
		}

		/// <summary>
		/// If there are any changes, asks the user if he wants to save them.
		/// </summary>
		public override async Task<bool> CanClose()
		{
			var vm = (SettingsWindowViewModel)ViewModel;
			if(!vm.Settings.IsDirty) {
				return true;
			}
			int? answer = await _DialogPanel.Create<ChooseDialog>().Show($"Do you want to save the changes?", "Yes", "No");
			switch(answer) {
				case null:
					return false;
				case 1:
					return true;
				case 0:
					vm.Save();
					return true;
				default:
					throw new NotImplementedException();
			}
		}
	}
}
