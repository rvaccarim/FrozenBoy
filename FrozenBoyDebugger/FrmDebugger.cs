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

        private const string romPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\";
        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debugger\";

        private const string opcodeFormat = "{0,-15} ;${1,-6:x4} O=0x{2:x2}";
        private const string stateFormat = "{0}   {1}";

        private int i;

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
        public int prevPC;
        public int prevSP;

        GameBoy gb;
        Dictionary<int, int> addressLineMap = new Dictionary<int, int>();

        public FrmDebugger() {
            InitializeComponent();
            disasmGrid.DefaultCellStyle.Font = new Font("Consolas", 8);
            historyGrid.DefaultCellStyle.Font = new Font("Consolas", 8);
            // disable the annoying lateral arrow
            // historyGrid.RowTemplate.Height = 16;
            historyGrid.RowHeadersWidth = 4;
        }

        private void FrmDebugger_Load(object sender, EventArgs e) {
            gb = new GameBoy();
            Disassemble();
            disasmGrid.Rows[0].Selected = true;

        }

        private void Disassemble() {

            Memory memory = new Memory();
            memory.data = File.ReadAllBytes(romPath + @"boot\boot_rom.gb");
            // memory.data = File.ReadAllBytes(romPath + @"blargg\cpu_instrs\cpu_instrs.gb");
            // memory.data = File.ReadAllBytes(romPath + @"blargg\cpu_instrs\individual\11-op a,(hl).gb");

            using (StreamWriter outputFile = new StreamWriter(@"D:\Users\frozen\Documents\99_temp\GB_Debugger\11-op a,(hl).gb.txt")) {

                int line = 0;

                while (i < memory.data.Length) {
                    byte b = memory.data[i];

                    if (gb.cpu.opcodes.ContainsKey(b)) {
                        addressLineMap.Add(i, line);

                        Opcode opcode = gb.cpu.opcodes[b];
                        string lineStr = OpcodeToStr(opcodeFormat, opcode, i, memory);

                        AddInstruction(lineStr);
                        outputFile.WriteLine(lineStr);

                        if (opcode.value == 0xCB) {
                            i += 2;
                        }
                        else {
                            i += opcode.length;
                        }

                        line++;
                    }
                    else {
                        string lineStr = String.Format(opcodeFormat, String.Format("0x{0:x2}---->TODO", b), i, b);
                        AddInstruction(lineStr);
                        outputFile.WriteLine(lineStr);

                        addressLineMap.Add(i, line);
                        line++;

                        i++;
                    }
                }
            }
        }

        private void BtnNext_Click(object sender, EventArgs e) {
            Next();
        }

        private void Next() {
            gb.cpu.Execute();

            // update UI
            int index = addressLineMap[(int)gb.cpu.regs.PC];
            disasmGrid.Rows[index].Selected = true;
            disasmGrid.CurrentCell = disasmGrid[0, index];

            string instruction = disasmGrid.Rows[addressLineMap[prevPC]].Cells[0].Value.ToString();
            AddHistory(instruction, gb.cpu.regs);

            // backup 
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
            prevPC = gb.cpu.regs.PC;
            prevSP = gb.cpu.regs.SP;
        }


        private void AddInstruction(string value) {
            int rowId = disasmGrid.Rows.Add();
            DataGridViewRow row = disasmGrid.Rows[rowId];
            row.Cells["Instruction"].Value = value;
        }

        private void AddHistory(string instruction, Registers r) {
            // history.AppendText(DumpState() + Environment.NewLine);

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

            row.Cells["histPC"].Value = String.Format("{0:x4}", r.PC);
            StyleCell(r.PC, prevPC, row.Cells["histPC"]);

            row.Cells["histSP"].Value = String.Format("{0:x4}", r.SP);
            StyleCell(r.SP, prevSP, row.Cells["histSP"]);
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

        private void StyleCell(int current, int previous, DataGridViewCell cell) {
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
                                   String.Format(o.asmInstruction, m.Read8((u16)(address + 1))),
                                   address,
                                   o.value),

                3 => String.Format(format,
                                   String.Format(o.asmInstruction, m.Read16((u16)(address + 1))),
                                   address,
                                   o.value),

                _ => String.Format(format,
                                   String.Format(o.value != 0xCB ? o.asmInstruction
                                                                 : gb.cpu.cbOpcodes[m.Read8((u16)(address + 1))].asmInstruction),
                                   address,
                                   o.value),
            };
        }

        private string DumpState() {
            return String.Format(stateFormat, OpcodeToStr(opcodeFormat, gb.cpu.opcode, gb.cpu.regs.PC, gb.cpu.mem), gb.cpu.regs.ToString());
        }

        //private void History_TextChanged(object sender, EventArgs e) {
        //    // set the current caret position to the end
        //    history.SelectionStart = history.Text.Length;
        //    // scroll it automatically
        //    history.ScrollToCaret();
        //}

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.F10) {
                Next();
                // Handle key at form level.
                // Do not send event to focused control by returning true.
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void HistoryGrid_SelectionChanged(object sender, EventArgs e) {
            historyGrid.ClearSelection();
        }
    }
}
