using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class DesktopPet : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    // --- WINDOWS API DEFINITIONS (Only compiles for Windows Builds) ---
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern uint GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    private struct MARGINS { public int cxLeftWidth; public int cxRightWidth; public int cyTopHeight; public int cyBottomHeight; }

    const int GWL_EXSTYLE = -20;
    const uint WS_EX_LAYERED = 0x80000;
    const uint WS_EX_TRANSPARENT = 0x20;
    const int GWL_STYLE = -16;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    
    private IntPtr hWnd;
#endif

    private bool isClickable = true;

    void Start()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // 1. Get Window Handle
        hWnd = GetActiveWindow();

        // 2. Make Background Transparent (The Glass Effect)
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // 3. Set Initial Window Style (Layered so we can control transparency)
        // We start nicely clickable
        SetWindowExStyle(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        
        // Optional: Force removal of borders if Unity didn't do it
        // SetWindowExStyle(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
#endif
    }

    void Update()
    {
        // --- LOGIC THAT WORKS IN EDITOR (FOR TESTING) ---
        // We still run the raycast so you can debug "hitting" the pet in Linux
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitPet = Physics.Raycast(ray, out RaycastHit hit);

        // --- WINDOWS-ONLY LOGIC (THE MAGIC) ---
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (hitPet && !isClickable)
        {
            SetClickable(true); // Enable Clicking
        }
        else if (!hitPet && isClickable)
        {
            SetClickable(false); // Enable Click-Through (Ghost Mode)
        }
#endif

        // Fun Test: Click to jump (Works in Editor too!)
        if (hitPet && Input.GetMouseButtonDown(0))
        {
            if (hit.rigidbody != null) hit.rigidbody.AddForce(Vector3.up * 300);
        }
    }

    // --- HELPER FUNCTION TO SWITCH MODES ---
    private void SetClickable(bool clickable)
    {
        isClickable = clickable;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // If clickable, we just want Layered.
        // If NOT clickable (ghost), we want Layered + Transparent.
        uint flags = clickable ? WS_EX_LAYERED : (WS_EX_LAYERED | WS_EX_TRANSPARENT);
        
        if (IntPtr.Size == 8) // 64-bit
            SetWindowLongPtr64(hWnd, GWL_EXSTYLE, new IntPtr(flags));
        else // 32-bit
            SetWindowLong32(hWnd, GWL_EXSTYLE, flags);
#endif
    }

    // Helper to abstract 32/64 bit differences
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
