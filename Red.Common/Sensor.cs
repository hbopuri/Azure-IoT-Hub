using System;

namespace Red.Common
{
    public class Sensor
    {
        public string Comment { get; set; }
        public DateTime ReadingDateTimeUtc { get; set; }
        public double Reading { get; set; }
        public int StatusCode { get; set; }
    }
}