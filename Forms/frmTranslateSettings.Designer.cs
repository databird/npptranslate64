namespace nppTranslateCS.Forms
{
    partial class frmTranslateSettings
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
            this.from = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnExchange = new System.Windows.Forms.Button();
            this.to = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.chkAlways = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // from
            // 
            this.from.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.from.FormattingEnabled = true;
            this.from.Location = new System.Drawing.Point(8, 29);
            this.from.Margin = new System.Windows.Forms.Padding(4);
            this.from.Name = "from";
            this.from.Size = new System.Drawing.Size(158, 30);
            this.from.TabIndex = 7;
            this.from.Text = "FROM";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExchange);
            this.groupBox1.Controls.Add(this.to);
            this.groupBox1.Controls.Add(this.from);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(25, 18);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(368, 76);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Language Preference";
            // 
            // btnExchange
            // 
            this.btnExchange.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnExchange.Image = global::nppTranslateCS.Properties.Resources.arrow_right_left1;
            this.btnExchange.Location = new System.Drawing.Point(171, 28);
            this.btnExchange.Name = "btnExchange";
            this.btnExchange.Size = new System.Drawing.Size(39, 32);
            this.btnExchange.TabIndex = 9;
            this.btnExchange.UseVisualStyleBackColor = true;
            this.btnExchange.Click += new System.EventHandler(this.btnExchange_Click);
            // 
            // to
            // 
            this.to.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.to.FormattingEnabled = true;
            this.to.Location = new System.Drawing.Point(216, 29);
            this.to.Margin = new System.Windows.Forms.Padding(4);
            this.to.Name = "to";
            this.to.Size = new System.Drawing.Size(144, 30);
            this.to.TabIndex = 8;
            this.to.Text = "TO";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(133, 168);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(144, 46);
            this.button1.TabIndex = 10;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // chkAlways
            // 
            this.chkAlways.AutoSize = true;
            this.chkAlways.Location = new System.Drawing.Point(25, 112);
            this.chkAlways.Name = "chkAlways";
            this.chkAlways.Size = new System.Drawing.Size(380, 29);
            this.chkAlways.TabIndex = 11;
            this.chkAlways.Text = "Always display this dialog on translation";
            this.chkAlways.UseVisualStyleBackColor = true;
            // 
            // frmTranslateSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 227);
            this.Controls.Add(this.chkAlways);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmTranslateSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Translate Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TranslateSettings_FormClosing);
            this.Load += new System.EventHandler(this.TranslateSettings_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox from;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox to;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnExchange;
        private System.Windows.Forms.CheckBox chkAlways;
    }
}