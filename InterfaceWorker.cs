using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BestSeller
{
    interface Iworker
    {
        abstract Task<List<string>> GetDataAsync(List<string> inputs);
        abstract Task runDataProcessingTasksAsync(List<String> stringTask);
        abstract Task processData(string inputJson);
        abstract Task<string> storeDataInFileAsync();
    }
}
