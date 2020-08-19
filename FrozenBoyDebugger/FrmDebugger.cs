using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyDebugger {
    public partial class FrmDebugger : Form {

        private StreamWriter dumpFile;
        private const string romPath = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\blargg\cpu_instrs\individual\";
        private const string debugPath = @"D:\Users\frozen\Documents\99_temp\GB_Debug\";

        private const string opcodeFormat = "{0,-15} ;${1,-6:x4} O=0x{2:x2}";
        // private const string stateFormat = "{0}   {1}->{2:x4}";
        private const string stateFormat = "{0}   {1} {2:x2}";

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
        public int prevPC;
        public int prevSP;

        GameBoy gb;
        Dictionary<int, int> addressLineMap = new Dictionary<int, int>();

        public FrmDebugger() {
            InitializeComponent();
            disasmGrid.DefaultCellStyle.Font = new Font("Consolas", 8);

            SetAlignment(historyGrid);
            historyGrid.DefaultCellStyle.Font = new Font("Consolas", 8);
            historyGrid.RowHeadersWidth = 4;
        }

        private void FrmDebugger_Load(object sender, EventArgs e) {
            string romName = @"01-special.gb";

            string romFilename = romPath + romName;
            string disasmFilename = debugPath + romName + ".disasm.txt";
            string dumpFilename = debugPath + romName + ".dump.frozenBoy.txt";

            dumpFile = new StreamWriter(dumpFilename);
            gb = new GameBoy(romFilename);
            prevPC = gb.cpu.regs.PC;

            this.Text = "Debugger - " + romName;
            // Disassemble(romFilename, disasmFilename);
            // disasmGrid.Rows[0].Selected = true;
            // MessageBox.Show("Done!");

        }

        private void SetAlignment(DataGridView d) {
            var col = historyGrid.Columns;

            for (int i = 0; i < col.Count; i++) {
                d.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                d.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        //private void Disassemble(string romFilename, string disasmFilename) {
        //    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

        //    MMU mmu = new MMU();

        //    List<DataGridViewRow> rows = new List<DataGridViewRow>();
        //    mmu.data = File.ReadAllBytes(romFilename);

        //    using (StreamWriter disasmFile = new StreamWriter(disasmFilename)) {
        //        int line = 0;

        //        while (PC < mmu.data.Length) {
        //            byte b = mmu.data[PC];

        //            if (gb.cpu.opcodes.ContainsKey(b)) {
        //                addressLineMap.Add(PC, line);

        //                Opcode opcode = gb.cpu.opcodes[b];
        //                string instruction = OpcodeToStr(opcodeFormat, opcode, PC, mmu);

        //                AddInstruction(rows, instruction);
        //                disasmFile.WriteLine(instruction);

        //                if (opcode.value == 0xCB) {
        //                    PC += 2;
        //                }
        //                else {
        //                    PC += opcode.length;
        //                }

        //                line++;
        //            }
        //            else {
        //                string lineStr = String.Format(opcodeFormat, String.Format("0x{0:x2}---->TODO", b), PC, b);
        //                AddInstruction(rows, lineStr);
        //                disasmFile.WriteLine(lineStr);

        //                addressLineMap.Add(PC, line);
        //                line++;

        //                PC++;
        //            }
        //        }
        //    }

        //    disasmGrid.Rows.AddRange(rows.ToArray());

        //    int index = addressLineMap[gb.cpu.regs.PC];
        //    disasmGrid.Rows[index].Selected = true;
        //    disasmGrid.CurrentCell = disasmGrid[0, index];

        //    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        //}

        private void BtnRun_Click(object sender, EventArgs e) {
            int count = 0;
            while (true) {
                Next();

                if (chkInteractive.Checked) {
                    if (count == 800) {
                        Application.DoEvents();
                        historyGrid.FirstDisplayedScrollingRowIndex = historyGrid.RowCount - 1;
                        count = 0;
                    }

                    count++;
                }
            }
        }

        private void BtnNext_Click(object sender, EventArgs e) {
            Next();

            if (chkInteractive.Checked) {
                // outside in order not to delay RunTo
                historyGrid.FirstDisplayedScrollingRowIndex = historyGrid.RowCount - 1;
            }
        }

        private void BtnRunTo_Click(object sender, EventArgs e) {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            u16 address = Convert.ToUInt16(txtRunTo.Text, 16);
            int count = 0;
            while (gb.cpu.regs.PC != address) {
                Next();

                if (chkInteractive.Checked) {
                    if (count == 800) {
                        Application.DoEvents();
                        historyGrid.FirstDisplayedScrollingRowIndex = historyGrid.RowCount - 1;
                        count = 0;
                    }

                    count++;
                }
            }

            historyGrid.FirstDisplayedScrollingRowIndex = historyGrid.RowCount - 1;
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        private void Next() {
            gb.cpu.Execute();

            string instruction = OpcodeToStr(opcodeFormat, gb.cpu.prevOpcode, prevPC, gb.cpu.mmu);

            if (chkInteractive.Checked) {
                // update UI
                int index = addressLineMap[gb.cpu.regs.PC];
                disasmGrid.Rows[index].Selected = true;
                disasmGrid.CurrentCell = disasmGrid[0, index];
                AddHistory(instruction, gb.cpu.regs);
            }

            Log(instruction);

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

        private void AddInstruction(List<DataGridViewRow> rows, string instruction) {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(disasmGrid);
            row.Cells[0].Value = instruction;
            rows.Add(row);
        }

        private void AddHistory(string instruction, Registers r) {

            if (chkInteractive.Checked) {
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

        }

        private void Log(string instruction) {
            // dumpFile.WriteLine(String.Format(stateFormat, instruction.Substring(16), gb.cpu.regs.ToString(), gb.cpu.mmu.Read16(gb.cpu.regs.SP)));
            dumpFile.WriteLine(String.Format(stateFormat, instruction.Substring(16), gb.cpu.regs.ToString(), gb.cpu.mmu.Read8(0xFF44)));
            dumpFile.Flush();
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

        private string OpcodeToStr(string format, Opcode o, int address, MMU m) {
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

        private void FrmDebugger_FormClosed(object sender, FormClosedEventArgs e) {
            dumpFile.Flush();
            dumpFile.Close();
            dumpFile.Dispose();
        }


    }
}
