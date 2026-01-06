using NitroDock;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace NitroDock
{
    public partial class NitroDockMain_StyleProperties : Form
    {
        private NitroDock.NitroDockMain _mainForm;
        private string _nitroSkinsPath;
        private PictureBox _selectedSkinPictureBox;
        private string _selectedSkinName = "Default";
        private string logPath;
        private string appPath;

        public NitroDockMain_StyleProperties(NitroDock.NitroDockMain mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
            appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _nitroSkinsPath = Path.Combine(appPath, "NitroSkins");
            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SourceDirectory.Text = _nitroSkinsPath;

            // Initialize logging
            logPath = Path.Combine(appPath, "NitroDockX_StyleProperties.log");
            File.AppendAllText(logPath, $"=== StyleProperties STARTUP ===\n");
            File.AppendAllText(logPath, $"Skins Path: {_nitroSkinsPath}\n");
            File.AppendAllText(logPath, $"Skins Exists: {Directory.Exists(_nitroSkinsPath)}\n");

            _selectedSkinPictureBox = new PictureBox
            {
                Size = new Size(256, 128),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Controls.Add(_selectedSkinPictureBox);
            CenterPictureBox();

            PopulateSkinsPreview();

            NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_Button_OpenNitroStylesDirectory.Click +=
                (s, e) => System.Diagnostics.Process.Start("explorer.exe", _nitroSkinsPath);

            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Resize +=
                (s, e) => CenterPictureBox();
        }

        private void CenterPictureBox()
        {
            if (_selectedSkinPictureBox != null && NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay != null)
            {
                _selectedSkinPictureBox.Location = new Point(
                    (NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Width - _selectedSkinPictureBox.Width) / 2,
                    (NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_SelectedStyleDisplay.Height - _selectedSkinPictureBox.Height) / 2
                );
            }
        }

        private void PopulateSkinsPreview()
        {
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.Controls.Clear();
            NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.AutoScroll = true;

            if (Directory.Exists(_nitroSkinsPath))
            {
                int x = 10, y = 10;
                int maxWidth = NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.ClientSize.Width - 20;

                foreach (string skinFolder in Directory.GetDirectories(_nitroSkinsPath))
                {
                    string skinName = Path.GetFileName(skinFolder);
                    string previewImagePath = Path.Combine(skinFolder, "01.png");

                    if (File.Exists(previewImagePath))
                    {
                        PictureBox pic = new PictureBox
                        {
                            Size = new Size(128, 64),
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Tag = skinName,
                            Cursor = Cursors.Hand,
                            Location = new Point(x, y)
                        };

                        try
                        {
                            pic.Image = Image.FromFile(previewImagePath);
                            pic.Click += (s, e) => ShowSelectedSkin(skinName, previewImagePath);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logPath, $"Error loading skin preview: {ex}\n");
                            pic.Image = CreateErrorPlaceholder();
                        }

                        NitroDockMain_SplitContainer_Panel1_StyleProperties_OpacityPanel_StylePreview.Controls.Add(pic);

                        x += 138;
                        if (x + 138 > maxWidth)
                        {
                            x = 10;
                            y += 74;
                        }
                    }
                }
            }
        }

        private Bitmap CreateErrorPlaceholder()
        {
            Bitmap placeholder = new Bitmap(128, 64);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                g.Clear(Color.LightGray);
                g.DrawString("Invalid", Font, Brushes.Red, new PointF(5, 5));
            }
            return placeholder;
        }

        public void ShowSelectedSkin(string skinName, string previewImagePath)
        {
            try
            {
                _selectedSkinName = skinName;
                _selectedSkinPictureBox.Image = Image.FromFile(previewImagePath);
                NitroDockMain_SplitContainer_Panel2_Properties_OpacityPanel_TextBox_SelectedStyle.Text = skinName;
                CenterPictureBox();

                if (_mainForm != null)
                {
                    // Apply skin visually
                    _mainForm.ApplySkin(skinName, "Stretch");

                    // Save to INI
                    string iniPath = Path.Combine(appPath, "NitroDockX.ini");
                    IniFile ini = new IniFile(iniPath);
                    ini.Write("DockSettings", "Skin", skinName);
                    ini.Write("DockSettings", "SkinMode", "Stretch");

                    // Sync with Configuration form if open
                    var configForm = Application.OpenForms.OfType<NitroDockMain_Configuration>().FirstOrDefault();
                    if (configForm != null)
                    {
                        configForm.SetSelectedSkin(skinName);
                        configForm.SetSkinDisplayMode("Stretch");
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"Error loading skin: {ex}\n");
                MessageBox.Show($"Error loading skin: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
