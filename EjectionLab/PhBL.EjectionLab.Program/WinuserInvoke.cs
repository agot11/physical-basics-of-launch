using System;
using System.Runtime.InteropServices;

namespace PhBL.EjectionLab.Program
{
    public static class WinuserInvoke
    {
        public static uint WM_COMMAND = 0x0111;

        [DllImport("user32.dll")]
		public static extern IntPtr GetDlgItem(IntPtr hWnd, int nIDDlgItem);

        [DllImport("user32.dll")]
        public static extern int GetDlgCtrlID(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSubMenu(IntPtr hWnd, int nPos);

        [DllImport("user32.dll")]
        public static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll")]
        public static extern uint GetMenuItemID(IntPtr hMenu, int nPos);

        [DllImport("user32.dll")]
        public static extern uint CheckMenuItem(IntPtr hMenu, uint uIDCheckItem, uint uCheck);

        [DllImport("user32.dll")]
        public static extern uint EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
		public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);
    }
}