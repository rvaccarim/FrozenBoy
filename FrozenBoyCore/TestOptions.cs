using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public enum TestOutput { None, LinkPort, Memory, MD5 }

    public class TestOptions {
        public TestOutput testOutput;
        public bool logExecution;
        public string logFilename;

        public TestOptions(TestOutput testOutput, bool logExecution, string logFilename) {
            this.testOutput = testOutput;
            this.logExecution = logExecution;
            this.logFilename = logFilename;
        }
    }
}
