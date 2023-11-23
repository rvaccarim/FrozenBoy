using u8 = System.Byte;

namespace FrozenBoyCore.Processor
{
    public delegate void Step();

    public class Opcode(u8 value, string label, int length, int tcycles, Step[] steps)
    {
        public u8 value = value;
        public string label = label;
        public int length = length;
        public int tcycles = tcycles;
        public int mcycles = tcycles / 4;
        public Step[] steps = steps;
    }
}
