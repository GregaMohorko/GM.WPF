using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GM.WPF.Utility
{
	/// <summary>
	/// Utilities for <see cref="DependencyProperty"/>.
	/// </summary>
	public static class DependencyPropertyUtility
	{
		/// <summary>
		/// Gets the default value of this dependency property.
		/// <para>Make sure that the type is correct, otherwise it will throw an error while casting.</para>
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="dependencyProperty">The dependency property.</param>
		public static T GetDefaultValue<T>(this DependencyProperty dependencyProperty)
		{
			return (T)GetDefaultValue(dependencyProperty);
		}

		/// <summary>
		/// Gets the default value of this dependency property.
		/// </summary>
		/// <param name="dependencyProperty">The dependency property.</param>
		public static object GetDefaultValue(this DependencyProperty dependencyProperty)
		{
			return dependencyProperty.DefaultMetadata.DefaultValue;
		}
	}
}
