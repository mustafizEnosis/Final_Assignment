using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BestSeller
{
    class Review
    {
        public enum Type { T_BOOK, T_AUTHOR };

        public Type type { get; set; }

        public string review { get; set; }

        public Review(Type ty, string rev)
        {
            type = ty;
            review = rev;
        }

    }
    class Book
    {
        //[JsonPropertyName("results.books.title")]
        public string title { get; set; }

        //[JsonPropertyName("results.books.author")]
        public string author { get; set; }

        //[JsonPropertyName("results.bestsellers_date")]
        public string date { get; set; }

        public Book(string Name, string Author, string Date)
        {
            title = Name;
            author = Author;
            date = Date;
        }
    }
}
