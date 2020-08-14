using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    public class Opcodes {

        public static Dictionary<byte, Opcode> unprefixed = new Dictionary<byte, Opcode> {
            // (m, r) 
            // m = Memory, r = Registers

            // NOP
            { 0x00, new Opcode(0x00, "NOP", 1, (m, r) => { })},

            // INC
            { 0x04, new Opcode(0x04, "INC B", 1, (m, r) => { INC(ref r.B, r); })},
            { 0x0C, new Opcode(0x0C, "INC C", 1, (m, r) => { INC(ref r.C, r); })},
            { 0x14, new Opcode(0x14, "INC D", 1, (m, r) => { INC(ref r.D, r); })},
            { 0x1C, new Opcode(0x1C, "INC E", 1, (m, r) => { INC(ref r.E, r); })},
            { 0x24, new Opcode(0x24, "INC H", 1, (m, r) => { INC(ref r.H, r); })},
            { 0x3C, new Opcode(0x3C, "INC A", 1, (m, r) => { INC(ref r.A, r); })},

            // INC XX
            { 0x03, new Opcode(0x03, "INC BC",   1, (m, r) => { r.BC++; })},
            { 0x13, new Opcode(0x13, "INC DE",   1, (m, r) => { r.DE++; })},
            { 0x23, new Opcode(0x23, "INC HL",   1, (m, r) => { r.HL++; })},
            { 0x33, new Opcode(0x33, "INC SP",   1, (m, r) => { r.SP++; })},

            // RETURN
            { 0xC9, new Opcode(0xC9, "RET", 1, (m, r) => { })},

            { 0x05, new Opcode(0x05, "DEC B", 1, (m, r) => Opcode_0x05(m, r))},
            { 0x06, new Opcode(0x06, "LD B,${0:x2}", 2, (m, r) => Opcode_0x06(m, r))},

            { 0x0D, new Opcode(0x0D, "DEC C", 1, (m, r) => Opcode_0x0D(m, r))},
            { 0x0E, new Opcode(0x0E, "LD C,${0:x2}", 2, (m, r) => Opcode_0x0E(m, r))},
            { 0x11, new Opcode(0x11, "LD DE,${1:x2}{0:x2}", 3, (m, r) => Opcode_0x11(m, r))},
            { 0x15, new Opcode(0x15, "DEC D", 1, (m, r) => Opcode_0x15(m, r))},
            { 0x16, new Opcode(0x16, "LD D,${0:x2}", 2, (m, r) => Opcode_0x16(m, r))},
            { 0x17, new Opcode(0xC1, "RLA", 1, (m, r) => Opcode_0x17(m, r))},
            { 0x18, new Opcode(0x18, "JR Addr_{0:X4}", 2, (m, r) => Opcode_0x18(m, r))},
            { 0x1A, new Opcode(0x1A, "LD A,(DE)", 1, (m, r) => Opcode_0x1A(m, r))},
            { 0x1D, new Opcode(0x1D, "DEC E", 1, (m, r) => Opcode_0x1D(m, r))},
            { 0x1E, new Opcode(0x1E, "LD E,${0:x2}", 2, (m, r) => Opcode_0x1E(m, r))},
            { 0x20, new Opcode(0x20, "JR NZ, Addr_{0:X4}", 2, (m, r) => Opcode_0x20(m, r))},
            { 0x21, new Opcode(0x21, "LD HL,${1:x2}{0:x2}", 3, (m, r) => Opcode_0x21(m, r))},
            { 0x22, new Opcode(0x22, "LD (HL+),A", 1, (m, r) => Opcode_0x22(m, r))},

            { 0x28, new Opcode(0x28, "JR Z, Addr_{0:X4}", 2, (m, r) => Opcode_0x28(m, r))},
            { 0x2E, new Opcode(0x2E, "LD L,${0:x2}", 2, (m, r) => Opcode_0x2E(m, r))},
            { 0x31, new Opcode(0x31, "LD SP,${1:x2}{0:x2}", 3, (m, r) => Opcode_0x31(m, r))},
            { 0x32, new Opcode(0x32, "LD (HL-),A", 1, (m, r) => Opcode_0x32(m, r))},

            { 0x3D, new Opcode(0x3D, "DEC A", 1, (m, r) => Opcode_0x3D(m, r))},
            { 0x3E, new Opcode(0x3E, "LD A,${0:x2}", 2, (m, r) => Opcode_0x3E(m, r))},
            { 0x4F, new Opcode(0x4F, "LD C,A ", 1, (m, r) => Opcode_0x4F(m, r))},
            { 0x57, new Opcode(0x57, "LD D,A", 1, (m, r) => Opcode_0x57(m, r))},
            { 0x67, new Opcode(0x67, "LD H,A", 1, (m, r) => Opcode_0x67(m, r))},
            { 0x77, new Opcode(0x77, "LD (HL),A", 1, (m, r) => Opcode_0x77(m, r))},
            { 0x78, new Opcode(0x78, "LD A,B", 1, (m, r) => Opcode_0x78(m, r))},
            { 0x7B, new Opcode(0x7B, "LD A,E", 1, (m, r) => Opcode_0x7B(m, r))},
            { 0x7C, new Opcode(0x7C, "LD A,H", 1, (m, r) => Opcode_0x7C(m, r))},
            { 0x7D, new Opcode(0x7D, "LD A,L", 1, (m, r) => Opcode_0x7D(m, r))},
            { 0x86, new Opcode(0x86, "ADD (HL)", 1, (m, r) => Opcode_0x86(m, r))},
            { 0x90, new Opcode(0x90, "SUB B", 1, (m, r) => Opcode_0x90(m, r))},
            { 0xAF, new Opcode(0xAF, "XOR A", 1, (m, r) => Opcode_0xAF(m, r))},
            { 0xBE, new Opcode(0xBE, "CP (HL)", 1, (m, r) => Opcode_0xBE(m, r))},
            { 0xC1, new Opcode(0xC1, "POP BC", 1, (m, r) => Opcode_0xC1(m, r))},
            { 0xC5, new Opcode(0xC5, "PUSH BC", 1, (m, r) => Opcode_0xC5(m, r))},

            { 0xCB, new Opcode(0xCB, "CB", 2, (m, r) => Opcode_0xCB(m, r))},
            { 0xCD, new Opcode(0xCD, "CALL ${1:x2}{0:x2}", 3, (m, r) => Opcode_0xCD(m, r))},
            { 0xE0, new Opcode(0xE0, "LD ($FF00+${0:X}),A", 2, (m, r) => Opcode_0xE0(m, r))},
            { 0xE2, new Opcode(0xE2, "LD ($FF00+C),A", 1, (m, r) => Opcode_0xE2(m, r))},
            { 0xEA, new Opcode(0xEA, "LD (${1:x2}{0:x2}),A", 3, (m, r) => Opcode_0xEA(m, r))},
            { 0xF0, new Opcode(0xF0, "LD A,($FF00+${0:x2})", 2, (m, r) => Opcode_0xF0(m, r))},
            { 0xFE, new Opcode(0xFE, "CP ${0:x2}", 2,(m, r) => Opcode_0xFE(m, r))}
        };

        public static bool CalculateFlagZ(u8 b) {
            return b == 0;
        }

        private static bool CalculateFlagH(u8 b1, u8 b2) {
            return ((b1 & 0xF) + (b2 & 0xF)) > 0xF;
        }

        public static void INC(ref u8 regu8, Registers r) {
            regu8 += 1;
            r.FlagZ = CalculateFlagZ(regu8);
            r.FlagN = false;
            r.FlagH = CalculateFlagH(regu8, 1);
            // r.FlagC -> unmodified
        }

        public static void Opcode_0x3C(Memory m, Registers r) {
        }

        private static void Opcode_0x04(Memory m, Registers r) {
        }

        private static void Opcode_0x05(Memory m, Registers r) {
        }

        private static void Opcode_0x06(Memory m, Registers r) {
        }

        private static void Opcode_0x15(Memory m, Registers r) {
        }

        private static void Opcode_0xEA(Memory m, Registers r) {
        }

        private static void Opcode_0xF0(Memory m, Registers r) {
        }

        private static void Opcode_0x3D(Memory m, Registers r) {
        }

        private static void Opcode_0xFE(Memory m, Registers r) {
        }

        private static void Opcode_0xCD(Memory m, Registers r) {
        }

        private static void Opcode_0x7B(Memory m, Registers r) {
        }

        private static void Opcode_0x7C(Memory m, Registers r) {
        }

        private static void Opcode_0x7D(Memory m, Registers r) {
        }

        private static void Opcode_0x86(Memory m, Registers r) {
        }

        private static void Opcode_0x90(Memory m, Registers r) {
        }

        private static void Opcode_0x11(Memory m, Registers r) {
        }

        private static void Opcode_0x1A(Memory m, Registers r) {
        }

        private static void Opcode_0xE0(Memory m, Registers r) {
        }

        private static void Opcode_0x77(Memory m, Registers r) {
        }

        private static void Opcode_0x78(Memory m, Registers r) {
        }

        private static void Opcode_0x22(Memory m, Registers r) {
        }

        private static void Opcode_0x0C(Memory m, Registers r) {
        }

        private static void Opcode_0x0D(Memory m, Registers r) {
        }

        private static void Opcode_0x13(Memory m, Registers r) {
        }

        private static void Opcode_0x16(Memory m, Registers r) {
        }

        private static void Opcode_0x18(Memory m, Registers r) {
        }

        private static void Opcode_0x1D(Memory m, Registers r) {
        }

        private static void Opcode_0x1E(Memory m, Registers r) {
        }

        private static void Opcode_0x23(Memory m, Registers r) {
        }

        private static void Opcode_0x24(Memory m, Registers r) {
        }

        private static void Opcode_0x28(Memory m, Registers r) {
        }

        private static void Opcode_0x2E(Memory m, Registers r) {
        }

        private static void Opcode_0xE2(Memory m, Registers r) {
        }

        private static void Opcode_0x3E(Memory m, Registers r) {
        }

        private static void Opcode_0x4F(Memory m, Registers r) {
        }

        private static void Opcode_0x57(Memory m, Registers r) {
        }

        private static void Opcode_0x0E(Memory m, Registers r) {
        }

        private static void Opcode_0x21(Memory m, Registers r) {
        }

        private static void Opcode_0x32(Memory m, Registers r) {
        }

        private static void Opcode_0x31(Memory m, Registers r) {
        }

        private static void Opcode_0xCB(Memory m, Registers r) {
        }

        private static void Opcode_0x20(Memory m, Registers r) {
        }

        private static void Opcode_0xAF(Memory m, Registers r) {
        }

        private static void Opcode_0xBE(Memory m, Registers r) {
        }

        private static void Opcode_0x17(Memory m, Registers r) {
        }

        private static void Opcode_0xC1(Memory m, Registers r) {
        }

        private static void Opcode_0xC5(Memory m, Registers r) {
        }

        private static void Opcode_0xC9(Memory m, Registers r) {
        }

        private static void Opcode_0x67(Memory m, Registers r) {
        }
    }
}
