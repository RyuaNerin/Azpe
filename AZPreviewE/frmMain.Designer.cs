namespace AZPreviewE
{
	partial class frmMain
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다.
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.elem = new System.Windows.Forms.Integration.ElementHost();
			this.media = new AZPreviewE.MediaElement();
			this.image = new AZPreviewE.PictureBoxE();
			this.sfd = new System.Windows.Forms.SaveFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.image)).BeginInit();
			this.SuspendLayout();
			// 
			// elem
			// 
			this.elem.Location = new System.Drawing.Point(12, 22);
			this.elem.Name = "elem";
			this.elem.Size = new System.Drawing.Size(87, 73);
			this.elem.TabIndex = 1;
			this.elem.Child = this.media;
			// 
			// image
			// 
			this.image.Location = new System.Drawing.Point(105, 22);
			this.image.Name = "image";
			this.image.Size = new System.Drawing.Size(167, 128);
			this.image.TabIndex = 0;
			this.image.TabStop = false;
			// 
			// sfd
			// 
			this.sfd.RestoreDirectory = true;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 162);
			this.Controls.Add(this.elem);
			this.Controls.Add(this.image);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(150, 150);
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "[ 0 / 0 ] AZPreview-E";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
			this.Shown += new System.EventHandler(this.frmMain_Shown);
			this.ResizeEnd += new System.EventHandler(this.frmMain_ResizeEnd);
			((System.ComponentModel.ISupportInitialize)(this.image)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private PictureBoxE image;
		private System.Windows.Forms.Integration.ElementHost elem;
		private MediaElement media;
		internal System.Windows.Forms.SaveFileDialog sfd;
	}
}

