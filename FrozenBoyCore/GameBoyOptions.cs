using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public enum TestOutput { LinkPort, Memory }

    public class GameBoyOptions {
        public bool testingMode;
        public TestOutput testOutput;
        public bool logExecution;
        public string logFilename;

        public GameBoyOptions(bool testingMode, TestOutput testOutpt, bool logExecution, string logFilename) {
            this.testingMode = testingMode;
            this.testOutput = testOutpt;
            this.logExecution = logExecution;
            this.logFilename = logFilename;
        }
    }
}
