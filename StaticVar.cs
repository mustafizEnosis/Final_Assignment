using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace BestSeller
{
    static class StaticInfo
    {
        public const string API_KEY = "rMPW0Jpp7ZEzCD5NBNNAaRInu8ToCBRy";
        public const string listType = "hardcover-fiction";
        public static readonly HttpClient client = new HttpClient();
    }
}
