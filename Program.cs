using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Diagnostics;
using BestSellerHelper;

namespace BestSeller
{
    class Program
    {
        const string API_KEY = "rMPW0Jpp7ZEzCD5NBNNAaRInu8ToCBRy";
        const string listType = "hardcover-fiction";
        private static readonly HttpClient client = new HttpClient();

        struct st
        {
            public int max_book_length;
            public int max_author_length;
        };

        static st maxInfo;

        private static async Task<List<Review>> GetReviewsAsync(Review.Type T, string val)
        {
            List<Review> reviewsList = new List<Review>();
            string url = $"https://api.nytimes.com/svc/books/v3/reviews.json?api-key={API_KEY}";
            if (T == Review.Type.T_BOOK)
                url += $"&title={val}";
            else if(T == Review.Type.T_AUTHOR)
                url += $"&author={val}";
            try
            {
                var stringTask = client.GetStringAsync(url);

                var reviews = await stringTask;

                return await Task.Run(() => {
                    JsonDocument jdoc = JsonDocument.Parse(reviews);
                    JsonElement root = jdoc.RootElement;
                    JsonElement resultsElement = root.GetProperty("results");

                    int count = resultsElement.GetArrayLength();
                    Console.WriteLine(count);

                    foreach (JsonElement result in resultsElement.EnumerateArray())
                    {
                        JsonElement summaryElement = result.GetProperty("summary");
                        reviewsList.Add(new Review(T, summaryElement.GetString()));
                    }
                    return reviewsList;
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }

            return reviewsList;

        }
        private static async Task<List<Book>> GetBestSellerBooksAsync(List<string> dates)
        {
            List<Book> booksList = new List<Book>();
            try
            {
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                var stringTask = from date in dates select client.GetStringAsync($"https://api.nytimes.com/svc/books/v3/lists.json?api-key={API_KEY}&published-date={date}&list={listType}").ContinueWith(t => {

                    //Stopwatch sw = new Stopwatch();
                    //sw.Start();
                    JsonDocument jdoc = JsonDocument.Parse(t.Result);
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

                            maxInfo.max_book_length = Math.Max(maxInfo.max_book_length, title.GetString().Length);
                            maxInfo.max_author_length = Math.Max(maxInfo.max_author_length, author.GetString().Length);

                            booksList.Add(new Book(title.GetString(), author.GetString(), dateElement.GetString()));
                        }

                        //Console.WriteLine(title.GetString());
                    }

             //       Console.WriteLine("In deserialization, a thread has been launched. (main thread={0})",
             //Thread.CurrentThread.ManagedThreadId);

             //       sw.Stop();
             //       Console.WriteLine(sw.ElapsedMilliseconds);


                });
                //sw.Stop();
                //Console.WriteLine(sw.ElapsedMilliseconds);

                
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                Console.WriteLine("In func, no thread has been launched. (main thread={0})",
                 Thread.CurrentThread.ManagedThreadId);

                await Task.WhenAll(stringTask);

                Console.WriteLine("In func, thread has been launched. (main thread={0})",
                 Thread.CurrentThread.ManagedThreadId);

                //foreach (var task in stringTask)
                //{
                //    await task.ContinueWith(t => {
                //        Stopwatch sw = new Stopwatch();
                //        sw.Start();
                //        JsonDocument jdoc = JsonDocument.Parse(t.Result);
                //        JsonElement root = jdoc.RootElement;
                //        JsonElement resultsElement = root.GetProperty("results");

                //        int count = resultsElement.GetArrayLength();
                //        Console.WriteLine(count);

                //        foreach (JsonElement result in resultsElement.EnumerateArray())
                //        {
                //            JsonElement dateElement = result.GetProperty("bestsellers_date");
                //            JsonElement booksElement = result.GetProperty("book_details");
                //            foreach (JsonElement book in booksElement.EnumerateArray())
                //            {
                //                JsonElement title = book.GetProperty("title");
                //                JsonElement author = book.GetProperty("author");

                //                maxInfo.max_book_length = Math.Max(maxInfo.max_book_length, title.GetString().Length);
                //                maxInfo.max_author_length = Math.Max(maxInfo.max_author_length, author.GetString().Length);

                //                booksList.Add(new Book(title.GetString(), author.GetString(), dateElement.GetString()));
                //            }

                //            //Console.WriteLine(title.GetString());
                //        }

                //        Console.WriteLine("In deserialization, a thread has been launched. (main thread={0})",
                // Thread.CurrentThread.ManagedThreadId);

                //        sw.Stop();
                //        Console.WriteLine(sw.ElapsedMilliseconds);

                //    }).ConfigureAwait(false);
                //}

                //sw.Stop();
                //Console.WriteLine(sw.ElapsedMilliseconds);


                /* Try to ensure all the result is obtained. If any result is not obtained then mention reason for that */

                return booksList;

                //Console.Write(books);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());

                return booksList;
            }
        }

        private static async Task<string> storeDataAsync(List<Book> books)
        {
            return await Task.Run(() => {

                string filename = "output.txt";

                try
                {

                    StreamWriter sw = new StreamWriter(filename);

                    sw.WriteLine("Book".PadRight(maxInfo.max_book_length + 1) + "Author".PadRight(maxInfo.max_author_length + 1) + "Date");
                    sw.WriteLine("____".PadRight(maxInfo.max_book_length + 1) + "____".PadRight(maxInfo.max_author_length + 1) + "_____");

                    for (int i = 0; i < books.Count; i++)
                    {
                        sw.WriteLine(books[i].title.PadRight(maxInfo.max_book_length + 1) + books[i].author.PadRight(maxInfo.max_author_length + 1) + books[i].date);
                    }
                    sw.Close();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }

                return filename;
            });
        }
        
