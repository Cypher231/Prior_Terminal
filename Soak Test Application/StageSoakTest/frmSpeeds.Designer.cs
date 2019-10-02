namespace StageSoakTest
{
    partial class frmSpeeds
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lstSpeed = new System.Windows.Forms.ListBox();
            this.lstAcc = new System.Windows.Forms.ListBox();
            this.lstCurve = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Speed";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(88, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Acc";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(150, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Curve";
            // 
            // lstSpeed
            // 
            this.lstSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstSpeed.FormattingEnabled = true;
            this.lstSpeed.ItemHeight = 20;
            this.lstSpeed.Location = new System.Drawing.Point(10, 25);
            this.lstSpeed.Name = "lstSpeed";
            this.lstSpeed.Size = new System.Drawing.Size(65, 124);
            this.lstSpeed.TabIndex = 3;
            this.lstSpeed.SelectedIndexChanged += new System.EventHandler(this.lstSpeed_SelectedIndexChanged);
            // 
            // lstAcc
            // 
            this.lstAcc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstAcc.FormattingEnabled = true;
            this.lstAcc.ItemHeight = 20;
            this.lstAcc.Location = new System.Drawing.Point(80, 25);
            this.lstAcc.Name = "lstAcc";
            this.lstAcc.Size = new System.Drawing.Size(65, 124);
            this.lstAcc.TabIndex = 4;
            this.lstAcc.SelectedIndexChanged += new System.EventHandler(this.lstAcc_SelectedIndexChanged);
            // 
            // lstCurve
            // 
            this.lstCurve.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstCurve.FormattingEnabled = true;
            this.lstCurve.ItemHeight = 20;
            this.lstCurve.Location = new System.Drawing.Point(150, 25);
            this.lstCurve.Name = "lstCurve";
            this.lstCurve.Size = new System.Drawing.Size(65, 124);
            this.lstCurve.TabIndex = 5;
            this.lstCurve.SelectedIndexChanged += new System.EventHandler(this.lstCurve_SelectedIndexChanged);
            // 
            // frmSpeeds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(224, 185);
            this.Controls.Add(this.lstCurve);
            this.Controls.Add(this.lstAcc);
            this.Controls.Add(this.lstSpeed);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSpeeds";
            this.Text = "SAC";
            this.Load += new System.EventHandler(this.frmSpeeds_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSpeeds_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ListBox lstSpeed;
        public System.Windows.Forms.ListBox lstAcc;
        public System.Windows.Forms.ListBox lstCurve;
    }
}