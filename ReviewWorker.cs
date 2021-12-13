using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace BestSeller
{
    class ReviewWorker
    {
        public List<Review> reviewsList = new List<Review>();

        public string filename { get; }  = "review.txt";
        public async Task GetDataAsync(Review.Type T, string val)
        {
            string url = $"https://api.nytimes.com/svc/books/v3/reviews.json?api-key={StaticInfo.API_KEY}";
            if (T == Review.Type.T_BOOK)
                url += $"&title={val}";
            else if (T == Review.Type.T_AUTHOR)
                url += $"&author={val}";
            try
            {
                var stringTask = StaticInfo.client.GetStringAsync(url);

                var reviews = await stringTask;

                await Task.Run(() => {
                    processData(T, reviews);
                    //return reviewsList;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }

            //return reviewsList;

        }

        public void processData(Review.Type T, string json)
        {
            JsonDocument jdoc = JsonDocument.Parse(json);
            JsonElement root = jdoc.RootElement;
            JsonElement resultsElement = root.GetProperty("results");

            int count = resultsElement.GetArrayLength();
            Console.WriteLine(count);

            foreach (JsonElement result in resultsElement.EnumerateArray())
            {
                JsonElement summaryElement = result.GetProperty("summary");
                reviewsList.Add(new Review(T, summaryElement.GetString()));
            }
        }

        public async Task<string> storeDataInFileAsync()
        {
            return await Task.Run(() => {

                

                try
                {

                    StreamWriter sw = new StreamWriter(filename);

                    for (int i = 0; i < reviewsList.Count; i++)
                    {
                        sw.WriteLine("Review {0}", i + 1);
                        sw.WriteLine("__________");
                        sw.WriteLine(reviewsList[i].review);
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
