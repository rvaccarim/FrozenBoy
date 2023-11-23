using System.Runtime.CompilerServices;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Memory
{
    public class Space(u16 from, u16 toInclusive)
    {
        private readonly u8[] data = new u8[toInclusive - from + 1];
        private readonly u16 from = from;
        private readonly u16 toInclusive = toInclusive;

        public byte this[u16 address] {
            get {
                return data[address - from];
            }
            set {
                data[address - from] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Manages(u16 address) {
            if (address >= from && address <= toInclusive) {
                return true;
            }
            return false;
        }

    }
}
