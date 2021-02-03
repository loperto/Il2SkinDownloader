namespace Il2SkinDownloader
{
    partial class FormSkinDownloader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSkinDownloader));
            this.textBox_Il2Path = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OpenIl2Folder = new System.Windows.Forms.Button();
            this.label_Status = new System.Windows.Forms.Label();
            this.buttonCheckUpdates = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.listViewDiffs = new System.Windows.Forms.ListView();
            this.buttonExec = new System.Windows.Forms.Button();
            this.labelPercentage = new System.Windows.Forms.Label();
            this.progressBarSkinDownload = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox_Il2Path
            // 
            this.textBox_Il2Path.Location = new System.Drawing.Point(184, 27);
            this.textBox_Il2Path.Name = "textBox_Il2Path";
            this.textBox_Il2Path.Size = new System.Drawing.Size(553, 20);
            this.textBox_Il2Path.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(184, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(294, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Il2 Installation Path (IL-2 Sturmovik Battle of Stalingrad folder)";
            // 
            // button_OpenIl2Folder
            // 
            this.button_OpenIl2Folder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.button_OpenIl2Folder.Location = new System.Drawing.Point(738, 26);
            this.button_OpenIl2Folder.Name = "button_OpenIl2Folder";
            this.button_OpenIl2Folder.Size = new System.Drawing.Size(26, 21);
            this.button_OpenIl2Folder.TabIndex = 2;
            this.button_OpenIl2Folder.Text = "...";
            this.button_OpenIl2Folder.UseVisualStyleBackColor = true;
            this.button_OpenIl2Folder.Click += new System.EventHandler(this.Button_OpenIl2Folder_Click);
            // 
            // label_Status
            // 
            this.label_Status.AutoSize = true;
            this.label_Status.Location = new System.Drawing.Point(184, 350);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(0, 13);
            this.label_Status.TabIndex = 3;
            // 
            // buttonCheckUpdates
            // 
            this.buttonCheckUpdates.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.buttonCheckUpdates.Image = ((System.Drawing.Image)(resources.GetObject("buttonCheckUpdates.Image")));
            this.buttonCheckUpdates.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonCheckUpdates.Location = new System.Drawing.Point(404, 53);
            this.buttonCheckUpdates.Name = "buttonCheckUpdates";
            this.buttonCheckUpdates.Size = new System.Drawing.Size(177, 31);
            this.buttonCheckUpdates.TabIndex = 4;
            this.buttonCheckUpdates.Text = "Check for updates";
            this.buttonCheckUpdates.UseVisualStyleBackColor = true;
            this.buttonCheckUpdates.Click += new System.EventHandler(this.ButtonCheckUpdates_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(595, 409);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(170, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Powered by =GEMINI=Hawkmoon";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 11);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(146, 201);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // listViewDiffs
            // 
            this.listViewDiffs.CheckBoxes = true;
            this.listViewDiffs.FullRowSelect = true;
            this.listViewDiffs.HideSelection = false;
            this.listViewDiffs.Location = new System.Drawing.Point(184, 90);
            this.listViewDiffs.Name = "listViewDiffs";
            this.listViewDiffs.Size = new System.Drawing.Size(580, 257);
            this.listViewDiffs.TabIndex = 9;
            this.listViewDiffs.UseCompatibleStateImageBehavior = false;
            this.listViewDiffs.View = System.Windows.Forms.View.Details;
            this.listViewDiffs.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewDiffs_ItemChecked);
            // 
            // buttonExec
            // 
            this.buttonExec.Enabled = false;
            this.buttonExec.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.buttonExec.Image = ((System.Drawing.Image)(resources.GetObject("buttonExec.Image")));
            this.buttonExec.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonExec.Location = new System.Drawing.Point(587, 53);
            this.buttonExec.Name = "buttonExec";
            this.buttonExec.Size = new System.Drawing.Size(177, 31);
            this.buttonExec.TabIndex = 10;
            this.buttonExec.Text = "Download and Install";
            this.buttonExec.UseVisualStyleBackColor = true;
            this.buttonExec.Click += new System.EventHandler(this.buttonExec_Click);
            // 
            // labelPercentage
            // 
            this.labelPercentage.AutoSize = true;
            this.labelPercentage.Location = new System.Drawing.Point(740, 383);
            this.labelPercentage.Name = "labelPercentage";
            this.labelPercentage.Size = new System.Drawing.Size(0, 13);
            this.labelPercentage.TabIndex = 12;
            this.labelPercentage.Visible = false;
            // 
            // progressBarSkinDownload
            // 
            this.progressBarSkinDownload.Location = new System.Drawing.Point(184, 373);
            this.progressBarSkinDownload.Name = "progressBarSkinDownload";
            this.progressBarSkinDownload.Size = new System.Drawing.Size(550, 23);
            this.progressBarSkinDownload.Step = 1;
            this.progressBarSkinDownload.TabIndex = 11;
            this.progressBarSkinDownload.Visible = false;
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(184, 399);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(0, 13);
            this.labelProgress.TabIndex = 13;
            this.labelProgress.Visible = false;
            // 
            // FormSkinDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(777, 431);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.labelPercentage);
            this.Controls.Add(this.progressBarSkinDownload);
            this.Controls.Add(this.buttonExec);
            this.Controls.Add(this.listViewDiffs);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonCheckUpdates);
            this.Controls.Add(this.label_Status);
            this.Controls.Add(this.button_OpenIl2Folder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_Il2Path);
            this.ForeColor = System.Drawing.SystemColors.Window;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormSkinDownloader";
            this.Text = "Il2 Skin Downloader";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_Il2Path;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OpenIl2Folder;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.Button buttonCheckUpdates;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListView listViewDiffs;
        private System.Windows.Forms.Button buttonExec;
        private System.Windows.Forms.Label labelPercentage;
        private System.Windows.Forms.ProgressBar progressBarSkinDownload;
        private System.Windows.Forms.Label labelProgress;
    }
}

