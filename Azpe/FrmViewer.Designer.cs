namespace Azpe
{
	partial class FrmViewer
	{
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmViewer));
			this.sfd = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// sfd
			// 
			this.sfd.RestoreDirectory = true;
			// 
			// FrmViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(284, 162);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(150, 150);
			this.Name = "FrmViewer";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Activated += new System.EventHandler(this.FrmViewer_Activated);
			this.Deactivate += new System.EventHandler(this.FrmViewer_Deactivate);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmViewer_FormClosing);
			this.ResumeLayout(false);
		}

		internal System.Windows.Forms.SaveFileDialog sfd;
	}
}

