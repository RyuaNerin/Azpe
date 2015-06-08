namespace Azpe
{
	partial class FrmInstall
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmInstall));
			this.btnInstall = new System.Windows.Forms.Button();
			this.bgw = new System.ComponentModel.BackgroundWorker();
			this.lblVersion = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnInstall
			// 
			this.btnInstall.Location = new System.Drawing.Point(12, 13);
			this.btnInstall.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btnInstall.Name = "btnInstall";
			this.btnInstall.Size = new System.Drawing.Size(120, 34);
			this.btnInstall.TabIndex = 0;
			this.btnInstall.Text = "설치";
			this.btnInstall.UseVisualStyleBackColor = true;
			this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
			// 
			// bgw
			// 
			this.bgw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgw_DoWork);
			this.bgw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgw_RunWorkerCompleted);
			// 
			// lblVersion
			// 
			this.lblVersion.Location = new System.Drawing.Point(12, 51);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(120, 16);
			this.lblVersion.TabIndex = 1;
			this.lblVersion.Text = "v1.1.0";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// FrmInstall
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(144, 76);
			this.Controls.Add(this.lblVersion);
			this.Controls.Add(this.btnInstall);
			this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmInstall";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Azpe 설치";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnInstall;
		private System.ComponentModel.BackgroundWorker bgw;
		private System.Windows.Forms.Label lblVersion;
	}
}