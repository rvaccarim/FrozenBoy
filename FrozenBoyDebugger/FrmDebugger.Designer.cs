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
            this.btnSave = new System.Windows.Forms.Button();
            this.Instruction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.disasmGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.disasmGrid)).BeginInit();
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
            this.history.Size = new System.Drawing.Size(813, 635);
            this.history.TabIndex = 1;
            this.history.Text = "";
            this.history.TextChanged += new System.EventHandler(this.History_TextChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(471, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(106, 29);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save Disasm";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            // FrmDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1194, 691);
            this.Controls.Add(this.disasmGrid);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.history);
            this.Controls.Add(this.btnNext);
            this.Name = "FrmDebugger";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Debugger";
            this.Load += new System.EventHandler(this.FrmDebugger_Load);
            ((System.ComponentModel.ISupportInitialize)(this.disasmGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.RichTextBox history;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridView disasmGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Instruction;
    }
}