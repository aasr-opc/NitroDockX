using NitroDockX;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NitroDock
{
    [ToolboxItem(true)]
    public class OpacityPanel : Panel
    {
        private float _opacity = 0.50f;

        public OpacityPanel()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Selectable,
                true);
            BackColor = Color.Transparent;
        }

        [Category("Appearance")]
        [Description("Sets the opacity of the panel (0.0 to 1.0)")]
        [DefaultValue(0.35f)]
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                if (value < 0.0f) _opacity = 0.0f;
                else if (value > 1.0f) _opacity = 1.0f;
                else _opacity = value;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (_opacity > 0.0f)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb((int)(_opacity * 255), Color.Black)))
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }
    }

    [ToolboxItem(true)]
    public class IconContainer : Panel
    {
        private Button _iconButton;
        private ContextMenuStrip _containerContextMenu;

        public IconContainer(string path, int iconSize)
        {
            this.Size = new Size(64, 64);
            this.BackColor = Color.Transparent;
            this.BorderStyle = BorderStyle.None;

            _iconButton = new Button
            {
                Size = new Size(iconSize, iconSize),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Tag = path,
                ImageAlign = ContentAlignment.MiddleCenter
            };

            _iconButton.Location = new Point(
                (this.Width - _iconButton.Width) / 2,
                (this.Height - _iconButton.Height) / 2
            );

            try
            {
                if (path == "NitroDockMain_Configuration")
                {
                    string configIconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NitroIcons", "Config.png");
                    if (File.Exists(configIconPath))
                    {
                        using (Bitmap bitmap = new Bitmap(configIconPath))
                        {
                            _iconButton.Image = ResizeImage(bitmap, iconSize, iconSize);
                        }
                    }
                    else
                    {
                        _iconButton.Image = ResizeImage(SystemIcons.Shield.ToBitmap(), iconSize, iconSize);
                    }
                }
                else
                {
                    Icon icon = GetIconForPath(path);
                    if (icon != null)
                    {
                        _iconButton.Image = ResizeImage(icon.ToBitmap(), iconSize, iconSize);
                    }
                    else
                    {
                        _iconButton.Image = ResizeImage(SystemIcons.Application.ToBitmap(), iconSize, iconSize);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading icon: {ex.Message}");
                _iconButton.Image = ResizeImage(SystemIcons.Application.ToBitmap(), iconSize, iconSize);
            }

            this.Controls.Add(_iconButton);
            InitializeContainerContextMenu();

            _iconButton.MouseDown += (s, ev) =>
            {
                if (ev.Button == MouseButtons.Left)
                {
                    string fullPath = _iconButton.Tag.ToString();
                    if (fullPath != "NitroDockMain_Configuration" && File.Exists(fullPath))
                    {
                        Process.Start(fullPath);
                    }
                    else if (fullPath != "NitroDockMain_Configuration" && Directory.Exists(fullPath))
                    {
                        Process.Start("explorer.exe", fullPath);
                    }
                }
            };

            _iconButton.MouseEnter += (sender, e) =>
            {
                Color glowColor = Color.Cyan;
                _iconButton.BackColor = Color.FromArgb(50, glowColor);
            };
            _iconButton.MouseLeave += (sender, e) =>
            {
                _iconButton.BackColor = Color.Transparent;
            };
        }

        private void InitializeContainerContextMenu()
        {
            _containerContextMenu = new ContextMenuStrip();

            ToolStripMenuItem addBackgroundTextureItem = new ToolStripMenuItem("Add Background Texture");
            addBackgroundTextureItem.Click += (s, e) => AddBackgroundTexture();
            _containerContextMenu.Items.Add(addBackgroundTextureItem);

            ToolStripMenuItem changeBackgroundColorItem = new ToolStripMenuItem("Change Background Color");
            changeBackgroundColorItem.Click += (s, e) => ChangeBackgroundColor();
            _containerContextMenu.Items.Add(changeBackgroundColorItem);

            ToolStripMenuItem makeTransparentItem = new ToolStripMenuItem("Make Transparent");
            makeTransparentItem.Click += (s, e) => MakeTransparent();
            _containerContextMenu.Items.Add(makeTransparentItem);

            this.ContextMenuStrip = _containerContextMenu;
        }

        private void AddBackgroundTexture()
        {
            NitroDockMain_IconContainerTextures texturesForm = new NitroDockMain_IconContainerTextures(this);
            texturesForm.ShowDialog();
        }

        private void ChangeBackgroundColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                this.BackColor = colorDialog.Color;
            }
        }

        private void MakeTransparent()
        {
            this.BackgroundImage = null;
            this.BackColor = Color.Transparent;

            // Save the transparency state to the INI file
            string iniPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NitroDockX.ini");
            IniFile ini = new IniFile(iniPath);

            // Find the index of this IconContainer in the parent's controls
            int index = -1;
            if (this.Parent != null)
            {
                for (int i = 0; i < this.Parent.Controls.Count; i++)
                {
                    if (this.Parent.Controls[i] == this)
                    {
                        index = i;
                        break;
                    }
                }
            }

            // If this is not the ConfigContainer, save the transparency state
            if (index >= 0 && (_iconButton.Tag?.ToString() != "NitroDockMain_Configuration"))
            {
                // Calculate the actual icon index (excluding the ConfigContainer)
                int iconIndex = 0;
                for (int i = 0; i < this.Parent.Controls.Count; i++)
                {
                    if (this.Parent.Controls[i] is IconContainer container &&
                        container != this &&
                        (container.Controls[0] as Button)?.Tag?.ToString() != "NitroDockMain_Configuration")
                    {
                        iconIndex++;
                    }
                    else if (this.Parent.Controls[i] == this)
                    {
                        break;
                    }
                }

                // Save the transparency state
                ini.Write("Icons", $"Icon{iconIndex}_ContainerBackgroundColor", Color.Transparent.ToArgb().ToString());
                ini.Write("Icons", $"Icon{iconIndex}_ContainerBackgroundTexture", "");
            }
        }


        public void UpdateIconSize(int newSize)
        {
            _iconButton.Size = new Size(newSize, newSize);
            _iconButton.Location = new Point(
                (this.Width - _iconButton.Width) / 2,
                (this.Height - _iconButton.Height) / 2
            );

            string customIconPath = _iconButton.Image?.Tag as string;

            try
            {
                if (!string.IsNullOrEmpty(customIconPath) && File.Exists(customIconPath))
                {
                    // Reload and resize the custom icon
                    using (Image customImage = Image.FromFile(customIconPath))
                    {
                        _iconButton.Image = ResizeImage(customImage, newSize, newSize);
                        _iconButton.Image.Tag = customIconPath;
                    }
                }
                else
                {
                    string path = _iconButton.Tag?.ToString() ?? string.Empty;
                    Icon icon = null;

                    if (path == "NitroDockMain_Configuration")
                    {
                        string configIconPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NitroIcons", "Config.png");
                        if (File.Exists(configIconPath))
                        {
                            using (Image configImage = Image.FromFile(configIconPath))
                            {
                                _iconButton.Image = ResizeImage(configImage, newSize, newSize);
                                _iconButton.Image.Tag = configIconPath;
                            }
                        }
                        else
                        {
                            _iconButton.Image = ResizeImage(SystemIcons.Shield.ToBitmap(), newSize, newSize);
                        }
                    }
                    else if (File.Exists(path))
                    {
                        icon = Icon.ExtractAssociatedIcon(path);
                        _iconButton.Image = ResizeImage(icon.ToBitmap(), newSize, newSize);
                    }
                    else if (Directory.Exists(path))
                    {
                        try
                        {
                            SHFILEINFO shinfo = new SHFILEINFO();
                            uint attributes = (uint)FileAttributes.Directory;
                            uint result = SHGetFileInfo(
                                path,
                                attributes,
                                out shinfo,
                                (uint)Marshal.SizeOf(shinfo),
                                0x000000100 | 0x000000000 | 0x000000010);
                            if (shinfo.hIcon != IntPtr.Zero)
                            {
                                using (Icon dirIcon = Icon.FromHandle(shinfo.hIcon))
                                {
                                    _iconButton.Image = ResizeImage(dirIcon.ToBitmap(), newSize, newSize);
                                }
                            }
                            else
                            {
                                _iconButton.Image = ResizeImage(SystemIcons.Application.ToBitmap(), newSize, newSize);
                            }
                        }
                        catch
                        {
                            _iconButton.Image = ResizeImage(SystemIcons.Application.ToBitmap(), newSize, newSize);
                        }
                    }
                    else
                    {
                        _iconButton.Image = ResizeImage(SystemIcons.Application.ToBitmap(), newSize, newSize);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resizing icon: {ex.Message}");
                _iconButton.Image = ResizeImage(SystemIcons.Application.ToBitmap(), newSize, newSize);
            }
        }


        private Icon GetIconForPath(string path)
        {
            if (File.Exists(path))
            {
                return Icon.ExtractAssociatedIcon(path);
            }
            else if (Directory.Exists(path))
            {
                return GetFolderIcon(path);
            }
            else if (path.Length == 2 && path.EndsWith(":"))
            {
                return GetDriveIcon(path);
            }
            return SystemIcons.Application;
        }

        private Icon GetFolderIcon(string folderPath)
        {
            try
            {
                SHFILEINFO shinfo = new SHFILEINFO();
                uint attributes = (uint)FileAttributes.Directory;
                uint result = SHGetFileInfo(
                    folderPath,
                    attributes,
                    out shinfo,
                    (uint)Marshal.SizeOf(shinfo),
                    0x000000100 | 0x000000000 | 0x000000010);
                if (shinfo.hIcon != IntPtr.Zero)
                {
                    return Icon.FromHandle(shinfo.hIcon);
                }
            }
            catch { }
            try
            {
                IntPtr[] largeIconPtr = new IntPtr[1];
                int result = ExtractIconEx("shell32.dll", 3, largeIconPtr, null, 1);
                if (result > 0 && largeIconPtr[0] != IntPtr.Zero)
                {
                    return Icon.FromHandle(largeIconPtr[0]);
                }
            }
            catch { }
            return CreateFolderIcon();
        }

        private Icon GetDriveIcon(string drivePath)
        {
            try
            {
                DriveInfo drive = new DriveInfo(drivePath);
                if (drive.DriveType == DriveType.Removable || drive.DriveType == DriveType.Fixed)
                {
                    IntPtr[] largeIconPtr = new IntPtr[1];
                    int result = ExtractIconEx("shell32.dll", 8, largeIconPtr, null, 1);
                    if (result > 0 && largeIconPtr[0] != IntPtr.Zero)
                    {
                        return Icon.FromHandle(largeIconPtr[0]);
                    }
                }
            }
            catch { }
            return SystemIcons.WinLogo;
        }

        private Icon CreateFolderIcon()
        {
            Bitmap folderBitmap = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(folderBitmap))
            {
                g.Clear(Color.Transparent);
                g.FillRectangle(Brushes.Goldenrod, 4, 8, 24, 16);
                g.FillPolygon(Brushes.Goldenrod, new Point[] {
                    new Point(4, 8),
                    new Point(12, 4),
                    new Point(28, 4),
                    new Point(28, 8)
                });
                g.DrawRectangle(Pens.Black, 4, 8, 23, 16);
                g.DrawLine(Pens.White, 4, 8, 28, 8);
                g.DrawLine(Pens.White, 4, 8, 12, 4);
                g.DrawLine(Pens.DarkGray, 28, 8, 28, 24);
                g.DrawLine(Pens.DarkGray, 4, 24, 28, 24);
            }
            return Icon.FromHandle(folderBitmap.GetHicon());
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
    }
}