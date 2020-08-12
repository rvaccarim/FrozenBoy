using System;
using System.Collections.Generic;
using System.Text;

using u8 = System.Byte;
using u16 = System.UInt16;

#pragma warning disable IDE1006 // Naming Styles
namespace FrozenBoy {

    public class Registers {
        public FlagRegister flags = new FlagRegister();

        // 8 bit Real registers
        public u8 a { get; set; }
        public u8 b { get; set; }
        public u8 c { get; set; }
        public u8 d { get; set; }
        public u8 e { get; set; }
        public u8 f { get => flags.f; set => flags.f = value; }
        public u8 h { get; set; }
        public u8 l { get; set; }

        // 16 bit Virtual registers
        public u16 af {
            get {
                return (u16)((a << 8) | f);
            }

            set {
                a = ((u8)((value & 0xFF00) >> 8));
                f = ((u8)(value & 0xFF));
            }
        }

        public u16 bc {
            get {
                return (u16)((b << 8) | c);
            }

            set {
                b = ((u8)((value & 0xFF00) >> 8));
                c = ((u8)(value & 0xFF));
            }
        }

        public u16 de {
            get {
                return (u16)((d << 8) | e);
            }

            set {
                d = ((u8)((value & 0xFF00) >> 8));
                e = ((u8)(value & 0xFF));
            }
        }

        public u16 hl {
            get {
                return (u16)((h << 8) | l);
            }

            set {
                h = ((u8)((value & 0xFF00) >> 8));
                l = ((u8)(value & 0xFF));
            }
        }

    }
}
#pragma warning restore IDE1006 // Naming Styles