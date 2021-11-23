using System;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Extension
{
    public static class TaskExtensions
    {
        public static void RunTask(Action action)
            => Task.Run(action).ConfigureAwait(false);

        public static void RunAndWaitTasks(params Action[] actions)
        {
            Task[] tasks = new Task[actions.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(actions[i]);
            }
            Task.WaitAll(tasks);
        }
        public static T WaitResult<T>(this Task<T> task)
            => task.GetAwaiter().GetResult();
    }
}
