using Microsoft.Win32;
using NitroDockX;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Windows.Forms;
using static NitroDock.NitroDockMain;

namespace NitroDock
{
    public partial class NitroDockMain_Configuration : Form
    {
        private NitroDockMain _mainForm;

        public NitroDockMain_Configuration(NitroDockMain mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;

            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockPositioning.Items.AddRange(Enum.GetNames(typeof(DockPosition)));
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_HighlightGlow.Items.AddRange(Enum.GetNames(typeof(GlowColor)));
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockCorners.Items.AddRange(new object[] { "Round Dock Corners", "Square Dock Corners" });
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_SkinDisplayMode.Items.AddRange(new object[] { "None", "Tile", "Center", "Stretch", "Zoom" });

            // Set trackbar limits for icon resizing
            NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_ResizeIcons.Minimum = 16;
            NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_ResizeIcons.Maximum = 64;
            NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_ResizeIcons.Value = 48;

            string skinsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NitroSkins");
            if (Directory.Exists(skinsPath))
            {
                foreach (string skinFolder in Directory.GetDirectories(skinsPath))
                {
                    string skinName = Path.GetFileName(skinFolder);
                    NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.Items.Add(skinName);
                }
            }

            // Populate monitor combobox
            PopulateMonitorComboBox();

            LoadSettings();

            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockPositioning.SelectedIndexChanged += (s, e) => ApplySelectedPosition();
            NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOpacity.ValueChanged += (s, e) => ApplyFormOpacity();
            NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Edge.ValueChanged += (s, e) => ApplyDockOffset();
            NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Alignment.ValueChanged += (s, e) => ApplyDockOffsetZ();
            NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_ResizeIcons.ValueChanged += (s, e) => ApplyIconSize();
            NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_IconSpacing.ValueChanged += (s, e) => ApplyIconSpacing();
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_HighlightGlow.SelectedIndexChanged += (s, e) => ApplyGlowEffect();
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockCorners.SelectedIndexChanged += (s, e) =>
            {
                string cornerStyle = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockCorners.SelectedItem.ToString();
                _mainForm.CurrentCornerStyle = (cornerStyle == "Round Dock Corners") ? CornerStyle.Round : CornerStyle.Square;
                _mainForm.UpdateRoundedRegion();
            };


            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_SkinDisplayMode.SelectedIndexChanged += (s, e) =>
            {
                string selectedSkin = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.SelectedItem?.ToString() ?? "Default";
                string selectedSkinMode = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_SkinDisplayMode.SelectedItem?.ToString() ?? "Stretch";
                _mainForm.ApplySkin(selectedSkin, selectedSkinMode);
            };
        }

        private void PopulateMonitorComboBox()
        {
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_AssignMonitor.Items.Clear();
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                string monitorName = $"Monitor {i + 1}" + (Screen.AllScreens[i].Primary ? " (Primary)" : "");
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_AssignMonitor.Items.Add(monitorName);
            }
        }

        public void SetSelectedSkin(string skinName)
        {
            if (NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.Items.Contains(skinName))
            {
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.SelectedItem = skinName;
            }
        }

        public void SetSkinDisplayMode(string mode)
        {
            if (NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_SkinDisplayMode.Items.Contains(mode))
            {
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_SkinDisplayMode.SelectedItem = mode;
            }
        }

