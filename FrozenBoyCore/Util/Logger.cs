using FrozenBoyCore.Graphics;
using FrozenBoyCore.Memory;
using FrozenBoyCore.Processor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrozenBoyCore.Util {
    public class Logger {
        private const string stateFormatFull =
            "{0}   cycles:{1,6}  AF={2:x4} BC={3:x4} DE={4:x4} HL={5:x4}   Z={6} N={7} H={8} C={9}   PC={10:x4} SP={11:x4}   IME={12} IE={13:x4} IF={14:x4} halted={15}   DIV={16:x4} TIMA={17:x4} TMA={18:x4} TAC={19:x4}   LCDC={20:x2} STAT={21} LY={22:x2} LYC={23:x2} gpuClock={24} delay={25}";

        //private const string stateFormatFull =
        //    "{0}   cycles:{1,6}  AF={2:x4} BC={3:x4} DE={4:x4} HL={5:x4}   Z={6} N={7} H={8} C={9}   PC={10:x4} SP={11:x4}   DIV={12:x4} TIMA={13:x4} TMA={14:x4} TAC={15:x4}   LCDC={16:x2} STAT={17} LY={18,3} LYC={19,3}";

        //private const string stateFormatCoreBoy =
        //    "{0}   cycles:{1,6}  AF={2:x4} BC={3:x4} DE={4:x4} HL={5:x4}   Z={6} N={7} H={8} C={9}   PC={10:x4} SP={11:x4}   DIV={12:x4} TIMA={13:x4} TMA={14:x4} TAC={15:x4}";
        private const string stateFormatCoreBoy2 =
            "{0}   cycles:{1,6}  AF={2:x4} BC={3:x4} DE={4:x4} HL={5:x4}   Z={6} N={7} H={8} C={9}   PC={10:x4} SP={11:x4}   DIV={12:x4} TIMA={13:x4} TMA={14:x4} TAC={15:x4}   LCDC={16:x2} STAT={17} LY={18,3} LYC={19,3} gpuClock={20} delay={21}";

        private const string stateFormatGbNet =
        "{0}   cycles:{1,6}  AF={2:x4} BC={3:x4} DE={4:x4} HL={5:x4}   F={6}   PC={7:x4} SP={8:x4}   DIV={9:x4} TIMA={10:x4} TMA={11:x4} TAC={12:x4}";


        private readonly StreamWriter logFile;
        private readonly string logFilename;
        private readonly LogMode logMode;

        public Logger(string logFilename, LogMode logMode) {
            this.logFilename = logFilename;
            this.logMode = logMode;

            logFile = new StreamWriter(this.logFilename);
        }

        public void Close() {
            logFile.Flush();
            logFile.Close();
            logFile.Dispose();
        }

        ~Logger() {
            if (logFile != null) {
                Close();
            }
        }

        public void LogState(CPU cpu, GPU gpu, Timer timer, MMU mmu, InterruptManager intManager, int cycle) {
            // string instruction = Disassembler.OpcodeToStr(cpu, cpu.opcode, cpu.regs.OpcodePC);
            string instruction = Disassembler.OpcodeToStr(cpu, cpu.opcode, cpu.regs.OpcodePC).Substring(25);

            //logFile.WriteLine(
            //String.Format(stateFormatFull,
            //                    instruction, 0,
            //                    cpu.regs.AF, cpu.regs.BC, cpu.regs.DE, cpu.regs.HL,
            //                    Convert.ToInt32(cpu.regs.FlagZ), Convert.ToInt32(cpu.regs.FlagN),
            //                    Convert.ToInt32(cpu.regs.FlagH), Convert.ToInt32(cpu.regs.FlagC),
            //                    cpu.regs.PC, cpu.regs.SP,
            //                    //Convert.ToInt32(cpu.IME), mmu.IE, Convert.ToString(mmu.IF, 2).PadLeft(8, '0').Substring(3),
            //                    //Convert.ToInt32(cpu.IME_Scheduled), Convert.ToInt32(cpu.halted),
            //                    mmu.DIV, mmu.TIMA, mmu.TMA, mmu.TAC,
            //                    Convert.ToString(mmu.LCDC, 2).PadLeft(8, '0'), Convert.ToString(mmu.Status, 2).PadLeft(8, '0'),
            //                    mmu.LY, mmu.LYC));

            logFile.WriteLine(
            String.Format(stateFormatFull,
                                instruction, cycle,
                                cpu.regs.AF, cpu.regs.BC, cpu.regs.DE, cpu.regs.HL,
                                Convert.ToInt32(cpu.regs.FlagZ), Convert.ToInt32(cpu.regs.FlagN),
                                Convert.ToInt32(cpu.regs.FlagH), Convert.ToInt32(cpu.regs.FlagC),
                                cpu.regs.PC, cpu.regs.SP,
                                Convert.ToInt32(intManager.IME), intManager.IE, Convert.ToString(intManager.IF, 2).PadLeft(8, '0').Substring(3),
                                Convert.ToInt32(intManager.halted),
                                timer.DIV, timer.TIMA, timer.TMA, timer.tac,
                                Convert.ToString(gpu.LCDC, 2).PadLeft(8, '0'), Convert.ToString(gpu.STAT, 2).PadLeft(8, '0'),
                                gpu.LY, gpu.LYC, gpu.lineTicks, (gpu.wasDisabled && !gpu.IsLcdEnabled()) ? -1 : gpu.enableDelay));

            logFile.Flush();

            //logFile.WriteLine(
            //String.Format(stateFormatGbNet,
            //                    instruction, cycles,
            //                    cpu.regs.A, cpu.regs.BC, cpu.regs.DE, cpu.regs.HL,
            //                    Convert.ToString(cpu.regs.F, 2).PadLeft(8, '0'),
            //                    cpu.regs.PC, cpu.regs.SP,
            //                    timer.DIV, timer.TIMA, timer.TMA, timer.TAC));

        }

    }
}
