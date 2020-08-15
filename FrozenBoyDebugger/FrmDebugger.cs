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
        public bool prevFlagZ;
        public bool prevFlagN;
        public bool prevFlagH;
        public bool prevFlagC;


        GameBoy gb;
        Dictionary<int, int> addressLineMap = new Dictionary<int, int>();

        public FrmDebugger() {
            InitializeComponent();
            disasmGrid.DefaultCellStyle.Font = new Font("Consolas", 8);
            historyGrid.DefaultCellStyle.Font = new Font("Consolas", 8);
        }

        private void FrmDebugger_Load(object sender, EventArgs e) {
            gb = new GameBoy();
            Disassemble();
            disasmGrid.Rows[0].Selected = true;

        }

        private void Disassemble() {

            Memory memory = new Memory();
            memory.data = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\cpu_instrs.gb");
            // memory.data = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\boot_rom.gb");

            int line = 0;

            while (PC < memory.data.Length) {
                byte b = memory.data[PC];

                if (gb.cpu.opcodes.ContainsKey(b)) {
                    Opcode opcode = gb.cpu.opcodes[b];

                    AddInstruction(OpcodeToStr(opcodeFormat, opcode, PC, memory));

                    addressLineMap.Add(PC, line);
                    line++;

                    PC += opcode.length;
                }
                else {
                    AddInstruction(String.Format(opcodeFormat, String.Format("0x{0:x2}---->TODO", b), PC, b));

                    addressLineMap.Add(PC, line);
                    line++;

                    PC++;
                }
            }

        }

        private void BtnNext_Click(object sender, EventArgs e) {
            gb.cpu.Execute();

            history.AppendText(DumpState() + Environment.NewLine);
            AddHistory(OpcodeToStr(opcodeFormat, gb.cpu.opcode, gb.cpu.regs.PC, gb.cpu.mem), gb.cpu.regs);

            prevA = gb.cpu.regs.A;
            prevB = gb.cpu.regs.B;
            prevC = gb.cpu.regs.C;
            prevD = gb.cpu.regs.D;
            prevE = gb.cpu.regs.E;
            prevF = gb.cpu.regs.F;
            prevH = gb.cpu.regs.H;
            prevL = gb.cpu.regs.L;
            prevFlagZ = gb.cpu.regs.FlagZ;
            prevFlagN = gb.cpu.regs.FlagN;
            prevFlagH = gb.cpu.regs.FlagH;
            prevFlagC = gb.cpu.regs.FlagC;

            disasmGrid.Rows[addressLineMap[gb.cpu.regs.PC]].Selected = true;
            disasmGrid.CurrentCell = disasmGrid[0, addressLineMap[gb.cpu.regs.PC]];

        }

        private void AddInstruction(string value) {
            int rowId = disasmGrid.Rows.Add();
            DataGridViewRow row = disasmGrid.Rows[rowId];
            row.Cells["Instruction"].Value = value;
        }

        private void AddHistory(string instruction, Registers r) {
            int rowId = historyGrid.Rows.Add();
            DataGridViewRow row = historyGrid.Rows[rowId];
            row.Cells["histInstruction"].Value = instruction;

            row.Cells["histA"].Value = String.Format("{0:x2}", r.A);
            StyleCell(r.A, prevA, row.Cells["histA"]);

            row.Cells["histB"].Value = String.Format("{0:x2}", r.B);
            StyleCell(r.B, prevB, row.Cells["histB"]);

            row.Cells["histC"].Value = String.Format("{0:x2}", r.C);
            StyleCell(r.C, prevC, row.Cells["histC"]);

            row.Cells["histD"].Value = String.Format("{0:x2}", r.D);
            StyleCell(r.D, prevD, row.Cells["histD"]);

            row.Cells["histE"].Value = String.Format("{0:x2}", r.E);
            StyleCell(r.E, prevE, row.Cells["histE"]);

            row.Cells["histF"].Value = String.Format("{0:x2}", r.F);
            StyleCell(r.F, prevF, row.Cells["histF"]);

            row.Cells["histH"].Value = String.Format("{0:x2}", r.H);
            StyleCell(r.H, prevH, row.Cells["histH"]);

            row.Cells["histL"].Value = String.Format("{0:x2}", r.L);
            StyleCell(r.L, prevL, row.Cells["histL"]);

            row.Cells["histFlagZ"].Value = String.Format("{0}", Convert.ToInt32(r.FlagZ));
            StyleCell(r.FlagZ, prevFlagZ, row.Cells["histFlagZ"]);

            row.Cells["histFlagN"].Value = String.Format("{0}", Convert.ToInt32(r.FlagN));
            StyleCell(r.FlagN, prevFlagN, row.Cells["histFlagN"]);

            row.Cells["histFlagH"].Value = String.Format("{0}", Convert.ToInt32(r.FlagH));
            StyleCell(r.FlagH, prevFlagH, row.Cells["histFlagH"]);

            row.Cells["histFlagC"].Value = String.Format("{0}", Convert.ToInt32(r.FlagC));
            StyleCell(r.FlagC, prevFlagC, row.Cells["histFlagC"]);
        }


        private void StyleCell(u8 current, u8 previous, DataGridViewCell cell) {
            if (current != previous) {
                cell.Style.BackColor = Color.DarkRed;
                cell.Style.ForeColor = Color.White;
            }
            else {
                cell.Style.BackColor = Color.White;
                cell.Style.ForeColor = Color.Black;
            }
        }

        private void StyleCell(bool current, bool previous, DataGridViewCell cell) {
            if (current != previous) {
                cell.Style.BackColor = Color.DarkRed;
                cell.Style.ForeColor = Color.White;
            }
            else {
                cell.Style.BackColor = Color.White;
                cell.Style.ForeColor = Color.Black;
            }
        }

        private string OpcodeToStr(string format, Opcode o, int address, Memory m) {
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
                                                                 : gb.cpu.cb_opcodes[m.ReadNext8(address)].asmInstruction),
                                   address,
                                   o.value),
            };
        }




        private string DumpState() {
            return String.Format(stateFormat, OpcodeToStr(opcodeFormat, gb.cpu.opcode, gb.cpu.regs.PC, gb.cpu.mem), gb.cpu.regs.ToString());
        }

        private void History_TextChanged(object sender, EventArgs e) {
            // set the current caret position to the end
            history.SelectionStart = history.Text.Length;
            // scroll it automatically
            history.ScrollToCaret();
        }

    }
}
