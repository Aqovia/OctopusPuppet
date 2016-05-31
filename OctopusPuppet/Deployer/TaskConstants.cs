using System.Threading.Tasks;

namespace OctopusPuppet.Deployer
{
    public static class TaskConstants
    {   
        /// <summary>
        /// A <see cref="Task"/> that will never complete.
        /// </summary>
        public static Task Never
        {
            get
            {
                return TaskConstants<bool>.Never;
            }
        }

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task Canceled
        {
            get
            {
                return TaskConstants<bool>.Canceled;
            }
        }
    }

    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    public static class TaskConstants<T>
    {
        private static readonly Task<T> _never = new TaskCompletionSource<T>().Task;

        private static readonly Task<T> _canceled = CanceledTask();

        private static Task<T> CanceledTask()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        /// <summary>
        /// A <see cref="Task"/> that will never complete.
        /// </summary>
        public static Task<T> Never
        {
            get
            {
                return _never;
            }
        }

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task<T> Canceled
        {
            get
            {
                return _canceled;
            }
        }
    }
}
