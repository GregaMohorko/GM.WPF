﻿/*
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GM.Utility;
using GM.WPF.MVVM;

namespace GM.WPF.Controls
{
	/// <summary>
	/// The base class for user controls. If used inside <see cref="Windows.BaseWindow"/>, the view model (if present) and the control itself will both automatically be disposed (if disposable) when window closes.
	/// <para>For view model, use <see cref="ViewModel"/> property.</para>
	/// <para>For design time view model data, use 'd:DataContext="{d:DesignInstance Type=local:MainWindowViewModel,IsDesignTimeCreatable=True}"'.</para>
	/// <para>For registering dependency properties with view models, use <see cref="DependencyVMProperty(string, Type, Type, string, bool)"/>.</para>
	/// </summary>
	public class BaseControl : UserControl
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
			// only dispose the current view model if it is not inherited from parent control
			if(ReferenceEquals(DataContext, ViewModel)) {
				// this view model was inherited from parent control, do not dispose it!
				return;
			}
			if(ViewModel is IDisposable vmDisposable) {
				vmDisposable.Dispose();
			}
		}

		#region DEPENDENCY PROPERTIES

		#region DEPENDENCY PROPERTIES - DEPENDENCYNOTIFYPROPERTY
		/// <summary>
		/// Creates a dependency property that, when updated, will call the specified callback method.
		/// <para>The <paramref name="propertyChangedCallback"/> must be an instance method with one parameter of type <see cref="DependencyPropertyChangedEventArgs"/>.</para>
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="propertyChangedCallback">The name of the method to call when this property changes. The method must be an instance method with one parameter of type <see cref="DependencyPropertyChangedEventArgs"/>.</param>
		protected static DependencyProperty DependencyNotifyProperty<TOwner>(string name, string propertyChangedCallback)
		{
			return DependencyNotifyProperty(name, typeof(TOwner), propertyChangedCallback);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will call the specified callback method.
		/// <para>The <paramref name="propertyChangedCallback"/> must be an instance method with one parameter of type <see cref="DependencyPropertyChangedEventArgs"/>.</para>
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="propertyChangedCallback">The name of the method to call when this property changes. The method must be an instance method with one parameter of type <see cref="DependencyPropertyChangedEventArgs"/>.</param>
		protected static DependencyProperty DependencyNotifyProperty(string name, Type ownerType, string propertyChangedCallback)
		{
			return DependencyNotifyProperty(name, ownerType, null, propertyChangedCallback);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will call the specified callback method.
		/// <para>The <paramref name="propertyChangedCallback"/> must be an instance method with one parameter of type <see cref="DependencyPropertyChangedEventArgs"/>.</para>
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="propertyChangedCallback">The name of the method to call when this property changes. The method must be an instance method with one parameter of type <see cref="DependencyPropertyChangedEventArgs"/>.</param>
		protected static DependencyProperty DependencyNotifyProperty<TOwner>(string name, object defaultValue, string propertyChangedCallback)
		{
			return DependencyNotifyProperty(name, typeof(TOwner), defaultValue, propertyChangedCallback);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will call the specified callback method.
		/// <para>The <paramref name="propertyChangedCallback"/> must be an instance method with one parameter of type <see cref="DependencyPropertyChangedEventArgs"/>.</para>
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="propertyChangedCallback">The name of the method to call when this property changes. The method must be an instance method with one parameter of type <see cref="DependencyPropertyChangedEventArgs"/>.</param>
		protected static DependencyProperty DependencyNotifyProperty(string name, Type ownerType, object defaultValue, string propertyChangedCallback)
		{
			if(!ownerType.IsSubclassOf(typeof(BaseControl))) {
				throw new ArgumentException($"The owner type must be a child of {nameof(BaseControl)}. {ownerType.Name} is not.", nameof(ownerType));
			}

			MethodInfo callbackMethod = ownerType.GetMethod(propertyChangedCallback, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			{
				if(callbackMethod == null) {
					throw new ArgumentException($"The callback method named '{propertyChangedCallback}' does not exist in type '{ownerType.Name}'.", nameof(propertyChangedCallback));
				}
				if(callbackMethod.ReturnType != typeof(void)) {
					throw new ArgumentException("The callback method must be void.", nameof(propertyChangedCallback));
				}
				var parameters = callbackMethod.GetParameters();
				if(parameters.Length != 1) {
					throw new ArgumentException("The callback method must have exactly one parameter.", nameof(propertyChangedCallback));
				}
				if(parameters[0].ParameterType != typeof(DependencyPropertyChangedEventArgs)) {
					throw new ArgumentException($"The parameter of the callback method must be of type {nameof(DependencyPropertyChangedEventArgs)}.", nameof(propertyChangedCallback));
				}
			}

			void staticCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				callbackMethod.Invoke(d, new object[] { e });
			}

			Type propertyType = ownerType.GetPropertyTypeReal(name);
			return DependencyProperty.Register(name, propertyType, ownerType, new PropertyMetadata(defaultValue, staticCallback));
		}
		#endregion DEPENDENCY PROPERTIES - DEPENDENCYNOTIFYPROPERTY

		#region DEPENDENCY PROPERTIES - DEPENDENCYVMPROPERTY
		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, string viewModelPropertyName = null, bool nullifyWhenParentTabItemIsNotSelected = false)
		{
			return DependencyVMProperty(name, typeof(TOwner), typeof(TViewModel), viewModelPropertyName, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property with the same name in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, typeof(TOwner), typeof(TViewModel), null, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, Type viewModelType, string viewModelPropertyName = null, bool nullifyWhenParentTabItemIsNotSelected = false)
		{
			return DependencyVMProperty(name, ownerType, null, viewModelType, viewModelPropertyName, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property with the same name in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, Type viewModelType, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, ownerType, null, viewModelType, null, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, object defaultValue, string viewModelPropertyName = null, bool nullifyWhenParentTabItemIsNotSelected = false)
		{
			return DependencyVMProperty(name, typeof(TOwner), defaultValue, typeof(TViewModel), viewModelPropertyName, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property with the same name in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, object defaultValue, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, typeof(TOwner), defaultValue, typeof(TViewModel), null, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property with the same name in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, object defaultValue, Type viewModelType, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, ownerType, defaultValue, viewModelType, null, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, object defaultValue, Type viewModelType, string viewModelPropertyName = null, bool nullifyWhenParentTabItemIsNotSelected = false)
		{
			if(viewModelPropertyName == null) {
				viewModelPropertyName = name;
			}

			if(!ownerType.IsSubclassOf(typeof(BaseControl))) {
				throw new ArgumentException($"The owner type must be a child of {nameof(BaseControl)}. {ownerType.Name} is not.", nameof(ownerType));
			}
			if(!viewModelType.IsSubclassOf(typeof(ViewModel))) {
				throw new ArgumentException("The view model type must be a child of ViewModel.", nameof(viewModelType));
			}
			if(!viewModelType.HasProperty(viewModelPropertyName, true)) {
				throw new ArgumentException("A property with the specified view model property name must exist in the specified view model type.", nameof(viewModelPropertyName));
			}

			Type propertyType = ownerType.GetPropertyTypeReal(name);

			object typeDefaultValue = propertyType.GetDefault();

			TabItem tabItem = null;
			TabControl tabControl;
			object actualValue = typeDefaultValue;
			void propertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				var baseControl = (BaseControl)d;
				var vm = baseControl.ViewModel;

				if(!nullifyWhenParentTabItemIsNotSelected) {
					// sets the new value of the property to the property in the view model
					vm.SetProperty(viewModelPropertyName, e.NewValue);
				} else {
					void SetValueBasedOnTabItemIsSelected()
					{
						object currentValue = vm.GetPropertyValue(viewModelPropertyName);
						if(tabItem.IsSelected) {
							if(currentValue != actualValue) {
								vm.SetProperty(viewModelPropertyName, actualValue);
							}
						} else {
							if(currentValue != typeDefaultValue) {
								vm.SetProperty(viewModelPropertyName, typeDefaultValue);
							}
						}
					}

					if(tabItem == null) {
						// this is the first time this is called
						tabItem = baseControl.TryToFindParentTabItem();
						if(tabItem == null) {
							throw new Exception($"If you set the {nameof(nullifyWhenParentTabItemIsNotSelected)} parameter to true, then that control must be placed inside a TabItem.");
						}
						tabControl = baseControl.FindParentTabControl(tabItem);
						tabControl.SelectionChanged += (sender, selectionChangedArgs) =>
						{
							if(selectionChangedArgs.Source != tabControl) {
								return;
							}
							SetValueBasedOnTabItemIsSelected();
						};
					}
					actualValue = e.NewValue;
					SetValueBasedOnTabItemIsSelected();
				}
			}

			if(defaultValue == null) {
				defaultValue = typeDefaultValue;
			}

			return DependencyProperty.Register(name, propertyType, ownerType, new PropertyMetadata(defaultValue, propertyChangedCallback));
		}

		private TabItem TryToFindParentTabItem()
		{
			var current = Parent as FrameworkElement;
			while(current != null) {
				if(current is TabItem tabItem) {
					return tabItem;
				}
				current = current.Parent as FrameworkElement;
			}
			return null;
		}

		private TabControl FindParentTabControl(TabItem tabItem)
		{
			var current = tabItem.Parent as FrameworkElement;
			while(current != null) {
				if(current is TabControl tabControl) {
					return tabControl;
				}
				current = current.Parent as FrameworkElement;
			}
			return null;
		}
		#endregion DEPENDENCY PROPERTIES - DEPENDENCYVMPROPERTY

		#endregion DEPENDENCY PROPERTIES
	}
}
