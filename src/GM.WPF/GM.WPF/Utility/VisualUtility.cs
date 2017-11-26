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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GM.WPF.Utility
{
	/// <summary>
	/// Utilities for <see cref="DependencyObject"/>.
	/// </summary>
	public static class VisualUtility
	{
		/// <summary>
		/// Returns all the child visual objects of the specified visual type within the specified parent.
		/// </summary>
		/// <typeparam name="T">The type of visuals to search for.</typeparam>
		/// <param name="parent">The parent visual whose children to look through.</param>
		public static IEnumerable<T> GetVisualChildCollection<T>(this DependencyObject parent) where T : Visual
		{
			return GetVisualChildCollection(parent, new List<Type>() { typeof(T) }).Cast<T>();
		}

		/// <summary>
		/// Returns all the child visual objects of the specified generic types within the specified parent.
		/// </summary>
		/// <typeparam name="T1">The first type of visuals to search for.</typeparam>
		/// <typeparam name="T2">The second type of visuals to search for.</typeparam>
		/// <param name="parent">The parent visual whose children to look through.</param>
		public static IEnumerable<Visual> GetVisualChildCollection<T1,T2>(this DependencyObject parent) where T1:Visual where T2:Visual
		{
			return GetVisualChildCollection(parent, new List<Type>() { typeof(T1), typeof(T2) });
		}

		/// <summary>
		/// Returns all the child visual objects of the specified types within the specified parent.
		/// </summary>
		/// <param name="parent">The parent visual whose children to look through.</param>
		/// <param name="typesOfChildren">The types of visuals to search for. All the types should be child types of <see cref="Visual"/>.</param>
		public static IEnumerable<Visual> GetVisualChildCollection(this DependencyObject parent, IEnumerable<Type> typesOfChildren)
		{
			var currentVisuals = new List<DependencyObject>
			{
				parent
			};

			while(currentVisuals.Count > 0) {
				var newVisuals = new List<DependencyObject>();

				foreach(DependencyObject visual in currentVisuals) {
					int count = VisualTreeHelper.GetChildrenCount(visual);

					for(int i = 0; i < count; ++i) {
						DependencyObject child = VisualTreeHelper.GetChild(visual, i);
						Type childType = child.GetType();
						if(typesOfChildren.Any(type => type.IsAssignableFrom(childType))) {
							yield return (Visual)child;
						}
						if(child != null) {
							newVisuals.Add(child);
						}
					}
				}

				currentVisuals = newVisuals;
			}
		}
	}
}
