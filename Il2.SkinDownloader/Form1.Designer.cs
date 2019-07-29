namespace Il2SkinDownloader
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
            this.textBox_Il2Path = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OpenIl2Folder = new System.Windows.Forms.Button();
            this.label_Status = new System.Windows.Forms.Label();
            this.buttonCheckUpdates = new System.Windows.Forms.Button();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.progressBarSkinDownload = new System.Windows.Forms.ProgressBar();
            this.labelPercentage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_Il2Path
            // 
            this.textBox_Il2Path.Location = new System.Drawing.Point(12, 31);
            this.textBox_Il2Path.Name = "textBox_Il2Path";
            this.textBox_Il2Path.Size = new System.Drawing.Size(500, 20);
            this.textBox_Il2Path.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(294, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Il2 Installation Path (IL-2 Sturmovik Battle of Stalingrad folder)";
            // 
            // button_OpenIl2Folder
            // 
            this.button_OpenIl2Folder.Location = new System.Drawing.Point(513, 30);
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
            this.label_Status.Location = new System.Drawing.Point(12, 96);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(13, 13);
            this.label_Status.TabIndex = 3;
            this.label_Status.Text = "_";
            // 
            // buttonCheckUpdates
            // 
            this.buttonCheckUpdates.Location = new System.Drawing.Point(12, 55);
            this.buttonCheckUpdates.Name = "buttonCheckUpdates";
            this.buttonCheckUpdates.Size = new System.Drawing.Size(500, 23);
            this.buttonCheckUpdates.TabIndex = 4;
            this.buttonCheckUpdates.Text = "Check for updates";
            this.buttonCheckUpdates.UseVisualStyleBackColor = true;
            this.buttonCheckUpdates.Click += new System.EventHandler(this.ButtonCheckUpdates_Click);
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerCompleted);
            // 
            // progressBarSkinDownload
            // 
            this.progressBarSkinDownload.Location = new System.Drawing.Point(12, 112);
            this.progressBarSkinDownload.Name = "progressBarSkinDownload";
            this.progressBarSkinDownload.Size = new System.Drawing.Size(500, 23);
            this.progressBarSkinDownload.Step = 1;
            this.progressBarSkinDownload.TabIndex = 5;
            // 
            // labelPercentage
            // 
            this.labelPercentage.AutoSize = true;
            this.labelPercentage.Location = new System.Drawing.Point(516, 117);
            this.labelPercentage.Name = "labelPercentage";
            this.labelPercentage.Size = new System.Drawing.Size(0, 13);
            this.labelPercentage.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 145);
            this.Controls.Add(this.labelPercentage);
            this.Controls.Add(this.progressBarSkinDownload);
            this.Controls.Add(this.buttonCheckUpdates);
            this.Controls.Add(this.label_Status);
            this.Controls.Add(this.button_OpenIl2Folder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_Il2Path);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Il2 Skin Downloader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_Il2Path;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OpenIl2Folder;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.Button buttonCheckUpdates;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.Windows.Forms.ProgressBar progressBarSkinDownload;
        private System.Windows.Forms.Label labelPercentage;
    }
}

