using System;
using System.Collections.Generic;
using System.Text;

using u8 = System.Byte;
using u16 = System.UInt16;

#pragma warning disable IDE1006 // Naming Styles
namespace FrozenBoy {

    public class FlagRegister {
        public bool zero;
        public bool subtract;
        public bool half_carry;
        public bool carry;

        public u8 f {
            get {
                // 7, 6, 5, 4 are the bit positions
                return (u8)((zero ? 1 : 0) << 7 |
                            (subtract ? 1 : 0) << 6 |
                            (half_carry ? 1 : 0) << 5 |
                            (carry ? 1 : 0) << 4
                           );
            }

            set {
                //   11010000
                // & 10000000
                //   --------
                //   10000000 != 0, so the bit is set
                //
                // 7, 6, 5, 4 are the bit positions
                zero = (value & (1 << 7)) != 0;
                subtract = (value & (1 << 6)) != 0;
                half_carry = (value & (1 << 5)) != 0;
                carry = (value & (1 << 4)) != 0;
            }
        }

    }
}
#pragma warning disable IDE1006 // Naming Styles