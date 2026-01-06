using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace NitroDock
{
    public partial class NitroDockMain_IconProperties : Form
    {
        private Button targetButton;
        private string nitroIconsPath;
        private string originalPath;
        private PictureBox selectedIconPictureBox;

        public NitroDockMain_IconProperties(Button button)
        {
            InitializeComponent();
            targetButton = button;
            nitroIconsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NitroIcons");
            originalPath = targetButton.Tag?.ToString() ?? string.Empty;
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory.Text = nitroIconsPath;
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedIcon.Text =
                Path.GetFileName(targetButton.Tag?.ToString() ?? "None");

            // Initialize the PictureBox for the selected icon preview
            selectedIconPictureBox = new PictureBox
            {
                Size = new Size(128, 128),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_SelectedIconDisplay.Controls.Add(selectedIconPictureBox);
            CenterPictureBox();

            // Populate the icons preview panel
            PopulateIconPreview();

            // Open the NitroIcons directory
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroIconsDirectory.Click +=
                (s, e) => System.Diagnostics.Process.Start("explorer.exe", nitroIconsPath);

            // Handle panel resize to keep the PictureBox centered
            NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_SelectedIconDisplay.Resize +=
                (s, e) => CenterPictureBox();

            if (targetButton.Tag?.ToString() == "NitroDockMain_Configuration")
            {
                HideRemoveOption();
            }

            this.DialogResult = DialogResult.Cancel;
        }

        // Centers the PictureBox in the panel
        private void CenterPictureBox()
        {
            if (selectedIconPictureBox != null && NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_SelectedIconDisplay != null)
            {
                selectedIconPictureBox.Location = new Point(
                    (NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_SelectedIconDisplay.Width - selectedIconPictureBox.Width) / 2,
                    (NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_SelectedIconDisplay.Height - selectedIconPictureBox.Height) / 2
                );
            }
        }

        // Hides the "Remove Item" option for the config button
        public void HideRemoveOption()
        {
            if (targetButton?.ContextMenuStrip != null)
            {
                foreach (ToolStripItem item in targetButton.ContextMenuStrip.Items)
                {
                    if (item.Text == "Remove Item")
                    {
                        item.ForeColor = Color.Gray;
                        item.Enabled = false;
                        break;
                    }
                }
            }
        }

        // Populates the icons preview panel with thumbnails
        private void PopulateIconPreview()
        {
            NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_IconPreview.Controls.Clear();
            NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_IconPreview.AutoScroll = true;

            if (Directory.Exists(nitroIconsPath))
            {
                int x = 10, y = 10;
                int maxWidth = NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_IconPreview.ClientSize.Width - 20;

                foreach (string iconFile in Directory.GetFiles(nitroIconsPath, "*.png")
                    .Concat(Directory.GetFiles(nitroIconsPath, "*.ico")))
                {
                    PictureBox pic = new PictureBox
                    {
                        Size = new Size(64, 64),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Tag = iconFile,
                        Cursor = Cursors.Hand,
                        Location = new Point(x, y)
                    };

                    // When you double-click an icon, apply it to the button
                    pic.DoubleClick += (s, e) => SelectIcon(iconFile);

                    // When you click an icon, show it in the preview panel
                    pic.Click += (s, e) => ShowSelectedIcon(iconFile);

                    try
                    {
                        if (iconFile.EndsWith(".ico"))
                            pic.Image = Icon.ExtractAssociatedIcon(iconFile).ToBitmap();
                        else
                            pic.Image = Image.FromFile(iconFile);
                    }
                    catch
                    {
                        pic.Image = new Bitmap(64, 64, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        using (Graphics g = Graphics.FromImage(pic.Image))
                        {
                            g.Clear(Color.LightGray);
                            g.DrawString("Invalid", Font, Brushes.Red, new PointF(5, 5));
                        }
                    }

                    NitroDockMain_SplitContainer_Panel1_IconProperties_OpacityPanel_IconPreview.Controls.Add(pic);

                    x += 74;
                    if (x + 74 > maxWidth)
                    {
                        x = 10;
                        y += 74;
                    }
                }
            }
        }

        // Shows the selected icon in the preview panel and updates the textbox
        private void ShowSelectedIcon(string iconPath)
        {
            try
            {
                // Show the icon in the preview panel
                if (iconPath.EndsWith(".ico"))
                    selectedIconPictureBox.Image = Icon.ExtractAssociatedIcon(iconPath).ToBitmap();
                else
                    selectedIconPictureBox.Image = Image.FromFile(iconPath);

                // Show the icon name in the textbox
                NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedIcon.Text = Path.GetFileName(iconPath);

                // Ensure the PictureBox remains centered after updating the image
                CenterPictureBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icon: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Applies the selected icon to the button on the main dock
        private void SelectIcon(string iconPath)
        {
            try
            {
                targetButton.Image = ResizeImage(Image.FromFile(iconPath), targetButton.Width, targetButton.Height);
                targetButton.Image.Tag = iconPath;

                if (this.Owner is NitroDockMain mainForm)
                {
                    mainForm.SaveIconToIni(targetButton.Parent as IconContainer);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icon: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Resizes the image to fit the button
        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (System.Drawing.Imaging.ImageAttributes wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
