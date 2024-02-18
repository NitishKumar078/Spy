using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static Spy.Actionlist;


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


        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public uint dwExtraInfo;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;


        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP  = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;



        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_RBUTTONDOWN  =  0x0204;
        private const int WM_RBUTTONUP = 0x0205;
       


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

        public static int selectedpid;
        private static System.Windows.Controls.ListView _actionList; // Static variable to store the reference

        public static void SetActionList(System.Windows.Controls.ListView actionList)
        {
            _actionList = actionList;
        }

        public static void StartHook(int processId, bool isKChecked, bool isMChecked)
        {
            selectedpid = processId;
            Process process = Process.GetProcessById(processId);
            SetForegroundWindow(process.MainWindowHandle);
            

            // Display information about the active process
            if (process != null)
            {
                    if (isKChecked) keyboardHook = SetKBHook(keyboardProc, WH_KEYBOARD_LL, process.MainWindowHandle);
                    if (isMChecked) mouseHook = SetMSHook(mouseProc, WH_MOUSE_LL, process.MainWindowHandle);
                   
            }
            else
            {
                Console.WriteLine("No active process found.");
            }
        }

  

        public static void removeHook()
        {
            // Unhook the hooks when the application exits
            UnhookWindowsHookEx(mouseHook);
            UnhookWindowsHookEx(keyboardHook);
        }

        private static IntPtr SetKBHook(Delegate hookProc, int hookType, IntPtr handle)
        {

            IntPtr hModule = GetModuleHandle(handle.ToString());
            return SetWindowsHookEx(hookType, hookProc as LowLevelKeyboardProc, hModule, 0);

        }

        private static IntPtr SetMSHook(Delegate hookProc, int hookType, IntPtr handle)
        {

            IntPtr hModule = GetModuleHandle(handle.ToString());
            return SetWindowsHookEx(hookType, hookProc as LowLevelMouseProc, hModule, 0);

        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Get the currently active process
            Process activeProcess = GetActiveProcess();

            if (activeProcess != null)
            {

                if (_actionList != null)
                {
                    if (nCode >= 0 && selectedpid == activeProcess.Id)
                    {
                        string Type, Value;
                        KBDLLHOOKSTRUCT KBhookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                        switch (wParam)
                        {
                            case WM_KEYUP:
                                Type = "key Button Up";
                                Value = ((Keys)KBhookStruct.vkCode).ToString();
                                AddToList(Type, KBhookStruct, Value);
                                break;
                            case WM_KEYDOWN:
                                Type = "key Button Down";
                                Value = ((Keys)KBhookStruct.vkCode).ToString();
                                AddToList(Type, KBhookStruct, Value);
                                break;
                            case WM_SYSKEYDOWN:
                                Type = "Syskey Button Down";
                                Value = ((Keys)KBhookStruct.vkCode).ToString();
                                AddToList(Type, KBhookStruct, Value);
                                break;
                            case WM_SYSKEYUP:
                                Type = "Syskey Button Up";
                                Value = ((Keys)KBhookStruct.vkCode).ToString();
                                AddToList(Type, KBhookStruct, Value);
                                break;

                        }
                    }
                }
            }

            return CallNextHookEx(keyboardHook, nCode, wParam, lParam);
        }


        static void  AddToList(string Type, MSLLHOOKSTRUCT MShookStruct) // for mouse 
        {
            // Create an instance of ActionListItem
            ActionListItem actionItem = new ActionListItem
            {
                Type = Type,
                Struct = GetMSStructItems(MShookStruct),
                Value = "-"
            };

            // Add the item to the ListView using data binding
            _actionList.Items.Add(actionItem);
            _actionList.ScrollIntoView(actionItem);
        }
        static void AddToList(string Type, KBDLLHOOKSTRUCT KBhookStruct, string value) // for keyboard 
        {
            // Create an instance of ActionListItem
            ActionListItem actionItem = new ActionListItem
            {
                Type = Type,
                Struct = GetKBStructItems(KBhookStruct),
                Value = ((Keys)KBhookStruct.vkCode).ToString()
            };

            // Add the item to the ListView using data binding
            _actionList.Items.Add(actionItem);
            _actionList.ScrollIntoView(actionItem);
        }

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Get the currently active process
            Process activeProcess = GetActiveProcess();
            if(activeProcess != null) {
               
                if (_actionList != null)
                {
                    if (nCode >= 0 && selectedpid == activeProcess.Id)
                    {
                        string Type = "";
                        MSLLHOOKSTRUCT MShookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                        switch (wParam){
                            case WM_LBUTTONUP:
                                Type = "Mouse Left Button Up";
                                AddToList(Type, MShookStruct);
                                break;
                            case WM_LBUTTONDOWN:
                                Type = "Mouse Left Button Down";
                                AddToList(Type, MShookStruct);
                                break;
                            case WM_MOUSEWHEEL:
                                Type = "Mouse wheel";
                                AddToList(Type, MShookStruct);
                                break;
                            case WM_RBUTTONUP:
                                Type = "Mouse Right Button Up";
                                AddToList(Type, MShookStruct);
                                break;
                            case WM_RBUTTONDOWN:
                                Type = "Mouse Right Button Down";
                                AddToList(Type, MShookStruct);
                                break;
                           /* case WM_MOUSEMOVE: 
                                Type = "Mouse move";
                                break;
                            */

                                
                        }
                    }
                }
            }

            return CallNextHookEx(mouseHook, nCode, wParam, lParam);
        }

        private static ObservableCollection<StructItem> GetMSStructItems(MSLLHOOKSTRUCT hookStruct)
        {
            ObservableCollection<StructItem> structItems = new ObservableCollection<StructItem>();

            StructItem rootNode = new StructItem
            {
                Name = "struct",
                StructValue = $"pt: ({hookStruct.pt.x}, {hookStruct.pt.y})" + Environment.NewLine +
                              $"mouseData: {hookStruct.mouseData}" + Environment.NewLine +
                              $"flags: {hookStruct.flags}" + Environment.NewLine +
                              $"time: {hookStruct.time}" + Environment.NewLine +
                              $"dwExtraInfo: {hookStruct.dwExtraInfo}",
                IsExpanded = false // Set this to true if you want it expanded by default
            };

            structItems.Add(rootNode);

            return structItems;
        }

        private static ObservableCollection<StructItem> GetKBStructItems(KBDLLHOOKSTRUCT hookStruct)
        {
            ObservableCollection<StructItem> structItems = new ObservableCollection<StructItem>();

            StructItem rootNode = new StructItem
            {
                Name = "struct",
                StructValue = $"vkCode: ({hookStruct.vkCode})" + Environment.NewLine +
                              $"mouseData: {hookStruct.scanCode}" + Environment.NewLine +
                              $"flags: {hookStruct.flags}" + Environment.NewLine +
                              $"time: {hookStruct.time}" + Environment.NewLine +
                              $"dwExtraInfo: {hookStruct.dwExtraInfo}",

        IsExpanded = false // Set this to true if you want it expanded by default
            };

            structItems.Add(rootNode);

            return structItems;
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
public class ActionListItem
{
    public string Type { get; set; }
    public ObservableCollection<StructItem> Struct { get; set; } = new ObservableCollection<StructItem>(); // ObservableCollection for dynamic updates
    public string Value { get; set; }
}

public class StructItem
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string StructValue { get; set; } // Add this property
    public ObservableCollection<StructItem> Children { get; set; } = new ObservableCollection<StructItem>();
    public bool IsExpanded { get; set; } // Add this property
}
