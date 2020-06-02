namespace BigQuery_Example
{
    partial class frmMain
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
            this.btnAddData = new System.Windows.Forms.Button();
            this.txtData = new System.Windows.Forms.TextBox();
            this.lblData = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnAddData
            // 
            this.btnAddData.Location = new System.Drawing.Point(16, 73);
            this.btnAddData.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAddData.Name = "btnAddData";
            this.btnAddData.Size = new System.Drawing.Size(100, 28);
            this.btnAddData.TabIndex = 0;
            this.btnAddData.Text = "Add Data";
            this.btnAddData.UseVisualStyleBackColor = true;
            this.btnAddData.Click += new System.EventHandler(this.btnAddData_Click);
            // 
            // txtData
            // 
            this.txtData.Location = new System.Drawing.Point(16, 41);
            this.txtData.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtData.Name = "txtData";
            this.txtData.Size = new System.Drawing.Size(352, 22);
            this.txtData.TabIndex = 1;
            // 
            // lblData
            // 
            this.lblData.AutoSize = true;
            this.lblData.Location = new System.Drawing.Point(17, 17);
            this.lblData.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblData.Name = "lblData";
            this.lblData.Size = new System.Drawing.Size(92, 17);
            this.lblData.TabIndex = 2;
            this.lblData.Text = "Data To Add:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(135, 73);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 28);
            this.button1.TabIndex = 3;
            this.button1.Text = "Query Recipe";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += BtnQueryClick;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 112);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblData);
            this.Controls.Add(this.txtData);
            this.Controls.Add(this.btnAddData);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmMain";
            this.Text = "BigQuery C# Example";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAddData;
        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.Label lblData;
        private System.Windows.Forms.Button button1;
    }
}

