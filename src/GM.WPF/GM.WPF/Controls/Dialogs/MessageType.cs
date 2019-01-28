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
Created: 2019-1-28
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM.WPF.Controls.Dialogs
{
	/// <summary>
	/// The type of message for <see cref="MessageDialog"/>. Determines the color of the background.
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// Normal message type. The background will be <see cref="MessageDialog.NormalBackground"/>.
		/// </summary>
		NORMAL = 0,
		/// <summary>
		/// Message type that represents a warning. The background will be <see cref="MessageDialog.WarningBackground"/>.
		/// </summary>
		WARNING = 1,
		/// <summary>
		/// Message type that represents an error. The background will be <see cref="MessageDialog.ErrorBackground"/>.
		/// </summary>
		ERROR = 2
	}
}
