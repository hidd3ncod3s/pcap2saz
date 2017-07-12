using Fiddler;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
namespace BasicFormats
{
	public class AppCacheOptions : Form
	{
		private IContainer components;
		private Label lblBase;
		private Button btnSave;
		private Button btnCancel;
		public CheckBox cbNetworkFallback;
		public TextBox txtBase;
		private ColumnHeader colURL;
		private ColumnHeader colSize;
		private ColumnHeader colType;
		public ListView lvItems;
		private Label lblResources;
		private Label lblinstructions;
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
			ListViewGroup listViewGroup = new ListViewGroup("Markup", HorizontalAlignment.Left);
			ListViewGroup listViewGroup2 = new ListViewGroup("Images", HorizontalAlignment.Left);
			ListViewGroup listViewGroup3 = new ListViewGroup("CSS", HorizontalAlignment.Left);
			ListViewGroup listViewGroup4 = new ListViewGroup("Script", HorizontalAlignment.Left);
			ListViewGroup listViewGroup5 = new ListViewGroup("Other", HorizontalAlignment.Left);
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(AppCacheOptions));
			this.cbNetworkFallback = new CheckBox();
			this.txtBase = new TextBox();
			this.lblBase = new Label();
			this.btnSave = new Button();
			this.btnCancel = new Button();
			this.lvItems = new ListView();
			this.colURL = new ColumnHeader();
			this.colSize = new ColumnHeader();
			this.colType = new ColumnHeader();
			this.lblResources = new Label();
			this.lblinstructions = new Label();
			base.SuspendLayout();
			this.cbNetworkFallback.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.cbNetworkFallback.AutoSize = true;
			this.cbNetworkFallback.Checked = true;
			this.cbNetworkFallback.CheckState = CheckState.Checked;
			this.cbNetworkFallback.Location = new Point(12, 375);
			this.cbNetworkFallback.Name = "cbNetworkFallback";
			this.cbNetworkFallback.Size = new Size(175, 17);
			this.cbNetworkFallback.TabIndex = 3;
			this.cbNetworkFallback.Text = "&Allow Network for unlisted items";
			this.cbNetworkFallback.UseVisualStyleBackColor = true;
			this.txtBase.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.txtBase.Location = new Point(229, 392);
			this.txtBase.Name = "txtBase";
			this.txtBase.Size = new Size(508, 20);
			this.txtBase.TabIndex = 5;
			this.lblBase.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.lblBase.AutoSize = true;
			this.lblBase.Location = new Point(226, 376);
			this.lblBase.Name = "lblBase";
			this.lblBase.Size = new Size(59, 13);
			this.lblBase.TabIndex = 4;
			this.lblBase.Text = "&Base URL:";
			this.btnSave.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.btnSave.DialogResult = DialogResult.OK;
			this.btnSave.Location = new Point(755, 375);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new Size(75, 46);
			this.btnSave.TabIndex = 6;
			this.btnSave.Text = "&Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnCancel.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left);
			this.btnCancel.DialogResult = DialogResult.Cancel;
			this.btnCancel.Location = new Point(12, 398);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new Size(75, 23);
			this.btnCancel.TabIndex = 7;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.lvItems.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.lvItems.CheckBoxes = true;
			this.lvItems.Columns.AddRange(new ColumnHeader[]
			{
				this.colURL,
				this.colSize,
				this.colType
			});
			this.lvItems.FullRowSelect = true;
			listViewGroup.Header = "Markup";
			listViewGroup.Name = "lvgMarkup";
			listViewGroup2.Header = "Images";
			listViewGroup2.Name = "lvgImages";
			listViewGroup3.Header = "CSS";
			listViewGroup3.Name = "lvgCSS";
			listViewGroup4.Header = "Script";
			listViewGroup4.Name = "lvgScript";
			listViewGroup5.Header = "Other";
			listViewGroup5.Name = "lvgOther";
			this.lvItems.Groups.AddRange(new ListViewGroup[]
			{
				listViewGroup,
				listViewGroup2,
				listViewGroup3,
				listViewGroup4,
				listViewGroup5
			});
			this.lvItems.HeaderStyle = ColumnHeaderStyle.Nonclickable;
			this.lvItems.HideSelection = false;
			this.lvItems.LabelEdit = true;
			this.lvItems.Location = new Point(12, 62);
			this.lvItems.Name = "lvItems";
			this.lvItems.Size = new Size(817, 307);
			this.lvItems.Sorting = SortOrder.Ascending;
			this.lvItems.TabIndex = 2;
			this.lvItems.UseCompatibleStateImageBehavior = false;
			this.lvItems.View = View.Details;
			this.lvItems.KeyDown += new KeyEventHandler(this.lvItems_KeyDown);
			this.colURL.Text = "URL";
			this.colURL.Width = 450;
			this.colSize.Text = "Size";
			this.colSize.TextAlign = HorizontalAlignment.Right;
			this.colSize.Width = 100;
			this.colType.Text = "Type";
			this.colType.Width = 150;
			this.lblResources.AutoSize = true;
			this.lblResources.Location = new Point(12, 45);
			this.lblResources.Name = "lblResources";
			this.lblResources.Size = new Size(72, 13);
			this.lblResources.TabIndex = 1;
			this.lblResources.Text = "&Resource List";
			this.lblinstructions.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.lblinstructions.ForeColor = SystemColors.ControlDarkDark;
			this.lblinstructions.Location = new Point(12, 5);
			this.lblinstructions.Name = "lblinstructions";
			this.lblinstructions.Size = new Size(817, 41);
			this.lblinstructions.TabIndex = 0;
			this.lblinstructions.Text = componentResourceManager.GetString("lblinstructions.Text");
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new Size(842, 433);
			base.ControlBox = false;
			base.Controls.Add(this.txtBase);
			base.Controls.Add(this.lblinstructions);
			base.Controls.Add(this.lblResources);
			base.Controls.Add(this.lvItems);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnSave);
			base.Controls.Add(this.lblBase);
			base.Controls.Add(this.cbNetworkFallback);
			base.Name = "AppCacheOptions";
			base.ShowIcon = false;
			this.Text = "Adjust AppCache Manifest";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
		public AppCacheOptions()
		{
			this.InitializeComponent();
			Utilities.SetCueText(this.txtBase, "(Optional) Specify URL to use as a base, e.g. http://example.com");
		}
		private void lvItems_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				foreach (ListViewItem item in this.lvItems.SelectedItems)
				{
					this.lvItems.Items.Remove(item);
				}
			}
			if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ListViewItem listViewItem in this.lvItems.SelectedItems)
				{
					stringBuilder.AppendLine(listViewItem.Text);
				}
				Clipboard.SetText(stringBuilder.ToString());
			}
		}
	}
}
