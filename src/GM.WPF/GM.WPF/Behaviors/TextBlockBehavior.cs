/*
MIT License

Copyright (c) 2020 Gregor Mohorko

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
Created: 2020-02-19
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GM.WPF.Behaviors
{
	/// <summary>
	/// A behavior for <see cref="TextBlock"/>.
	/// </summary>
	public static class TextBlockBehavior
	{
		#region IsTextSelectionEnabled
		// implementation from: https://stackoverflow.com/questions/136435/any-way-to-make-a-wpf-textblock-selectable/45627524#45627524

		/// <summary>
		/// Determines whether text selection is enabled in the <see cref="TextBlock"/>.
		/// </summary>
		public static readonly DependencyProperty IsTextSelectionEnabledProperty = DependencyProperty.RegisterAttached("IsTextSelectionEnabled", typeof(bool), typeof(TextBlockBehavior), new FrameworkPropertyMetadata(false, OnIsTextSelectionEnabledChanged));

		/// <summary>
		/// Gets the current effective value of <see cref="IsTextSelectionEnabledProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		public static bool GetIsTextSelectionEnabled(DependencyObject target)
		{
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
			return (bool)target.GetValue(IsTextSelectionEnabledProperty);
		}

		/// <summary>
		/// Sets the local value of <see cref="IsTextSelectionEnabledProperty"/> for the specified target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetIsTextSelectionEnabled(DependencyObject target, bool value)
		{
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
			target.SetValue(IsTextSelectionEnabledProperty, value);
		}

		private static Dictionary<TextBlock, TextEditorWrapper> registeredEditors;

		private static void OnIsTextSelectionEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			var textBlock = (TextBlock)target;

			if((bool)e.NewValue) {
				EnableTextSelection(textBlock);
			} else {
				DisableTextSelection(textBlock);
			}
		}

		private static void EnableTextSelection(TextBlock textBlock)
		{
			if(registeredEditors == null) {
				registeredEditors = new Dictionary<TextBlock, TextEditorWrapper>();
				// first time here, register class event handlers
				TextEditorWrapper.RegisterCommandHandlers(typeof(TextBlock), true, true, true);
			}

			// there is a requirement that the control's Focusable property is set to True
			textBlock.Focusable = true;

			var editor = TextEditorWrapper.CreateFor(textBlock);
			registeredEditors.Add(textBlock, editor);
		}

		private static void DisableTextSelection(TextBlock textBlock)
		{
			if(registeredEditors == null) {
				// nothings was registered yet
				return;
			}
			TextEditorWrapper editor = registeredEditors[textBlock];
			editor.Dispose();
			_ = registeredEditors.Remove(textBlock);

			// set it back to false which is the default value (it was set to true when selection was enabled)
			textBlock.Focusable = false;
		}

		/// <summary>
		/// After hours of digging around and reading the WPF source code (https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Controls/FlowDocumentScrollViewer.cs,694a70a8f6818358), I instead discovered a way of enabling the native WPF text selection for <see cref="TextBlock"/> controls (or really any other controls). Most of the functionality around text selection is implemented in System.Windows.Documents.TextEditor system class. Unfortunately TextEditor class is marked as internal. <see cref="TextEditorWrapper"/> is a reflection wrapper around it.
		/// </summary>
		private class TextEditorWrapper
		{
			private static readonly Type TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
			private static readonly PropertyInfo TextEditor_IsReadOnlyProperty = TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
			private static readonly PropertyInfo TextEditor_TextViewProperty = TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
			private static readonly MethodInfo TextEditor_RegisterMethod = TextEditorType.GetMethod("RegisterCommandHandlers",
				BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);
			private static readonly MethodInfo TextEditor_OnDetachMethod = TextEditorType.GetMethod("OnDetach", BindingFlags.Instance | BindingFlags.NonPublic);

			private static readonly Type TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
			private static readonly PropertyInfo TextContainer_TextViewProperty = TextContainerType.GetProperty("TextView");

			private static readonly PropertyInfo TextBlock_TextContainerProperty = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

			/// <summary>
			/// System.Windows.Documents.TextEditor
			/// </summary>
			private object _editor;

			private TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
			{
				// _editor = new TextEditor(textContainer, uiScope, isUndoEnabled);
				_editor = Activator.CreateInstance(TextEditorType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
					null, new[] { textContainer, uiScope, isUndoEnabled }, null);
			}

			/// <summary>
			/// Detaches this editor from the associated <see cref="TextBlock"/> (disables selection on the <see cref="TextBlock"/>) and releases all resources used by this editor.
			/// </summary>
			public void Dispose()
			{
				// _editor.OnDetach();
				_ = TextEditor_OnDetachMethod.Invoke(_editor, null);

				_editor = null;
			}

			/// <summary>
			/// Call this once, to register class event handlers.
			/// </summary>
			public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
			{
				// TextEditor.Register(controlType, acceptsRichContent, readOnly, registerEventListeners);
				_ = TextEditor_RegisterMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
			}

			public static TextEditorWrapper CreateFor(TextBlock textBlock)
			{
				// System.Windows.Documents.TextContainer textContainer = textBlock.TextContainer;
				object textContainer = TextBlock_TextContainerProperty.GetValue(textBlock);
				// MS.Internal.Documents.TextParagraphView textView = textContainer.TextView;
				object textView = TextContainer_TextViewProperty.GetValue(textContainer);

				var wrapper = new TextEditorWrapper(textContainer, textBlock, false);
				// wrapper._editor.IsReadOnly = true;
				TextEditor_IsReadOnlyProperty.SetValue(wrapper._editor, true);
				// wrapper._editor.TextView = textView;
				TextEditor_TextViewProperty.SetValue(wrapper._editor, textView);

				return wrapper;
			}
		}
		#endregion IsTextSelectionEnabled
	}
}
