using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DrawBridge
{
    public class EventBarrier
    {
        // Define semaphore object here
        private static Semaphore semaphore = new Semaphore(0, 20);
        private static int count = 0;
        public void arrive()
        {
            Console.WriteLine("Traveller arrived!");
            count++;
            // Lock the threads here
            semaphore.WaitOne();
        }

        public void raise()
        {
            // Producer has raised the event; release the lock
            Console.WriteLine("Gatekeeper opens the bridge!");
            semaphore.Release(count);

        }

        public void complete()
        {
            // When all the tasks complete, reaquire the lock
            
            Console.WriteLine("Traveller has entered the castle");
            count--;
            if(count == 0)
            {
                Console.WriteLine("All the travellers finished crossing the bridge. Gate is closed.");
                try
                {
                    Semaphore.OpenExisting("semaphore");
                }
                catch(Exception e)
                {
                    // Console.WriteLine(e.ToString());
                    semaphore = new Semaphore(0, 20);
                }
            }
        }

        public int waiters()
        {
            return count;
        }
    }

    // Threads
    public class ProducerConsumer
    { 

        static void Main(string[] args)
        {
            EventBarrier EB = new EventBarrier();
            Action action = () =>
            {
                Console.WriteLine("Producer is going to raise the event");
                Task.Delay(1000);
                EB.raise();
            };

            //First set of tasks
            List<Task> tasks = new List<Task>();
            for(int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
               {
                   EB.arrive();
               })
               .ContinueWith(t =>
               {
                   EB.complete();
               }));
            }

            // Call producer thread to raise the event
            Task producer = new Task(action);
            producer.Start();

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("All travellers from the first set crossed the bridge.");

            // Second set of tasks
            List<Task> tasks1 = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks1.Add(Task.Run(() =>
                {
                    EB.arrive();
                })
               .ContinueWith(t =>
               {
                   EB.complete();
               }));
            }

            // Producer raises the event for the second time
            Task producer1 = new Task(action);
            producer1.Start();

            Task.WaitAll(tasks1.ToArray());
            Console.WriteLine("All travellers from the second set crossed the bridge.");

        }

    }
}
