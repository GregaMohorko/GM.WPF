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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GM.WPF.Utility
{
	/// <summary>
	/// Utilities for <see cref="Visual"/>.
	/// </summary>
	public static class VisualUtility
	{
		/// <summary>
		/// Returns all the child visual objects of the specified visual type within the specified parent.
		/// </summary>
		/// <typeparam name="T">The type of visuals to search for.</typeparam>
		/// <param name="parent">The parent visual whose children to look through.</param>
		/// <param name="treeTraverseStrategy">The strategy to use.</param>
		public static IEnumerable<T> GetVisualChildCollection<T>(this Visual parent, TreeTraverseStrategy treeTraverseStrategy = TreeTraverseStrategy.BreadthFirst) where T : Visual
		{
			return GetVisualChildCollection(parent, new List<Type>() { typeof(T) }, treeTraverseStrategy).Cast<T>();
		}

		/// <summary>
		/// Returns all the child visual objects of the specified generic types within the specified parent.
		/// </summary>
		/// <typeparam name="T1">The first type of visuals to search for.</typeparam>
		/// <typeparam name="T2">The second type of visuals to search for.</typeparam>
		/// <param name="parent">The parent visual whose children to look through.</param>
		/// <param name="treeTraverseStrategy">The strategy to use.</param>
		public static IEnumerable<Visual> GetVisualChildCollection<T1,T2>(this Visual parent, TreeTraverseStrategy treeTraverseStrategy = TreeTraverseStrategy.BreadthFirst) where T1:Visual where T2:Visual
		{
			return GetVisualChildCollection(parent, new List<Type>() { typeof(T1), typeof(T2) }, treeTraverseStrategy);
		}

		/// <summary>
		/// Returns all the child visual objects of the specified types within the specified parent.
		/// </summary>
		/// <param name="parent">The parent visual whose children to look through.</param>
		/// <param name="typesOfChildren">The types of visuals to search for. All the types should be child types of <see cref="Visual"/>.</param>
		/// <param name="treeTraverseStrategy">The strategy to use.</param>
		public static IEnumerable<Visual> GetVisualChildCollection(this Visual parent, IEnumerable<Type> typesOfChildren, TreeTraverseStrategy treeTraverseStrategy = TreeTraverseStrategy.BreadthFirst)
		{
			return GetVisualChildCollection(parent, (Visual v) =>
			{
				Type childType = v.GetType();
				return typesOfChildren.Any(type => type.IsAssignableFrom(childType));
			}, treeTraverseStrategy);
		}

		/// <summary>
		/// Returns all the child visual objects within this parent that satisfies a condition.
		/// </summary>
		/// <param name="parent">The parent visual whose children to look through.</param>
		/// <param name="predicate">A function to test each visual for a condition.</param>
		/// <param name="treeTraverseStrategy">The strategy to use.</param>
		public static IEnumerable<Visual> GetVisualChildCollection(this Visual parent, Func<Visual,bool> predicate, TreeTraverseStrategy treeTraverseStrategy=TreeTraverseStrategy.BreadthFirst)
		{
			ICollection visuals;
			switch(treeTraverseStrategy) {
				case TreeTraverseStrategy.BreadthFirst:
					visuals = new Queue<Visual>();
					((Queue<Visual>)visuals).Enqueue(parent);
					break;
				case TreeTraverseStrategy.DepthFirst:
					visuals = new Stack<Visual>();
					((Stack<Visual>)visuals).Push(parent);
					break;
				default:
					throw new NotImplementedException($"Uknown TreeTraverseStrategy: '{treeTraverseStrategy}'.");
			}

			bool firstTime = true;

			while(visuals.Count > 0) {
				Visual visual;
				switch(treeTraverseStrategy) {
					case TreeTraverseStrategy.BreadthFirst:
						visual = ((Queue<Visual>)visuals).Dequeue();
						break;
					case TreeTraverseStrategy.DepthFirst:
						visual = ((Stack<Visual>)visuals).Pop();
						break;
					default:
						throw new NotImplementedException($"Uknown TreeTraverseStrategy: '{treeTraverseStrategy}'.");
				}

				if(firstTime) {
					// parent should not be tested because it is not a child
					firstTime = false;
				} else if(predicate(visual)) {
					yield return visual;
				}
				
				int childrenCount = VisualTreeHelper.GetChildrenCount(visual);
				for(int i = childrenCount-1; i >= 0; --i) {
					DependencyObject child = VisualTreeHelper.GetChild(visual, i);
					if(!(child is Visual childVisual)) {
						continue;
					}
					switch(treeTraverseStrategy) {
						case TreeTraverseStrategy.BreadthFirst:
							((Queue<Visual>)visuals).Enqueue(childVisual);
							break;
						case TreeTraverseStrategy.DepthFirst:
							((Stack<Visual>)visuals).Push(childVisual);
							break;
						default:
							throw new NotImplementedException($"Uknown TreeTraverseStrategy: '{treeTraverseStrategy}'.");
					}
				}
			}
		}

		/// <summary>
		/// A strategy for traversing/searching tree/graph data structures.
		/// </summary>
		public enum TreeTraverseStrategy
		{
			/// <summary>
			/// It starts at the tree root and explores all of the neighbor nodes at the present depth prior to moving on to the nodes at the next depth level.
			/// </summary>
			BreadthFirst,
			/// <summary>
			/// The algorithm starts at the root node and explores as far as possible along each branch before backtracking.
			/// </summary>
			DepthFirst
		}
	}
}
