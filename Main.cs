using NitroDockX;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NitroDock
{
    public partial class NitroDockMain : Form
    {
        #region Fields
        private bool isDragging = false;
        private Point lastCursor;
        private Point lastForm;
        private const int minDockWidth = 64 + 10;
        private const int cornerRadius = 24;
        private const int maxIconSpacing = 50;
        private const int containerSize = 64;
        private const int dockWidth = 74;
        private const int dockHeight = 74;
        private Button selectedButton = null;
        private bool isIconSelected = false;
        private MouseHook mouseHook;
        private string logPath;
        private string appPath;
        private IntPtr _originalWndProc;
        private bool _subclassed;
        private int _assignedMonitorIndex = 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AssignedMonitorIndex
        {
            get { return _assignedMonitorIndex; }
            set
            {
                _assignedMonitorIndex = Math.Clamp(value, 0, Screen.AllScreens.Length - 1);
                EnsureFormOnScreen();
            }
        }

        [DefaultValue(45)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int IconSize { get; set; } = 45;

        [DefaultValue(13)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int IconSpacing { get; set; } = 13;

        public enum DockPosition { Left, Right, Top, Bottom }
        public DockPosition currentDockPosition = DockPosition.Right;

        [DefaultValue(0)]
        public int DockOffset { get; set; } = 0;

        [DefaultValue(0)]
        public int DockOffsetZ { get; set; } = 0;

        public enum GlowColor { Blue, Cyan, Pink, Red, SlateGray, White, Yellow }

        [DefaultValue(GlowColor.Cyan)]
        public GlowColor SelectedGlowColor { get; set; } = GlowColor.Cyan;

        public enum CornerStyle { Round, Square }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(CornerStyle.Round)]
        public CornerStyle CurrentCornerStyle { get; set; } = CornerStyle.Round;

        private const int roundCornerRadius = 24;
        private const int squareCornerRadius = 0;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetLongPathName(string lpszShortPath, StringBuilder lpszLongPath, uint cchBuffer);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

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

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x00000000;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        private const int GWL_WNDPROC = -4;
        private const int WM_WINDOWPOSCHANGING = 0x0046;
        private const int WM_MOVE = 0x0003;
        private const int WM_SIZE = 0x0005;
        private const int WM_ACTIVATE = 0x0006;
        private const int WM_SHOWWINDOW = 0x0018;
        private const int WM_WINDOWPOSCHANGED = 0x0047;

        private static readonly object _logLock = new object();
        #endregion

        public NitroDockMain()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            // Initialize paths and logging
            appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            logPath = Path.Combine(appPath, "NitroDockX_Debug.log");
            Log("=== STARTUP ===");
            Log($"Working Directory: {Environment.CurrentDirectory}");
            Log($"Executable Path: {Assembly.GetExecutingAssembly().Location}");

            string skinsPath = Path.Combine(appPath, "NitroSkins");
            string iconsPath = Path.Combine(appPath, "NitroIcons");
            string iniPath = Path.Combine(appPath, "NitroDockX.ini");

            Log($"Skins Path: {skinsPath}");
            Log($"Icons Path: {iconsPath}");
            Log($"INI Path: {iniPath}");
            Log($"Skins Exists: {Directory.Exists(skinsPath)}");
            Log($"Icons Exists: {Directory.Exists(iconsPath)}");
            Log($"INI Exists: {File.Exists(iniPath)}");

            mouseHook = MouseHook.Instance;
            mouseHook.MouseMiddleButtonDown += OnMouseMiddleButtonDown;
            mouseHook.MouseWheel += OnMouseWheel;

            NitroDockMain_OpacityPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    lastCursor = Cursor.Position;
                    lastForm = Location;
                }
            };

            NitroDockMain_OpacityPanel.MouseMove += (s, e) =>
            {
                if (isDragging)
                {
                    this.Location = new Point(
                        lastForm.X + (Cursor.Position.X - lastCursor.X),
                        lastForm.Y + (Cursor.Position.Y - lastForm.Y)
                    );
                }
            };

            NitroDockMain_OpacityPanel.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = false;
                    SnapToEdge(currentDockPosition);
                    SaveDockLocation();
                }
            };

            NitroDockMain_OpacityPanel.AllowDrop = true;
            NitroDockMain_OpacityPanel.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            };

            NitroDockMain_OpacityPanel.DragOver += NitroDockMain_OpacityPanel_DragOver;
            NitroDockMain_OpacityPanel.DragDrop += NitroDockMain_OpacityPanel_DragDrop;

            AddConfigButton();
            LoadSettings();
            SnapToEdge(currentDockPosition);
            ApplyGlowEffect();

            // Log form events
            this.VisibleChanged += (s, e) => Log($"VisibleChanged: {this.Visible}");
            this.LocationChanged += (s, e) => Log($"LocationChanged: {this.Location}");
            this.Deactivate += (s, e) => Log("Form Deactivated");
            this.Activated += (s, e) => Log("Form Activated");

            SubclassWindow();
        }

        private void SubclassWindow()
        {
            if (!_subclassed)
            {
                _originalWndProc = SetWindowLong(this.Handle, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate((WndProc)WindowProc));
                _subclassed = true;
            }
        }

        private IntPtr WindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            Screen assignedScreen = Screen.AllScreens[_assignedMonitorIndex];
            Rectangle workingArea = assignedScreen.WorkingArea;

            switch (msg)
            {
                case WM_WINDOWPOSCHANGING:
                    {
                        WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
                        pos.x = Math.Clamp(pos.x, workingArea.Left, workingArea.Right - this.Width);
                        pos.y = Math.Clamp(pos.y, workingArea.Top, workingArea.Bottom - this.Height);
                        Marshal.StructureToPtr(pos, lParam, true);
                        Log($"WM_WINDOWPOSCHANGING: Location={pos.x}, {pos.y}");
                        break;
                    }
                case WM_MOVE:
                    {
                        int x = (short)(lParam.ToInt32() & 0xFFFF);
                        int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
                        x = Math.Clamp(x, workingArea.Left, workingArea.Right - this.Width);
                        y = Math.Clamp(y, workingArea.Top, workingArea.Bottom - this.Height);
                        this.Location = new Point(x, y);
                        Log($"WM_MOVE: Location={x}, {y}");
                        break;
                    }
                case WM_SIZE:
                case WM_ACTIVATE:
                case WM_SHOWWINDOW:
                case WM_WINDOWPOSCHANGED:
                    {
                        EnsureFormOnScreen();
                        break;
                    }
            }

            return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
        }

        private delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public uint flags;
        }

        private void Log(string message)
        {
            lock (_logLock)
            {
                try
                {
                    File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to log: {ex.Message}");
                }
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                Screen assignedScreen = Screen.AllScreens[_assignedMonitorIndex];
                Rectangle workingArea = assignedScreen.WorkingArea;
                cp.X = Math.Clamp(cp.X, workingArea.Left, workingArea.Right - this.Width);
                cp.Y = Math.Clamp(cp.Y, workingArea.Top, workingArea.Bottom - this.Height);
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateRoundedRegion();
            EnsureFormOnScreen();

            Screen assignedScreen = Screen.AllScreens[_assignedMonitorIndex];
            Rectangle workingArea = assignedScreen.WorkingArea;
            this.Location = new Point(
                Math.Clamp(this.Location.X, workingArea.Left, workingArea.Right - this.Width),
                Math.Clamp(this.Location.Y, workingArea.Top, workingArea.Bottom - this.Height)
            );
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            EnsureFormOnScreen();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            EnsureFormOnScreen();
        }

        public void EnsureFormOnScreen()
        {
            Screen assignedScreen = Screen.AllScreens[_assignedMonitorIndex];
            Rectangle workingArea = assignedScreen.WorkingArea;

            int newX = Math.Clamp(this.Location.X, workingArea.Left, workingArea.Right - this.Width);
            int newY = Math.Clamp(this.Location.Y, workingArea.Top, workingArea.Bottom - this.Height);

            if (this.Location.X != newX || this.Location.Y != newY)
            {
                this.Location = new Point(newX, newY);
                Log($"[EnsureFormOnScreen] Dock position clamped to: {this.Location}");
            }
        }

        private int GetIconIndex(IconContainer container)
        {
            int index = 0;
            foreach (IconContainer c in NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>())
            {
                if (c == container && (c.Controls[0] as Button).Tag?.ToString() != "NitroDockMain_Configuration")
                    return index;
                if ((c.Controls[0] as Button).Tag?.ToString() != "NitroDockMain_Configuration")
                    index++;
            }
            return -1;
        }

        public void SaveIconToIni(IconContainer container)
        {
            string iniPath = Path.Combine(appPath, "NitroDockX.ini");
            IniFile ini = new IniFile(iniPath);
            var button = container.Controls[0] as Button;
            string path = button.Tag?.ToString() ?? string.Empty;

            int index = GetIconIndex(container);
            if (index >= 0)
            {
                ini.Write("Icons", $"Icon{index}_Path", path);
                if (button.Image?.Tag is string customIconPath)
                {
                    ini.Write("Icons", $"Icon{index}_CustomIcon", customIconPath);
                }
                // Save background color (including transparency)
                ini.Write("Icons", $"Icon{index}_ContainerBackgroundColor", container.BackColor.ToArgb().ToString());
                if (container.BackgroundImage?.Tag is string texturePath)
                {
                    ini.Write("Icons", $"Icon{index}_ContainerBackgroundTexture", texturePath);
                }
                else
                {
                    // If no texture, save an empty string
                    ini.Write("Icons", $"Icon{index}_ContainerBackgroundTexture", "");
                }
            }
            // Save config container
            else if (button.Tag?.ToString() == "NitroDockMain_Configuration")
            {
                ini.Write("Icons", "ConfigContainer_ContainerBackgroundColor", container.BackColor.ToArgb().ToString());
                if (container.BackgroundImage?.Tag is string configTexturePath)
                {
                    ini.Write("Icons", "ConfigContainer_ContainerBackgroundTexture", configTexturePath);
                }
                else
                {
                    ini.Write("Icons", "ConfigContainer_ContainerBackgroundTexture", "");
                }
                if (button.Image?.Tag is string configCustomIconPath)
                {
                    ini.Write("Icons", "ConfigContainer_CustomIcon", configCustomIconPath);
                }
                else
                {
                    ini.Write("Icons", "ConfigContainer_CustomIcon", "");
                }
            }
        }

        private void SaveDockLocation()
        {
            string iniPath = Path.Combine(appPath, "NitroDockX.ini");
            IniFile ini = new IniFile(iniPath);
            ini.Write("DockSettings", "DockLocationX", Location.X.ToString());
            ini.Write("DockSettings", "DockLocationY", Location.Y.ToString());
        }

        private void LoadSettings()
        {
            string iniPath = Path.Combine(appPath, "NitroDockX.ini");
            Log($"Loading settings from: {iniPath}");

            IniFile ini = new IniFile(iniPath);
            int locationX = 100; // Default
            int locationY = 100; // Default

            // Load saved location if it exists
            if (int.TryParse(ini.Read("DockSettings", "DockLocationX"), out int savedX) &&
                int.TryParse(ini.Read("DockSettings", "DockLocationY"), out int savedY))
            {
                locationX = savedX;
                locationY = savedY;
                Log($"Loaded saved location: X={locationX}, Y={locationY}");
            }
            else
            {
                Log("No saved location found. Using default (100, 100).");
            }

            // Load monitor assignment
            if (int.TryParse(ini.Read("DockSettings", "AssignedMonitor"), out int assignedMonitor))
            {
                AssignedMonitorIndex = assignedMonitor - 1;
            }
            else
            {
                AssignedMonitorIndex = 0;
            }

            // Force the dock to stay within the assigned monitor
            Screen assignedScreen = Screen.AllScreens[_assignedMonitorIndex];
            Rectangle workingArea = assignedScreen.WorkingArea;
            locationX = Math.Clamp(locationX, workingArea.Left, workingArea.Right - 100);
            locationY = Math.Clamp(locationY, workingArea.Top, workingArea.Bottom - 100);
            this.Location = new Point(locationX, locationY);
            Log($"Final dock location set to: {this.Location}");

            if (Enum.TryParse(ini.Read("DockSettings", "DockPosition"), out DockPosition position))
                currentDockPosition = position;

            if (float.TryParse(ini.Read("DockSettings", "DockOpacity"), out float opacity))
                Opacity = opacity;

            if (int.TryParse(ini.Read("DockSettings", "DockOffset"), out int offset))
                DockOffset = offset;

            if (int.TryParse(ini.Read("DockSettings", "DockOffsetZ"), out int offsetZ))
                DockOffsetZ = offsetZ;

            if (int.TryParse(ini.Read("DockSettings", "IconSize"), out int iconSize))
                IconSize = Math.Clamp(iconSize, 16, 64);
            else
                IconSize = 48;

            if (int.TryParse(ini.Read("DockSettings", "IconSpacing"), out int iconSpacing))
                IconSpacing = iconSpacing;

            if (Enum.TryParse(ini.Read("DockSettings", "GlowColor"), out GlowColor glowColor))
                SelectedGlowColor = glowColor;

            string cornerStyle = ini.Read("DockSettings", "DockCornerStyle", "Round Dock Corners");
            CurrentCornerStyle = cornerStyle == "Round Dock Corners" ? CornerStyle.Round : CornerStyle.Square;

            string skinName = ini.Read("DockSettings", "Skin", "Default");
            string skinMode = ini.Read("DockSettings", "SkinMode", "Stretch");
            ApplySkin(skinName, skinMode);
            ClearIcons();
            LoadIcons(ini);
            LoadConfigContainerBackgroundColor(ini);

            // Load configuration container settings
            string configTexturePath = ini.Read("Icons", "ConfigContainer_ContainerBackgroundTexture");
            string configCustomIconPath = ini.Read("Icons", "ConfigContainer_CustomIcon");
            var configContainer = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .FirstOrDefault(c => (c.Controls[0] as Button).Tag?.ToString() == "NitroDockMain_Configuration");

            if (configContainer != null)
            {
                // Load texture
                if (!string.IsNullOrEmpty(configTexturePath) && File.Exists(configTexturePath))
                {
                    try
                    {
                        configContainer.BackgroundImage = Image.FromFile(configTexturePath);
                        configContainer.BackgroundImage.Tag = configTexturePath;
                        configContainer.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                    catch (Exception ex)
                    {
                        Log($"Error loading config container texture: {ex}");
                    }
                }
                // Load custom icon
                if (!string.IsNullOrEmpty(configCustomIconPath) && File.Exists(configCustomIconPath))
                {
                    try
                    {
                        (configContainer.Controls[0] as Button).Image = ResizeImage(Image.FromFile(configCustomIconPath), IconSize, IconSize);
                        (configContainer.Controls[0] as Button).Image.Tag = configCustomIconPath;
                    }
                    catch (Exception ex)
                    {
                        Log($"Error loading config container custom icon: {ex}");
                    }
                }
            }
        }

        private void LoadConfigContainerBackgroundColor(IniFile ini)
        {
            if (int.TryParse(ini.Read("Icons", "ConfigContainer_ContainerBackgroundColor"), out int configBackgroundColorArgb))
            {
                var configContainer = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                    .FirstOrDefault(c => (c.Controls[0] as Button).Tag.ToString() == "NitroDockMain_Configuration");

                if (configContainer != null)
                {
                    configContainer.BackColor = Color.FromArgb(configBackgroundColorArgb);
                }
            }
        }

        private void ClearIcons()
        {
            var containers = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .Where(c => (c.Controls[0] as Button).Tag.ToString() != "NitroDockMain_Configuration")
                .ToList();

            foreach (IconContainer container in containers)
            {
                NitroDockMain_OpacityPanel.Controls.Remove(container);
                container.Dispose();
            }
        }

        private void LoadIcons(IniFile ini)
        {
            int index = 0;
            while (true)
            {
                string path = ini.Read("Icons", $"Icon{index}_Path");
                if (string.IsNullOrEmpty(path)) break;

                string customIcon = ini.Read("Icons", $"Icon{index}_CustomIcon");
                IconContainer container = CreateIconContainerForFileOrDirectory(path);

                if (!string.IsNullOrEmpty(customIcon))
                {
                    Log($"Loading custom icon for Icon{index}: {customIcon}");
                    if (File.Exists(customIcon))
                    {
                        try
                        {
                            (container.Controls[0] as Button).Image = ResizeImage(Image.FromFile(customIcon), IconSize, IconSize);
                            (container.Controls[0] as Button).Image.Tag = customIcon;
                            Log($"Successfully loaded custom icon for Icon{index}");
                        }
                        catch (Exception ex)
                        {
                            Log($"Error loading custom icon for Icon{index}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Log($"Custom icon file not found for Icon{index}: {customIcon}");
                    }
                }

                if (int.TryParse(ini.Read("Icons", $"Icon{index}_ContainerBackgroundColor"), out int backgroundColorArgb))
                {
                    container.BackColor = Color.FromArgb(backgroundColorArgb);
                }
                else
                {
                    container.BackColor = Color.Transparent;
                }

                string texturePath = ini.Read("Icons", $"Icon{index}_ContainerBackgroundTexture");
                if (!string.IsNullOrEmpty(texturePath))
                {
                    if (File.Exists(texturePath))
                    {
                        try
                        {
                            container.BackgroundImage = Image.FromFile(texturePath);
                            container.BackgroundImage.Tag = texturePath;
                            container.BackgroundImageLayout = ImageLayout.Stretch;
                        }
                        catch (Exception ex)
                        {
                            Log($"Error loading texture for Icon{index}: {ex.Message}");
                            container.BackgroundImage = null;
                        }
                    }
                }

                NitroDockMain_OpacityPanel.Controls.Add(container);
                index++;
            }

            RedistributeContainers();
        }

        public void ApplySkin(string skinName, string skinMode)
        {
            string skinPath = Path.Combine(appPath, "NitroSkins", skinName, "01.png");
            Log($"Applying skin: {skinName} from {skinPath}");

            // Save the current custom icons and their containers
            var containers = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .Where(c => (c.Controls[0] as Button).Tag.ToString() != "NitroDockMain_Configuration")
                .ToList();

            List<(string path, string customIcon, Color backColor, string texturePath)> iconStates = new List<(string, string, Color, string)>();
            foreach (var container in containers)
            {
                var button = container.Controls[0] as Button;
                string path = button.Tag?.ToString() ?? string.Empty;
                string customIcon = button.Image?.Tag as string;
                Color backColor = container.BackColor;
                string texturePath = container.BackgroundImage?.Tag as string;
                iconStates.Add((path, customIcon, backColor, texturePath));
            }

            if (File.Exists(skinPath))
            {
                try
                {
                    Image skinImage = Image.FromFile(skinPath);
                    NitroDockMain_OpacityPanel.BackgroundImage = skinImage;

                    switch (skinMode)
                    {
                        case "None":
                            NitroDockMain_OpacityPanel.BackgroundImageLayout = ImageLayout.None;
                            break;
                        case "Tile":
                            NitroDockMain_OpacityPanel.BackgroundImageLayout = ImageLayout.Tile;
                            break;
                        case "Center":
                            NitroDockMain_OpacityPanel.BackgroundImageLayout = ImageLayout.Center;
                            break;
                        case "Stretch":
                            NitroDockMain_OpacityPanel.BackgroundImageLayout = ImageLayout.Stretch;
                            break;
                        case "Zoom":
                            NitroDockMain_OpacityPanel.BackgroundImageLayout = ImageLayout.Zoom;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error applying skin: {ex}");
                    NitroDockMain_OpacityPanel.BackgroundImage = null;
                }
            }
            else
            {
                Log($"Skin file not found: {skinPath}");
                NitroDockMain_OpacityPanel.BackgroundImage = null;
            }

            // Force form to restore visibility and ensure it's on-screen
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
            this.Focus();
            EnsureFormOnScreen();
            Log($"Form restored: Visible={this.Visible}, Location={this.Location}");

            // Reapply custom icons and states
            int index = 0;
            foreach (var container in containers)
            {
                var button = container.Controls[0] as Button;
                var (path, customIcon, backColor, texturePath) = iconStates[index];

                if (!string.IsNullOrEmpty(customIcon) && File.Exists(customIcon))
                {
                    try
                    {
                        button.Image = ResizeImage(Image.FromFile(customIcon), IconSize, IconSize);
                        button.Image.Tag = customIcon;
                    }
                    catch (Exception ex)
                    {
                        Log($"Error reapplying custom icon: {ex}");
                    }
                }

                container.BackColor = backColor;

                if (!string.IsNullOrEmpty(texturePath) && File.Exists(texturePath))
                {
                    try
                    {
                        container.BackgroundImage = Image.FromFile(texturePath);
                        container.BackgroundImage.Tag = texturePath;
                        container.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                    catch (Exception ex)
                    {
                        Log($"Error reapplying texture: {ex}");
                        container.BackgroundImage = null;
                    }
                }

                index++;
            }
        }

        private void OnMouseMiddleButtonDown(MouseEventArgs e)
        {
            Point clientPoint = NitroDockMain_OpacityPanel.PointToClient(new Point(e.X, e.Y));
            var containers = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .Where(c => c.Bounds.Contains(clientPoint) && (c.Controls[0] as Button).Tag.ToString() != "NitroDockMain_Configuration")
                .ToList();

            var selectedContainer = containers.FirstOrDefault();
            if (selectedContainer != null)
            {
                selectedButton = selectedContainer.Controls[0] as Button;
                isIconSelected = (selectedButton != null);
                NitroDockMain_OpacityPanel.Focus();
            }
        }

        private void OnMouseWheel(MouseEventArgs e)
        {
            Point clientPoint = NitroDockMain_OpacityPanel.PointToClient(new Point(e.X, e.Y));
            if (NitroDockMain_OpacityPanel.ClientRectangle.Contains(clientPoint) && isIconSelected && selectedButton != null)
            {
                bool isVerticalDock = (currentDockPosition == DockPosition.Left || currentDockPosition == DockPosition.Right);
                ReorderSelectedButton(isVerticalDock ? e.Delta > 0 : e.Delta < 0);
            }
        }

        private void ReorderSelectedButton(bool moveUp)
        {
            var containers = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .Where(c => (c.Controls[0] as Button).Tag.ToString() != "NitroDockMain_Configuration")
                .ToList();

            int currentIndex = containers.FindIndex(c => c.Controls[0] == selectedButton);
            if (currentIndex == -1) return;

            int nextIndex = moveUp ? currentIndex - 1 : currentIndex + 1;
            if (nextIndex < 0 || nextIndex >= containers.Count) return;

            var nextContainer = containers[nextIndex];
            Point currentLocation = containers[currentIndex].Location;
            containers[currentIndex].Location = nextContainer.Location;
            nextContainer.Location = currentLocation;

            NitroDockMain_OpacityPanel.Controls.SetChildIndex(containers[currentIndex], nextIndex);
            NitroDockMain_OpacityPanel.Controls.SetChildIndex(nextContainer, currentIndex);
        }

        private string GetNitroIconsPath()
        {
            string nitroIconsPath = Path.Combine(appPath, "NitroIcons");

            if (!Directory.Exists(nitroIconsPath))
            {
                Directory.CreateDirectory(nitroIconsPath);
            }

            return nitroIconsPath;
        }

        private void AddConfigButton()
        {
            IconContainer configContainer = new IconContainer("NitroDockMain_Configuration", IconSize);
            ContextMenuStrip configContextMenu = new ContextMenuStrip();

            ToolStripMenuItem propertiesItem = new ToolStripMenuItem("Icon Properties");
            propertiesItem.Click += (s, e) => ShowIconProperties(configContainer.Controls[0] as Button);
            configContextMenu.Items.Add(propertiesItem);

            ToolStripMenuItem DockTexturizer = new ToolStripMenuItem("Dock Texturizer");
            DockTexturizer.Click += (s, e) => OpenDockTexturizer();
            configContextMenu.Items.Add(DockTexturizer);

            ToolStripMenuItem skinPropertiesItem = new ToolStripMenuItem("Style Properties");
            skinPropertiesItem.Click += (s, e) => OpenStyleProperties();
            configContextMenu.Items.Add(skinPropertiesItem);

            ToolStripMenuItem clearIniItem = new ToolStripMenuItem("Clear .ini File");
            clearIniItem.Click += (s, ev) => ClearIniFile();
            configContextMenu.Items.Add(clearIniItem);

            configContextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit NitroDockX");
            exitItem.Click += (s, ev) => Application.Exit();
            configContextMenu.Items.Add(exitItem);

            (configContainer.Controls[0] as Button).ContextMenuStrip = configContextMenu;
            (configContainer.Controls[0] as Button).MouseDown += (s, ev) =>
            {
                if (ev.Button == MouseButtons.Left)
                {
                    NitroDockMain_Configuration configForm = new NitroDockMain_Configuration(this);
                    configForm.Show();
                }
            };

            NitroDockMain_OpacityPanel.Controls.Add(configContainer);
            PositionConfigButton(configContainer);
        }

        private void OpenStyleProperties()
        {
            NitroDockMain_StyleProperties stylePropertiesForm = new NitroDockMain_StyleProperties(this);
            stylePropertiesForm.ShowDialog();
        }


        private void OpenDockTexturizer()
        {
            DockTexturizer dockTexturizerForm = new DockTexturizer();
            dockTexturizerForm.ShowDialog();
        }

        private void ShowIconProperties(Button button)
        {
            NitroDockMain_IconProperties propertiesForm = new NitroDockMain_IconProperties(button);

            if (button.Tag?.ToString() == "NitroDockMain_Configuration")
            {
                propertiesForm.HideRemoveOption();
            }

            if (propertiesForm.ShowDialog() == DialogResult.OK)
            {
                SaveIconToIni(button.Parent as IconContainer);
            }
        }



        private void PositionConfigButton(IconContainer configContainer)
        {
            int containerWidth = configContainer.Width;
            int containerHeight = configContainer.Height;

            switch (currentDockPosition)
            {
                case DockPosition.Left:
                case DockPosition.Right:
                    configContainer.Location = new Point(
                        (NitroDockMain_OpacityPanel.Width / 2) - (containerWidth / 2),
                        IconSpacing
                    );
                    break;
                case DockPosition.Top:
                case DockPosition.Bottom:
                    configContainer.Location = new Point(
                        IconSpacing,
                        (NitroDockMain_OpacityPanel.Height / 2) - (containerHeight / 2)
                    );
                    break;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (GraphicsPath path = GetRoundedRect(this.ClientRectangle, (CurrentCornerStyle == CornerStyle.Round) ? roundCornerRadius : squareCornerRadius))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = radius * 2f;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        public void UpdateRoundedRegion()
        {
            try
            {
                int radius = (CurrentCornerStyle == CornerStyle.Round) ? roundCornerRadius : squareCornerRadius;
                using (GraphicsPath path = GetRoundedRect(this.ClientRectangle, radius))
                {
                    this.Region = new Region(path);
                }
            }
            catch (Exception ex)
            {
                Log($"Error updating dock region: {ex.Message}");
                MessageBox.Show($"Error updating dock region: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NitroDockMain_OpacityPanel_DragOver(object sender, DragEventArgs e)
        {
            Point clientPoint = NitroDockMain_OpacityPanel.PointToClient(new Point(e.X, e.Y));
            foreach (Control control in NitroDockMain_OpacityPanel.Controls)
            {
                if (control is IconContainer && control.Bounds.Contains(clientPoint))
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }
            }
            e.Effect = DragDropEffects.Copy;
        }

        private void NitroDockMain_OpacityPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Point dropPoint = NitroDockMain_OpacityPanel.PointToClient(new Point(e.X, e.Y));

                foreach (string filePath in files)
                {
                    string resolvedPath = ResolveShortcut(filePath);
                    IconContainer newContainer = CreateIconContainerForFileOrDirectory(resolvedPath);
                    InsertContainerAtDropPosition(newContainer, dropPoint);
                }
            }
        }

        private IconContainer CreateIconContainerForFileOrDirectory(string path)
        {
            IconContainer container = new IconContainer(path, IconSize);
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem propertiesItem = new ToolStripMenuItem("Icon Properties");
            propertiesItem.Click += (s, ev) =>
            {
                ShowIconProperties(container.Controls[0] as Button);
                SaveIconToIni(container);
            };
            contextMenu.Items.Add(propertiesItem);

            ToolStripMenuItem removeItem = new ToolStripMenuItem("Remove Item");
            removeItem.Click += (s, ev) => RemoveContainer(container);
            contextMenu.Items.Add(removeItem);

            (container.Controls[0] as Button).ContextMenuStrip = contextMenu;
            return container;
        }

        private void ClearIniFile()
        {
            string iniPath = Path.Combine(appPath, "NitroDockX.ini");
            if (File.Exists(iniPath))
            {
                File.WriteAllText(iniPath, string.Empty);
                MessageBox.Show("The .ini file has been cleared. Restart NitroDockX to apply changes.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InsertContainerAtDropPosition(IconContainer newContainer, Point dropPoint)
        {
            var containers = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .Where(c => (c.Controls[0] as Button).Tag.ToString() != "NitroDockMain_Configuration")
                .ToList();

            if (containers.Count == 0)
            {
                NitroDockMain_OpacityPanel.Controls.Add(newContainer);
                RedistributeContainers();
                return;
            }

            IconContainer closestContainer = null;
            int minDistance = int.MaxValue;

            foreach (IconContainer container in containers)
            {
                int distance = Math.Abs(container.Top - dropPoint.Y);
                if (currentDockPosition == DockPosition.Top || currentDockPosition == DockPosition.Bottom)
                {
                    distance = Math.Abs(container.Left - dropPoint.X);
                }

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestContainer = container;
                }
            }

            if (closestContainer != null)
            {
                int insertIndex = NitroDockMain_OpacityPanel.Controls.GetChildIndex(closestContainer);
                NitroDockMain_OpacityPanel.Controls.Add(newContainer);
                NitroDockMain_OpacityPanel.Controls.SetChildIndex(newContainer, insertIndex);
            }
            else
            {
                NitroDockMain_OpacityPanel.Controls.Add(newContainer);
            }

            RedistributeContainers();
        }

        private string ResolveShortcut(string shortcutPath)
        {
            if (!shortcutPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                return shortcutPath;
            }

            try
            {
                IShellLinkW shellLink = (IShellLinkW)new ShellLink();
                IPersistFile persistFile = (IPersistFile)shellLink;
                persistFile.Load(shortcutPath, 0);

                StringBuilder targetPath = new StringBuilder(260);
                shellLink.GetPath(targetPath, targetPath.Capacity, out _, 0);
                return targetPath.ToString();
            }
            catch (Exception ex)
            {
                Log($"Error resolving shortcut: {ex}");
                return shortcutPath;
            }
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out WIN32_FIND_DATAW pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out ushort pwHotkey);
            void SetHotkey(ushort wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        public void RedistributeContainers()
        {
            var containers = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .Where(c => (c.Controls[0] as Button).Tag.ToString() != "NitroDockMain_Configuration")
                .ToList();

            IconContainer configContainer = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .FirstOrDefault(c => (c.Controls[0] as Button).Tag.ToString() == "NitroDockMain_Configuration");

            int startY = IconSpacing;
            int startX = IconSpacing;

            if (configContainer != null)
            {
                PositionConfigButton(configContainer);
                startY = configContainer.Bottom + IconSpacing;
                startX = configContainer.Right + IconSpacing;
            }

            switch (currentDockPosition)
            {
                case DockPosition.Left:
                case DockPosition.Right:
                    {
                        int totalContainersHeight = containers.Count * containerSize + (containers.Count > 0 ? (containers.Count - 1) * IconSpacing : 0);
                        int dockHeight = startY + totalContainersHeight + IconSpacing;
                        ClientSize = new Size(dockWidth, dockHeight);
                        int centerX = ClientSize.Width / 2;

                        for (int i = 0; i < containers.Count; i++)
                        {
                            containers[i].Location = new Point(
                                centerX - (containerSize / 2),
                                startY + i * (containerSize + IconSpacing)
                            );
                        }
                        break;
                    }
                case DockPosition.Top:
                case DockPosition.Bottom:
                    {
                        int totalContainersWidth = containers.Count * containerSize + (containers.Count > 0 ? (containers.Count - 1) * IconSpacing : 0);
                        int dockWidth = startX + totalContainersWidth + IconSpacing;
                        ClientSize = new Size(dockWidth, dockHeight);
                        int centerY = ClientSize.Height / 2;

                        for (int i = 0; i < containers.Count; i++)
                        {
                            containers[i].Location = new Point(
                                startX + i * (containerSize + IconSpacing),
                                centerY - (containerSize / 2)
                            );
                        }
                        break;
                    }
            }

            UpdateRoundedRegion();
        }


        public void UpdateAllIconSizes(int newSize)
        {
            IconSize = Math.Clamp(newSize, 16, 64);

            foreach (IconContainer container in NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>())
            {
                container.UpdateIconSize(IconSize);

                if (container.Controls[0] is Button button)
                {
                    string customIconPath = button.Image?.Tag as string;

                    // If a custom icon is set, reapply it
                    if (!string.IsNullOrEmpty(customIconPath) && File.Exists(customIconPath))
                    {
                        try
                        {
                            // Reload and resize the custom icon
                            Image customImage = Image.FromFile(customIconPath);
                            button.Image = ResizeImage(customImage, IconSize, IconSize);
                            button.Image.Tag = customIconPath;
                            customImage.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Log($"Error reapplying custom icon: {ex.Message}");
                            // Fallback to default icon if custom icon fails
                            button.Image = ResizeImage(SystemIcons.Application.ToBitmap(), IconSize, IconSize);
                        }
                    }
                }
            }

            // Explicitly update config button
            var configContainer = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .FirstOrDefault(c => (c.Controls[0] as Button).Tag?.ToString() == "NitroDockMain_Configuration");

            if (configContainer != null)
            {
                configContainer.UpdateIconSize(IconSize);

                if (configContainer.Controls[0] is Button configButton)
                {
                    string configCustomIconPath = configButton.Image?.Tag as string;

                    if (!string.IsNullOrEmpty(configCustomIconPath) && File.Exists(configCustomIconPath))
                    {
                        try
                        {
                            // Reload and resize the custom icon for the config button
                            Image customImage = Image.FromFile(configCustomIconPath);
                            configButton.Image = ResizeImage(customImage, IconSize, IconSize);
                            configButton.Image.Tag = configCustomIconPath;
                            customImage.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Log($"Error reapplying config custom icon: {ex.Message}");
                            configButton.Image = ResizeImage(SystemIcons.Shield.ToBitmap(), IconSize, IconSize);
                        }
                    }
                }
            }

            // Force a visual refresh
            NitroDockMain_OpacityPanel.Invalidate(true);
            NitroDockMain_OpacityPanel.Update();
            NitroDockMain_OpacityPanel.Refresh();

            SnapToEdge(currentDockPosition);
            ApplyGlowEffect();
        }







        public void UpdateAllIconSpacings(int newSpacing)
        {
            IconSpacing = newSpacing;
            RedistributeContainers();
        }

        public void SnapToEdge(DockPosition position)
        {
            Screen assignedScreen = Screen.AllScreens[_assignedMonitorIndex];
            Rectangle workingArea = assignedScreen.WorkingArea;
            const int minDockSize = 100;

            var containers = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .Where(c => (c.Controls[0] as Button).Tag.ToString() != "NitroDockMain_Configuration")
                .ToList();

            int calculatedDockWidth, calculatedDockHeight;

            if (position == DockPosition.Left || position == DockPosition.Right)
            {
                calculatedDockWidth = dockWidth;
                int totalContainersHeight = containers.Count * containerSize + (containers.Count > 0 ? (containers.Count - 1) * IconSpacing : 0);
                calculatedDockHeight = Math.Max(totalContainersHeight + 2 * IconSpacing, minDockSize);
            }
            else
            {
                calculatedDockHeight = dockHeight;
                int totalContainersWidth = containers.Count * containerSize + (containers.Count > 0 ? (containers.Count - 1) * IconSpacing : 0);
                calculatedDockWidth = Math.Max(totalContainersWidth + 2 * IconSpacing, minDockSize);
            }

            int maxZOffsetVertical = workingArea.Height - calculatedDockHeight;
            int maxZOffsetHorizontal = workingArea.Width - calculatedDockWidth;

            DockOffset = Math.Min(DockOffset, workingArea.Width / 2);
            DockOffsetZ = Math.Min(DockOffsetZ, position == DockPosition.Left || position == DockPosition.Right ? maxZOffsetVertical : maxZOffsetHorizontal);

            int newX, newY;
            switch (position)
            {
                case DockPosition.Left:
                    newX = workingArea.Left + DockOffset;
                    newY = workingArea.Top + Math.Min(DockOffsetZ, maxZOffsetVertical);
                    break;
                case DockPosition.Right:
                    newX = workingArea.Right - calculatedDockWidth - DockOffset;
                    newY = workingArea.Top + Math.Min(DockOffsetZ, maxZOffsetVertical);
                    break;
                case DockPosition.Top:
                    newX = workingArea.Left + Math.Min(DockOffsetZ, maxZOffsetHorizontal);
                    newY = workingArea.Top + DockOffset;
                    break;
                case DockPosition.Bottom:
                    newX = workingArea.Left + Math.Min(DockOffsetZ, maxZOffsetHorizontal);
                    newY = workingArea.Bottom - calculatedDockHeight - DockOffset;
                    break;
                default:
                    newX = this.Location.X;
                    newY = this.Location.Y;
                    break;
            }

            newX = Math.Clamp(newX, workingArea.Left, workingArea.Right - calculatedDockWidth);
            newY = Math.Clamp(newY, workingArea.Top, workingArea.Bottom - calculatedDockHeight);

            this.Location = new Point(newX, newY);
            ClientSize = new Size(calculatedDockWidth, calculatedDockHeight);

            var cfgContainer = NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>()
                .FirstOrDefault(c => (c.Controls[0] as Button).Tag.ToString() == "NitroDockMain_Configuration");
            if (cfgContainer != null)
            {
                PositionConfigButton(cfgContainer);
            }

            RedistributeContainers();
            UpdateRoundedRegion();
        }

        private void RemoveContainer(IconContainer container)
        {
            if ((container.Controls[0] as Button).Tag?.ToString() == "NitroDockMain_Configuration")
            {
                MessageBox.Show("The Configuration button cannot be removed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string iniPath = Path.Combine(appPath, "NitroDockX.ini");
            IniFile ini = new IniFile(iniPath);
            int index = GetIconIndex(container);
            if (index >= 0)
            {
                ini.Write("Icons", $"Icon{index}_Path", "");
                ini.Write("Icons", $"Icon{index}_CustomIcon", "");
                ini.Write("Icons", $"Icon{index}_ContainerBackgroundColor", "");
                ini.Write("Icons", $"Icon{index}_ContainerBackgroundTexture", "");
            }
            NitroDockMain_OpacityPanel.Controls.Remove(container);
            container.Dispose();
            RedistributeContainers();
        }

        public void ApplyGlowEffect()
        {
            foreach (IconContainer container in NitroDockMain_OpacityPanel.Controls.OfType<IconContainer>())
            {
                (container.Controls[0] as Button).FlatAppearance.BorderSize = 0;
                (container.Controls[0] as Button).BackColor = Color.Transparent;

                (container.Controls[0] as Button).MouseEnter += (sender, e) =>
                {
                    Color glowColor = GetGlowColor(SelectedGlowColor);
                    (container.Controls[0] as Button).BackColor = Color.FromArgb(50, glowColor);
                };

                (container.Controls[0] as Button).MouseLeave += (sender, e) =>
                {
                    (container.Controls[0] as Button).BackColor = Color.Transparent;
                };
            }
        }

        private Color GetGlowColor(GlowColor glowColor)
        {
            return glowColor switch
            {
                GlowColor.Blue => Color.Blue,
                GlowColor.Cyan => Color.Cyan,
                GlowColor.Pink => Color.HotPink,
                GlowColor.Red => Color.Red,
                GlowColor.SlateGray => Color.SlateGray,
                GlowColor.White => Color.WhiteSmoke,
                GlowColor.Yellow => Color.LightYellow,
                _ => Color.Blue,
            };
        }

        public Bitmap ResizeImage(Image image, int width, int height)
        {
            if (image == null)
                return null;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            }

            return destImage;
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
    }
}
