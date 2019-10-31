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
Created: 2019-09-27
Author: Grega Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GM.Windows.Utility;

namespace GM.WPF
{
	/// <summary>
	/// A tool that was designed to be used for asynchronous search textbox loading.
	/// <para>It is designed to asynchronously handle multiple asynchronous requests on the UI thread.</para>
	/// <para>It contains a waiting queue where all the requests are stored. When new requests come while a previous one is still loading, they are put into a queue. When the request that was loading finishes, it only invokes the last request in the queue.</para>
	/// <para>Everything is done on the UI thread, so requests should contain asynchronous calls to non-blocking subroutines.</para>
	/// </summary>
	public class AsyncRequestLoader : ObservableObject, IDisposable
	{
		/// <summary>
		/// Determines whether any request is currently in the process of loading.
		/// </summary>
		public bool IsLoading { get; private set; }

		private CancellationTokenSource cts;

		private readonly Queue<TaskCompletionSource<bool>> waitingQueue;

		/// <summary>
		/// Creates a new instance of <see cref="AsyncRequestLoader"/>.
		/// </summary>
		public AsyncRequestLoader()
		{
			waitingQueue = new Queue<TaskCompletionSource<bool>>();
		}

		/// <summary>
		/// If any request is still running, it is cancelled.
		/// </summary>
		public void Dispose()
		{
			cts?.Cancel();
			cts?.Dispose();
			cts = null;
		}

		/// <summary>
		/// Puts a new request into a queue.
		/// <para>This request is invoked right away if the queue is empty and there is no previous request still loading. Otherwise, it will be invoked when the previous request finishes loading.</para>
		/// <para>This request will not be invoked at all, if a new request comes after this one before the previous request has finished loading.</para>
		/// <para>Must be called from the UI thread.</para>
		/// <para>If a previous request is still loading, a cancellation request will be communicated via the <see cref="CancellationToken"/>.</para>
		/// </summary>
		/// <param name="newRequest">The asynchronous request that will possibly be invoked on the UI thread.</param>
		public async Task InvokeWhenIfLast(Func<CancellationToken, Task> newRequest)
		{
			if(newRequest == null) {
				throw new ArgumentNullException(nameof(newRequest));
			}
			if(!Application.Current.Dispatcher.IsCurrentlyRunning()) {
				throw new Exception("RequestLoader is designed to asynchronously handle multiple asynchronous requests on the UI thread. Please schedule new requests only on the UI thread.");
			}

			if(IsLoading) {
				// a previous request is still loading
				// lets cancel it and wait for it to finish (because only one request can be loading at a time)

				// add to waiting list
				var myAwaiter = new TaskCompletionSource<bool>();
				waitingQueue.Enqueue(myAwaiter);
				// cancel previous request
				cts?.Cancel();
				// wait
				bool shouldContinue = await myAwaiter.Task;
				if(!shouldContinue) {
					return;
				}
			} else {
				IsLoading = true;
			}

			// invoke
			cts = new CancellationTokenSource();
			try {
				await newRequest.Invoke(cts.Token);
			} catch(OperationCanceledException) {
			}
			cts?.Dispose();
			cts = null;

			if(waitingQueue.Count > 0) {
				// requests are waiting ...
				while(waitingQueue.Count > 0) {
					TaskCompletionSource<bool> waitingRequest = waitingQueue.Dequeue();
					bool isLast = waitingQueue.Count == 0;
					waitingRequest.SetResult(isLast);
				}
			} else {
				IsLoading = false;
			}
		}
	}
}
