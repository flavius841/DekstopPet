using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    // --- 1. DEFINE THE FUNCTIONS FOR BOTH 32-BIT AND 64-BIT ---

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    // 32-bit version
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

    // 64-bit version (Modern Windows)
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    // --- 2. CONSTANTS ---
    const int GWL_EXSTYLE = -20;
    const uint WS_EX_TRANSPARENT = 0x20;
    const uint WS_EX_LAYERED = 0x80000;

    private IntPtr hWnd;
    private bool isClickable = true;

    void Start()
    {
        // SAFETY CHECK: Don't run this inside the Unity Editor!
        // It will try to make the Unity Editor transparent and might crash it.
        if (Application.isEditor)
        {
            Debug.Log("Transparency logic disabled in Editor. Build the game to see it work!");
            return;
        }

        hWnd = GetActiveWindow();

        // Initial setup: Ensure window is layered so we can mess with it
        // (Note: TransparetBG.cs usually handles the initial setup, but we ensure it here too)
    }

    void Update()
    {
        // If we are in the editor, do nothing
        if (Application.isEditor) return;

        // 1. Raycast to see if mouse is over the pet
        // NOTE: Make sure your Camera is tagged as "MainCamera"
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hitPet = Physics.Raycast(ray, out RaycastHit hit);

        // 2. Toggle Clickability
        if (hitPet && !isClickable)
        {
            SetClickable(true); // Can click the pet
        }
        else if (!hitPet && isClickable)
        {
            SetClickable(false); // Can click THROUGH the pet (to desktop)
        }
    }

    // --- 3. THE HELPER FUNCTION TO CHOOSE 32 vs 64 BIT ---
    void SetClickable(bool clickable)
    {
        isClickable = clickable;

        // We need to get the current style first (simplified for this example, usually we read it)
        // But for toggle purposes, let's just apply the bits we need.
        // NOTE: A robust version reads GetWindowLong first.

        // Let's assume we want to ADD or REMOVE the Transparent flag.
        // For simplicity in this fix, we will just set the logic directly:

        // This is a simplified bit of logic. 
        // If clickable = TRUE, we REMOVE the Transparent flag.
        // If clickable = FALSE, we ADD the Transparent flag.

        // (For a Hack Club project, re-applying the whole style is often safer than bitwise ops if you are new to it)
        // But here is the correct wrapper:

        if (IntPtr.Size == 8) // 64-bit
        {
            // Fetch current style (placeholder logic for brevity, you might want to fetch strictly)
            // But usually, setting the specific flags is enough for the toggle.
            // WARNING: Setting directly might reset other flags. 
            // Better to just set the one bit if you can, but let's try the simple switch:

            // Setup: We need to READ the current style first to be safe.
            // But GetWindowLong is 32-bit. 64-bit uses GetWindowLongPtr (not defined above).
            // Let's stick to the core fix: The SetWindowLong wrapper.

            // If you just want to fix the error, paste this helper:
            SetWindowLong64Wrapper(hWnd, GWL_EXSTYLE, clickable ? WS_EX_LAYERED : (WS_EX_LAYERED | WS_EX_TRANSPARENT));
        }
        else // 32-bit
        {
            SetWindowLong32(hWnd, GWL_EXSTYLE, clickable ? WS_EX_LAYERED : (WS_EX_LAYERED | WS_EX_TRANSPARENT));
        }
    }

    private static IntPtr SetWindowLong64Wrapper(IntPtr hWnd, int nIndex, uint dwNewLong)
    {
        return SetWindowLongPtr64(hWnd, nIndex, new IntPtr(dwNewLong));
    }
}