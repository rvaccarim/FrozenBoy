using System;
using System.IO;
using FrozenBoyCore.Processor;
using FrozenBoyCore.Memory;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Util {
    public class Logger {
        private const string stateFormat =
        "{0}   cycles:{1,6}  AF={2:x4} BC={3:x4} DE={4:x4} HL={5:x4}   Z={6} N={7} H={8} C={9}   PC={10:x4} SP={11:x4}   IME={12} IE={13:x4} IF={14:x4} IME_Scheduled={15} halted={16}   DIV={17:x4} TIMA={18:x4} TMA={19:x4} TAC={20:x4}   LCDC={21:x2} STAT={22} LY={23:x2} LYC={24:x2}";

        private readonly StreamWriter logFile;
        private readonly string logFilename;

        public Logger(string logFilename) {
            this.logFilename = logFilename;
            logFile = new StreamWriter(this.logFilename);
        }

        public void Close() {
            logFile.Flush();
            logFile.Close();
            logFile.Dispose();
        }

        //~Logger() {
        //    Close();
        //}

        public void LogState(CPU cpu, MMU mmu, int totalCycles) {
            // tring instruction = Disassembler.OpcodeToStr(cpu, cpu.opcode, cpu.opLocation);
            string instruction = Disassembler.OpcodeToStr(cpu, cpu.opcode, cpu.opLocation).Substring(25);
            logFile.WriteLine(
                  String.Format(stateFormat,
                                instruction, totalCycles,
                                cpu.regs.AF, cpu.regs.BC, cpu.regs.DE, cpu.regs.HL,
                                Convert.ToInt32(cpu.regs.FlagZ), Convert.ToInt32(cpu.regs.FlagN),
                                Convert.ToInt32(cpu.regs.FlagH), Convert.ToInt32(cpu.regs.FlagC),
                                cpu.regs.PC, cpu.regs.SP,
                                Convert.ToInt32(cpu.IME), mmu.IE, Convert.ToString(mmu.IF, 2).PadLeft(8, '0').Substring(3), Convert.ToInt32(cpu.IME_Scheduled), Convert.ToInt32(cpu.halted),
                                mmu.DIV, mmu.TIMA, mmu.TMA, mmu.TAC,
                                mmu.LCDC, Convert.ToString(mmu.Status, 2).PadLeft(8, '0'), mmu.LY, mmu.LYC));
        }

    }
}
