using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NitroDock
{
    public class MouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MOUSEWHEEL = 0x020A;

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public delegate void MouseEventHandler(MouseEventArgs e);
        public event MouseEventHandler MouseMiddleButtonDown;
        public event MouseEventHandler MouseWheel;

        public MouseHook()
        {
            _hookID = SetHook(_proc);
        }

        ~MouseHook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(module.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                if (wParam.ToInt32() == WM_MBUTTONDOWN)
                {
                    MouseEventArgs e = new MouseEventArgs(MouseButtons.Middle, 1, hookStruct.pt.x, hookStruct.pt.y, 0);
                    instance?.MouseMiddleButtonDown?.Invoke(e);
                }
                else if (wParam.ToInt32() == WM_MOUSEWHEEL)
                {
                    short delta = (short)((hookStruct.mouseData >> 16) & 0xffff);
                    MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, hookStruct.pt.x, hookStruct.pt.y, delta);
                    instance?.MouseWheel?.Invoke(e);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static MouseHook instance;

        public static MouseHook Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MouseHook();
                }
                return instance;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}
