
namespace FrozenBoyCore {
    public enum TestOutput { None, LinkPort, Memory, MD5 }

    public class TestOptions(TestOutput testOutput, bool logExecution, string logFilename)
    {
        public TestOutput testOutput = testOutput;
        public bool logExecution = logExecution;
        public string logFilename = logFilename;
    }
}
