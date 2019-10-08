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

namespace GM.WPF.Windows.Model.SettingsWindow
{
	/// <summary>
	/// A settings factory, used to create <see cref="SettingControl{T}"/> controls.
	/// </summary>
	public class SettingsFactory
	{
		private readonly Func<string, object> valueGetter;
		private readonly Action<string, object> applyMethod;

		internal SettingsFactory(Func<string, object> valueGetter, Action<string, object> applyMethod)
		{
			this.valueGetter = valueGetter;
			this.applyMethod = applyMethod;
		}

		/// <summary>
		/// Creates a new <see cref="SettingControl{T}"/> control.
		/// </summary>
		/// <typeparam name="T">The type of the setting.</typeparam>
		/// <param name="name">The name of the control.</param>
		/// <param name="propertyPath">The path to the property, used in the methods provided when creating this factory.</param>
		public ISettingsUI Create<T>(string name, string propertyPath)
		{
			return Create<T>(name, propertyPath, false);
		}

		/// <summary>
		/// Creates a new read-only <see cref="SettingControl{T}"/> control.
		/// </summary>
		/// <typeparam name="T">The type of the setting.</typeparam>
		/// <param name="name">The name of the control.</param>
		/// <param name="propertyPath">The path to the property, used in the methods provided when creating this factory.</param>
		public ISettingsUI CreateReadOnly<T>(string name, string propertyPath)
		{
			return Create<T>(name, propertyPath, true);
		}

		private ISettingsUI Create<T>(string name, string propertyPath, bool isReadOnly)
		{
			if(applyMethod == null && !isReadOnly) {
				throw new InvalidOperationException("A read-only factory can only create read-only settings.");
			}
			T originalValue = (T)valueGetter(propertyPath);
			if(typeof(T) == typeof(string)) {
				return new StringSettingControl(name, propertyPath, CastApplyMethod<string>(), originalValue as string, isReadOnly);
			}

			throw new ArgumentOutOfRangeException(nameof(T), $"The type '{typeof(T)}' is not supported as a setting.");
		}

		private Action<string, T> CastApplyMethod<T>()
		{
			return (string name, T value) => { applyMethod(name, value); };
		}
	}
}
