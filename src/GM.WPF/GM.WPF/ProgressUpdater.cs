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
Created: 2019-09-25
Author: Gregor Mohorko
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GM.WPF
{
	/// <summary>
	/// This class can be used for updating progress from a background thread.
	/// <para>Contains some useful static methods to be used while updating the progress.</para>
	/// </summary>
	public class ProgressUpdater
	{
		private readonly Action<object> titleContentSetter;
		private readonly Action<object> messageContentSetter;
		private readonly Action<double?> progressSetter;

		/// <summary>
		/// Creates a new instance of <see cref="ProgressUpdater"/>. All the setters will be invoked on the UI thread.
		/// <para>At least one parameter must not be null.</para>
		/// </summary>
		/// <param name="titleContentSetter">A method that sets the content of the title.</param>
		/// <param name="messageContentSetter">A method that sets the content of the message.</param>
		/// <param name="progressSetter">A method that sets the progress. The values will be between 0 and 100.</param>
		public ProgressUpdater(Action<object> titleContentSetter, Action<object> messageContentSetter, Action<double?> progressSetter)
		{
			if(titleContentSetter == null && messageContentSetter == null && progressSetter == null) {
				throw new ArgumentNullException("At least one parameter must not be null.", (Exception)null);
			}
			this.titleContentSetter = titleContentSetter;
			this.messageContentSetter = messageContentSetter;
			this.progressSetter = progressSetter;
		}

		/// <summary>
		/// Determines whether a setter for the title content exists.
		/// </summary>
		public bool IsTitleEnabled => titleContentSetter != null;
		/// <summary>
		/// Determines whether a setter for the message content exists.
		/// </summary>
		public bool IsMessageEnabled => messageContentSetter != null;
		/// <summary>
		/// Determines whether a setter for the progress exists.
		/// </summary>
		public bool IsProgressEnabled => progressSetter != null;

		/// <summary>
		/// Updates the title content, message content and progress. The progress value should be between 0 and 100.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		/// <param name="messageContent">The message content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set(object titleContent, object messageContent, double? progress)
		{
			Application.Current?.Dispatcher.Invoke(delegate
			{
				titleContentSetter?.Invoke(titleContent);
				messageContentSetter?.Invoke(messageContent);
				progressSetter?.Invoke(progress);
			});
		}

		/// <summary>
		/// Updates the title content, message content and progress. The progress value should be between 0 and 1.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		/// <param name="messageContent">The message content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set2(object titleContent, object messageContent, double? progress)
		{
			Set(titleContent, messageContent, progress * 100);
		}

		/// <summary>
		/// Updates the message content and progress. The progress value should be between 0 and 100.
		/// </summary>
		/// <param name="messageContent">The message content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set(object messageContent, double? progress)
		{
			Application.Current?.Dispatcher.Invoke(delegate
			{
				messageContentSetter?.Invoke(messageContent);
				progressSetter?.Invoke(progress);
			});
		}

		/// <summary>
		/// Updates the message content and progress. The progress value should be between 0 and 1.
		/// </summary>
		/// <param name="messageContent">The message content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set2(object messageContent, double? progress)
		{
			Set(messageContent, progress * 100);
		}

		/// <summary>
		/// Updates the title content and progress. The progress value should be between 0 and 100.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set3(object titleContent, double? progress)
		{
			Application.Current?.Dispatcher.Invoke(delegate
			{
				titleContentSetter?.Invoke(titleContent);
				progressSetter?.Invoke(progress);
			});
		}

		/// <summary>
		/// Updates the title content and progress. The progress value should be between 0 and 1.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		/// <param name="progress">The progress to set.</param>
		public void Set4(object titleContent, double? progress)
		{
			Set3(titleContent, progress * 100);
		}

		/// <summary>
		/// Updates the title content.
		/// </summary>
		/// <param name="titleContent">The title content to set.</param>
		public void SetTitle(object titleContent)
		{
			Application.Current?.Dispatcher.Invoke(delegate
			{
				titleContentSetter?.Invoke(titleContent);
			});
		}

		/// <summary>
		/// Updates the message content.
		/// </summary>
		/// <param name="messageContent">The message content to set.</param>
		public void SetMessage(object messageContent)
		{
			Application.Current?.Dispatcher.Invoke(delegate
			{
				messageContentSetter?.Invoke(messageContent);
			});
		}

		/// <summary>
		/// Updates the progress. The value should be between 0 and 100.
		/// </summary>
		/// <param name="progress">The progress to set.</param>
		public void SetProgress(double? progress)
		{
			Application.Current?.Dispatcher.Invoke(delegate
			{
				progressSetter?.Invoke(progress);
			});
		}

		/// <summary>
		/// Updates the progress. The value should be between 0 and 1.
		/// </summary>
		/// <param name="progress">The progress to set.</param>
		public void SetProgress2(double? progress)
		{
			SetProgress(progress * 100);
		}

		/// <summary>
		/// Updates the progress to the specified loop state.
		/// <para>Check <see cref="GetProgress(int, int)"/> for details.</para>
		/// </summary>
		/// <param name="loopCounter">The current zero-based loop index.</param>
		/// <param name="totalIterations">The total count of iterations.</param>
		public void SetProgress(int loopCounter, int totalIterations)
		{
			double progress = GetProgress(loopCounter, totalIterations);
			SetProgress2(progress);
		}

		/// <summary>
		/// Updates the progress to the specified loop state. The progress is being calculated inside the [start, end] range.
		/// <para>Check <see cref="GetProgress(double, double, int, int)"/> for details.</para>
		/// </summary>
		/// <param name="start">The start of the progress range.</param>
		/// <param name="end">The end of the progress range.</param>
		/// <param name="loopCounter">The current zero-based loop index.</param>
		/// <param name="totalIterations">The total count of iterations.</param>
		public void SetProgress(double start, double end, int loopCounter, int totalIterations)
		{
			double progress = GetProgress(start, end, loopCounter, totalIterations);
			SetProgress2(progress);
		}

		// fields for loop
		private int? reasonableStep;
		private double? start, end;
		private int totalIterations;
		private int lastUpdateAt;
		/// <summary>
		/// Call this before entering a loop. Then, in every iteration, call either <see cref="SetForLoop(int, bool)"/>. It will only update the progress when it is reasonable (so that the progress moves by not less than 1 percent).
		/// <para>This is usefull for very long loops where updating for each iteration is pointless and would take too much of CPU time in total.</para>
		/// </summary>
		/// <param name="iterationCount">The number of total iterations (loop count).</param>
		public void StartNewLoop(int iterationCount)
		{
			StartNewLoop(0, 1, iterationCount);
		}

		/// <summary>
		/// Call this before entering a loop. Then, in every iteration, call either <see cref="SetForLoop(int, bool)"/>. It will only update the progress when it is reasonable (so that the progress moves by not less than 1 percent).
		/// <para>This is usefull for very long loops where updating for each iteration is pointless and would take too much of CPU time in total.</para>
		/// </summary>
		/// <param name="start">The start of the progress range.</param>
		/// <param name="end">The end of the progress range.</param>
		/// <param name="iterationCount">The number of total iterations (loop count).</param>
		public void StartNewLoop(double start, double end, int iterationCount)
		{
			this.start = start;
			this.end = end;
			reasonableStep = GetStepForProgressUpdate(iterationCount);
			totalIterations = iterationCount;
			lastUpdateAt = -1;
			SetProgress(0d);
		}

		/// <summary>
		/// If a reasonable amount of iterations have passed, it updates the progress to the current loop state. This must be called after <see cref="StartNewLoop(int)"/> or <see cref="StartNewLoop(double, double, int)"/>.
		/// <para>Check <see cref="GetProgress(int, int)"/> for details.</para>
		/// </summary>
		/// <param name="loopCounter">The current zero-based loop index.</param>
		/// <param name="setMessage">Determines whether or not to also update the message to "(loopCounter + 1)/totalIterations". It will also only be updated when reasonable, which means that the number might be skipping some values.</param>
		public void SetForLoop(int loopCounter, bool setMessage = false)
		{
			SetForLoop(loopCounter, setMessage ? string.Empty : null);
		}

		/// <summary>
		/// If a reasonable amount of iterations have passed, it updates the progress to the current loop state and sets the specified step message. This must be called after <see cref="StartNewLoop(int)"/> or <see cref="StartNewLoop(double, double, int)"/>.
		/// </summary>
		/// <param name="loopCounter">The current zero-based loop index.</param>
		/// <param name="stepMessage">The message that describes the current step. It will be showed after the steps progress: "(loopCounter + 1)/totalIterations: <paramref name="stepMessage"/>".
		/// <para>If null, it doesn't show any message.</para>
		/// <para>If empty, it will only show steps progress "(loopCounter + 1)/totalIterations".</para>
		/// </param>
		public void SetForLoop(int loopCounter, string stepMessage)
		{
			if(reasonableStep == null) {
				throw new InvalidOperationException("You must first call StartNewLoop before using this method.");
			}
			if(loopCounter < totalIterations) {
				if(lastUpdateAt != -1 && loopCounter < lastUpdateAt + reasonableStep.Value) {
					return;
				}
				lastUpdateAt = loopCounter;
			}
			SetProgress(start.Value, end.Value, loopCounter, totalIterations);
			if(stepMessage != null) {
				string message = $"{loopCounter + 1}/{totalIterations}";
				if(stepMessage.Length > 0) {
					message += $": {stepMessage}";
				}
				SetMessage(message);
			}
		}

		/// <summary>
		/// Starts a loop on the thread pool that will simulate the progress for the specified amount of time, going from 1% to 100%.
		/// </summary>
		/// <param name="estimatedLoadingTime">The estimated loading time. The simulation will be approximately this long.</param>
		/// <param name="ct">The cancellation token that will cancel this loop. Use it to let this simulation know that the loading has already finished (or has been cancelled).</param>
		/// <param name="setAsIndeterminateAfter">If true, it will set the progress as indeterminate after the simulation is completed.</param>
		public void SimulateForKnownLoadingTime(
			TimeSpan estimatedLoadingTime,
			CancellationToken ct,
			bool setAsIndeterminateAfter = true
			)
		{
			_ = Task.Run(async delegate
			{
				// calculate delay (in milliseconds) between steps
				int msDelayPerStep = (int)Math.Round(estimatedLoadingTime.TotalMilliseconds / 100);

				// start at 1
				SetProgress(1);

				// go from 1 to 100
				for(int i = 1; i <= 100; ++i) {
					// wait
					await Task.Delay(msDelayPerStep);
					// check if the task already finished (or was cancelled, not important)
					if(ct.IsCancellationRequested) {
							break;
						}
					// simulate progress
					SetProgress(i);
				}

				if(setAsIndeterminateAfter) {
					await Task.Delay(msDelayPerStep);
					if(!ct.IsCancellationRequested) {
						SetProgress(null);
					}
				}
			});
		}

		/// <summary>
		/// Returns the progress for the specified loop state. The progress is being calculated inside the [start, end] range.
		/// <para>This is useful when you have multiple loops and different loops have different ranges. For instance, you can have a two loops where the first has more iterations so you would give it a range between [0, 0.7] and then the second between [0.7, 1]. This way the progress will go smoothly from 0 to 1 even if there are actually two loops being iterated.</para>
		/// <para>This returns start + (end - start) * ((loopCounter + 1) / (double)totalIterations).</para>
		/// </summary>
		/// <param name="start">The start of the progress range.</param>
		/// <param name="end">The end of the progress range.</param>
		/// <param name="loopCounter">The current zero-based loop index.</param>
		/// <param name="totalIterations">The total count of iterations.</param>
		public static double GetProgress(double start, double end, int loopCounter, int totalIterations)
		{
			if(start < 0 || start >= 1) {
				throw new ArgumentOutOfRangeException(nameof(start), "Start must be non-negative and less than 1.");
			}
			if(end <= start || end > 1) {
				throw new ArgumentOutOfRangeException(nameof(end), "End must be more than start and not bigger than 1.");
			}
			double progress = GetProgress(loopCounter, totalIterations);
			double range = end - start;
			return start + range * progress;
		}

		/// <summary>
		/// Returns the progress for the specified loop state.
		/// <para>This returns (loopCounter + 1) / (double)totalIterations.</para>
		/// </summary>
		/// <param name="loopCounter">The current zero-based loop index.</param>
		/// <param name="totalIterations">The total count of iterations.</param>
		public static double GetProgress(int loopCounter, int totalIterations)
		{
			if(loopCounter < 0 || loopCounter >= totalIterations) {
				throw new ArgumentOutOfRangeException(nameof(loopCounter), "The loop counter must be non-negative and less than the total iteration count.");
			}
			return (loopCounter + 1) / (double)totalIterations;
		}

		/// <summary>
		/// Returns a reasonable step at which the progress is best updated (so that the progress moves by not less than 1 percent).
		/// <para>This is usefull for very long loops where updating for each iteration is pointless and would take too much of CPU time in total.</para>
		/// <para>For instance, if you have 100000 items to process, a reasonable step to update the progress is 100000/100 = 1000. So the progress is updated once every 1000 items.</para>
		/// </summary>
		/// <param name="iterationCount">The number of total iterations (loop count).</param>
		public static int GetStepForProgressUpdate(int iterationCount)
		{
			if(iterationCount < 200) {
				return 1;
			}
			return iterationCount / 100;
		}
	}
}
