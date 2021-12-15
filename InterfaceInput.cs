using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BestSeller
{
    interface IInterfaceInput
    {
        abstract public Task TakeDateInput();
        abstract public Task TakeReviewInput();
    }
}
