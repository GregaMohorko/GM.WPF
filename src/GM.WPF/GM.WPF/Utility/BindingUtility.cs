/*
MIT License

Copyright (c) 2019 Gregor Mohorko

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
Created: 2018-12-11
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GM.WPF.Utility
{
	/// <summary>
	/// Utilities for <see cref="BindingBase"/>.
	/// </summary>
	public static class BindingUtility
	{
		private static object ThrowIfFailed((bool Success, object Value) result, bool throwException)
		{
			if(result.Success) {
				return result.Value;
			}
			if(!throwException) {
				return null;
			}
			throw new InvalidOperationException($"The evaluation of the binding was unsuccessful ({result.Value}).");
		}

		/// <summary>
		/// Evaluates the value of this binding on the provided object.
		/// <para>Supported: Binding and PriorityBinding.</para>
		/// </summary>
		/// <param name="bindingBase">The binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		/// <param name="throwException">Determines whether to throw an exception when the evaluation is not successful.</param>
		/// <exception cref="InvalidOperationException">Thrown when the evaluation of the binding is not successful.</exception>
		public static object GetValueFor(this BindingBase bindingBase, object obj, bool throwException = false)
		{
			return ThrowIfFailed(TryGetValueFor(bindingBase, obj), throwException);
		}

		/// <summary>
		/// Evaluates the value of this priority binding on the provided object.
		/// </summary>
		/// <param name="priorityBinding">The priority binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		/// <param name="throwException">Determines whether to throw an exception when the evaluation is not successful.</param>
		/// <exception cref="InvalidOperationException">Thrown when the evaluation of the binding is not successful.</exception>
		public static object GetValueFor(this PriorityBinding priorityBinding, object obj, bool throwException = false)
		{
			return ThrowIfFailed(TryGetValueFor(priorityBinding, obj), throwException);
		}

		/// <summary>
		/// Evaluates the value of this binding on the provided object.
		/// </summary>
		/// <param name="binding">The binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		/// <param name="throwException">Determines whether to throw an exception when the evaluation is not successful.</param>
		/// <exception cref="InvalidOperationException">Thrown when the evaluation of the binding is not successful.</exception>
		public static object GetValueFor(this Binding binding, object obj, bool throwException = false)
		{
			return ThrowIfFailed(TryGetValueFor(binding, obj), throwException);
		}

		/// <summary>
		/// Sets the value of the property specified by this binding in the provided object to the provided value.
		/// <para>Supported: Binding only.</para>
		/// </summary>
		/// <param name="bindingBase">The binding to use for setting the value.</param>
		/// <param name="obj">The object to which to set the value.</param>
		/// <param name="value">The value to set.</param>
		/// <exception cref="InvalidOperationException">Thrown when the evaluation of the binding is not successful.</exception>
		public static void SetValueFor(this BindingBase bindingBase, object obj, object value)
		{
			if(!TrySetValueFor(bindingBase, obj, value)) {
				throw new InvalidOperationException($"Setting the provided value to the provided object was not successful using the specified binding.");
			}
		}

		/// <summary>
		/// Sets the value of the property specified by this binding in the provided object to the provided value.
		/// </summary>
		/// <param name="binding">The binding to use for setting the value.</param>
		/// <param name="obj">The object to which to set the value.</param>
		/// <param name="value">The value to set.</param>
		/// <exception cref="InvalidOperationException">Thrown when the evaluation of the binding is not successful.</exception>
		public static void SetValueFor(this Binding binding, object obj, object value)
		{
			if(!TrySetValueFor(binding, obj, value)) {
				throw new InvalidOperationException($"Setting the provided value to the provided object was not successful using the specified binding.");
			}
		}

		/// <summary>
		/// Tries to evaluate the value of this binding on the provided object and returns a tuple with a bool that determines whether the evaluation was successful and the resulting value.
		/// <para>Supported: Binding and PriorityBinding.</para>
		/// </summary>
		/// <param name="bindingBase">The binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		public static (bool Success, object Value) TryGetValueFor(this BindingBase bindingBase, object obj)
		{
			if(bindingBase == null) {
				throw new ArgumentNullException(nameof(bindingBase));
			}
			if(bindingBase is Binding b) {
				return TryGetValueFor(b, obj);
			} else if(bindingBase is PriorityBinding pb) {
				return TryGetValueFor(pb, obj);
			} else if(bindingBase is MultiBinding) {
				throw new ArgumentException("MultiBinding is not supported.", nameof(bindingBase));
			} else {
				throw new ArgumentException($"Unsupported binding type: {bindingBase.GetType().ToString()}.", nameof(bindingBase));
			}
		}

		/// <summary>
		/// Tries to set the value of the property specified by this binding in the provided object to the provided value and returns true if it was successful.
		/// <para>Supported: Binding only.</para>
		/// </summary>
		/// <param name="bindingBase">The binding to use for setting the value.</param>
		/// <param name="obj">The object to which to set the value.</param>
		/// <param name="value">The value to set.</param>
		public static bool TrySetValueFor(this BindingBase bindingBase, object obj, object value)
		{
			if(bindingBase == null) {
				throw new ArgumentNullException(nameof(bindingBase));
			}
			if(bindingBase is Binding b) {
				return TrySetValueFor(b, obj, value);
			} else {
				throw new ArgumentException($"Unsupported binding type: {bindingBase.GetType().ToString()}.", nameof(bindingBase));
			}
		}

		/// <summary>
		/// Tries to evaluate the value of this priority binding on the provided object and returns a tuple with a bool that determines whether the evaluation was successful and the resulting value.
		/// </summary>
		/// <param name="priorityBinding">The priority binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		public static (bool Success, object Value) TryGetValueFor(this PriorityBinding priorityBinding, object obj)
		{
			if(priorityBinding == null) {
				throw new ArgumentNullException(nameof(priorityBinding));
			}
			// loop through with reflection
			foreach(BindingBase pbChild in priorityBinding.Bindings) {
				(bool Success, object Value) result = TryGetValueFor(pbChild, obj);
				if(result.Success) {
					return result;
				}
			}
			// none of them were successful ...
			return (false, null);
		}

		private class DummyDO : DependencyObject
		{
			public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(DummyDO));
			public object Value
			{
				get => GetValue(ValueProperty);
				set => SetValue(ValueProperty, value);
			}
		}

		/// <summary>
		/// Tries to evaluate the value of this binding on the provided object and returns a tuple with a bool that determines whether the evaluation was successful and the resulting value.
		/// </summary>
		/// <param name="binding">The binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		public static (bool Success, object Value) TryGetValueFor(this Binding binding, object obj)
		{
			if(binding == null) {
				throw new ArgumentNullException(nameof(binding));
			}

			// idea from: https://stackoverflow.com/a/3886477/6277755

			var tmpBinding = new Binding
			{
				Path = binding.Path,
				Converter = binding.Converter,
				ConverterParameter = binding.ConverterParameter,
				ConverterCulture = binding.ConverterCulture,
				FallbackValue = binding.FallbackValue,
				StringFormat = binding.StringFormat,
				TargetNullValue = binding.TargetNullValue,
				ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
				ValidatesOnExceptions = binding.ValidatesOnExceptions,
				ValidatesOnNotifyDataErrors = binding.ValidatesOnNotifyDataErrors,
				Source = obj,
				Mode = BindingMode.OneWay
			};

			var dummyDO = new DummyDO();
			_ = BindingOperations.SetBinding(dummyDO, DummyDO.ValueProperty, tmpBinding);

			object value = dummyDO.Value;

			// check if it was successful
			BindingExpression bindingExpression = BindingOperations.GetBindingExpression(dummyDO, DummyDO.ValueProperty);
			if(bindingExpression?.Status != BindingStatus.Active) {
				// it was not
				return (false, bindingExpression?.Status);
			}

			return (true, value);
		}

		/// <summary>
		/// Tries to set the value of the property specified by this binding in the provided object to the provided value and returns true if it was successful.
		/// </summary>
		/// <param name="binding">The binding to use for setting the value.</param>
		/// <param name="obj">The object to which to set the value.</param>
		/// <param name="value">The value to set.</param>
		public static bool TrySetValueFor(this Binding binding, object obj, object value)
		{
			if(binding == null) {
				throw new ArgumentNullException(nameof(binding));
			}

			var tmpBinding = new Binding
			{
				Path = binding.Path,
				Converter = binding.Converter,
				ConverterParameter = binding.ConverterParameter,
				ConverterCulture = binding.ConverterCulture,
				FallbackValue = binding.FallbackValue,
				StringFormat = binding.StringFormat,
				TargetNullValue = binding.TargetNullValue,
				ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
				ValidatesOnExceptions = binding.ValidatesOnExceptions,
				ValidatesOnNotifyDataErrors = binding.ValidatesOnNotifyDataErrors,
				Source = obj,
				Mode = BindingMode.OneWayToSource
			};

			var dummyDO = new DummyDO();
			_ = BindingOperations.SetBinding(dummyDO, DummyDO.ValueProperty, tmpBinding);

			dummyDO.Value = value;

			// check if it was successful
			BindingExpression bindingExpression = BindingOperations.GetBindingExpression(dummyDO, DummyDO.ValueProperty);
			if(bindingExpression?.Status != BindingStatus.Active) {
				// it was not
				return false;
			}
			return true;
		}
	}
}
