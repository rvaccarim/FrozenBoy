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

            this.histInstruction.HeaderText = "Instruction";
            this.histInstruction.MinimumWidth = 6;
            this.histInstruction.Name = "histInstruction";
            this.histInstruction.Width = 125;

            // 
            // histA
            // 
            this.histA.HeaderText = "A";
            this.histA.MinimumWidth = 6;
            this.histA.Name = "histA";
            this.histA.Width = 125;
            // 
            // histB
            // 
            this.histB.HeaderText = "B";
            this.histB.MinimumWidth = 6;
            this.histB.Name = "histB";
            this.histB.Width = 125;
            // 
            // histC
            // 
            this.histC.HeaderText = "C";
            this.histC.MinimumWidth = 6;
            this.histC.Name = "histC";
            this.histC.Width = 125;
            // 
            // histD
            // 
            this.histD.HeaderText = "D";
            this.histD.MinimumWidth = 6;
            this.histD.Name = "histD";
            this.histD.Width = 125;
            // 
            // histE
            // 
            this.histE.HeaderText = "E";
            this.histE.MinimumWidth = 6;
            this.histE.Name = "histE";
            this.histE.Width = 125;
            // 
            // histF
            // 
            this.histF.HeaderText = "F";
            this.histF.MinimumWidth = 6;
            this.histF.Name = "histF";
            this.histF.Width = 125;
            // 
            // histH
            // 
            this.histH.HeaderText = "H";
            this.histH.MinimumWidth = 6;
            this.histH.Name = "histH";
            this.histH.Width = 125;
            // 
            // histL
            // 
            this.histL.HeaderText = "L";
            this.histL.MinimumWidth = 6;
            this.histL.Name = "histL";
            this.histL.Width = 125;
            // 
            // histFlagZ
            // 
            this.histFlagZ.HeaderText = "FZ";
            this.histFlagZ.MinimumWidth = 6;
            this.histFlagZ.Name = "histFlagZ";
            this.histFlagZ.Width = 125;
            // 
            // histFlagN
            // 
            this.histFlagN.HeaderText = "FN";
            this.histFlagN.MinimumWidth = 6;
            this.histFlagN.Name = "histFlagN";
            this.histFlagN.Width = 125;
            // 
            // histFlagH
            // 
            this.histFlagH.HeaderText = "FH";
            this.histFlagH.MinimumWidth = 6;
            this.histFlagH.Name = "histFlagH";
            this.histFlagH.Width = 125;
            // 
            // histFlagC
            // 
            this.histFlagC.HeaderText = "FC";
            this.histFlagC.MinimumWidth = 6;
            this.histFlagC.Name = "histFlagC";
            this.histFlagC.Width = 125;
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
            this.historyGrid.Name = "historyGrid";
            this.historyGrid.RowHeadersWidth = 51;
            this.historyGrid.Size = new System.Drawing.Size(745, 416);
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