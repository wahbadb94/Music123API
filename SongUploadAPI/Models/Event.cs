using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Models
{
    public class Event<T> where T : class
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public string Subject { get; set; }
        public DateTime EventTime { get; set; }
        public T Data { get; set; }
        public string Topic { get; set; }
    }
}
