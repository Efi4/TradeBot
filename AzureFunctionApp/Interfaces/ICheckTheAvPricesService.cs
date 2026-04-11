using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureFunctionApp.Interfaces
{
    public interface ICheckTheAvPricesService
    {
        Task<CheckPricesResult> CheckPricesAsync();
    }

    public class CheckPricesResult
    {
        public bool Success { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public int ItemsChecked { get; set; }
        public int DealsFound { get; set; }
        public DateTime CheckedAt { get; set; }
    }
}

