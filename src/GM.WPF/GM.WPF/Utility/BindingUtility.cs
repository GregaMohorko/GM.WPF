﻿/*
MIT License

Copyright (c) 2018 Grega Mohorko

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
Author: Grega Mohorko
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
		private static object ThrowIfFailed(Tuple<bool, object> result, bool throwException)
		{
			if(result.Item1) {
				return result.Item2;
			}
			if(!throwException) {
				return null;
			}
			throw new InvalidOperationException($"The evaluation of the binding was unsuccessful ({result.Item2}).");
		}

		/// <summary>
		/// Evaluates the value of this binding on the provided object.
		/// <para>Note: while evaluating, the source of the binding will be set to the provided object and then set back to what it was. This may cause side effects.</para>
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
		/// <para>Note: while evaluating, the source of the bindings will be set to the provided object and then set back to what it was. This may cause side effects.</para>
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
		/// <para>Note: while evaluating, the source of the binding will be set to the provided object and then set back to what it was. This may cause side effects.</para>
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
		/// Tries to evaluate the value of this binding on the provided object and returns a tuple with a bool that determines whether the evaluation was successful and the resulting value.
		/// <para>Note: while evaluating, the source of the binding will be set to the provided object and then set back to what it was. This may cause side effects.</para>
		/// </summary>
		/// <param name="bindingBase">The binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		public static Tuple<bool, object> TryGetValueFor(this BindingBase bindingBase, object obj)
		{
			if(bindingBase is Binding b) {
				return TryGetValueFor(b, obj);
			} else if(bindingBase is PriorityBinding pb) {
				return TryGetValueFor(pb, obj);
			} else if(bindingBase is MultiBinding mb) {
				throw new ArgumentException("MultiBinding is not supported.", nameof(bindingBase));
			} else {
				throw new ArgumentException($"Unsupported binding type: {bindingBase.GetType().ToString()}.", nameof(bindingBase));
			}
		}

		/// <summary>
		/// Tries to evaluate the value of this priority binding on the provided object and returns a tuple with a bool that determines whether the evaluation was successful and the resulting value.
		/// <para>Note: while evaluating, the source of the child bindings will be set to the provided object and then set back to what it was. This may cause side effects.</para>
		/// </summary>
		/// <param name="priorityBinding">The priority binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		public static Tuple<bool, object> TryGetValueFor(this PriorityBinding priorityBinding, object obj)
		{
			// loop through with reflection
			foreach(BindingBase pbChild in priorityBinding.Bindings) {
				Tuple<bool, object> result = TryGetValueFor(pbChild, obj);
				if(result.Item1) {
					return result;
				}
			}
			// none of them were successful ...
			return Tuple.Create<bool, object>(false, null);
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
		/// <para>Note: while evaluating, the source of the binding will be set to the provided object and then set back to what it was. This may cause side effects.</para>
		/// </summary>
		/// <param name="binding">The binding to use for getting the value.</param>
		/// <param name="obj">The object from which to get the value.</param>
		public static Tuple<bool, object> TryGetValueFor(this Binding binding, object obj)
		{
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
				Source = obj
			};

			var dummyDO = new DummyDO();
			BindingOperations.SetBinding(dummyDO, DummyDO.ValueProperty, tmpBinding);

			object value = dummyDO.Value;

			// check if it was successful
			BindingExpression bindingExpression = BindingOperations.GetBindingExpression(dummyDO, DummyDO.ValueProperty);
			if(bindingExpression.Status != BindingStatus.Active) {
				// it was not
				return Tuple.Create<bool, object>(false, bindingExpression.Status);
			}

			return Tuple.Create(true, value);
		}
	}
}
