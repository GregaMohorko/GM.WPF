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

namespace GM.WPF.Windows.Model.SettingsWindow
{
	/// <summary>
	/// A settings model to be passed to <see cref="Windows.SettingsWindow"/>.
	/// </summary>
	public class Settings : ObservableObject
	{
		/// <summary>
		/// Event that is fired after the save command is executed.
		/// </summary>
		public event EventHandler Saved;

		/// <summary>
		/// The name of the application.
		/// </summary>
		public string ClientName { get; private set; }

		/// <summary>
		/// Setting tabs.
		/// </summary>
		public ObservableCollection<SettingsTab> Tabs { get; private set; }

		/// <summary>
		/// Determines whether any setting is changed.
		/// </summary>
		public bool IsDirty => Tabs.Any(st => st.IsDirty);

		private readonly List<Action> saveActions;

		/// <summary>
		/// Creates a new instance of <see cref="Settings"/>.
		/// </summary>
		/// <param name="clientName">The name of the application.</param>
		public Settings(string clientName)
		{
			ClientName = clientName;
			Tabs = new ObservableCollection<SettingsTab>();
			Tabs.CollectionChanged += Tabs_CollectionChanged;
			saveActions = new List<Action>();
		}

		private void Tabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// old tabs?
			switch(e.Action) {
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					foreach(SettingsTab settingsTab in e.OldItems) {
						settingsTab.PropertyChanged -= SettingsTab_PropertyChanged;
					}
					break;
			}

			// new tabs?
			switch(e.Action) {
				case NotifyCollectionChangedAction.Add:
					foreach(SettingsTab settingsTab in e.NewItems) {
						settingsTab.PropertyChanged += SettingsTab_PropertyChanged;
					}
					break;
			}
		}

		private void SettingsTab_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(SettingsTab.IsDirty):
					OnPropertyChanged(nameof(IsDirty));
					break;
			}
		}

		/// <summary>
		/// Saves everything and invokes the <see cref="Saved"/> event.
		/// <para>Applies all the changed values.</para>
		/// <para>Invokes all the save methods that were provided when creating setting factories.</para>
		/// </summary>
		public void Save()
		{
			foreach(SettingsTab settingsTab in Tabs) {
				foreach(ISettingsUI settingsUI in settingsTab.Children) {
					settingsUI.Apply();
				}
			}
			foreach(Action saveAction in saveActions) {
				saveAction();
			}
			OnPropertyChanged(nameof(IsDirty));
			Saved?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Creates a new settings factory, used to create <see cref="SettingControl{T}"/> controls.
		/// </summary>
		/// <param name="valueGetter">The method that gets the value of a single setting.</param>
		/// <param name="applyMethod">The method that applies the value of a single setting. This method should not save anything.</param>
		/// <param name="save">The method that saves the current values of settings created with this factory.</param>
		public SettingsFactory CreateFactory(Func<string, object> valueGetter, Action<string, object> applyMethod, Action save)
		{
			if(valueGetter == null) {
				throw new ArgumentNullException(nameof(valueGetter));
			}
			if(applyMethod == null) {
				throw new ArgumentNullException(nameof(applyMethod));
			}
			if(save == null) {
				throw new ArgumentNullException(nameof(save));
			}

			saveActions.Add(save);
			return new SettingsFactory(valueGetter, applyMethod);
		}

		/// <summary>
		/// Creates a new settings factory that can only create read-only <see cref="SettingControl{T}"/> controls.
		/// </summary>
		/// <param name="valueGetter">The method that gets the value of a single setting.</param>
		public SettingsFactory CreateFactoryReadOnly(Func<string, object> valueGetter)
		{
			return new SettingsFactory(valueGetter, null);
		}

		/// <summary>
		/// Creates a new <see cref="SettingsNote"/>.
		/// </summary>
		/// <param name="text">The text to be displayed.</param>
		public SettingsNote CreateNote(string text)
		{
			return CreateNote(null, text);
		}

		/// <summary>
		/// Creates a new <see cref="SettingsNote"/> with a title.
		/// </summary>
		/// <param name="title">The title of the note.</param>
		/// <param name="text">The text to be displayed.</param>
		public SettingsNote CreateNote(string title, string text)
		{
			return new SettingsNote(title, text);
		}
	}
}
