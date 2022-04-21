namespace JDP {
    partial class frmCTWAbout {
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
            this.pictureBoxCTWLogo = new System.Windows.Forms.PictureBox();
            this.btnCloseAbout = new System.Windows.Forms.Button();
            this.lblAboutTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblCTWVersion = new System.Windows.Forms.Label();
            this.lblBuildDate = new System.Windows.Forms.Label();
            this.lblJDPAuthor = new System.Windows.Forms.Label();
            this.lblJDPURL = new System.Windows.Forms.LinkLabel();
            this.lblSuperGougeAuthor = new System.Windows.Forms.Label();
            this.lblSuperGougeRepo = new System.Windows.Forms.LinkLabel();
            this.lblNoodleAuthor = new System.Windows.Forms.Label();
            this.lblNoodleRepo = new System.Windows.Forms.LinkLabel();
            this.lblVerticalDivider = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCTWLogo)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxCTWLogo
            // 
            this.pictureBoxCTWLogo.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBoxCTWLogo.Image = global::JDP.Properties.Resources.CTWLogo;
            this.pictureBoxCTWLogo.Location = new System.Drawing.Point(2, 2);
            this.pictureBoxCTWLogo.MaximumSize = new System.Drawing.Size(128, 128);
            this.pictureBoxCTWLogo.MinimumSize = new System.Drawing.Size(128, 128);
            this.pictureBoxCTWLogo.Name = "pictureBoxCTWLogo";
            this.pictureBoxCTWLogo.Size = new System.Drawing.Size(128, 128);
            this.pictureBoxCTWLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxCTWLogo.TabIndex = 0;
            this.pictureBoxCTWLogo.TabStop = false;
            // 
            // btnCloseAbout
            // 
            this.btnCloseAbout.AutoSize = true;
            this.btnCloseAbout.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCloseAbout.Location = new System.Drawing.Point(33, 191);
            this.btnCloseAbout.Margin = new System.Windows.Forms.Padding(0);
            this.btnCloseAbout.MaximumSize = new System.Drawing.Size(75, 23);
            this.btnCloseAbout.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnCloseAbout.Name = "btnCloseAbout";
            this.btnCloseAbout.Size = new System.Drawing.Size(75, 23);
            this.btnCloseAbout.TabIndex = 1;
            this.btnCloseAbout.Text = "Close";
            this.btnCloseAbout.UseVisualStyleBackColor = true;
            this.btnCloseAbout.Click += new System.EventHandler(this.btnCloseAbout_Click);
            // 
            // lblAboutTitle
            // 
            this.lblAboutTitle.AutoSize = true;
            this.lblAboutTitle.Location = new System.Drawing.Point(3, 0);
            this.lblAboutTitle.Name = "lblAboutTitle";
            this.lblAboutTitle.Size = new System.Drawing.Size(104, 13);
            this.lblAboutTitle.TabIndex = 2;
            this.lblAboutTitle.Text = "Chan Thread Watch";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblAboutTitle, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblCTWVersion, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblBuildDate, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblJDPAuthor, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblJDPURL, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblSuperGougeAuthor, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblNoodleAuthor, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.lblSuperGougeRepo, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblNoodleRepo, 0, 10);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(133, 10);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.MaximumSize = new System.Drawing.Size(266, 211);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(266, 211);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 19F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(266, 211);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // lblCTWVersion
            // 
            this.lblCTWVersion.AutoSize = true;
            this.lblCTWVersion.Location = new System.Drawing.Point(3, 17);
            this.lblCTWVersion.Name = "lblCTWVersion";
            this.lblCTWVersion.Size = new System.Drawing.Size(48, 13);
            this.lblCTWVersion.TabIndex = 3;
            this.lblCTWVersion.Text = "Version: ";
            // 
            // lblBuildDate
            // 
            this.lblBuildDate.AutoSize = true;
            this.lblBuildDate.Location = new System.Drawing.Point(3, 36);
            this.lblBuildDate.Name = "lblBuildDate";
            this.lblBuildDate.Size = new System.Drawing.Size(62, 13);
            this.lblBuildDate.TabIndex = 4;
            this.lblBuildDate.Text = "Build Date: ";
            // 
            // lblJDPAuthor
            // 
            this.lblJDPAuthor.AutoSize = true;
            this.lblJDPAuthor.Location = new System.Drawing.Point(3, 71);
            this.lblJDPAuthor.Name = "lblJDPAuthor";
            this.lblJDPAuthor.Size = new System.Drawing.Size(212, 13);
            this.lblJDPAuthor.TabIndex = 5;
            this.lblJDPAuthor.Text = "Original Author: JDP (jart1126@yahoo.com)";
            // 
            // lblJDPURL
            // 
            this.lblJDPURL.AutoSize = true;
            this.lblJDPURL.Location = new System.Drawing.Point(3, 89);
            this.lblJDPURL.Name = "lblJDPURL";
            this.lblJDPURL.Size = new System.Drawing.Size(236, 13);
            this.lblJDPURL.TabIndex = 6;
            this.lblJDPURL.TabStop = true;
            this.lblJDPURL.Text = "https://sites.google.com/site/chanthreadwatch/";
            this.lblJDPURL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.FormLinkClick);
            // 
            // lblSuperGougeAuthor
            // 
            this.lblSuperGougeAuthor.AutoSize = true;
            this.lblSuperGougeAuthor.Location = new System.Drawing.Point(3, 117);
            this.lblSuperGougeAuthor.Name = "lblSuperGougeAuthor";
            this.lblSuperGougeAuthor.Size = new System.Drawing.Size(191, 13);
            this.lblSuperGougeAuthor.TabIndex = 7;
            this.lblSuperGougeAuthor.Text = "Previously Maintained By: SuperGouge";
            // 
            // lblSuperGougeRepo
            // 
            this.lblSuperGougeRepo.AutoSize = true;
            this.lblSuperGougeRepo.Location = new System.Drawing.Point(3, 134);
            this.lblSuperGougeRepo.Name = "lblSuperGougeRepo";
            this.lblSuperGougeRepo.Size = new System.Drawing.Size(256, 13);
            this.lblSuperGougeRepo.TabIndex = 9;
            this.lblSuperGougeRepo.TabStop = true;
            this.lblSuperGougeRepo.Text = "https://github.com/SuperGouge/ChanThreadWatch";
            this.lblSuperGougeRepo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.FormLinkClick);
            // 
            // lblNoodleAuthor
            // 
            this.lblNoodleAuthor.AutoSize = true;
            this.lblNoodleAuthor.Location = new System.Drawing.Point(3, 169);
            this.lblNoodleAuthor.Name = "lblNoodleAuthor";
            this.lblNoodleAuthor.Size = new System.Drawing.Size(156, 13);
            this.lblNoodleAuthor.TabIndex = 10;
            this.lblNoodleAuthor.Text = "Currently Maintained By: noodle";
            // 
            // lblNoodleRepo
            // 
            this.lblNoodleRepo.AutoSize = true;
            this.lblNoodleRepo.Location = new System.Drawing.Point(3, 186);
            this.lblNoodleRepo.Name = "lblNoodleRepo";
            this.lblNoodleRepo.Size = new System.Drawing.Size(238, 13);
            this.lblNoodleRepo.TabIndex = 12;
            this.lblNoodleRepo.TabStop = true;
            this.lblNoodleRepo.Text = "https://github.com/jwshields/ChanThreadWatch";
            this.lblNoodleRepo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.FormLinkClick);
            // 
            // lblVerticalDivider
            // 
            this.lblVerticalDivider.BackColor = System.Drawing.Color.Black;
            this.lblVerticalDivider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblVerticalDivider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblVerticalDivider.ForeColor = System.Drawing.Color.Black;
            this.lblVerticalDivider.Location = new System.Drawing.Point(130, 9);
            this.lblVerticalDivider.Name = "lblVerticalDivider";
            this.lblVerticalDivider.Size = new System.Drawing.Size(1, 212);
            this.lblVerticalDivider.TabIndex = 4;
            // 
            // frmCTWAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnCloseAbout;
            this.ClientSize = new System.Drawing.Size(400, 228);
            this.Controls.Add(this.lblVerticalDivider);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.pictureBoxCTWLogo);
            this.Controls.Add(this.btnCloseAbout);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(416, 267);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(416, 267);
            this.Name = "frmCTWAbout";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About ChanThreadWatch";
            this.TopMost = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormKeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCTWLogo)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxCTWLogo;
        private System.Windows.Forms.Button btnCloseAbout;
        private System.Windows.Forms.Label lblAboutTitle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblNoodleAuthor;
        private System.Windows.Forms.Label lblJDPAuthor;
        private System.Windows.Forms.Label lblSuperGougeAuthor;
        private System.Windows.Forms.Label lblCTWVersion;
        private System.Windows.Forms.Label lblBuildDate;
        private System.Windows.Forms.LinkLabel lblJDPURL;
        private System.Windows.Forms.LinkLabel lblSuperGougeRepo;
        private System.Windows.Forms.LinkLabel lblNoodleRepo;
        private System.Windows.Forms.Label lblVerticalDivider;
    }
}