        private void LoadSettings()
        {
            string iniPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NitroDockX.ini");
            IniFile ini = new IniFile(iniPath);

            if (Enum.TryParse(ini.Read("DockSettings", "DockPosition"), out DockPosition position))
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockPositioning.SelectedItem = position.ToString();

            if (int.TryParse(ini.Read("DockSettings", "DockOpacityValue"), out int opacityValue))
                NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOpacity.Value = opacityValue;

            if (int.TryParse(ini.Read("DockSettings", "DockOffset"), out int dockOffset))
            {
                int minValue = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Edge.Minimum;
                int maxValue = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Edge.Maximum;
                dockOffset = Math.Max(minValue, Math.Min(maxValue, dockOffset));
                NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Edge.Value = dockOffset;
            }

            if (int.TryParse(ini.Read("DockSettings", "DockOffsetZ"), out int dockOffsetZ))
            {
                int minValue = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Alignment.Minimum;
                int maxValue = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Alignment.Maximum;
                dockOffsetZ = Math.Max(minValue, Math.Min(maxValue, dockOffsetZ));
                NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Alignment.Value = dockOffsetZ;
            }

            if (int.TryParse(ini.Read("DockSettings", "IconSize"), out int iconSize))
                NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_ResizeIcons.Value = Math.Clamp(iconSize, 16, 64);
            else
                NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_ResizeIcons.Value = 48;

            if (int.TryParse(ini.Read("DockSettings", "IconSpacing"), out int iconSpacing))
                NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_IconSpacing.Value = iconSpacing;

            string glowColor = ini.Read("DockSettings", "GlowColor", "Cyan");
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_HighlightGlow.SelectedItem = glowColor;

            string cornerStyle = ini.Read("DockSettings", "DockCornerStyle", "Round Dock Corners");
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockCorners.SelectedItem = cornerStyle;
            _mainForm.CurrentCornerStyle = (cornerStyle == "Round Dock Corners") ? CornerStyle.Round : CornerStyle.Square;
            _mainForm.UpdateRoundedRegion();

            string savedSkin = ini.Read("DockSettings", "Skin", "Default");
            if (NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.Items.Contains(savedSkin))
            {
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.SelectedItem = savedSkin;
            }
            else if (NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.Items.Count > 0)
            {
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.SelectedIndex = 0;
            }

            string savedSkinMode = ini.Read("DockSettings", "SkinMode", "Stretch");
            NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_SkinDisplayMode.SelectedItem = savedSkinMode;

            var styleForm = Application.OpenForms.OfType<NitroDockMain_StyleProperties>().FirstOrDefault();
            if (styleForm != null && !string.IsNullOrEmpty(savedSkin))
            {
                string skinPreviewPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NitroSkins", savedSkin, "01.png");
                if (File.Exists(skinPreviewPath))
                {
                    styleForm.ShowSelectedSkin(savedSkin, skinPreviewPath);
                }
            }

            bool launchOnRestart = bool.TryParse(ini.Read("DockSettings", "LaunchOnRestart"), out bool result) && result;
            NitroDockMain_Configuration_OpacityPanel_GroupBox_CheckBox_LaunchOnRestart.Checked = launchOnRestart;

            // Load monitor assignment
            if (int.TryParse(ini.Read("DockSettings", "AssignedMonitor"), out int assignedMonitor))
            {
                if (assignedMonitor >= 1 && assignedMonitor <= Screen.AllScreens.Length)
                {
                    NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_AssignMonitor.SelectedIndex = assignedMonitor - 1;
                }
                else
                {
                    NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_AssignMonitor.SelectedIndex = 0;
                }
            }
            else
            {
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_AssignMonitor.SelectedIndex = 0;
            }
        }

        private void ApplySelectedPosition()
        {
            if (Enum.TryParse(
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockPositioning.SelectedItem?.ToString(),
                out DockPosition position
            ))
            {
                _mainForm.currentDockPosition = position;
                Screen screen = Screen.AllScreens[_mainForm.AssignedMonitorIndex];
                Rectangle workingArea = screen.WorkingArea;
                int maxZOffset = position == DockPosition.Left || position == DockPosition.Right
                    ? workingArea.Height - _mainForm.ClientSize.Height
                    : workingArea.Width - _mainForm.ClientSize.Width;

                NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Alignment.Maximum = maxZOffset;
                _mainForm.SnapToEdge(position);
                _mainForm.EnsureFormOnScreen();
            }
        }



