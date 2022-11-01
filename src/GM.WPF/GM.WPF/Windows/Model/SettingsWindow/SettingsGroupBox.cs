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

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GM.WPF.Windows.Model.SettingsWindow
{
	/// <summary>
	/// Will display a <see cref="GroupBox"/>.
	/// </summary>
	public class SettingsGroupBox : ObservableObject, ISettingsUIPanel, ISettingsUI
	{
		/// <summary>
		/// The title of the group.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// A collection of child settings.
		/// </summary>
		public ObservableCollection<ISettingsUI> Children { get; private set; }

		/// <summary>
		/// Determines if any of the child settings is changed.
		/// </summary>
		public bool IsDirty => Children.Any(isu => isu.IsDirty);

		/// <summary>
		/// Determines if all child settings are read-only.
		/// </summary>
		public bool IsReadOnly => Children.All(isu => isu.IsReadOnly);

		/// <summary>
		/// Creates a new instance of <see cref="SettingsGroupBox"/>.
		/// </summary>
		/// <param name="title">The title of the group.</param>
		public SettingsGroupBox(string title)
		{
			Name = title;
			Children = new ObservableCollection<ISettingsUI>();
			Children.CollectionChanged += Children_CollectionChanged;
		}

		private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// old settings?
			switch(e.Action) {
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					foreach(ISettingsUI settingsUI in e.OldItems) {
						settingsUI.PropertyChanged -= SettingsUI_PropertyChanged;
					}
					break;
			}

			// new settings?
			switch(e.Action) {
				case NotifyCollectionChangedAction.Add:
					foreach(ISettingsUI settingsUI in e.NewItems) {
						settingsUI.PropertyChanged += SettingsUI_PropertyChanged;
					}
					break;
			}
		}

		private void SettingsUI_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(ISettingsUI.IsDirty):
					OnPropertyChanged(nameof(IsDirty));
					break;
			}
		}

		/// <summary>
		/// Applies the current values to all the child settings.
		/// </summary>
		public void Apply()
		{
			foreach(ISettingsUI settingsUI in Children) {
				settingsUI.Apply();
			}
		}
	}
}
