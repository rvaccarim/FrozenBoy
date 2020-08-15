using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using u8 = System.Byte;
using u16 = System.UInt16;
using System.Drawing;

namespace FrozenBoyDebugger {
    public partial class FrmDebugger : Form {

        private const string opcodeFormat = "{0,-15} ;${1,-6:x4} O=0x{2:x2}";
        private const string stateFormat = "{0}   {1}";

        private int PC;

        public u8 prevA;
        public u8 prevB;
        public u8 prevC;
        public u8 prevD;
        public u8 prevE;
        public u8 prevF;
        public u8 prevH;
        public u8 prevL;

        GameBoy gb;
        Dictionary<int, int> addressLineMap = new Dictionary<int, int>();

        public FrmDebugger() {
            InitializeComponent();
            disasmGrid.DefaultCellStyle.Font = new Font("Consolas", 8);
            historyGrid.DefaultCellStyle.Font = new Font("Consolas", 8);
        }

        private void FrmDebugger_Load(object sender, EventArgs e) {
            Disassemble();
            disasmGrid.Rows[0].Selected = true;

            gb = new GameBoy();

        }

        private void Disassemble() {
            Memory memory = new Memory();
            memory.data = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\cpu_instrs.gb");
            // memory.data = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\boot_rom.gb");

            int line = 0;

            while (PC < memory.data.Length) {
                byte b = memory.data[PC];

                if (Opcodes.unprefixed.ContainsKey(b)) {
                    Opcode opcode = Opcodes.unprefixed[b];

                    DisAsmAddRow(DisassembleOpcode(opcodeFormat, opcode, PC, memory));

                    addressLineMap.Add(PC, line);
                    line++;

                    PC += opcode.length;
                }
                else {
                    DisAsmAddRow(String.Format(opcodeFormat, String.Format("0x{0:x2}---->TODO", b), PC, b));

                    addressLineMap.Add(PC, line);
                    line++;

                    PC++;
                }
            }

        }

        private void DisAsmAddRow(string value) {
            int rowId = disasmGrid.Rows.Add();
            DataGridViewRow row = disasmGrid.Rows[rowId];
            row.Cells["Instruction"].Value = value;
        }

        private void HistoryAddRow(string instruction, Registers r) {
            int rowId = historyGrid.Rows.Add();
            DataGridViewRow row = historyGrid.Rows[rowId];
            row.Cells["histInstruction"].Value = instruction;
            row.Cells["histA"].Value = String.Format("{0:x2}", r.A);
            row.Cells["histB"].Value = String.Format("{0:x2}", r.B);
            row.Cells["histC"].Value = String.Format("{0:x2}", r.C);
            row.Cells["histD"].Value = String.Format("{0:x2}", r.D);
            row.Cells["histE"].Value = String.Format("{0:x2}", r.E);
            row.Cells["histF"].Value = String.Format("{0:x2}", r.F);
            row.Cells["histH"].Value = String.Format("{0:x2}", r.H);
            row.Cells["histL"].Value = String.Format("{0:x2}", r.L);
            row.Cells["histFlagZ"].Value = String.Format("{0:x2}", r.FlagZ);
            row.Cells["histFlagN"].Value = String.Format("{0:x2}", r.FlagN);
            row.Cells["histFlagH"].Value = String.Format("{0:x2}", r.FlagH);
            row.Cells["histFlagC"].Value = String.Format("{0:x2}", r.FlagC);
        }


        private string DisassembleOpcode(string format, Opcode o, int address, Memory m) {
            return o.length switch
            {
                2 => String.Format(format,
                                   String.Format(o.asmInstruction, m.ReadNext8(address)),
                                   address,
                                   o.value),

                3 => String.Format(format,
                                   String.Format(o.asmInstruction, m.ReadNext16(address)),
                                   address,
                                   o.value),

                _ => String.Format(format,
                                   String.Format(o.value != 0xCB ? o.asmInstruction
                                                                 : Opcodes.prefixed[m.ReadNext8(address)].asmInstruction),
                                   address,
                                   o.value),
            };
        }


        private void BtnNext_Click(object sender, EventArgs e) {
            gb.cpu.Step();

            history.AppendText(DumpState() + Environment.NewLine);
            HistoryAddRow(DisassembleOpcode(opcodeFormat, gb.cpu.opcode, gb.cpu.registers.PC, gb.cpu.memory), gb.cpu.registers);

            prevA = gb.cpu.registers.A;
            prevB = gb.cpu.registers.B;
            prevC = gb.cpu.registers.C;
            prevD = gb.cpu.registers.D;
            prevE = gb.cpu.registers.E;
            prevF = gb.cpu.registers.F;
            prevH = gb.cpu.registers.H;
            prevL = gb.cpu.registers.L;

            disasmGrid.Rows[addressLineMap[gb.cpu.registers.PC]].Selected = true;
            disasmGrid.CurrentCell = disasmGrid[0, addressLineMap[gb.cpu.registers.PC]];

        }

        private string DumpState() {
            return String.Format(stateFormat, DisassembleOpcode(opcodeFormat, gb.cpu.opcode, gb.cpu.registers.PC, gb.cpu.memory), gb.cpu.registers.ToString());
        }

        private void History_TextChanged(object sender, EventArgs e) {
            // set the current caret position to the end
            history.SelectionStart = history.Text.Length;
            // scroll it automatically
            history.ScrollToCaret();
        }

    }
}
