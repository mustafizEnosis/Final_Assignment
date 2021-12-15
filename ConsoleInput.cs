using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BestSeller
{
    class ConsoleInput : IInterfaceInput
    {
        public async Task TakeDateInput()
        {
            List<string> inputDates = new List<string>();


            Console.WriteLine("Please input the number of days of data you want.");
            int dateCnt = -1;
            try
            {
                dateCnt = Int32.Parse(Console.ReadLine());
                Console.WriteLine(dateCnt);
            }
            catch (FormatException)
            {
                Console.WriteLine($"Invalid input. Please enter integer without any leading or trailing spaces.");
            }

            Console.WriteLine("Please enter the dates in this (YYYY-MM-DD) format one by one without any leading or trailing spaces.");

            while (dateCnt > 0)
            {
                try
                {
                    string date = Console.ReadLine();
                    if (Regex.IsMatch(date, @"^\d{4}-\d{2}-\d{2}$"))
                        inputDates.Add(date);
                    else throw new Exception("Invalid Input!!!");
                    dateCnt--;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
            }

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
            Console.WriteLine(" {0}", sw.ElapsedMilliseconds);


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

                Display disp = new Display();

                disp.openEditor(filename);

            }


            inputDates.Clear();

        }


        public async Task TakeReviewInput()
        {
            Console.WriteLine("If you want to get any review then please enter \"r\". \n " +
                   "If you want to continue then please enter \"c\". \n" +
                   "If you want to quit the application then please enter \"q\". \n");

            string input = Console.ReadLine();

            if (input.ToLower() == "r")
            {
                Console.WriteLine("Please enter 1 for book review and 2 for author review");

                string option = Console.ReadLine().Trim(' ');

                if (option == "1")
                    Console.WriteLine("reading books for review");
                else if (option == "2")
                    Console.WriteLine("reading authors for review");
                else
                {
                    Console.WriteLine("Invalid Input !!!");
                    return;
                }
                List<string> inputs = new List<string>();

                ReviewWorker rw = new ReviewWorker(option == "1" ? Review.Type.T_BOOK : Review.Type.T_AUTHOR);
                Console.WriteLine("Please enter the book name");
                inputs.Add(Console.ReadLine());
                Console.WriteLine("Please wait, fetching data....");
                var results = await rw.GetDataAsync(inputs);
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

         

                inputs.Clear();
            }
            else if (input.ToLower() == "q")
            {
                return;
            }
            else
            {
                Console.WriteLine("Invalid Input!!!");
            }
        }
    }
}
