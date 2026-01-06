using System;
using System.Drawing;
using System.Windows.Forms;

namespace NitroDock
{
    public partial class DockTexturizer : Form
    {
        private Bitmap resizedImage;
        private Bitmap seamlessImage;

        public DockTexturizer()
        {
            InitializeComponent();
        }

        private void DockTexturizer_OpacityPanel_Button_LoadANewTextureFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files|*.png;*.jpg;*.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                Bitmap originalImage = new Bitmap(open.FileName);

                // Resize to 74x74
                resizedImage = new Bitmap(74, 74);
                using (Graphics g = Graphics.FromImage(resizedImage))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(originalImage, 0, 0, 74, 74);
                }

                DockTexturizer_OpacityPanel_PictureBox_Resized.Image = resizedImage;
                DockTexturizer_OpacityPanel_PictureBox_Seamless.Image = null;
                seamlessImage = null;
            }
        }

        private void DockTexturizer_OpacityPanel_Button_ModifyTheImageEdges_Click(object sender, EventArgs e)
        {
            if (resizedImage == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            // Initialize seamlessImage if not already done
            seamlessImage = new Bitmap(74, 74);
            UpdateSeamlessImage(DockTexturizer_OpacityPanel_Trackbar_SeamlessBlend_Touchup.Value);
            DockTexturizer_OpacityPanel_PictureBox_Seamless.Image = seamlessImage;
        }

        private void DockTexturizer_OpacityPanel_Trackbar_SeamlessBlend_Touchup_Scroll(object sender, EventArgs e)
        {
            if (seamlessImage != null && resizedImage != null)
            {
                UpdateSeamlessImage(DockTexturizer_OpacityPanel_Trackbar_SeamlessBlend_Touchup.Value);
                DockTexturizer_OpacityPanel_PictureBox_Seamless.Refresh();
            }
        }

        private void UpdateSeamlessImage(int blendWidth)
        {
            if (resizedImage == null) return;

            for (int y = 0; y < 74; y++)
            {
                for (int x = 0; x < 74; x++)
                {
                    // Blend all four edges and corners
                    if (x < blendWidth || y < blendWidth || x >= 74 - blendWidth || y >= 74 - blendWidth)
                    {
                        // Calculate the corresponding pixel for blending
                        int blendX = (x < blendWidth) ? (74 - blendWidth + x) : ((x >= 74 - blendWidth) ? (x - (74 - 2 * blendWidth)) : x);
                        int blendY = (y < blendWidth) ? (74 - blendWidth + y) : ((y >= 74 - blendWidth) ? (y - (74 - 2 * blendWidth)) : y);

                        Color currentPixel = resizedImage.GetPixel(x, y);
                        Color blendPixel = resizedImage.GetPixel(blendX, blendY);

                        int r = (currentPixel.R + blendPixel.R) / 2;
                        int g = (currentPixel.G + blendPixel.G) / 2;
                        int b = (currentPixel.B + blendPixel.B) / 2;

                        seamlessImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                    // Copy the rest
                    else
                    {
                        seamlessImage.SetPixel(x, y, resizedImage.GetPixel(x, y));
                    }
                }
            }
        }


        private void DockTexturizer_OpacityPanel_Button_SaveNitroDockXTexture_Click(object sender, EventArgs e)
        {
            if (seamlessImage == null)
            {
                MessageBox.Show("No seamless image to save. Click 'Modify The Image Edges' first.");
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "PNG Files|*.png";
            if (save.ShowDialog() == DialogResult.OK)
            {
                seamlessImage.Save(save.FileName, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("Seamless image saved!");
            }
        }
    }
}
