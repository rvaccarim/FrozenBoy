using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyTest {
    public class Result {
        public Result(bool passed, string message) {
            Passed = passed;
            Message = message;
        }

        public bool Passed { get; set; }
        public string Message { get; set; }
    }
}
