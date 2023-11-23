namespace FrozenBoyTest
{
    public class Result(bool passed, string message)
    {
        public bool Passed { get; set; } = passed;
        public string Message { get; set; } = message;
    }
}
