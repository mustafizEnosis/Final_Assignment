using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BestSeller
{
    class InputTakerService
    {
        IInterfaceInput Input;

        public InputTakerService (IInterfaceInput input)
        {
            this.Input = input;
        }

        public async Task TakeInputAsync()
        {
            try
            {
                await Input.TakeDateInput();
                await Input.TakeReviewInput();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }
    }
}
