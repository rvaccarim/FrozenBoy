using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyTest {
    public class MD5_Progress {
        public MD5_Progress(string md5, bool passed) {
            Md5 = md5;
            Passed = passed;
        }

        public string Md5 { get; set; }
        public bool Passed { get; set; }
    }
}
