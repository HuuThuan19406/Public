using System;
using System.Threading.Tasks;

namespace Api.Models
{
    public class TaskArray
    {
        private readonly Task[] tasks;
        private int i = 0;

        public TaskArray(int length)
        {
            tasks = new Task[length];
        }

        public void AddAndStart(Task task)
        {
            tasks[i++] = task;
            task.Start();
        }

        public void AddAndStart(Action action)
        {
            AddAndStart(new Task(action));
        }

        public void WaitAll()
        {
            Task.WaitAll(tasks);
        }
    }
}