namespace FrozenBoyTest
{
    public class MD5_Item(string hash, bool passed)
    {
        public string Hash { get; set; } = hash;
        public bool Passed { get; set; } = passed;
    }
}
