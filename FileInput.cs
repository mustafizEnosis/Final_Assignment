using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BestSeller
{
    class FileInput : IInterfaceInput
    {
        public string DateFilename { get; }
        public string ReviewFilename { get; }

        public FileInput(string DateFilename, string ReviewFilename)
        {
            this.DateFilename = DateFilename;
            this.ReviewFilename = ReviewFilename;
        }

        public async Task TakeDateInput()
        {
            StreamReader reader = new StreamReader(DateFilename);
            List<string> inputDates = new List<string>();

            Console.WriteLine("reading dates from file");

            while (!reader.EndOfStream)
            {
                string date = reader.ReadLine();

                try
                {
                    if (Regex.IsMatch(date, @"^\d{4}-\d{2}-\d{2}$"))
                        inputDates.Add(date);
                    else throw new Exception("Invalid Input!!!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
            }

            Console.WriteLine(inputDates.Count);

            BookWorker bw = new BookWorker();

            Console.WriteLine("Please wait, fetching data....");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("no thread has been launched. (main thread={0})",
             Thread.CurrentThread.ManagedThreadId);
            var tasks = await bw.GetDataAsync(inputDates);
            Console.WriteLine("thread has been launched. (main thread={0})",
             Thread.CurrentThread.ManagedThreadId);
            sw.Stop();
            //Console.WriteLine("{0} {1}", i + 1, sw.ElapsedMilliseconds);

            sw.Start();
            Console.WriteLine("no thread has been launched. (main thread={0})",
             Thread.CurrentThread.ManagedThreadId);
            await bw.runDataProcessingTasksAsync(tasks);
            Console.WriteLine("thread has been launched. (main thread={0})",
             Thread.CurrentThread.ManagedThreadId);
            sw.Stop();
            Console.WriteLine(" {0} ", sw.ElapsedMilliseconds);


            //System.Threading.Thread.Sleep(10000);

            if (bw.BooksList.Count == 0)
                Console.WriteLine("No data found");
            else
            {
                Console.WriteLine("Total {0} data fetched.", bw.BooksList.Count);
                //for (int i = 0; i < list.Count; i++)
                //    Console.WriteLine("Id = {0} \t\t Info = {1}", i, list[i].title);
                Console.WriteLine("Please wait, preparing data....");
                var filename = await bw.storeDataInFileAsync();
                Console.WriteLine("Displaying data in default editor");

                //make this abstracted

                Display disp = new Display();

                disp.openEditor(filename);

            }

            inputDates.Clear();
        }

        public async Task TakeReviewInput()
        {
            StreamReader reader = new StreamReader(ReviewFilename);
            List<string> inputReviews = new List<string>();

            int type = Int32.Parse(reader.ReadLine());

            if (type == 1)
                Console.WriteLine("reading books for review");
            else if (type == 2)
                Console.WriteLine("reading authors for review");
            else
            {
                Console.WriteLine("Invalid Input !!!");
                return;
            }

            ReviewWorker rw = new ReviewWorker(type == 1 ? Review.Type.T_BOOK : Review.Type.T_AUTHOR);

            while (!reader.EndOfStream)
            {

                inputReviews.Add(reader.ReadLine());
            }
           
            Console.WriteLine("Please wait, fetching data....");
            var results = await rw.GetDataAsync(inputReviews);
            await rw.runDataProcessingTasksAsync(results);
            if (rw.ReviewsList.Count == 0)
                Console.WriteLine("No review found");
            else
            {
                Console.WriteLine("Please wait, preparing data....");
                string filename = await rw.storeDataInFileAsync();
                Display disp = new Display();
                disp.openEditor(filename);
            }
        }
    }
}
