using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BestSeller
{
    class Program
    {
      
        static async Task Main(string[] args)
        {
            Console.WriteLine("*****Welcome to the bestseller application*****");

            ConsoleInput CInput = new ConsoleInput();
            //FileInput FInput = new FileInput("inputDates.txt", "inputReviews.txt");

            InputTakerService InputService = new InputTakerService(CInput);

            await InputService.TakeInputAsync();
        }
    }
}
