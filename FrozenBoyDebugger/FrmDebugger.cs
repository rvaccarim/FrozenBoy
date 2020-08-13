using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.IO;
using System.Reflection.Metadata.Ecma335;

namespace FrozenBoyDebugger {
    public partial class FrmDebugger : Form {

        private int PC;
        private byte[] romBytes;
        private const string disasmFormat = "${0,-6:x4} {1,-15}";

        // 8 bit Real registers
        public u8 prevA;
        public u8 prevB;
        public u8 prevC;
        public u8 prevD;
        public u8 prevE;
        public u8 prevF;
        public u8 prevH;
        public u8 prevL;


        GameBoy gb;
        Dictionary<int, int> map = new Dictionary<int, int>();

        public FrmDebugger() {
            InitializeComponent();
        }

        private void FrmDebugger_Load(object sender, EventArgs e) {
            Disassemble();
            disassembly.SelectedIndex = 0;

            gb = new GameBoy();

        }

        private void BtnNext_Click(object sender, EventArgs e) {
            history.AppendText(gb.cpu.GetCurrentInstruction() + Environment.NewLine);
            gb.cpu.Next();

            Log();

            prevA = gb.cpu.regs.A;
            prevB = gb.cpu.regs.B;
            prevC = gb.cpu.regs.C;
            prevD = gb.cpu.regs.D;
            prevE = gb.cpu.regs.E;
            prevF = gb.cpu.regs.F;
            prevH = gb.cpu.regs.H;
            prevL = gb.cpu.regs.L;

            disassembly.SelectedIndex = map[gb.cpu.regs.PC];
        }


        private void Log() {
            int aOffset = 0;
            int bOffset = aOffset + 5;
            int cOffset = bOffset + 5;
            int dOffset = cOffset + 5;

            string cpuStateFormat = @"a={0:x2} b={1:x2} c={2:x2} d={3:x2} e={4:x2} f={5:x2} h={6:x2} l={7:x2}    Z={8} N={9} H={10} C={11}";

            string cpuState = String.Format(cpuStateFormat,
                                            gb.cpu.regs.A, gb.cpu.regs.B, gb.cpu.regs.C, gb.cpu.regs.D,
                                            gb.cpu.regs.E, gb.cpu.regs.F, gb.cpu.regs.H, gb.cpu.regs.L,
                                            Convert.ToInt32(gb.cpu.regs.FlagZ), Convert.ToInt32(gb.cpu.regs.FlagN),
                                            Convert.ToInt32(gb.cpu.regs.FlagH), Convert.ToInt32(gb.cpu.regs.FlagC));

            int historyLength = history.Text.Length;
            history.AppendText(cpuState + Environment.NewLine);

            //if (gb.cpu.regs.A != prevA) {
            //    history.SelectionStart = historyLength + aOffset;
            //    history.SelectionLength = 4;
            //    history.SelectionColor = Color.Red;
            //}

            //gb.cpu.regs.B = 3;
            //if (gb.cpu.regs.B != prevB) {
            //    history.SelectionStart = historyLength + bOffset;
            //    history.SelectionLength = 4;
            //    history.SelectionColor = Color.Red;
            //}
        }

        private void Disassemble() {
            romBytes = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\cpu_instrs.gb");
            // romBytes = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\boot_rom.gb");

            int line = 0;

            while (PC < romBytes.Length) {
                byte b = romBytes[PC];

                if (Opcodes.unprefixed.ContainsKey(b)) {
                    Opcode opcode = Opcodes.unprefixed[b];

                    switch (opcode.size) {
                        case 2:
                            disassembly.Items.Add(String.Format(disasmFormat, PC, String.Format(opcode.assembler, romBytes[PC + 1])));
                            break;
                        case 3:
                            disassembly.Items.Add(String.Format(disasmFormat, PC, String.Format(opcode.assembler, romBytes[PC + 1], romBytes[PC + 2])));
                            break;
                        default:
                            disassembly.Items.Add(String.Format(disasmFormat, PC, opcode.assembler));
                            break;
                    }

                    map.Add(PC, line);
                    line++;

                    PC += opcode.size;
                }
                else {
                    disassembly.Items.Add(String.Format(disasmFormat, PC, String.Format("0x{0:x2}---->TODO", b)));
                    map.Add(PC, line);
                    line++;

                    PC++;
                }
            }
        }

        private void History_TextChanged(object sender, EventArgs e) {
            // set the current caret position to the end
            history.SelectionStart = history.Text.Length;
            // scroll it automatically
            history.ScrollToCaret();
        }
    }
}
