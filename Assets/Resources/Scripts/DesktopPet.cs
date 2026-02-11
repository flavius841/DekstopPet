using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class DesktopPet : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    private struct MARGINS { public int cxLeftWidth; public int cxRightWidth; public int cyTopHeight; public int cyBottomHeight; }

    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x80000;
    const uint WS_EX_TRANSPARENT = 0x20;

    // Always On Top
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_SHOWWINDOW = 0x0040;

    private IntPtr hWnd;
#endif

    private bool isClickable = true;

    void Start()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        hWnd = GetActiveWindow();

        // Start coroutine to repeatedly apply transparency
        StartCoroutine(ApplyTransparencyRepeatedly());

        // Force Always On Top
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
#endif
    }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    IEnumerator ApplyTransparencyRepeatedly()
    {
        for (int i = 0; i < 10; i++)
        {
            ApplyTransparency();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ApplyTransparency()
    {
        // Glass effect
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // Layered style
        SetWindowExStyle(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
    }
#endif

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitPet = Physics.Raycast(ray, out RaycastHit hit);

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (hitPet && !isClickable)
        {
            SetClickable(true);
        }
        else if (!hitPet && isClickable)
        {
            SetClickable(false);
        }
#endif

        // Jump logic
        if (hitPet && Input.GetMouseButtonDown(0))
        {
            if (hit.rigidbody != null)
                hit.rigidbody.AddForce(Vector3.up * 300);
        }
    }

    private void SetClickable(bool clickable)
    {
        isClickable = clickable;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        uint flags = clickable
            ? WS_EX_LAYERED
            : (WS_EX_LAYERED | WS_EX_TRANSPARENT);

        if (IntPtr.Size == 8)
            SetWindowLongPtr64(hWnd, GWL_EXSTYLE, new IntPtr(flags));
        else
            SetWindowLong32(hWnd, GWL_EXSTYLE, flags);
#endif
    }

    private void SetWindowExStyle(IntPtr hWnd, int nIndex, uint flags)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (IntPtr.Size == 8)
            SetWindowLongPtr64(hWnd, nIndex, new IntPtr(flags));
        else
            SetWindowLong32(hWnd, nIndex, flags);
#endif
    }
}
