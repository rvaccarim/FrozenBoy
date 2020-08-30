using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public enum TestOutput { None, LinkPort, Memory }

    public class TestOptions {
        public TestOutput testOutput;
        public bool logExecution;
        public string logFilename;

        public TestOptions(TestOutput testOutpt, bool logExecution, string logFilename) {
            this.testOutput = testOutpt;
            this.logExecution = logExecution;
            this.logFilename = logFilename;
        }
    }
}
