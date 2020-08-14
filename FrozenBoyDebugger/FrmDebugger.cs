using FrozenBoyCore;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using u8 = System.Byte;
using u16 = System.UInt16;


namespace FrozenBoyDebugger {
    public partial class FrmDebugger : Form {

        private const string lineFormat = "{0,-15} ;${1,-6:x4} {2}";
        private const string opcodeFormat = "{0,-15} ;${1,-6:x4}";

        private int PC;
        private byte[] romBytes;

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
        }

        private void FrmDebugger_Load(object sender, EventArgs e) {
            Disassemble();
            disassemblerView.SelectedIndex = 0;

            gb = new GameBoy();

        }

        private void Disassemble() {
            romBytes = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\cpu_instrs.gb");
            // romBytes = File.ReadAllBytes(@"D:\Users\frozen\Documents\99_temp\GB_ROM\boot_rom.gb");

            int line = 0;

            while (PC < romBytes.Length) {
                byte b = romBytes[PC];

                if (Opcodes.unprefixed.ContainsKey(b)) {
                    Opcode opcode = Opcodes.unprefixed[b];

                    disassemblerView.Items.Add(DisassembleOpcode(opcodeFormat, opcode, PC, romBytes));

                    addressLineMap.Add(PC, line);
                    line++;

                    PC += opcode.length;
                }
                else {
                    disassemblerView.Items.Add(String.Format(opcodeFormat, PC, String.Format("0x{0:x2}---->TODO", b)));
                    addressLineMap.Add(PC, line);
                    line++;

                    PC++;
                }
            }
        }

        private string DisassembleOpcode(string format, Opcode o, int address, byte[] m) {
            return o.length switch
            {
                2 => String.Format(format, String.Format(o.asmInstruction, address, m[address + 1]), address),
                3 => String.Format(format, String.Format(o.asmInstruction, m[address + 1], m[address + 2]), address),
                _ => String.Format(format, o.asmInstruction, address),
            };
        }


        private void BtnNext_Click(object sender, EventArgs e) {
            gb.cpu.Step();

            history.AppendText(DumpState() + Environment.NewLine);

            prevA = gb.cpu.registers.A;
            prevB = gb.cpu.registers.B;
            prevC = gb.cpu.registers.C;
            prevD = gb.cpu.registers.D;
            prevE = gb.cpu.registers.E;
            prevF = gb.cpu.registers.F;
            prevH = gb.cpu.registers.H;
            prevL = gb.cpu.registers.L;

            disassemblerView.SelectedIndex = addressLineMap[gb.cpu.registers.PC];
        }

        private string DumpState() {
            return gb.cpu.opcode.length switch
            {
                2 => String.Format(lineFormat,
                                   String.Format(gb.cpu.opcode.asmInstruction,
                                                 gb.cpu.registers.PC,
                                                 gb.cpu.memory.data[gb.cpu.registers.PC + 1]),
                                   gb.cpu.registers.PC,
                                   gb.cpu.registers.ToString()),

                3 => String.Format(lineFormat,
                                   String.Format(gb.cpu.opcode.asmInstruction,
                                                 gb.cpu.registers.PC,
                                                 gb.cpu.memory.data[gb.cpu.registers.PC + 1],
                                                 gb.cpu.memory.data[gb.cpu.registers.PC + 2]),
                                   gb.cpu.registers.PC,
                                   gb.cpu.registers.ToString()),

                _ => String.Format(lineFormat, gb.cpu.opcode.asmInstruction, gb.cpu.registers.PC, gb.cpu.registers.ToString()),
            };
        }

        private void History_TextChanged(object sender, EventArgs e) {
            // set the current caret position to the end
            history.SelectionStart = history.Text.Length;
            // scroll it automatically
            history.ScrollToCaret();
        }
    }
}
