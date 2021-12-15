using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace BestSeller
{
    class ReviewWorker : Iworker
    {
        public List<Review> ReviewsList = new List<Review>();

        public Review.Type T { get; set; }

        public string Filename { get; } = "review.txt";

        public ReviewWorker(Review.Type T)
        {
            this.T = T;
        }

        string GetUrl(string val)
        {
            string url = $"https://api.nytimes.com/svc/books/v3/reviews.json?api-key={StaticInfo.API_KEY}";
            if (T == Review.Type.T_BOOK)
                url += $"&title={val}";
            else if (T == Review.Type.T_AUTHOR)
                url += $"&author={val}";

            return url;
        }
        public async Task<List<string>> GetDataAsync(List<string> inputs)
        {
            List<string> results = new List<string>();
            try
            {
                var stringTask = from input in inputs select StaticInfo.client.GetStringAsync(GetUrl(input));

                await Task.WhenAll(stringTask);

                foreach (var task in stringTask)
                {
                    results.Add(((Task<string>)task).Result);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }

            return results;

        }

        public async Task runDataProcessingTasksAsync(List<String> stringTask)
        {
            List<Task> processingTasks = new List<Task>();

            foreach (var task in stringTask)
            {
                processingTasks.Add(processData(task));
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("In func, no thread has been launched. (main thread={0})",
             Thread.CurrentThread.ManagedThreadId);

            await Task.WhenAll(processingTasks);

            Console.WriteLine("In func, thread has been launched. (main thread={0})",
             Thread.CurrentThread.ManagedThreadId);

            sw.Stop();

            Console.WriteLine("{0} elapsed time",
            sw.ElapsedMilliseconds);

            //using var processingtasks = from task in stringTask select processDataAsync(task.Result);
        }

        public async Task processData(string json)
        {
            try
            {
                await Task.Run(() => {
                    JsonDocument jdoc = JsonDocument.Parse(json);
                    JsonElement root = jdoc.RootElement;
                    JsonElement resultsElement = root.GetProperty("results");

                    int count = resultsElement.GetArrayLength();
                    Console.WriteLine(count);

                    foreach (JsonElement result in resultsElement.EnumerateArray())
                    {
                        JsonElement summaryElement = result.GetProperty("summary");
                        ReviewsList.Add(new Review(T, summaryElement.GetString()));
                    }
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        public async Task<string> storeDataInFileAsync()
        {
            return await Task.Run(() => {



                try
                {

                    StreamWriter sw = new StreamWriter(Filename);

                    for (int i = 0; i < ReviewsList.Count; i++)
                    {
                        sw.WriteLine("Review {0}", i + 1);
                        sw.WriteLine("__________");
                        sw.WriteLine(ReviewsList[i].review);
                    }
                    sw.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }


                return Filename;
            });
        }
    }
}
