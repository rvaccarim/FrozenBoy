using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyTest {
    public class MD5_Item {
        public MD5_Item(string hash, bool passed) {
            Hash = hash;
            Passed = passed;
        }

        public string Hash { get; set; }
        public bool Passed { get; set; }
    }
}
