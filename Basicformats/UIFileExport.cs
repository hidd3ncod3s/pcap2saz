using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace BasicFormats
{
	public class UIFileExport : Form
	{
		private IContainer components;
		private Button btnExport;
		private Button btnBrowse;
		internal TextBox txtLocation;
		internal CheckBox cbRecreateFolderStructure;
		private GroupBox gbOptions;
		private Label lblTo;
		internal CheckBox cbOpenFolder;
		internal CheckBox cbHTTP200Only;
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}
		private void InitializeComponent()
		{
			this.btnExport = new Button();
			this.btnBrowse = new Button();
			this.txtLocation = new TextBox();
			this.cbRecreateFolderStructure = new CheckBox();
			this.gbOptions = new GroupBox();
			this.cbHTTP200Only = new CheckBox();
			this.cbOpenFolder = new CheckBox();
			this.lblTo = new Label();
			this.gbOptions.SuspendLayout();
			base.SuspendLayout();
			this.btnExport.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.btnExport.DialogResult = DialogResult.OK;
			this.btnExport.Location = new Point(375, 90);
			this.btnExport.Name = "btnExport";
			this.btnExport.Size = new Size(77, 29);
			this.btnExport.TabIndex = 0;
			this.btnExport.Text = "&Export >>";
			this.btnExport.UseVisualStyleBackColor = true;
			this.btnBrowse.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.btnBrowse.Location = new Point(375, 7);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new Size(77, 29);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "&Browse...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
			this.txtLocation.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.txtLocation.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			this.txtLocation.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
			this.txtLocation.Location = new Point(58, 12);
			this.txtLocation.Name = "txtLocation";
			this.txtLocation.Size = new Size(311, 21);
			this.txtLocation.TabIndex = 1;
			this.cbRecreateFolderStructure.AutoSize = true;
			this.cbRecreateFolderStructure.Checked = true;
			this.cbRecreateFolderStructure.CheckState = CheckState.Checked;
			this.cbRecreateFolderStructure.Location = new Point(16, 20);
			this.cbRecreateFolderStructure.Name = "cbRecreateFolderStructure";
			this.cbRecreateFolderStructure.Size = new Size(148, 17);
			this.cbRecreateFolderStructure.TabIndex = 0;
			this.cbRecreateFolderStructure.Text = "&Recreate folder structure";
			this.cbRecreateFolderStructure.UseVisualStyleBackColor = true;
			this.gbOptions.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.gbOptions.Controls.Add(this.cbHTTP200Only);
			this.gbOptions.Controls.Add(this.cbOpenFolder);
			this.gbOptions.Controls.Add(this.cbRecreateFolderStructure);
			this.gbOptions.Location = new Point(12, 39);
			this.gbOptions.Name = "gbOptions";
			this.gbOptions.Size = new Size(199, 80);
			this.gbOptions.TabIndex = 3;
			this.gbOptions.TabStop = false;
			this.gbOptions.Text = "Options";
			this.cbHTTP200Only.AutoSize = true;
			this.cbHTTP200Only.Checked = true;
			this.cbHTTP200Only.CheckState = CheckState.Checked;
			this.cbHTTP200Only.Location = new Point(16, 53);
			this.cbHTTP200Only.Name = "cbHTTP200Only";
			this.cbHTTP200Only.Size = new Size(169, 17);
			this.cbHTTP200Only.TabIndex = 2;
			this.cbHTTP200Only.Text = "&Skip non-HTTP/200 responses";
			this.cbHTTP200Only.UseVisualStyleBackColor = true;
			this.cbOpenFolder.AutoSize = true;
			this.cbOpenFolder.Checked = true;
			this.cbOpenFolder.CheckState = CheckState.Checked;
			this.cbOpenFolder.Location = new Point(16, 36);
			this.cbOpenFolder.Name = "cbOpenFolder";
			this.cbOpenFolder.Size = new Size(158, 17);
			this.cbOpenFolder.TabIndex = 1;
			this.cbOpenFolder.Text = "&Open folder when complete";
			this.cbOpenFolder.UseVisualStyleBackColor = true;
			this.lblTo.AutoSize = true;
			this.lblTo.Location = new Point(15, 15);
			this.lblTo.Name = "lblTo";
			this.lblTo.Size = new Size(33, 13);
			this.lblTo.TabIndex = 1;
			this.lblTo.Text = "&Path:";
			base.AcceptButton = this.btnExport;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(464, 131);
			base.Controls.Add(this.lblTo);
			base.Controls.Add(this.gbOptions);
			base.Controls.Add(this.txtLocation);
			base.Controls.Add(this.btnBrowse);
			base.Controls.Add(this.btnExport);
			this.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			base.KeyPreview = true;
			this.MinimumSize = new Size(480, 170);
			base.Name = "UIFileExport";
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "File Exporter";
			base.KeyDown += new KeyEventHandler(this.UIFileExport_KeyDown);
			this.gbOptions.ResumeLayout(false);
			this.gbOptions.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
		public UIFileExport()
		{
			this.InitializeComponent();
		}
		private void btnBrowse_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.ShowNewFolderButton = true;
			folderBrowserDialog.Description = "Select the folder in which files should be placed:";
			DialogResult dialogResult = folderBrowserDialog.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				this.txtLocation.Text = folderBrowserDialog.SelectedPath;
			}
		}
		private void UIFileExport_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				e.SuppressKeyPress = (e.Handled = true);
				base.DialogResult = DialogResult.Cancel;
			}
		}
	}
}
