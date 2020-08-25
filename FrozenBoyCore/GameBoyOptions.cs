using System;
using System.Collections.Generic;
using System.Text;

namespace FrozenBoyCore {
    public enum LogMode { Basic, Time, Full }

    public class GameBoyOptions {
        public bool testingMode;
        public bool logExecution;
        public string logFilename;
        public LogMode logMode;

        public GameBoyOptions(bool testingMode, bool logExecution, string logFilename, LogMode logMode) {
            this.testingMode = testingMode;
            this.logExecution = logExecution;
            this.logFilename = logFilename;
            this.logMode = logMode;
        }
    }
}
