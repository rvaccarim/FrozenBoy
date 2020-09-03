using FrozenBoyCore.Graphics;
using FrozenBoyCore.Memory;
using FrozenBoyCore.Processor;
using System;
using System.IO;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore.Util {
    public class Logger {
        private const string stateFormato =
            "{0}   cycles:{1,6}  AF={2:x4} BC={3:x4} DE={4:x4} HL={5:x4}   Z={6} N={7} H={8} C={9}   PC={10:x4} SP={11:x4}   IME={12} IE={13:x4} IF={14:x4} halted={15}   DIV={16:x4} TIMA={17:x4} TMA={18:x4} TAC={19:x4}   LCDC={20:x2} STAT={21} LY={22:x2} LYC={23:x2} gpuClock={24} delay={25} {26:x4} {27:x4} {28:x4} {29:x4} {30:x4} {31:x4}";

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

        public void LogState(CPU cpu, GPU gpu, Timer timer, MMU mmu, InterruptManager intManager, int cycle) {
            string instruction;

            bool disAsm = false;

            if (cpu.opcode != null) {
                // if (disAsm) {
                // instruction = Disassembler.OpcodeToStr(cpu, cpu.opcode, cpu.regs.OpcodePC);
                // }
                // else {
                instruction = String.Format("O=0x{0:x2}", cpu.opcode.value);
                // }
            }
            else {
                instruction = "";
            }

            //  if (cycle >= 45000 && cycle <= 45300) {
            logFile.WriteLine(
                String.Format(stateFormato,
                instruction, cycle,
                cpu.regs.AF, cpu.regs.BC, cpu.regs.DE, cpu.regs.HL,
                Convert.ToInt32(cpu.regs.FlagZ), Convert.ToInt32(cpu.regs.FlagN),
                Convert.ToInt32(cpu.regs.FlagH), Convert.ToInt32(cpu.regs.FlagC),
                cpu.regs.PC, cpu.regs.SP,
                Convert.ToInt32(intManager.IME), intManager.IE, Convert.ToString(intManager.IF, 2).PadLeft(8, '0').Substring(3),
                Convert.ToInt32(cpu.haltBug),
                timer.DIV, timer.TIMA, timer.TMA, timer.tac,
                Convert.ToString(gpu.LCDC, 2).PadLeft(8, '0'), "",
                gpu.LY, gpu.LYC, gpu.lineTicks, (gpu.wasDisabled && !gpu.IsLcdEnabled()) ? -1 : gpu.enableDelay,
                mmu.Read8(cpu.regs.SP),
                mmu.Read8((u16)(cpu.regs.SP - 1)),
                mmu.Read8((u16)(cpu.regs.SP - 2)),
                mmu.Read8(0xFFDE),
                mmu.Read8(0xFDFF),
                mmu.Read8(0xFE00)
            )); ; ;
            // }
        }

    }
}
