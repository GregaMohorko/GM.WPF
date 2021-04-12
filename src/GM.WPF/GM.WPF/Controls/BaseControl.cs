/*
MIT License

Copyright (c) 2021 Gregor Mohorko

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
Author: Gregor Mohorko
*/

using System;
using System.Collections.Concurrent;
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
	/// <para>For design time view model data, use 'd:DataContext="{d:DesignInstance Type=local:MyControlViewModel,IsDesignTimeCreatable=True}"'.</para>
	/// <para>For registering dependency properties with view models, use <see cref="DependencyVMProperty{TOwner, TViewModel}(string, object, string)"/>.</para>
	/// </summary>
	public class BaseControl : UserControl
	{
		/// <summary>
		/// Gets a value indicating whether the control is in design mode (running under Blend or Visual Studio).
		/// </summary>
		protected bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(this);

		/// <summary>
		/// Creates a new instance of <see cref="BaseControl"/>.
		/// </summary>
		public BaseControl()
		{
			Type type = GetType();

			// DependencyVMProperty => default values
			if(dVMp_defaultValues?.ContainsKey(type) == true) {
				ViewModelChanged += DependencyVMProperty_DefaultValues_ViewModelChanged;
			}
			// DependencyVMPropertyReadOnly
			if(dVMp_ReadOnly_registeredProperties?.ContainsKey(type) == true) {
				ViewModelChanged += DependencyVMPropertyReadOnly_ViewModelChanged;
			}
		}

		#region VIEWMODEL
		/// <summary>
		/// Contains information about changed view models.
		/// </summary>
		public class ViewModelChangedEventArgs : EventArgs
		{
			/// <summary>
			/// Creates a new instance of <see cref="ViewModelChangedEventArgs"/>.
			/// </summary>
			/// <param name="oldViewModel">Old view model.</param>
			/// <param name="newViewModel">New view model.</param>
			public ViewModelChangedEventArgs(ViewModel oldViewModel, ViewModel newViewModel)
			{
				OldViewModel = oldViewModel;
				NewViewModel = newViewModel;
			}
			/// <summary>
			/// Old view model.
			/// </summary>
			public ViewModel OldViewModel { get; }
			/// <summary>
			/// New view model.
			/// </summary>
			public ViewModel NewViewModel { get; }
		}

		/// <summary>
		/// Triggered when the <see cref="ViewModel"/> changes.
		/// </summary>
		protected event EventHandler<ViewModelChangedEventArgs> ViewModelChanged;

		/// <summary>
		/// Gets or sets the view model to the first child of this control. This is to enable DependencyProperty bindings. If setting, the current view model is first disposed.
		/// <para>
		/// The idea was taken from here: http://blog.jerrynixon.com/2013/07/solved-two-way-binding-inside-user.html and here: http://www.wintellect.com/devcenter/sloscialo/where-s-my-datacontext
		/// </para>
		/// </summary>
		protected ViewModel ViewModel
		{
			get => (Content as FrameworkElement)?.DataContext as ViewModel;
			set
			{
				if(Content == null) {
					throw new InvalidOperationException("You cannot set the ViewModel if Content of the control is null.");
				}
				ViewModel oldViewModel = ViewModel;
				DisposeViewModel();
				((FrameworkElement)Content).DataContext = value;
				var eventArgs = new ViewModelChangedEventArgs(oldViewModel, value);
				ViewModelChanged?.Invoke(this, eventArgs);
			}
		}

		/// <summary>
		/// Disposes this control, if disposable. And the <see cref="ViewModel"/>, if disposable.
		/// <para>This is called automatically if inside <see cref="Windows.BaseWindow"/> and the window closes.</para>
		/// </summary>
		public void DisposeBaseControl()
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
#endregion VIEWMODEL

		#region DEPENDENCY PROPERTIES

		#region DEPENDENCY PROPERTIES - DEPENDENCYDEFAULTPROPERTY
		/// <summary>
		/// Creates a dependency property.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		protected static DependencyProperty DependencyDefaultProperty<TOwner>(string name)
		{
			return DependencyDefaultProperty(name, typeof(TOwner));
		}

		/// <summary>
		/// Creates a dependency property.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		protected static DependencyProperty DependencyDefaultProperty(string name, Type ownerType)
		{
			Type propertyType = ownerType?.GetPropertyTypeReal(name);
			object propertyTypeDefaultValue = propertyType?.GetDefault();
			return RegisterDependencyProperty(name, ownerType, propertyType, propertyTypeDefaultValue, propertyTypeDefaultValue, null);
		}

		/// <summary>
		/// Creates a dependency property with the specified default value.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		protected static DependencyProperty DependencyDefaultProperty<TOwner>(string name, object defaultValue)
		{
			return DependencyDefaultProperty(name, typeof(TOwner), defaultValue);
		}

		/// <summary>
		/// Creates a dependency property with the specified default value.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		protected static DependencyProperty DependencyDefaultProperty(string name, Type ownerType, object defaultValue)
		{
			Type propertyType = ownerType?.GetPropertyTypeReal(name);
			object propertyTypeDefaultValue = propertyType?.GetDefault();
			return RegisterDependencyProperty(name, ownerType, propertyType, defaultValue, propertyTypeDefaultValue, null);
		}
		#endregion DEPENDENCY PROPERTIES - DEPENDENCYDEFAULTPROPERTY

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
			if(propertyChangedCallback == null) {
				throw new ArgumentNullException(nameof(propertyChangedCallback));
			}
			Type propertyType = ownerType?.GetPropertyTypeReal(name);
			object propertyTypeDefaultValue = propertyType?.GetDefault();
			return RegisterDependencyProperty(name, ownerType, propertyType, propertyTypeDefaultValue, propertyTypeDefaultValue, propertyChangedCallback);
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
			if(propertyChangedCallback == null) {
				throw new ArgumentNullException(nameof(propertyChangedCallback));
			}
			Type propertyType = ownerType?.GetPropertyTypeReal(name);
			object propertyTypeDefaultValue = propertyType?.GetDefault();
			return RegisterDependencyProperty(name, ownerType, propertyType, defaultValue, propertyTypeDefaultValue, propertyChangedCallback);
		}
		#endregion DEPENDENCY PROPERTIES - DEPENDENCYNOTIFYPROPERTY

		private static DependencyProperty RegisterDependencyProperty(string name, Type ownerType, Type propertyType, object defaultValue, object propertyTypeDefaultValue, string propertyChangedCallback)
		{
			if(ownerType == null) {
				throw new ArgumentNullException(nameof(ownerType));
			}
			if(!ownerType.IsSubclassOf(typeof(BaseControl))) {
				throw new ArgumentException($"The owner type must be a child of {nameof(BaseControl)}. {ownerType.Name} is not.", nameof(ownerType));
			}

			PropertyMetadata metadata;
			if(defaultValue == propertyTypeDefaultValue && propertyChangedCallback == null) {
				metadata = new PropertyMetadata();
			} else if(propertyChangedCallback == null) {
				metadata = new PropertyMetadata(defaultValue);
			} else {
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

				// create a static callback method that will call the instance callback method
				void staticCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
				{
					_ = callbackMethod.Invoke(d, new object[] { e });
				}

				metadata = new PropertyMetadata(defaultValue, staticCallback);
			}

			return DependencyProperty.Register(name, propertyType, ownerType, metadata);
		}

		#region DEPENDENCY PROPERTIES - DEPENDENCYVMPROPERTY
		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property with the same name in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		[Obsolete("Using nullifyWhenParentTabItemIsNotSelected has been marked obsolete. It is the responsibility of the one that is using this control to do the nullification when the parent TabItem is unselected. To help achieve the same behavior, GM.WPF.Behaviors.TabItemBehavior.NullifyDataContextWhenInactive can be used.", false)]
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, bool nullifyWhenParentTabItemIsNotSelected)
		{
			// TODO obsolete v1.4.1.1, 2021-02-08
			// repeat all this in all other methods below
			// in next release, mark it as compile-time error and add in the obsolete message (... and will be removed in the next release)
			// in next release, remove it
			return DependencyVMProperty(name, typeof(TOwner), typeof(TViewModel), null, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		[Obsolete("Using nullifyWhenParentTabItemIsNotSelected has been marked obsolete. It is the responsibility of the one that is using this control to do the nullification when the parent TabItem is unselected. To help achieve the same behavior, GM.WPF.Behaviors.TabItemBehavior.NullifyDataContextWhenInactive can be used.", false)]
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, string viewModelPropertyName, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, typeof(TOwner), typeof(TViewModel), viewModelPropertyName, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		[Obsolete("This method is deprecated and will be removed in the next releases. Please use DependencyVMProperty(string, object, string) instead.", false)]
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, string viewModelPropertyName)
		{
			return DependencyVMProperty<TOwner, TViewModel>(name, null, viewModelPropertyName);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property with the same name in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		[Obsolete("Using nullifyWhenParentTabItemIsNotSelected has been marked obsolete. It is the responsibility of the one that is using this control to do the nullification when the parent TabItem is unselected. To help achieve the same behavior, GM.WPF.Behaviors.TabItemBehavior.NullifyDataContextWhenInactive can be used.", false)]
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, Type viewModelType, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, ownerType, null, viewModelType, null, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		[Obsolete("Using nullifyWhenParentTabItemIsNotSelected has been marked obsolete. It is the responsibility of the one that is using this control to do the nullification when the parent TabItem is unselected. To help achieve the same behavior, GM.WPF.Behaviors.TabItemBehavior.NullifyDataContextWhenInactive can be used.", false)]
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, Type viewModelType, string viewModelPropertyName, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, ownerType, null, viewModelType, viewModelPropertyName, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		[Obsolete("This method is deprecated and will be removed in the next releases. Please use DependencyVMProperty(string, Type, Type, object, string) instead.", false)]
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, Type viewModelType, string viewModelPropertyName)
		{
			return DependencyVMPropertyPrivate(name, ownerType, viewModelType, null, viewModelPropertyName, false);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property with the same name in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		[Obsolete("Using nullifyWhenParentTabItemIsNotSelected has been marked obsolete. It is the responsibility of the one that is using this control to do the nullification when the parent TabItem is unselected. To help achieve the same behavior, GM.WPF.Behaviors.TabItemBehavior.NullifyDataContextWhenInactive can be used.", false)]
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, object defaultValue, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, typeof(TOwner), defaultValue, typeof(TViewModel), null, nullifyWhenParentTabItemIsNotSelected);
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
		[Obsolete("Using nullifyWhenParentTabItemIsNotSelected has been marked obsolete. It is the responsibility of the one that is using this control to do the nullification when the parent TabItem is unselected. To help achieve the same behavior, GM.WPF.Behaviors.TabItemBehavior.NullifyDataContextWhenInactive can be used.", false)]
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, object defaultValue, string viewModelPropertyName, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMProperty(name, typeof(TOwner), defaultValue, typeof(TViewModel), viewModelPropertyName, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property with the same name in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="nullifyWhenParentTabItemIsNotSelected">Determines whether or not to automatically set this property to the null (or default) value when the first found TabItem parent is not selected.</param>
		[Obsolete("Using nullifyWhenParentTabItemIsNotSelected has been marked obsolete. It is the responsibility of the one that is using this control to do the nullification when the parent TabItem is unselected. To help achieve the same behavior, GM.WPF.Behaviors.TabItemBehavior.NullifyDataContextWhenInactive can be used.", false)]
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
		[Obsolete("Using nullifyWhenParentTabItemIsNotSelected has been marked obsolete. It is the responsibility of the one that is using this control to do the nullification when the parent TabItem is unselected. To help achieve the same behavior, GM.WPF.Behaviors.TabItemBehavior.NullifyDataContextWhenInactive can be used.", false)]
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, object defaultValue, Type viewModelType, string viewModelPropertyName, bool nullifyWhenParentTabItemIsNotSelected)
		{
			return DependencyVMPropertyPrivate(name, ownerType, viewModelType, defaultValue, viewModelPropertyName, nullifyWhenParentTabItemIsNotSelected);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="defaultValue">The default value of this property.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		[Obsolete("This method is deprecated and will be removed in the next releases. Please use DependencyVMProperty(string, Type, Type, object, string) instead.", false)]
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, object defaultValue, Type viewModelType, string viewModelPropertyName)
		{
			return DependencyVMProperty(name, ownerType, viewModelType, defaultValue, viewModelPropertyName);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="defaultValue">The default value of this property. If null, it will use the default value of the property type.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		protected static DependencyProperty DependencyVMProperty<TOwner, TViewModel>(string name, object defaultValue = null, string viewModelPropertyName = null)
		{
			return DependencyVMProperty(name, typeof(TOwner), typeof(TViewModel), defaultValue, viewModelPropertyName);
		}

		/// <summary>
		/// Creates a dependency property that, when updated, will also update the value of a property in the view model.
		/// </summary>
		/// <param name="name">The name of the dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="defaultValue">The default value of this property. If null, it will use the default value of the property type.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		protected static DependencyProperty DependencyVMProperty(string name, Type ownerType, Type viewModelType, object defaultValue = null, string viewModelPropertyName = null)
		{
			return DependencyVMPropertyPrivate(name, ownerType, viewModelType, defaultValue, viewModelPropertyName, false);
		}

		// could probably use normal Dictionary, since all of this is always run on the main UI thread
		private static ConcurrentDictionary<BaseControl, SearchForTabItemResult> controlToTabItem;
		private static ConcurrentDictionary<TabControl, BaseControl> tabControlsWithSingleItems;
		private static Dictionary<Type, List<(string VMPropertyName, object DefaultValue)>> dVMp_defaultValues;

		private class SearchForTabItemResult
		{
			public TabItem TabItem;
			public (TabItem TabItem, object TypeDefaultValue, string ControlPropertyName, string ViewModelPropertyName)? SkippedDueToBeingSingle;
		}

		private static DependencyProperty DependencyVMPropertyPrivate(string name, Type ownerType, Type viewModelType, object defaultValue, string viewModelPropertyName, bool nullifyWhenParentTabItemIsNotSelected)
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

			void propertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				var baseControl = (BaseControl)d;

				void setPropertyWithoutNullifyingWhenParentTabItemIsNotSelected()
				{
					// sets the new value of the property to the property in the view model
					baseControl.ViewModel?.SetProperty(viewModelPropertyName, e.NewValue);
				}

				if(!nullifyWhenParentTabItemIsNotSelected) {
					setPropertyWithoutNullifyingWhenParentTabItemIsNotSelected();
				} else {
					void SetValueBasedOnTabItemIsSelected2(BaseControl control, TabItem tab, object typeDefault, string controlPropertyName, string vmPropertyName)
					{
						object newValue = control.GetPropertyValue(controlPropertyName);
						SetValueBasedOnTabItemIsSelected(control, tab, typeDefault, controlPropertyName, vmPropertyName, newValue);
					}

					void SetValueBasedOnTabItemIsSelected(BaseControl control, TabItem tab, object typeDefault, string controlPropertyName, string vmPropertyName, object newValue)
					{
						var viewModel = control.ViewModel;
						object currentValue = viewModel.GetPropertyValue(vmPropertyName);

						if(tab.IsSelected) {
							if(currentValue != newValue) {
								viewModel.SetProperty(vmPropertyName, newValue);
							}
						} else {
							if(currentValue != typeDefault) {
								viewModel.SetProperty(vmPropertyName, typeDefault);
							}
						}
					}

					// init
					if(controlToTabItem == null) {
						controlToTabItem = new ConcurrentDictionary<BaseControl, SearchForTabItemResult>();
						tabControlsWithSingleItems = new ConcurrentDictionary<TabControl, BaseControl>();
					}

					// get the parent tab item of this control
					TabItem tabItem;
					{
						if(!controlToTabItem.TryGetValue(baseControl, out SearchForTabItemResult tabItemSearchResult)) {
							// this is the first time this is called for this control
							tabItemSearchResult = new SearchForTabItemResult();
							_ = controlToTabItem.TryAdd(baseControl, tabItemSearchResult);
						}

						if(tabItemSearchResult.TabItem != null) {
							tabItem = tabItemSearchResult.TabItem;
						} else {
							// find the parent tab control that has at least 2 items (otherwise there is no sense because a single tab item cannot be deselected)
							TabControl tabControl;
							{
								FrameworkElement current = baseControl;
								while(true) {
									tabItem = current.TryGetParent<TabItem>();
									if(tabItem == null) {
										throw new Exception($"If you set the {nameof(nullifyWhenParentTabItemIsNotSelected)} parameter to true, then that control must be placed inside a TabItem.");
									}
									tabControl = tabItem.TryGetParent<TabControl>();
									if(tabControl.Items.Count < 2) {
										if(tabItemSearchResult.SkippedDueToBeingSingle == null) {
											// only mark and do the same thing as if nullifyWhenParentTabItemIsNotSelected=false
											// the search will be done the next time again, and hopefully, there will be more items then (but do it only once)
											// this scenario can happen when you have a TabControl in a DataTemplate and controls are added one-by-one and this method gets called for the 1st TabItem when it is indeed the only TabItem in the TabControl :)

											tabItemSearchResult.SkippedDueToBeingSingle = (tabItem, typeDefaultValue, name, viewModelPropertyName);
											// this should always successfully add since we check/set the flag above
											_ = tabControlsWithSingleItems.TryAdd(tabControl, baseControl);

											setPropertyWithoutNullifyingWhenParentTabItemIsNotSelected();
											return;
										}
										// apparently this tab control is going to stay only with 1 TabItem
										_ = tabControlsWithSingleItems.TryRemove(tabControl, out BaseControl baseControl1);
										if(baseControl != baseControl1) {
											//throw new Exception("This should never happen.");
										}
									} else {
										// we found a TabControl with more than 1 item
										break;
									}
									current = tabControl;
								};
							}

							tabItemSearchResult.TabItem = tabItem;

							tabControl.SelectionChanged += (sender, selectionChangedArgs) =>
							{
								if(selectionChangedArgs.Source != tabControl) {
									return;
								}
								SetValueBasedOnTabItemIsSelected2(baseControl, tabItem, typeDefaultValue, name, viewModelPropertyName);

								// check if this tab control was skipped
								if(tabControlsWithSingleItems.TryRemove(tabControl, out BaseControl skippedBaseControl)) {
									// this base control was skipped
									// get and set the TabItem of this base control
									TabItem tabItem2;
									object typeDefaultValue2;
									string name2;
									string viewModelPropertyName2;
									{
										SearchForTabItemResult skippedTabItemResult = controlToTabItem[skippedBaseControl];
										(tabItem2, typeDefaultValue2, name2, viewModelPropertyName2) = skippedTabItemResult.SkippedDueToBeingSingle.Value;
										skippedTabItemResult.TabItem = tabItem2;
									}
									tabControl.SelectionChanged += (sender2, selectionChangedArgs2) =>
									{
										if(selectionChangedArgs2.Source != tabControl) {
											return;
										}
										
										SetValueBasedOnTabItemIsSelected2(skippedBaseControl, tabItem2, typeDefaultValue2, name2, viewModelPropertyName2);
									};
								}
							};
						}
					}

					SetValueBasedOnTabItemIsSelected(baseControl, tabItem, typeDefaultValue, name, viewModelPropertyName, e.NewValue);
				}
			}

			if(defaultValue == null) {
				defaultValue = typeDefaultValue;
			} else if(defaultValue != typeDefaultValue) {
				// set default value to the property in VM
				if(dVMp_defaultValues == null) {
					dVMp_defaultValues = new Dictionary<Type, List<(string, object)>>();
				}
				if(!dVMp_defaultValues.TryGetValue(ownerType, out List<(string, object)> defaultValues)) {
					defaultValues = new List<(string, object)>();
					dVMp_defaultValues.Add(ownerType, defaultValues);
				}
				defaultValues.Add((viewModelPropertyName, defaultValue));
			}

			return DependencyProperty.Register(name, propertyType, ownerType, new PropertyMetadata(defaultValue, propertyChangedCallback));
		}

		private void DependencyVMProperty_DefaultValues_ViewModelChanged(object sender, ViewModelChangedEventArgs e)
		{
			ViewModel vm = e.NewViewModel;
			if(vm == null) {
				return;
			}
			Type vmType = vm.GetType();

			var defaultValues = dVMp_defaultValues[GetType()];
			foreach((string VMPropertyName, object defaultValue) in defaultValues) {
				object propertyDefaultValue = vmType.GetPropertyTypeReal(VMPropertyName).GetDefault();
				object currentVMPropertyValue = vm.GetPropertyValue(VMPropertyName);
				if(currentVMPropertyValue != propertyDefaultValue) {
					// don't override if the value is not default, because it was probably manually set somewhere
					continue;
				}
				vm.SetProperty(VMPropertyName, defaultValue);
			}
		}
		#endregion DEPENDENCY PROPERTIES - DEPENDENCYVMPROPERTY

		#region DEPENDENCY PROPERTIES - DEPENDENCYVMPROPERTYREADONLY
		/// <summary>
		/// Creates a read-only dependency property that, when updated in the view model, will also update it's value.
		/// </summary>
		/// <typeparam name="TOwner">The type of the control.</typeparam>
		/// <typeparam name="TViewModel">The type of the view model.</typeparam>
		/// <param name="name">The name of the read-only dependency property.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		protected static DependencyProperty DependencyVMPropertyReadOnly<TOwner, TViewModel>(string name, string viewModelPropertyName = null)
		{
			return DependencyVMPropertyReadOnly(name, typeof(TOwner), typeof(TViewModel), viewModelPropertyName);
		}

		private static Dictionary<Type, List<(string VMPropertyName, DependencyPropertyKey)>> dVMp_ReadOnly_registeredProperties;

		/// <summary>
		/// Creates a read-only dependency property that, when updated in the view model, will also update it's value.
		/// </summary>
		/// <param name="name">The name of the read-only dependency property.</param>
		/// <param name="ownerType">The type of the control.</param>
		/// <param name="viewModelType">The type of the view model.</param>
		/// <param name="viewModelPropertyName">The name of the property in the view model to bind to. If null, it is considered to be the same as the name of the owner property.</param>
		protected static DependencyProperty DependencyVMPropertyReadOnly(string name, Type ownerType, Type viewModelType, string viewModelPropertyName = null)
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

			DependencyPropertyKey readOnlyDPropertyKey = DependencyProperty.RegisterReadOnly(name, propertyType, ownerType, new PropertyMetadata());

			// init
			if(dVMp_ReadOnly_registeredProperties == null) {
				dVMp_ReadOnly_registeredProperties = new Dictionary<Type, List<(string VMPropertyName, DependencyPropertyKey)>>();
			}
			// get for this control type
			if(!dVMp_ReadOnly_registeredProperties.TryGetValue(ownerType, out List<(string, DependencyPropertyKey)> registeredProperties)) {
				registeredProperties = new List<(string, DependencyPropertyKey)>();
				dVMp_ReadOnly_registeredProperties.Add(ownerType, registeredProperties);
			}
			// add this property
			registeredProperties.Add((viewModelPropertyName, readOnlyDPropertyKey));

			return readOnlyDPropertyKey.DependencyProperty;
		}

		private void DependencyVMPropertyReadOnly_ViewModelChanged(object sender, ViewModelChangedEventArgs e)
		{
			// unregister the old view model
			if(e.OldViewModel != null) {
				e.NewViewModel.PropertyChanged -= DependencyVMPropertyReadOnly_ViewModel_PropertyChanged;
			}
			// register the new view model
			if(e.NewViewModel != null) {
				e.NewViewModel.PropertyChanged += DependencyVMPropertyReadOnly_ViewModel_PropertyChanged;
			}
		}

		private void DependencyVMPropertyReadOnly_ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var registeredReadOnlyProperties = dVMp_ReadOnly_registeredProperties[GetType()];
			foreach((string VMPropertyName, DependencyPropertyKey Key) in registeredReadOnlyProperties) {
				if(VMPropertyName == e.PropertyName) {
					// update the read-only dependency property
					object newValue = sender.GetValue(VMPropertyName);
					SetValue(Key, newValue);

					// do not break, since there can be multiple read-only dependency properties that are related to the same view model property
				}
			}
		}
		#endregion DEPENDENCY PROPERTIES - DEPENDENCYVMPROPERTYREADONLY

		#endregion DEPENDENCY PROPERTIES
	}
}