        private void ApplyFormOpacity()
        {
            float opacity = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOpacity.Value / 100f;
            _mainForm.Opacity = opacity;
        }

        private void ApplyDockOffset()
        {
            int offset = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Edge.Value;
            _mainForm.DockOffset = offset;
            _mainForm.SnapToEdge(_mainForm.currentDockPosition);
            _mainForm.EnsureFormOnScreen();
        }

        private void ApplyDockOffsetZ()
        {
            int offsetZ = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Alignment.Value;
            _mainForm.DockOffsetZ = offsetZ;
            _mainForm.SnapToEdge(_mainForm.currentDockPosition);
            _mainForm.EnsureFormOnScreen();
        }

        private void ApplyIconSize()
        {
            int newIconSize = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_ResizeIcons.Value;
            _mainForm.UpdateAllIconSizes(newIconSize);
        }

        private void ApplyIconSpacing()
        {
            int newIconSpacing = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_IconSpacing.Value;
            _mainForm.UpdateAllIconSpacings(newIconSpacing);
        }

        private void ApplyGlowEffect()
        {
            string selectedGlowColor = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_HighlightGlow.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedGlowColor) && Enum.TryParse(selectedGlowColor, out GlowColor glowColor))
            {
                _mainForm.SelectedGlowColor = glowColor;
                _mainForm.ApplyGlowEffect();
            }
        }

        private void NitroDockMain_Configuration_OpacityPanel_Button_ApplyChanges_Click(object sender, EventArgs e)
        {
            string iniPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NitroDockX.ini");
            IniFile ini = new IniFile(iniPath);

            // Save dock position
            if (Enum.TryParse(
                NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockPositioning.SelectedItem?.ToString(),
                out DockPosition position
            ))
            {
                _mainForm.currentDockPosition = position;
                ini.Write("DockSettings", "DockPosition", position.ToString());
            }

            // Save dock opacity
            int dockOpacityValue = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOpacity.Value;
            float dockOpacity = dockOpacityValue / 100f;
            _mainForm.Opacity = dockOpacity;
            ini.Write("DockSettings", "DockOpacity", dockOpacity.ToString());
            ini.Write("DockSettings", "DockOpacityValue", dockOpacityValue.ToString());

            // Save dock offsets
            int dockOffset = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Edge.Value;
            _mainForm.DockOffset = dockOffset;
            ini.Write("DockSettings", "DockOffset", dockOffset.ToString());

            int dockOffsetZ = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_DockOffset_Alignment.Value;
            _mainForm.DockOffsetZ = dockOffsetZ;
            ini.Write("DockSettings", "DockOffsetZ", dockOffsetZ.ToString());

            // Snap dock to edge
            _mainForm.SnapToEdge(_mainForm.currentDockPosition);
            _mainForm.EnsureFormOnScreen();

            // Save location
            ini.Write("DockSettings", "DockLocationX", _mainForm.Location.X.ToString());
            ini.Write("DockSettings", "DockLocationY", _mainForm.Location.Y.ToString());

            // Save icon size and spacing
            int iconSize = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_ResizeIcons.Value;
            _mainForm.IconSize = Math.Clamp(iconSize, 16, 64);
            ini.Write("DockSettings", "IconSize", iconSize.ToString());

            int iconSpacing = NitroDockMain_Configuration_OpacityPanel_GroupBox_TrackBar_IconSpacing.Value;
            _mainForm.IconSpacing = iconSpacing;
            ini.Write("DockSettings", "IconSpacing", iconSpacing.ToString());

            // Save glow color
            string selectedGlowColor = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_HighlightGlow.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedGlowColor) && Enum.TryParse(selectedGlowColor, out GlowColor glowColor))
            {
                _mainForm.SelectedGlowColor = glowColor;
                ini.Write("DockSettings", "GlowColor", glowColor.ToString());
            }

            // Save corner style
            string cornerStyle = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockCorners.SelectedItem?.ToString() ?? "Round Dock Corners";
            _mainForm.CurrentCornerStyle = (cornerStyle == "Round Dock Corners") ? CornerStyle.Round : CornerStyle.Square;
            ini.Write("DockSettings", "DockCornerStyle", cornerStyle);
            _mainForm.UpdateRoundedRegion();

            // Save skin and skin mode
            string selectedSkin = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_DockSkin.SelectedItem?.ToString() ?? "Default";
            ini.Write("DockSettings", "Skin", selectedSkin);

            string selectedSkinMode = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_SkinDisplayMode.SelectedItem?.ToString() ?? "Stretch";
            ini.Write("DockSettings", "SkinMode", selectedSkinMode);

            // Save launch on restart
            bool launchOnRestart = NitroDockMain_Configuration_OpacityPanel_GroupBox_CheckBox_LaunchOnRestart.Checked;
            ini.Write("DockSettings", "LaunchOnRestart", launchOnRestart.ToString());
            SetStartup(launchOnRestart);

            // Save monitor assignment
            int assignedMonitor = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_AssignMonitor.SelectedIndex + 1;
            ini.Write("DockSettings", "AssignedMonitor", assignedMonitor.ToString());
            _mainForm.AssignedMonitorIndex = NitroDockMain_Configuration_OpacityPanel_GroupBox_ComboBox_AssignMonitor.SelectedIndex;
            _mainForm.EnsureFormOnScreen();

            // Save icons and custom icons
            int iconIndex = 0;
            foreach (IconContainer container in _mainForm.NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>())
            {
                if ((container.Controls[0] as Button).Tag.ToString() == "NitroDockMain_Configuration")
                {
                    // Save background color (existing)
                    ini.Write("Icons", "ConfigContainer_ContainerBackgroundColor", container.BackColor.ToArgb().ToString());

                    // Save the texture path if it exists (NEW)
                    if (container.BackgroundImage?.Tag is string configTexturePath)
                    {
                        ini.Write("Icons", "ConfigContainer_ContainerBackgroundTexture", configTexturePath);
                    }
                    else
                    {
                        ini.Write("Icons", "ConfigContainer_ContainerBackgroundTexture", "");
                    }
                }
                else
                {
                    string iconKey = $"Icon{iconIndex}";
                    ini.Write("Icons", $"{iconKey}_Path", (container.Controls[0] as Button).Tag.ToString());

                    if ((container.Controls[0] as Button).Image?.Tag is string customIconPath)
                    {
                        ini.Write("Icons", $"{iconKey}_CustomIcon", customIconPath);
                    }

                    ini.Write("Icons", $"{iconKey}_ContainerBackgroundColor", container.BackColor.ToArgb().ToString());

                    if (container.BackgroundImage?.Tag is string texturePath)
                        ini.Write("Icons", $"{iconKey}_ContainerBackgroundTexture", texturePath);

                    iconIndex++;
                }
            }

            // Apply skin
            _mainForm.ApplySkin(selectedSkin, selectedSkinMode);

            // Reapply custom icons after any changes
            foreach (IconContainer container in _mainForm.NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>())
            {
                var button = container.Controls[0] as Button;
                string customIconPath = button.Image?.Tag as string;

                if (!string.IsNullOrEmpty(customIconPath) && File.Exists(customIconPath))
                {
                    try
                    {
                        button.Image = _mainForm.ResizeImage(Image.FromFile(customIconPath), _mainForm.IconSize, _mainForm.IconSize);
                        button.Image.Tag = customIconPath;
                    }
                    catch (Exception)
                    {
                        // Silently handle the error
                    }
                }
            }

            SystemSounds.Beep.Play();
            MessageBox.Show("Settings saved to NitroDockX.ini!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void SetStartup(bool enabled)
        {
            string appPath = Application.ExecutablePath;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (enabled)
            {
                rk.SetValue("NitroDockX", appPath);
            }
            else
            {
                rk.DeleteValue("NitroDockX", false);
            }
        }
    }
}
