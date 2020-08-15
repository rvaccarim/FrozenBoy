namespace FrozenBoyDebugger {
    partial class FrmDebugger {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.btnNext = new System.Windows.Forms.Button();
            this.history = new System.Windows.Forms.RichTextBox();
            this.Instruction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.disasmGrid = new System.Windows.Forms.DataGridView();
            this.histInstruction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histF = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histH = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histFlagZ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histFlagN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histFlagH = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.histFlagC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.historyGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.disasmGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.historyGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(371, 12);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(94, 29);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.BtnNext_Click);
            // 
            // history
            // 
            this.history.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.history.Location = new System.Drawing.Point(371, 47);
            this.history.Name = "history";
            this.history.Size = new System.Drawing.Size(813, 199);
            this.history.TabIndex = 1;
            this.history.Text = "";
            this.history.TextChanged += new System.EventHandler(this.History_TextChanged);
            // 
            // Instruction
            // 
            this.Instruction.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Instruction.HeaderText = "Instruction";
            this.Instruction.MinimumWidth = 6;
            this.Instruction.Name = "Instruction";
            this.Instruction.ReadOnly = true;
            this.Instruction.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Instruction.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // disasmGrid
            // 
            this.disasmGrid.AllowUserToAddRows = false;
            this.disasmGrid.AllowUserToDeleteRows = false;
            this.disasmGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.disasmGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Instruction});
            this.disasmGrid.Location = new System.Drawing.Point(12, 12);
            this.disasmGrid.MultiSelect = false;
            this.disasmGrid.Name = "disasmGrid";
            this.disasmGrid.ReadOnly = true;
            this.disasmGrid.RowHeadersWidth = 51;
            this.disasmGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.disasmGrid.Size = new System.Drawing.Size(353, 670);
            this.disasmGrid.TabIndex = 5;
            this.disasmGrid.Text = "dataGridView1";
            // 
            // histInstruction
            // 
            this.histInstruction.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histInstruction.HeaderText = "Instruction";
            this.histInstruction.MinimumWidth = 6;
            this.histInstruction.Name = "histInstruction";
            this.histInstruction.ReadOnly = true;
            this.histInstruction.Width = 107;
            // 
            // histA
            // 
            this.histA.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histA.HeaderText = "A";
            this.histA.MinimumWidth = 6;
            this.histA.Name = "histA";
            this.histA.ReadOnly = true;
            this.histA.Width = 48;
            // 
            // histB
            // 
            this.histB.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histB.HeaderText = "B";
            this.histB.MinimumWidth = 6;
            this.histB.Name = "histB";
            this.histB.ReadOnly = true;
            this.histB.Width = 47;
            // 
            // histC
            // 
            this.histC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histC.HeaderText = "C";
            this.histC.MinimumWidth = 6;
            this.histC.Name = "histC";
            this.histC.ReadOnly = true;
            this.histC.Width = 47;
            // 
            // histD
            // 
            this.histD.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histD.HeaderText = "D";
            this.histD.MinimumWidth = 6;
            this.histD.Name = "histD";
            this.histD.ReadOnly = true;
            this.histD.Width = 49;
            // 
            // histE
            // 
            this.histE.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histE.HeaderText = "E";
            this.histE.MinimumWidth = 6;
            this.histE.Name = "histE";
            this.histE.ReadOnly = true;
            this.histE.Width = 46;
            // 
            // histF
            // 
            this.histF.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histF.HeaderText = "F";
            this.histF.MinimumWidth = 6;
            this.histF.Name = "histF";
            this.histF.ReadOnly = true;
            this.histF.Width = 45;
            // 
            // histH
            // 
            this.histH.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histH.HeaderText = "H";
            this.histH.MinimumWidth = 6;
            this.histH.Name = "histH";
            this.histH.ReadOnly = true;
            this.histH.Width = 49;
            // 
            // histL
            // 
            this.histL.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histL.HeaderText = "L";
            this.histL.MinimumWidth = 6;
            this.histL.Name = "histL";
            this.histL.ReadOnly = true;
            this.histL.Width = 45;
            // 
            // histFlagZ
            // 
            this.histFlagZ.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histFlagZ.HeaderText = "FZ";
            this.histFlagZ.MinimumWidth = 6;
            this.histFlagZ.Name = "histFlagZ";
            this.histFlagZ.ReadOnly = true;
            this.histFlagZ.Width = 54;
            // 
            // histFlagN
            // 
            this.histFlagN.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histFlagN.HeaderText = "FN";
            this.histFlagN.MinimumWidth = 6;
            this.histFlagN.Name = "histFlagN";
            this.histFlagN.ReadOnly = true;
            this.histFlagN.Width = 56;
            // 
            // histFlagH
            // 
            this.histFlagH.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histFlagH.HeaderText = "FH";
            this.histFlagH.MinimumWidth = 6;
            this.histFlagH.Name = "histFlagH";
            this.histFlagH.ReadOnly = true;
            this.histFlagH.Width = 56;
            // 
            // histFlagC
            // 
            this.histFlagC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.histFlagC.HeaderText = "FC";
            this.histFlagC.MinimumWidth = 6;
            this.histFlagC.Name = "histFlagC";
            this.histFlagC.ReadOnly = true;
            this.histFlagC.Width = 54;
            // 
            // historyGrid
            // 
            this.historyGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.historyGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.histInstruction,
            this.histA,
            this.histB,
            this.histC,
            this.histD,
            this.histE,
            this.histF,
            this.histH,
            this.histL,
            this.histFlagZ,
            this.histFlagN,
            this.histFlagH,
            this.histFlagC});
            this.historyGrid.Location = new System.Drawing.Point(372, 263);
            this.historyGrid.MultiSelect = false;
            this.historyGrid.Name = "historyGrid";
            this.historyGrid.ReadOnly = true;
            this.historyGrid.RowHeadersWidth = 51;
            this.historyGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.historyGrid.Size = new System.Drawing.Size(810, 416);
            this.historyGrid.TabIndex = 6;
            this.historyGrid.Text = "dataGridView1";
            // 
            // FrmDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1194, 691);
            this.Controls.Add(this.historyGrid);
            this.Controls.Add(this.disasmGrid);
            this.Controls.Add(this.history);
            this.Controls.Add(this.btnNext);
            this.Name = "FrmDebugger";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Debugger";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmDebugger_Load);
            ((System.ComponentModel.ISupportInitialize)(this.disasmGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.historyGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.RichTextBox history;
        private System.Windows.Forms.DataGridView disasmGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Instruction;
        private System.Windows.Forms.DataGridView historyGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn histInstruction;
        private System.Windows.Forms.DataGridViewTextBoxColumn histA;
        private System.Windows.Forms.DataGridViewTextBoxColumn histB;
        private System.Windows.Forms.DataGridViewTextBoxColumn histC;
        private System.Windows.Forms.DataGridViewTextBoxColumn histD;
        private System.Windows.Forms.DataGridViewTextBoxColumn histE;
        private System.Windows.Forms.DataGridViewTextBoxColumn histF;
        private System.Windows.Forms.DataGridViewTextBoxColumn histH;
        private System.Windows.Forms.DataGridViewTextBoxColumn histL;
        private System.Windows.Forms.DataGridViewTextBoxColumn histFlagZ;
        private System.Windows.Forms.DataGridViewTextBoxColumn histFlagN;
        private System.Windows.Forms.DataGridViewTextBoxColumn histFlagH;
        private System.Windows.Forms.DataGridViewTextBoxColumn histFlagC;

    }
}