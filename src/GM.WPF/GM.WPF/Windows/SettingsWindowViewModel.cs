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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using GM.WPF.MVVM;
using GM.WPF.Windows.Model.SettingsWindow;

namespace GM.WPF.Windows
{
	class SettingsWindowViewModel : ViewModel
	{
		public RelayCommand Command_Save { get; private set; }

		public string Title => $"Settings | {Settings.ClientName}";
		public Settings Settings { get; private set; }
		public SettingsTab SelectedTab { get; set; }

		[Obsolete("Design only.", true)]
		public SettingsWindowViewModel()
		{
			Settings = new Settings("My Application");
			Settings.Tabs.Add(new SettingsTab("General"));
			Settings.Tabs.Add(new SettingsTab("Security"));
			Settings.Tabs.Add(new SettingsTab("Advanced"));
			SelectedTab = Settings.Tabs.First();
			var group = new SettingsGroupBox("A group of settings");
			SelectedTab.Children.Add(group);
			group.Children.Add(new StringSettingControl("Name of setting", null, null, null));
			group.Children.Add(new StringSettingControl("Read-only", null, null, null, true));
		}

		public SettingsWindowViewModel(Settings settings)
		{
			Command_Save = new RelayCommand(Save, () => Settings.IsDirty);

			Settings = settings;
			settings.PropertyChanged += Settings_PropertyChanged;
			SelectedTab = settings.Tabs.FirstOrDefault();
		}

		private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(Settings.IsDirty):
					Command_Save.RaiseCanExecuteChanged();
					break;
			}
		}

		public void Save()
		{
			Settings.Save();
		}
	}
}
