using NitroDock;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace NitroDock
{
    public partial class NitroDockMain_IconContainerTextures : Form
    {
        private string nitroTexturesPath;
        private PictureBox selectedTexturePictureBox;
        private IconContainer targetContainer;

        public NitroDockMain_IconContainerTextures(IconContainer container = null)
        {
            InitializeComponent();
            targetContainer = container;
            nitroTexturesPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "NitroIcons", "Textures"
            );

            // Initialize the PictureBox for the selected texture preview
            selectedTexturePictureBox = new PictureBox
            {
                Size = new Size(128, 128),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_SelectedTextureDisplay.Controls.Add(selectedTexturePictureBox);
            CenterPictureBox();

            // Populate the textures preview panel
            PopulateTexturesPreview();

            // Open the NitroIcons/Textures directory
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenIconContainerTextureDirectory.Click +=
                (s, e) => System.Diagnostics.Process.Start("explorer.exe", nitroTexturesPath);

            // Handle panel resize to keep the PictureBox centered
            NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_SelectedTextureDisplay.Resize +=
                (s, e) => CenterPictureBox();
        }

        // Centers the PictureBox in the panel
        private void CenterPictureBox()
        {
            if (selectedTexturePictureBox != null && NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_SelectedTextureDisplay != null)
            {
                selectedTexturePictureBox.Location = new Point(
                    (NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_SelectedTextureDisplay.Width - selectedTexturePictureBox.Width) / 2,
                    (NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_SelectedTextureDisplay.Height - selectedTexturePictureBox.Height) / 2
                );
            }
        }

        // Populates the textures preview panel with thumbnails
        private void PopulateTexturesPreview()
        {
            NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_TexturePreview.Controls.Clear();
            NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_TexturePreview.AutoScroll = true;

            if (Directory.Exists(nitroTexturesPath))
            {
                int x = 10, y = 10;
                int maxWidth = NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_TexturePreview.ClientSize.Width - 20;

                // Recursively get all image files from Textures and its subfolders
                foreach (string textureFile in Directory.GetFiles(nitroTexturesPath, "*.*", SearchOption.AllDirectories))
                {
                    string extension = Path.GetExtension(textureFile).ToLower();
                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp")
                    {
                        PictureBox pic = new PictureBox
                        {
                            Size = new Size(64, 64),
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Tag = textureFile,
                            Cursor = Cursors.Hand,
                            Location = new Point(x, y)
                        };

                        // When you double-click a texture, apply it to the container and close the form
                        pic.DoubleClick += (s, e) => ApplySelectedTexture(textureFile);

                        // When you click a texture, show it in the preview panel
                        pic.Click += (s, e) => ShowSelectedTexture(textureFile);

                        try
                        {
                            pic.Image = Image.FromFile(textureFile);
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

                        NitroDockMain_SplitContainer_Panel1_TextureProperties_OpacityPanel_TexturePreview.Controls.Add(pic);

                        x += 74;
                        if (x + 74 > maxWidth)
                        {
                            x = 10;
                            y += 74;
                        }
                    }
                }
            }
        }

        // Shows the selected texture in the preview panel and updates the textboxes
        private void ShowSelectedTexture(string texturePath)
        {
            try
            {
                // Show the texture in the preview panel
                selectedTexturePictureBox.Image = Image.FromFile(texturePath);

                // Show the texture name and full path in the textboxes
                NitroDockMain_SplitContainer_Panel2_TextureProperties_OpacityPanel_TextBox_SelectedTexture.Text = Path.GetFileName(texturePath);
                NitroDockMain_SplitContainer_Panel2_TextureProperties_OpacityPanel_TextBox_SourceDirectory.Text = texturePath;

                // Ensure the PictureBox remains centered after updating the image
                CenterPictureBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading texture: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Applies the selected texture to the target container and closes the form
        private void ApplySelectedTexture(string texturePath)
        {
            try
            {
                // Apply the selected texture to the target container
                if (targetContainer != null)
                {
                    targetContainer.BackgroundImage = Image.FromFile(texturePath);
                    targetContainer.BackgroundImage.Tag = texturePath;
                    targetContainer.BackgroundImageLayout = ImageLayout.Stretch;
                    // Notify main form to save
                    (this.Owner as NitroDockMain)?.SaveIconToIni(targetContainer);
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying texture: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenIconContainerTextureDirectory_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", nitroTexturesPath);
        }
    }
}
