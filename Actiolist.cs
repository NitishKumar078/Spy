using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Spy
{
    internal class Actionlist
    {
        // Structure for mouse hook callback
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONUP = 0x0202;

        private static IntPtr keyboardHook;
        private static IntPtr mouseHook;

        // Delegate for keyboard hook
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc keyboardProc = HookCallback;

        // Delegate for mouse hook
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelMouseProc mouseProc = MouseHookCallback;

        // Importing necessary methods from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        private static System.Windows.Controls.ListView _actionList; // Static variable to store the reference

        public static void SetActionList(System.Windows.Controls.ListView actionList)
        {
            _actionList = actionList;
        }

        public static void StartHook(int processId, bool isKChecked, bool isMChecked)
        {
            Process process = Process.GetProcessById(processId);
            SetForegroundWindow(process.MainWindowHandle);
            // Get the currently active process
            Process activeProcess = GetActiveProcess();

            // Display information about the active process
            if (activeProcess != null)
            {
                Console.WriteLine($"Active Process Name: {activeProcess.ProcessName}");
                Console.WriteLine($"Active Process ID: {activeProcess.Id}");
               /* if (activeProcess.Id == processId)
                {*/
                    if (isKChecked) keyboardHook = SetHook(keyboardProc, WH_KEYBOARD_LL, process.MainWindowHandle);
                    if (isMChecked) mouseHook = SetHook(mouseProc, WH_MOUSE_LL, process.MainWindowHandle);
                   
                //}
            }
            else
            {
                Console.WriteLine("No active process found.");
            }
        }

  

        public static void removeHook()
        {
            // Unhook the hooks when the application exits
            UnhookWindowsHookEx(keyboardHook);
            UnhookWindowsHookEx(mouseHook);
        }

        private static IntPtr SetHook(Delegate hookProc, int hookType, IntPtr handle)
        {

            IntPtr hModule = GetModuleHandle(handle.ToString());
            return SetWindowsHookEx(hookType, hookProc as LowLevelKeyboardProc, hModule, 0);

        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine($"Key pressed: {((Keys)vkCode).ToString()}");
                //MessageBox.Show($"Key pressed: {((Keys)vkCode).ToString()}");
                // Check if _actionList is set
                if (_actionList != null)
                {
                        _actionList.Items.Add(((Keys)vkCode).ToString());
                }

            }

            return CallNextHookEx(keyboardHook, nCode, wParam, lParam);
        }

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONUP)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                Console.WriteLine($"Mouse move at X: {hookStruct.pt.x}, Y: {hookStruct.pt.y}");
                MessageBox.Show("mouse clicked");
            }

            return CallNextHookEx(mouseHook, nCode, wParam, lParam);
        }




        static Process GetActiveProcess()
        {
            IntPtr hWnd = GetForegroundWindow(); // Get the handle of the foreground window

            if (hWnd != IntPtr.Zero)
            {
                uint processId;
                GetWindowThreadProcessId(hWnd, out processId); // Get the process ID

                return Process.GetProcessById((int)processId); // Get the process by ID
            }

            return null;
        }


    }
}
