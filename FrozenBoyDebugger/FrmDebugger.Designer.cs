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
            this.disassembly = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(416, 12);
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
            this.history.Location = new System.Drawing.Point(416, 47);
            this.history.Name = "history";
            this.history.Size = new System.Drawing.Size(768, 635);
            this.history.TabIndex = 1;
            this.history.Text = "";
            this.history.TextChanged += new System.EventHandler(this.History_TextChanged);
            // 
            // disassembly
            // 
            this.disassembly.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.disassembly.FormattingEnabled = true;
            this.disassembly.ItemHeight = 18;
            this.disassembly.Location = new System.Drawing.Point(12, 12);
            this.disassembly.Name = "disassembly";
            this.disassembly.Size = new System.Drawing.Size(389, 670);
            this.disassembly.TabIndex = 3;
            // 
            // FrmDebugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1194, 691);
            this.Controls.Add(this.disassembly);
            this.Controls.Add(this.history);
            this.Controls.Add(this.btnNext);
            this.Name = "FrmDebugger";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Debugger";
            this.Load += new System.EventHandler(this.FrmDebugger_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.RichTextBox history;
        private System.Windows.Forms.ListBox disassembly;
    }
}