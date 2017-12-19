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

Project: GM.WPF
Created: 2017-10-30
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GM.Utility;
using GM.WPF.MVVM;
using GM.WPF.Utility;

namespace GM.WPF.Controls
{
	/// <summary>
	/// The base class for user controls. If used inside <see cref="Windows.BaseWindow"/>, the view model (if present) and the control itself will both automatically be disposed (if disposable) when window closes.
	/// <para>For view model, use <see cref="ViewModel"/> property.</para>
	/// <para>For design time view model data, use 'd:DataContext="{d:DesignInstance Type=local:MainWindowViewModel,IsDesignTimeCreatable=True}"'.</para>
	/// <para>For registering dependency properties with view models, use <see cref="BindableVMProperty(string, Type, Type, string)"/>.</para>
	/// </summary>
	public class BaseControl:UserControl
	{
		/// <summary>
		/// Gets a value indicating whether the control is in design mode (running under Blend or Visual Studio).
		/// </summary>
		protected bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(this);

		/// <summary>
		/// Gets or sets the view model to the first child of this control. This is to enable DependencyProperty bindings. If setting, the current view model is first disposed.
		/// <para>
		/// The idea was taken from here: http://blog.jerrynixon.com/2013/07/solved-two-way-binding-inside-user.html and here: http://www.wintellect.com/devcenter/sloscialo/where-s-my-datacontext
		/// </para>
		/// </summary>
		protected ViewModel ViewModel
		{
			get => ((FrameworkElement)Content).DataContext as ViewModel;
			set
			{
				DisposeViewModel();
				((FrameworkElement)Content).DataContext = value;
			}
		}

		/// <summary>
		/// Disposes this control, if disposable. And the <see cref="ViewModel"/>, if disposable.
		/// </summary>
		internal void DisposeBaseControl()
		{
			// dispose this control
			if(this is IDisposable thisDisposable) {
				thisDisposable.Dispose();
			}

			// dispose the view model
			DisposeViewModel();
		}

		private void DisposeViewModel()
		{
			if(ViewModel is IDisposable vmDisposable) {
				vmDisposable.Dispose();
			}
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		protected static DependencyProperty BindableVMProperty(string name,Type ownerType, Type viewModelType, string viewModelPropertyName=null)
		{
			return BindableVMProperty(name, ownerType, null, viewModelType, viewModelPropertyName);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		protected static DependencyProperty BindableVMProperty(string name, Type ownerType, object defaultValue, Type viewModelType, string viewModelPropertyName = null)
		{
			if(viewModelPropertyName == null) {
				viewModelPropertyName = name;
			}

			if(!ownerType.IsSubclassOf(typeof(BaseControl))) {
				throw new ArgumentException("The owner type must be a child of BaseControl.", nameof(ownerType));
			}
			if(!viewModelType.IsSubclassOf(typeof(ViewModel))) {
				throw new ArgumentException("The view model type must be a child of ViewModel.", nameof(viewModelType));
			}
			if(!viewModelType.HasProperty(viewModelPropertyName, true)) {
				throw new ArgumentException("A property with the specified view model property name must exist in the specified view model type.", nameof(viewModelPropertyName));
			}
			
			Type propertyType = ownerType.GetPropertyTypeReal(name);

			void propertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				var baseControl = (BaseControl)d;
				var vm = baseControl.ViewModel;

				// the new value of the dependency property that was set
				var newValue = baseControl.GetPropertyValue(name);
				// sets the new value of the property to the property in the view model
				vm.SetProperty(name, newValue);
			}

			if(defaultValue == null) {
				defaultValue = propertyType.GetDefault();
			}

			return DependencyProperty.Register(name, propertyType, ownerType, new PropertyMetadata(defaultValue, propertyChangedCallback));
		}
	}
}
