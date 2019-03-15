namespace Red.Common
{
    public class CloudCommand
    {
        public Signal Signal { get; set; }
        public string Message { get; set; }
    }

    public enum Signal
    {
        Empty,
        Something
    }
}