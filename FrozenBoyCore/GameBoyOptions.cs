using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public enum LogMode { Basic, Time, Full }
    public enum TestOutput { LinkPort, Memory }

    public class GameBoyOptions {
        public bool testingMode;
        public TestOutput testOutput;
        public bool logExecution;
        public string logFilename;
        public LogMode logMode;

        public GameBoyOptions(bool testingMode, TestOutput testOutpt, bool logExecution, string logFilename, LogMode logMode) {
            this.testingMode = testingMode;
            this.testOutput = testOutpt;
            this.logExecution = logExecution;
            this.logFilename = logFilename;
            this.logMode = logMode;
        }
    }
}
