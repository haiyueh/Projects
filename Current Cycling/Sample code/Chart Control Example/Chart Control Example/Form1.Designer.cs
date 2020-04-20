namespace Chart_Control_Example
{
    partial class Form1
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chtMain = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chtMain)).BeginInit();
            this.SuspendLayout();
            // 
            // chtMain
            // 
            chartArea1.AxisX.MajorGrid.LineWidth = 0;
            chartArea1.AxisY.MajorGrid.LineWidth = 0;
            chartArea1.Name = "ChartArea1";
            this.chtMain.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            legend1.Position.Auto = false;
            legend1.Position.Height = 7.379135F;
            legend1.Position.Width = 17.76316F;
            legend1.Position.X = 79.23684F;
            legend1.Position.Y = 3F;
            this.chtMain.Legends.Add(legend1);
            this.chtMain.Location = new System.Drawing.Point(48, 15);
            this.chtMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chtMain.Name = "chtMain";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Legend = "Legend1";
            series1.Name = "Voltage by Cycle";
            this.chtMain.Series.Add(series1);
            this.chtMain.Size = new System.Drawing.Size(913, 394);
            this.chtMain.TabIndex = 0;
            this.chtMain.Text = "chart1";
            this.chtMain.Click += new System.EventHandler(this.chtMain_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(48, 442);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(287, 28);
            this.button1.TabIndex = 1;
            this.button1.Text = "Push Me";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.chtMain);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.chtMain)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chtMain;
        private System.Windows.Forms.Button button1;
    }
}

