using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public enum TestOutput { None, LinkPort, Memory, MD5 }

    public class TestOptions {
        public TestOutput testOutput;
        public string expectedMD5;
        public bool logExecution;
        public string logFilename;

        public TestOptions(TestOutput testOutput, string expectedMD5, bool logExecution, string logFilename) {
            this.testOutput = testOutput;
            this.expectedMD5 = expectedMD5;
            this.logExecution = logExecution;
            this.logFilename = logFilename;
        }
    }
}