        private static async Task<string> storeReviewsAsync(List<Review> reviews)
        {
            return await Task.Run(() => {

                string filename = "review.txt";

                try 
                { 

                    StreamWriter sw = new StreamWriter(filename);

                    for (int i = 0; i < reviews.Count; i++)
                    {
                        sw.WriteLine("Review {0}", i+1);
                        sw.WriteLine("__________");
                        sw.WriteLine(reviews[i].review);
                    }
                    sw.Close();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }


                return filename;
            });
        }

        private static void openEditor(string filename)
        {
            try
            {
                new Process
                {
                    StartInfo = new ProcessStartInfo(filename)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        private static Random gen = new Random();
        private static DateTime RandomDay()
        {
            DateTime start = new DateTime(2010, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(gen.Next(range));
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine("*****Welcome to the bestseller application*****");
            List<string> inputDates = new List<string>();
            int i = 0;
            StreamReader reader = new StreamReader("input.txt");
            //client.DefaultRequestHeaders.Add("Retry-After", "120");
            //for (int k = 1; k <= 10; k++)
            //    inputDates.Add(RandomDay().Date.ToString("yyyy-MM-dd"));
            
            while (true)
            {
                
                for (int k=1; k<=2; k++)
                    inputDates.Add(reader.ReadLine());
                //Console.WriteLine("Please input the number of days of data you want.");
                //int dateCnt = -1;
                //try
                //{
                //    dateCnt = Int32.Parse(Console.ReadLine());
                //    Console.WriteLine(dateCnt);
                //}
                //catch (FormatException)
                //{
                //    Console.WriteLine($"Invalid input. Please enter integer without any leading or trailing spaces.");
                //}

                //Console.WriteLine("Please enter the dates in this (YYYY-MM-DD) format one by one without any leading or trailing spaces.");

                //while (dateCnt>0)
                //{
                //    try
                //    {
                //        string date = Console.ReadLine();
                //        if (Regex.IsMatch(date, @"^\d{4}-\d{2}-\d{2}$"))
                //            inputDates.Add(date);
                //        else throw new Exception("Invalid Input!!!");
                //        dateCnt--;
                //    }
                //    catch(Exception e)
                //    {
                //        Console.WriteLine(e.Message.ToString());
                //    }
                //}

                //maxInfo.max_book_length = int.MinValue;
                //maxInfo.max_author_length = int.MinValue;

                Console.WriteLine("Please wait, fetching data....");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Console.WriteLine("no thread has been launched. (main thread={0})",
                 Thread.CurrentThread.ManagedThreadId);
                var list = await GetBestSellerBooksAsync(inputDates);
                Console.WriteLine("thread has been launched. (main thread={0})",
                 Thread.CurrentThread.ManagedThreadId);
                sw.Stop();
                Console.WriteLine("{0} {1}", i + 1, sw.ElapsedMilliseconds);


                //System.Threading.Thread.Sleep(10000);

                //if (list.Count == 0)
                //    Console.WriteLine("No data found");
                //else
                //{
                //    Console.WriteLine("Total {0} data fetched.", list.Count);
                //    //for (int i = 0; i < list.Count; i++)
                //    //    Console.WriteLine("Id = {0} \t\t Info = {1}", i, list[i].title);
                //    Console.WriteLine("Please wait, preparing data....");
                //    var filename = await storeDataAsync(list);
                //    Console.WriteLine("Displaying data in default editor");

                //    openEditor(filename);

                //}
                //i += 1;
                //if (i == 9) break;
                break;
                //inputDates.Clear();


                //while (true)
                //{
                //    Console.WriteLine("If you want to get any review then please enter \"r\". \n " +
                //   "If you want to continue then please enter \"c\". \n" +
                //   "If you want to quit the application then please enter \"q\". \n");

                //    string input = Console.ReadLine();

                //    if (input.ToLower() == "r")
                //    {
                //        Console.WriteLine("Please enter 1 for book review and 2 for author review");

                //        string option = Console.ReadLine().Trim(' ');

                //        if(option == "1")
                //        {
                //            Console.WriteLine("Please enter the book name");
                //            string bookName = Console.ReadLine();
                //            Console.WriteLine("Please wait, fetching data....");
                //            var reviewList = await GetReviewsAsync(Review.Type.T_BOOK, bookName);
                //            if (reviewList.Count == 0)
                //                Console.WriteLine("No review found");
                //            else
                //            {
                //                Console.WriteLine("Please wait, preparing data....");
                //                string filename = await storeReviewsAsync(reviewList);
                //                openEditor(filename);
                //            }
                           
                //        }
                //        else if(option == "2")
                //        {
                //            Console.WriteLine("Please enter the author name");
                //            string authorName = Console.ReadLine();
                //            Console.WriteLine("Please wait, fetching data....");
                //            var reviewList = await GetReviewsAsync(Review.Type.T_AUTHOR, authorName);
                //            if(reviewList.Count == 0)
                //            {
                //                Console.WriteLine("No review found");
                //            }
                //            else
                //            {
                //                Console.WriteLine("Please wait, preparing data....");
                //                string filename = await storeReviewsAsync(reviewList);
                //                openEditor(filename);
                //            }
                            
                //        }
                //        else
                //        {
                //            Console.WriteLine("Invalid Input!!!");
                //        }
                //    }
                //    else if (input.ToLower() == "q")
                //    {
                //        return;
                //    }
                //    else if (input.ToLower() == "c")
                //    {
                //        break;
                //    }
                //    else
                //    {
                //        Console.WriteLine("Invalid Input!!!");
                //    }
                //}
            }

            //writer.Close();

        }
    }
}
