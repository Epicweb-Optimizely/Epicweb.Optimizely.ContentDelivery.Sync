using System;
using System.Net;

namespace Epicweb.Optimizely.ContentDelivery.Sync.Models
{

    public class ErrorMessage
    {
        public Error error { get; set; }
    }

    public class Error
    {
        public string code { get; set; }
        public string message { get; set; }
        public string target { get; set; }
        public string[] details { get; set; }
    }

}
