using System;
using System.Threading;
using System.Threading.Tasks;

namespace PagarMePOS
{
    /// <summary>
    /// https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <param name="mpos"></param>
        /// <returns></returns>
        public static async Task<TResult> HandleMposException<TResult>(this Task<TResult> task, TimeSpan timeout, PagarMe.Mpos.Mpos mpos)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                int mposErrorCode = 0; 

                mpos.Errored += (sender, e) =>
                {
                    mposErrorCode = e;
                    timeoutCancellationTokenSource.Cancel();
                };

                Task mposErrorTask = new Task(() =>
                {
                    while (true)
                    {
                        if (mposErrorCode != 0)
                        {
                            return;
                        }
                    };
                });

                var completedTask = await Task.WhenAny(task, mposErrorTask, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else if (mposErrorCode != 0)
                {
                    throw new PinPadException(mposErrorCode, ((PinPadErrors)mposErrorCode).GetFriendlyMessage());
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}
