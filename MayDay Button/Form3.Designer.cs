namespace MayDayButton
{
    partial class Form3
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
            this.logView = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button10 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.yPos = new System.Windows.Forms.NumericUpDown();
            this.sUpdate = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cTimes = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.yAdj = new System.Windows.Forms.NumericUpDown();
            this.highDPI = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.xPos = new System.Windows.Forms.NumericUpDown();
            this.aAdmin = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button13 = new System.Windows.Forms.Button();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yPos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cTimes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yAdj)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xPos)).BeginInit();
            this.SuspendLayout();
            // 
            // logView
            // 
            this.logView.Location = new System.Drawing.Point(280, 38);
            this.logView.Name = "logView";
            this.logView.Size = new System.Drawing.Size(348, 596);
            this.logView.TabIndex = 0;
            this.logView.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(280, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Log";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button10);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.yPos);
            this.groupBox1.Controls.Add(this.sUpdate);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.cTimes);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.yAdj);
            this.groupBox1.Controls.Add(this.highDPI);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.xPos);
            this.groupBox1.Controls.Add(this.aAdmin);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(262, 180);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings - XXXX";
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(10, 144);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(246, 23);
            this.button10.TabIndex = 13;
            this.button10.Text = "Save Changes";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Y POS";
            // 
            // yPos
            // 
            this.yPos.Location = new System.Drawing.Point(7, 118);
            this.yPos.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.yPos.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.yPos.Name = "yPos";
            this.yPos.Size = new System.Drawing.Size(95, 20);
            this.yPos.TabIndex = 11;
            // 
            // sUpdate
            // 
            this.sUpdate.AutoSize = true;
            this.sUpdate.Location = new System.Drawing.Point(158, 49);
            this.sUpdate.Name = "sUpdate";
            this.sUpdate.Size = new System.Drawing.Size(97, 17);
            this.sUpdate.TabIndex = 10;
            this.sUpdate.Text = "Should Update";
            this.sUpdate.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(158, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Contact Times";
            // 
            // cTimes
            // 
            this.cTimes.Location = new System.Drawing.Point(158, 116);
            this.cTimes.Name = "cTimes";
            this.cTimes.Size = new System.Drawing.Size(95, 20);
            this.cTimes.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Y POS Adjustment";
            // 
            // yAdj
            // 
            this.yAdj.Location = new System.Drawing.Point(6, 75);
            this.yAdj.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.yAdj.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.yAdj.Name = "yAdj";
            this.yAdj.Size = new System.Drawing.Size(95, 20);
            this.yAdj.TabIndex = 6;
            // 
            // highDPI
            // 
            this.highDPI.AutoSize = true;
            this.highDPI.Location = new System.Drawing.Point(158, 26);
            this.highDPI.Name = "highDPI";
            this.highDPI.Size = new System.Drawing.Size(69, 17);
            this.highDPI.TabIndex = 5;
            this.highDPI.Text = "High DPI";
            this.highDPI.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "X POS";
            // 
            // xPos
            // 
            this.xPos.Location = new System.Drawing.Point(6, 36);
            this.xPos.Name = "xPos";
            this.xPos.Size = new System.Drawing.Size(95, 20);
            this.xPos.TabIndex = 3;
            // 
            // aAdmin
            // 
            this.aAdmin.AutoSize = true;
            this.aAdmin.Location = new System.Drawing.Point(158, 72);
            this.aAdmin.Name = "aAdmin";
            this.aAdmin.Size = new System.Drawing.Size(95, 17);
            this.aAdmin.TabIndex = 3;
            this.aAdmin.Text = "Start As Admin";
            this.aAdmin.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 198);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 40);
            this.button1.TabIndex = 3;
            this.button1.Text = "Set Startup";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(145, 198);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(129, 40);
            this.button2.TabIndex = 4;
            this.button2.Text = "Update All Devices";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 290);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(127, 40);
            this.button3.TabIndex = 5;
            this.button3.Text = "Change Vacation Status";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(145, 244);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(127, 40);
            this.button4.TabIndex = 6;
            this.button4.Text = "Close";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(12, 336);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(127, 40);
            this.button5.TabIndex = 7;
            this.button5.Text = "Restart";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(145, 290);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(127, 40);
            this.button6.TabIndex = 8;
            this.button6.Text = "Restart As Admin";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(12, 382);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(127, 40);
            this.button7.TabIndex = 9;
            this.button7.Text = "Reestablish Server Connection";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(145, 336);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(127, 40);
            this.button8.TabIndex = 10;
            this.button8.Text = "Open Command Prompt";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(12, 428);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(127, 40);
            this.button9.TabIndex = 11;
            this.button9.Text = " Restart Register";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(9, 545);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(127, 40);
            this.button11.TabIndex = 12;
            this.button11.Text = "Export Settings";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(145, 545);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(127, 40);
            this.button12.TabIndex = 13;
            this.button12.Text = "Import Settings";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(77, 591);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(127, 40);
            this.button13.TabIndex = 14;
            this.button13.Text = "Import Settings To All Registers";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button14
            // 
            this.button14.Location = new System.Drawing.Point(145, 382);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(127, 40);
            this.button14.TabIndex = 15;
            this.button14.Text = "Run Checks";
            this.button14.UseVisualStyleBackColor = true;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // button15
            // 
            this.button15.Location = new System.Drawing.Point(145, 428);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(127, 40);
            this.button15.TabIndex = 16;
            this.button15.Text = "Delete Log";
            this.button15.UseVisualStyleBackColor = true;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // button17
            // 
            this.button17.Location = new System.Drawing.Point(12, 244);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(127, 40);
            this.button17.TabIndex = 18;
            this.button17.Text = "Delete Startup";
            this.button17.UseVisualStyleBackColor = true;
            this.button17.Click += new System.EventHandler(this.button17_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 646);
            this.Controls.Add(this.button17);
            this.Controls.Add(this.button15);
            this.Controls.Add(this.button14);
            this.Controls.Add(this.button13);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.logView);
            this.Name = "Form3";
            this.Text = "Form3";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form3_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yPos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cTimes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yAdj)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xPos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox logView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown xPos;
        private System.Windows.Forms.CheckBox aAdmin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown yAdj;
        private System.Windows.Forms.CheckBox highDPI;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown cTimes;
        private System.Windows.Forms.CheckBox sUpdate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown yPos;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button17;
    }
}