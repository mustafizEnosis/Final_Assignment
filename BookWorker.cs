using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace BestSeller
{
    class BookWorker
    {
        
        public int max_book_length { get; set; } = int.MinValue;
        public int max_author_length { get; set; } = int.MinValue;

        public string filename { get; } = "output.txt";

        public List<Book> booksList = new List<Book>();

        public async Task GetDataAsync(List<string> dates)
        {
            var stringTask = from date in dates
                             select StaticInfo.client.GetStringAsync($"https://api.nytimes.com/svc/books/v3/lists.json?api-key={StaticInfo.API_KEY}&published-date={date}&list={StaticInfo.listType}").ContinueWith(t=> { processData(t.Result); });

            try
            {
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
               
                Console.WriteLine("In func, no thread has been launched. (main thread={0})",
                 Thread.CurrentThread.ManagedThreadId);

                await Task.WhenAll(stringTask);

                Console.WriteLine("In func, thread has been launched. (main thread={0})",
                 Thread.CurrentThread.ManagedThreadId);


                /* Try to ensure all the result is obtained. If any result is not obtained then mention reason for that */


                //Console.Write(books);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());

                //return stringTask;
            }

            //return stringTask.ToList();
        }

        //public async void runDataProcessingTasksAsync(IAsyncEnumerable<Task<String>> stringTask)
        //{
        //    List<Task> processingTasks = new List<Task>();

        //    await foreach(Task T in stringTask)
        //    {
        //        await processDataAsync(T.Result)
        //    }
            
        //    //using var processingtasks = from task in stringTask select processDataAsync(task.Result);
        //}

        public void processData(string inputJson)
        {
            try
            {
                //await Task.Run(() =>
                //{
                    JsonDocument jdoc = JsonDocument.Parse(inputJson);
                    JsonElement root = jdoc.RootElement;
                    JsonElement resultsElement = root.GetProperty("results");

                    int count = resultsElement.GetArrayLength();
                    Console.WriteLine(count);

                    foreach (JsonElement result in resultsElement.EnumerateArray())
                    {
                        JsonElement dateElement = result.GetProperty("bestsellers_date");
                        JsonElement booksElement = result.GetProperty("book_details");
                        foreach (JsonElement book in booksElement.EnumerateArray())
                        {
                            JsonElement title = book.GetProperty("title");
                            JsonElement author = book.GetProperty("author");

                            max_book_length = Math.Max(max_book_length, title.GetString().Length);
                            max_author_length = Math.Max(max_author_length, author.GetString().Length);

                            booksList.Add(new Book(title.GetString(), author.GetString(), dateElement.GetString()));
                        }

                    //Console.WriteLine(title.GetString());
                    }
                //});
            }
            catch(Exception e) { Console.WriteLine(e.Message.ToString()); }
            
        }

        public async Task<string> storeDataInFileAsync()
        {
            return await Task.Run(() => {

                try
                {

                    StreamWriter sw = new StreamWriter(filename);

                    sw.WriteLine("Book".PadRight(max_book_length + 1) + "Author".PadRight(max_author_length + 1) + "Date");
                    sw.WriteLine("____".PadRight(max_book_length + 1) + "____".PadRight(max_author_length + 1) + "_____");

                    for (int i = 0; i < booksList.Count; i++)
                    {
                        sw.WriteLine(booksList[i].title.PadRight(max_book_length + 1) + booksList[i].author.PadRight(max_author_length + 1) + booksList[i].date);
                    }
                    sw.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }

                return filename;
            });
        }


    }
}
