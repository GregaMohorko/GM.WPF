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
Created: 2017-11-10
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using GM.WPF.Windows;

namespace GM.WPF.Test
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : BaseWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			var vm = new MainWindowViewModel();
			vm.PropertyChanged += Vm_PropertyChanged;
			ViewModel = vm;
		}

		private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (MainWindowViewModel)ViewModel;

			switch(e.PropertyName) {
				case nameof(MainWindowViewModel.IsDialogProgressShown):
					if(vm.IsDialogProgressShown) {
						Random r = new Random();
						_ProgressDialog.Show("Title content.", "Message content.", r.NextDouble() * 130);
					} else {
						_ProgressDialog.Hide();
					}
					break;
			}
		}
	}
}
