using System;
using System.Collections.Generic;
using System.Text;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    public class Opcodes {

        public static Dictionary<byte, Opcode> unprefixed = new Dictionary<byte, Opcode> {
            // NOP
            { 0x00, new Opcode(0x00, "NOP", 1, (c, i) => { })},

            // INC
            { 0x04, new Opcode(0x04, "INC B", 1, (c, i) => { INC(ref c.regs.B, c.regs); })},
            { 0x0C, new Opcode(0x0C, "INC C", 1, (c, i) => { INC(ref c.regs.C, c.regs); })},
            { 0x14, new Opcode(0x14, "INC D", 1, (c, i) => { INC(ref c.regs.D, c.regs); })},
            { 0x1C, new Opcode(0x1C, "INC E", 1, (c, i) => { INC(ref c.regs.E, c.regs); })},
            { 0x24, new Opcode(0x24, "INC H", 1, (c, i) => { INC(ref c.regs.H, c.regs); })},
            { 0x3C, new Opcode(0x3C, "INC A", 1, (c, i) => { INC(ref c.regs.A, c.regs); })},

            // INC XX
            { 0x03, new Opcode(0x03, "INC BC",   1, (c, i) => { c.regs.BC++; })},
            { 0x13, new Opcode(0x13, "INC DE",   1, (c, i) => { c.regs.DE++; })},
            { 0x23, new Opcode(0x23, "INC HL",   1, (c, i) => { c.regs.HL++; })},
            { 0x33, new Opcode(0x33, "INC SP",   1, (c, i) => { c.regs.SP++; })},

            // RETURN
            { 0xC9, new Opcode(0xC9, "RET", 1, (c, i) => { c.Ret();  })},

            { 0x05, new Opcode(0x05, "DEC B", 1, (c, i) => Opcode_0x05(c, i))},
            { 0x06, new Opcode(0x06, "LD B,${0:x2}", 2, (c, i) => Opcode_0x06(c, i))},

            { 0x0D, new Opcode(0x0D, "DEC C", 1, (c, i) => Opcode_0x0D(c, i))},
            { 0x0E, new Opcode(0x0E, "LD C,${0:x2}", 2, (c, i) => Opcode_0x0E(c, i))},
            { 0x11, new Opcode(0x11, "LD DE,${1:x2}{0:x2}", 3, (c, i) => Opcode_0x11(c, i))},
            { 0x15, new Opcode(0x15, "DEC D", 1, (c, i) => Opcode_0x15(c, i))},
            { 0x16, new Opcode(0x16, "LD D,${0:x2}", 2, (c, i) => Opcode_0x16(c, i))},
            { 0x17, new Opcode(0xC1, "RLA", 1, (c, i) => Opcode_0x17(c, i))},
            { 0x18, new Opcode(0x18, "JR Addr_{0:X4}", 2, (c, i) => Opcode_0x18(c, i))},
            { 0x1A, new Opcode(0x1A, "LD A,(DE)", 1, (c, i) => Opcode_0x1A(c, i))},
            { 0x1D, new Opcode(0x1D, "DEC E", 1, (c, i) => Opcode_0x1D(c, i))},
            { 0x1E, new Opcode(0x1E, "LD E,${0:x2}", 2, (c, i) => Opcode_0x1E(c, i))},
            { 0x20, new Opcode(0x20, "JR NZ, Addr_{0:X4}", 2, (c, i) => Opcode_0x20(c, i))},
            { 0x21, new Opcode(0x21, "LD HL,${1:x2}{0:x2}", 3, (c, i) => Opcode_0x21(c, i))},
            { 0x22, new Opcode(0x22, "LD (HL+),A", 1, (c, i) => Opcode_0x22(c, i))},

            { 0x28, new Opcode(0x28, "JR Z, Addr_{0:X4}", 2, (c, i) => Opcode_0x28(c, i))},
            { 0x2E, new Opcode(0x2E, "LD L,${0:x2}", 2, (c, i) => Opcode_0x2E(c, i))},
            { 0x31, new Opcode(0x31, "LD SP,${1:x2}{0:x2}", 3, (c, i) => Opcode_0x31(c, i))},
            { 0x32, new Opcode(0x32, "LD (HL-),A", 1, (c, i) => Opcode_0x32(c, i))},

            { 0x3D, new Opcode(0x3D, "DEC A", 1, (c, i) => Opcode_0x3D(c, i))},
            { 0x3E, new Opcode(0x3E, "LD A,${0:x2}", 2, (c, i) => Opcode_0x3E(c, i))},
            { 0x4F, new Opcode(0x4F, "LD C,A ", 1, (c, i) => Opcode_0x4F(c, i))},
            { 0x57, new Opcode(0x57, "LD D,A", 1, (c, i) => Opcode_0x57(c, i))},
            { 0x67, new Opcode(0x67, "LD H,A", 1, (c, i) => Opcode_0x67(c, i))},
            { 0x77, new Opcode(0x77, "LD (HL),A", 1, (c, i) => Opcode_0x77(c, i))},
            { 0x78, new Opcode(0x78, "LD A,B", 1, (c, i) => Opcode_0x78(c, i))},
            { 0x7B, new Opcode(0x7B, "LD A,E", 1, (c, i) => Opcode_0x7B(c, i))},
            { 0x7C, new Opcode(0x7C, "LD A,H", 1, (c, i) => Opcode_0x7C(c, i))},
            { 0x7D, new Opcode(0x7D, "LD A,L", 1, (c, i) => Opcode_0x7D(c, i))},
            { 0x86, new Opcode(0x86, "ADD (HL)", 1, (c, i) => Opcode_0x86(c, i))},
            { 0x90, new Opcode(0x90, "SUB B", 1, (c, i) => Opcode_0x90(c, i))},
            { 0xAF, new Opcode(0xAF, "XOR A", 1, (c, i) => Opcode_0xAF(c, i))},
            { 0xBE, new Opcode(0xBE, "CP (HL)", 1, (c, i) => Opcode_0xBE(c, i))},
            { 0xC1, new Opcode(0xC1, "POP BC", 1, (c, i) => Opcode_0xC1(c, i))},
            { 0xC5, new Opcode(0xC5, "PUSH BC", 1, (c, i) => Opcode_0xC5(c, i))},

            { 0xCB, new Opcode(0xCB, "CB", 2, (c, i) => Opcode_0xCB(c, i))},
            { 0xCD, new Opcode(0xCD, "CALL ${1:x2}{0:x2}", 3, (c, i) => Opcode_0xCD(c, i))},
            { 0xE0, new Opcode(0xE0, "LD ($FF00+${0:X}),A", 2, (c, i) => Opcode_0xE0(c, i))},
            { 0xE2, new Opcode(0xE2, "LD ($FF00+C),A", 1, (c, i) => Opcode_0xE2(c, i))},
            { 0xEA, new Opcode(0xEA, "LD (${1:x2}{0:x2}),A", 3, (c, i) => Opcode_0xEA(c, i))},
            { 0xF0, new Opcode(0xF0, "LD A,($FF00+${0:x2})", 2, (c, i) => Opcode_0xF0(c, i))},
            { 0xFE, new Opcode(0xFE, "CP ${0:x2}", 2,(c, i) => Opcode_0xFE(c, i))}
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



        public static void Opcode_0x3C(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x04(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x05(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x06(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x15(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xEA(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xF0(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x3D(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xFE(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xCD(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x7B(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x7C(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x7D(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x86(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x90(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x11(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x1A(CPU cpu, Instruction instruction) {
        }
        private static void Opcode_0xE0(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x77(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x78(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x22(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x0C(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x0D(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x13(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x16(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x18(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x1D(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x1E(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x23(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x24(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x28(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x2E(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xE2(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x3E(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x4F(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x57(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x0E(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x21(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x32(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x31(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xCB(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x20(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xAF(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xBE(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x17(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xC1(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xC5(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0xC9(CPU cpu, Instruction instruction) {
        }

        private static void Opcode_0x67(CPU cpu, Instruction instruction) {
        }
    }
}
