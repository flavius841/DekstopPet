using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparetBG : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    private struct MARGINS { public int cxLeftWidth; public int cxRightWidth; public int cyTopHeight; public int cyBottomHeight; }

    // Constants for Windows API
    const int GWL_STYLE = -16;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const uint SWP_TOPMOST = 0x0001; // Window is always on top

    // Transparent layer Key
    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020; // Click-through (we will toggle this later)

    void Start()
    {
        // 1. Remove window borders (Title bar, minimize button, etc.)
        IntPtr hWnd = GetActiveWindow();
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };

        // 2. Tell DWM to extend the "glass" frame into the client area (making it transparent)
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // 3. Make the window "Always on Top"
        SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0, 2 | 1); // HWND_TOPMOST
    }
}
