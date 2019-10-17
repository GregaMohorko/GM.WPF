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
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace GM.WPF.Windows.Model.SettingsWindow
{
	/// <summary>
	/// A basic string setting control with a TextBox.
	/// </summary>
	public class StringSettingControl : SettingControl<string>
	{
		internal StringSettingControl(string name, string propertyPath, Action<string, string> applyMethod, string originalValue, bool isReadOnly = false) : base(name, propertyPath, applyMethod, originalValue, isReadOnly) { }
	}

	/// <summary>
	/// A string setting control with a TextBox and a "Browse..." button.
	/// </summary>
	public class DirectoryPathSettingControl : SettingControl<string>
	{
		/// <summary>
		/// The command for the Browse... button.
		/// </summary>
		public RelayCommand Command_Browse { get; private set; }

		internal DirectoryPathSettingControl(string name, string propertyPath, Action<string, string> applyMethod, string originalValue, bool isReadOnly = false) : base(name, propertyPath, applyMethod, originalValue, isReadOnly)
		{
			Command_Browse = new RelayCommand(Browse);
		}

		private void Browse()
		{
			string selected;
			using(var save = new FolderBrowserDialog()) {
				save.Description = $"Select the new directory for {Name}:";
				save.SelectedPath = Value;
				if(save.ShowDialog() != DialogResult.OK) {
					return;
				}
				selected = save.SelectedPath;
			}
			Value = selected;
		}
	}

	/// <summary>
	/// A setting control.
	/// </summary>
	/// <typeparam name="T">The type of the setting.</typeparam>
	public abstract class SettingControl<T> : ObservableObject, ISettingsUI
	{
		/// <summary>
		/// The name of this setting.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The path to the property containing this value.
		/// </summary>
		public string PropertyPath { get; private set; }
		/// <summary>
		/// Determines whether the value has changed.
		/// </summary>
		public bool IsDirty
		{
			get
			{
				if(IsReadOnly) {
					return false;
				}
				if(Value == null) {
					return OriginalValue != null;
				}
				return !Value.Equals(OriginalValue);
			}
		}
		/// <summary>
		/// Determines whether this setting is unmodifiable.
		/// </summary>
		public bool IsReadOnly { get; private set; }
		/// <summary>
		/// The current value.
		/// </summary>
		public T Value { get; set; }
		/// <summary>
		/// The original value.
		/// </summary>
		public T OriginalValue { get; private set; }

		private readonly Action<string, T> applyMethod;

		private protected SettingControl(string name, string propertyPath, Action<string, T> applyMethod, T originalValue, bool isReadOnly)
		{
			Name = name;
			PropertyPath = propertyPath;
			this.applyMethod = applyMethod;
			Value = originalValue;
			OriginalValue = originalValue;
			IsReadOnly = isReadOnly;
		}

		/// <summary>
		/// Applies the current value.
		/// </summary>
		public void Apply()
		{
			if(!IsDirty) {
				return;
			}
			applyMethod(PropertyPath, Value);
			OriginalValue = Value;
			RaisePropertyChanged(nameof(IsDirty));
		}
	}
}
