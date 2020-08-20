using System;
using System.IO;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {
    public class Logger {
        private const string stateFormat = "{0}   {1}";

        private readonly StreamWriter logFile;
        private readonly string logFilename;

        public Logger(string logFilename) {
            this.logFilename = logFilename;
            logFile = new StreamWriter(this.logFilename);
        }

        ~Logger() {
            logFile.Flush();
            logFile.Close();
            logFile.Dispose();
        }

        public void LogState(string instruction, string cpuState) {
            logFile.WriteLine(String.Format(stateFormat, instruction, cpuState));
        }

    }
}
