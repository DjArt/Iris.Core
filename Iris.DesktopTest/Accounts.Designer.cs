namespace Iris.DesktopTest
{
    partial class Accounts
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lb_Accounts = new System.Windows.Forms.ListBox();
            this.lb_Connections = new System.Windows.Forms.ListBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lb_Accounts);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 426);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Аккаунты";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lb_Connections);
            this.groupBox2.Location = new System.Drawing.Point(218, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(579, 426);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Подключения";
            // 
            // lb_Accounts
            // 
            this.lb_Accounts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lb_Accounts.FormattingEnabled = true;
            this.lb_Accounts.Location = new System.Drawing.Point(3, 16);
            this.lb_Accounts.Name = "lb_Accounts";
            this.lb_Accounts.Size = new System.Drawing.Size(194, 407);
            this.lb_Accounts.TabIndex = 0;
            // 
            // lb_Connections
            // 
            this.lb_Connections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lb_Connections.FormattingEnabled = true;
            this.lb_Connections.Location = new System.Drawing.Point(3, 16);
            this.lb_Connections.Name = "lb_Connections";
            this.lb_Connections.Size = new System.Drawing.Size(573, 407);
            this.lb_Connections.TabIndex = 0;
            // 
            // Accounts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Accounts";
            this.Text = "Accounts";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lb_Accounts;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lb_Connections;
    }
}