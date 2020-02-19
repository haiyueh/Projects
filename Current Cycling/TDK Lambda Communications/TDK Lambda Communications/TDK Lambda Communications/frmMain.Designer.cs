namespace TDK_Lambda_Communications
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
            this.components = new System.ComponentModel.Container();
            this.serTDK = new System.IO.Ports.SerialPort(this.components);
            this.btnEnumerate = new System.Windows.Forms.Button();
            this.cmbSerial = new System.Windows.Forms.ComboBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblInstVoltage = new System.Windows.Forms.Label();
            this.lblInstCurrent = new System.Windows.Forms.Label();
            this.btnSetAddress = new System.Windows.Forms.Button();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.txtVoltage = new System.Windows.Forms.TextBox();
            this.btnSetCurrent = new System.Windows.Forms.Button();
            this.txtCurrent = new System.Windows.Forms.TextBox();
            this.btnSetVoltage = new System.Windows.Forms.Button();
            this.btnOff = new System.Windows.Forms.Button();
            this.btnOn = new System.Windows.Forms.Button();
            this.tmrMain = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnEnumerate
            // 
            this.btnEnumerate.Location = new System.Drawing.Point(17, 65);
            this.btnEnumerate.Name = "btnEnumerate";
            this.btnEnumerate.Size = new System.Drawing.Size(121, 61);
            this.btnEnumerate.TabIndex = 0;
            this.btnEnumerate.Text = "Enumerate Serial Ports";
            this.btnEnumerate.UseVisualStyleBackColor = true;
            this.btnEnumerate.Click += new System.EventHandler(this.btnEnumerate_Click);
            // 
            // cmbSerial
            // 
            this.cmbSerial.FormattingEnabled = true;
            this.cmbSerial.Location = new System.Drawing.Point(17, 12);
            this.cmbSerial.Name = "cmbSerial";
            this.cmbSerial.Size = new System.Drawing.Size(121, 28);
            this.cmbSerial.TabIndex = 1;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(17, 141);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(121, 38);
            this.btnOpen.TabIndex = 2;
            this.btnOpen.Text = "Open Port";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose
            // 
            this.btnClose.Enabled = false;
            this.btnClose.Location = new System.Drawing.Point(17, 185);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(121, 38);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close Port";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblInstVoltage
            // 
            this.lblInstVoltage.AutoSize = true;
            this.lblInstVoltage.Location = new System.Drawing.Point(13, 276);
            this.lblInstVoltage.Name = "lblInstVoltage";
            this.lblInstVoltage.Size = new System.Drawing.Size(68, 20);
            this.lblInstVoltage.TabIndex = 4;
            this.lblInstVoltage.Text = "Voltage:";
            // 
            // lblInstCurrent
            // 
            this.lblInstCurrent.AutoSize = true;
            this.lblInstCurrent.Location = new System.Drawing.Point(13, 312);
            this.lblInstCurrent.Name = "lblInstCurrent";
            this.lblInstCurrent.Size = new System.Drawing.Size(70, 20);
            this.lblInstCurrent.TabIndex = 5;
            this.lblInstCurrent.Text = "Current: ";
            // 
            // btnSetAddress
            // 
            this.btnSetAddress.Location = new System.Drawing.Point(175, 12);
            this.btnSetAddress.Name = "btnSetAddress";
            this.btnSetAddress.Size = new System.Drawing.Size(121, 38);
            this.btnSetAddress.TabIndex = 6;
            this.btnSetAddress.Text = "Set Address";
            this.btnSetAddress.UseVisualStyleBackColor = true;
            this.btnSetAddress.Click += new System.EventHandler(this.btnSetAddress_Click);
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(314, 18);
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(63, 26);
            this.txtAddress.TabIndex = 7;
            this.txtAddress.Text = "1";
            // 
            // txtVoltage
            // 
            this.txtVoltage.Location = new System.Drawing.Point(314, 147);
            this.txtVoltage.Name = "txtVoltage";
            this.txtVoltage.Size = new System.Drawing.Size(63, 26);
            this.txtVoltage.TabIndex = 9;
            this.txtVoltage.Text = "0";
            // 
            // btnSetCurrent
            // 
            this.btnSetCurrent.Location = new System.Drawing.Point(175, 185);
            this.btnSetCurrent.Name = "btnSetCurrent";
            this.btnSetCurrent.Size = new System.Drawing.Size(121, 38);
            this.btnSetCurrent.TabIndex = 8;
            this.btnSetCurrent.Text = "Set Current";
            this.btnSetCurrent.UseVisualStyleBackColor = true;
            this.btnSetCurrent.Click += new System.EventHandler(this.btnSetCurrent_Click);
            // 
            // txtCurrent
            // 
            this.txtCurrent.Location = new System.Drawing.Point(314, 191);
            this.txtCurrent.Name = "txtCurrent";
            this.txtCurrent.Size = new System.Drawing.Size(63, 26);
            this.txtCurrent.TabIndex = 11;
            this.txtCurrent.Text = "0";
            // 
            // btnSetVoltage
            // 
            this.btnSetVoltage.Location = new System.Drawing.Point(175, 141);
            this.btnSetVoltage.Name = "btnSetVoltage";
            this.btnSetVoltage.Size = new System.Drawing.Size(121, 38);
            this.btnSetVoltage.TabIndex = 10;
            this.btnSetVoltage.Text = "Set Voltage";
            this.btnSetVoltage.UseVisualStyleBackColor = true;
            this.btnSetVoltage.Click += new System.EventHandler(this.btnSetVoltage_Click);
            // 
            // btnOff
            // 
            this.btnOff.Location = new System.Drawing.Point(423, 185);
            this.btnOff.Name = "btnOff";
            this.btnOff.Size = new System.Drawing.Size(121, 38);
            this.btnOff.TabIndex = 13;
            this.btnOff.Text = "Off";
            this.btnOff.UseVisualStyleBackColor = true;
            this.btnOff.Click += new System.EventHandler(this.btnOff_Click);
            // 
            // btnOn
            // 
            this.btnOn.Location = new System.Drawing.Point(423, 141);
            this.btnOn.Name = "btnOn";
            this.btnOn.Size = new System.Drawing.Size(121, 38);
            this.btnOn.TabIndex = 12;
            this.btnOn.Text = "On";
            this.btnOn.UseVisualStyleBackColor = true;
            this.btnOn.Click += new System.EventHandler(this.btnOn_Click);
            // 
            // tmrMain
            // 
            this.tmrMain.Tick += new System.EventHandler(this.tmrMain_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 406);
            this.Controls.Add(this.btnOff);
            this.Controls.Add(this.btnOn);
            this.Controls.Add(this.txtCurrent);
            this.Controls.Add(this.btnSetVoltage);
            this.Controls.Add(this.txtVoltage);
            this.Controls.Add(this.btnSetCurrent);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.btnSetAddress);
            this.Controls.Add(this.lblInstCurrent);
            this.Controls.Add(this.lblInstVoltage);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.cmbSerial);
            this.Controls.Add(this.btnEnumerate);
            this.Name = "frmMain";
            this.Text = "TDK Power Supply Communication Sample";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort serTDK;
        private System.Windows.Forms.Button btnEnumerate;
        private System.Windows.Forms.ComboBox cmbSerial;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblInstVoltage;
        private System.Windows.Forms.Label lblInstCurrent;
        private System.Windows.Forms.Button btnSetAddress;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.TextBox txtVoltage;
        private System.Windows.Forms.Button btnSetCurrent;
        private System.Windows.Forms.TextBox txtCurrent;
        private System.Windows.Forms.Button btnSetVoltage;
        private System.Windows.Forms.Button btnOff;
        private System.Windows.Forms.Button btnOn;
        private System.Windows.Forms.Timer tmrMain;
    }
}

