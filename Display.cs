using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace BestSeller
{
    class Display
    {
        public void openEditor(string filename)
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }
    }
}
