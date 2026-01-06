using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace NitroDock
{
    partial class NitroDockMain_StyleProperties
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NitroDockMain_StyleProperties));
            NitroDockMain_SplitContainer = new SplitContainer();
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay = new NitroDock.OpacityPanel();
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview = new NitroDock.OpacityPanel();
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image = new NitroDock.OpacityPanel();
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information = new NitroDock.OpacityPanel();
            labelSourceDirectory = new Label();
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory = new Button();
            labelSelectedTexture = new Label();
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory = new TextBox();
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle = new TextBox();
            ((System.ComponentModel.ISupportInitialize)NitroDockMain_SplitContainer).BeginInit();
            NitroDockMain_SplitContainer.Panel1.SuspendLayout();
            NitroDockMain_SplitContainer.Panel2.SuspendLayout();
            NitroDockMain_SplitContainer.SuspendLayout();
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.SuspendLayout();
            SuspendLayout();
            // 
            // NitroDockMain_SplitContainer
            // 
            NitroDockMain_SplitContainer.Dock = DockStyle.Fill;
            NitroDockMain_SplitContainer.Location = new Point(0, 0);
            NitroDockMain_SplitContainer.Name = "NitroDockMain_SplitContainer";
            // 
            // NitroDockMain_SplitContainer.Panel1
            // 
            NitroDockMain_SplitContainer.Panel1.Controls.Add(NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay);
            NitroDockMain_SplitContainer.Panel1.Controls.Add(NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview);
            // 
            // NitroDockMain_SplitContainer.Panel2
            // 
            NitroDockMain_SplitContainer.Panel2.Controls.Add(NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image);
            NitroDockMain_SplitContainer.Panel2.Controls.Add(NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information);
            NitroDockMain_SplitContainer.Size = new Size(676, 474);
            NitroDockMain_SplitContainer.SplitterDistance = 339;
            NitroDockMain_SplitContainer.TabIndex = 0;
            // 
            // NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay
            // 
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.BackColor = Color.Transparent;
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Dock = DockStyle.Bottom;
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Location = new Point(0, 253);
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Name = "NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay";
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Opacity = 0.8F;
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Size = new Size(339, 221);
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.TabIndex = 1;
            // 
            // NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview
            // 
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.BackColor = Color.Transparent;
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.Dock = DockStyle.Top;
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.Location = new Point(0, 0);
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.Name = "NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview";
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.Opacity = 0.8F;
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.Size = new Size(339, 247);
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.TabIndex = 0;
            // 
            // NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image
            // 
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.BackColor = Color.Transparent;
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.BackgroundImage = NitroDockX.Properties.Resources.bckd_002;
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.BackgroundImageLayout = ImageLayout.Stretch;
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.Dock = DockStyle.Bottom;
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.Location = new Point(0, 253);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.Name = "NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image";
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.Opacity = 0.1F;
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.Size = new Size(333, 221);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image.TabIndex = 1;
            // 
            // NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information
            // 
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.BackColor = Color.Transparent;
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Controls.Add(labelSourceDirectory);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Controls.Add(NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Controls.Add(labelSelectedTexture);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Controls.Add(NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Controls.Add(NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Dock = DockStyle.Top;
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Location = new Point(0, 0);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Name = "NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information";
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Opacity = 0.8F;
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.Size = new Size(333, 247);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.TabIndex = 0;
            // 
            // labelSourceDirectory
            // 
            labelSourceDirectory.AutoSize = true;
            labelSourceDirectory.ForeColor = Color.LightSlateGray;
            labelSourceDirectory.Location = new Point(45, 74);
            labelSourceDirectory.Name = "labelSourceDirectory";
            labelSourceDirectory.Size = new Size(97, 15);
            labelSourceDirectory.TabIndex = 8;
            labelSourceDirectory.Text = "Source Directory:";
            // 
            // NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory
            // 
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory.Location = new Point(45, 140);
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory.Name = "NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory";
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory.Size = new Size(262, 39);
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory.TabIndex = 2;
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory.Text = "Open NitroSkins";
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory.UseVisualStyleBackColor = true;
            // 
            // labelSelectedTexture
            // 
            labelSelectedTexture.AutoSize = true;
            labelSelectedTexture.ForeColor = Color.LightSlateGray;
            labelSelectedTexture.Location = new Point(45, 26);
            labelSelectedTexture.Name = "labelSelectedTexture";
            labelSelectedTexture.Size = new Size(95, 15);
            labelSelectedTexture.TabIndex = 7;
            labelSelectedTexture.Text = "Selected Texture:";
            // 
            // NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory
            // 
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory.Location = new Point(45, 93);
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory.Name = "NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory";
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory.Size = new Size(262, 23);
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory.TabIndex = 1;
            // 
            // NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle
            // 
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle.Location = new Point(45, 44);
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle.Name = "NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle";
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle.Size = new Size(262, 23);
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle.TabIndex = 0;
            // 
            // NitroDockMain_StyleProperties
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(676, 474);
            Controls.Add(NitroDockMain_SplitContainer);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "NitroDockMain_StyleProperties";
            Opacity = 0.85D;
            StartPosition = FormStartPosition.CenterParent;
            Text = "NitroDockX: Style Properties";
            NitroDockMain_SplitContainer.Panel1.ResumeLayout(false);
            NitroDockMain_SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)NitroDockMain_SplitContainer).EndInit();
            NitroDockMain_SplitContainer.ResumeLayout(false);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.ResumeLayout(false);
            NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer NitroDockMain_SplitContainer;
        private NitroDock.OpacityPanel NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview;
        private NitroDock.OpacityPanel NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay;
        private NitroDock.OpacityPanel NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Information;
        private NitroDock.OpacityPanel NitroDockMain_SplitContainer_Panel2__StyleProperties_OpacityPanel_Image;
        private TextBox NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle;
        private TextBox NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory;
        private Button NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory;
        private Label labelSourceDirectory;
        private Label labelSelectedTexture;
    }
}
