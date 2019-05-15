using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class RecognitionResultModel
    {
        public int errorCode { set; get; }
        public string errorDescription { set; get; }
        public string artist { set; get; }
    }
}
