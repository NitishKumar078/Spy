using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Automation;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Drawing.Imaging;
using System.Windows;
using Condition = System.Windows.Automation.Condition;
using System.Windows.Media.Imaging;

namespace Spy
{

    class Screen_Capture
    {
        // Class variables
        private const int MAX_CLASS_NAME = 256;
        private const int MAX_WINDOW_TITLE = 1024;
        private static RECT windowRect;
        private static int window_width;
        private static int window_height;
        private static IntPtr oldHandle;
        private static string RECORD_LOG_CHANNEL = "messaging.channels.logtopic";

        /*..........................    internal DLL imports   .........................*/

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);


        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);


        // Struct to store window coordinates
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Width;
            public int Height;
        }

        // Struct to store element information
        private struct Element_info
        {
            public string Element_Name;
            public string Element_ControlType;
            public string ActionElementHandle;
            public double left;
            public double top;
            public double width;
            public double height;
        }

        public Screen_Capture()
        {  }


        public static bool compareHandle(IntPtr winhandl)
        {
            if (winhandl != oldHandle)
            {
                oldHandle = winhandl;//NativeMethods.GetForegroundWindow();
                return true;
            }
            return false;
        }

        // Method to capture the window ScreenShot ...
        public static Bitmap CaptureWindow(int window_width, int window_height,  ImageFormat format)
        {
            Bitmap screenshot = new Bitmap(window_width, window_height, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                graphics.CopyFromScreen(windowRect.Left, windowRect.Top, 0, 0, new System.Drawing.Size(window_width, window_height), CopyPixelOperation.SourceCopy);
            }

           // ScreenShot.Source = new BitmapImage(screenshot);
            return screenshot;
        }

        // Method to capture visible elements using UI Automation with help of TreeWalker
        // static string CaptureVisibleElements(AutomationElement window, List<Element_info> elements)
        // {
        //     Stopwatch stopwatch = new Stopwatch();
        //     Stopwatch jsontime = new Stopwatch();
        //     Condition NamePropertycondition = new NotCondition(
        //       new PropertyCondition(AutomationElement.NameProperty, "")
        //       );
        //     Condition condition = new AndCondition(
        //         NamePropertycondition,
        //         new PropertyCondition(AutomationElement.IsEnabledProperty, true),
        //         new PropertyCondition(AutomationElement.IsOffscreenProperty, false),
        //         new PropertyCondition(AutomationElement.IsControlElementProperty, true),
        //         new PropertyCondition(AutomationElement.IsContentElementProperty, true)
        //     );
        //     TreeWalker treeWalker = new TreeWalker(condition);
        //     jsontime.Start();
        //     stopwatch.Start(); // Start timing
        //     TimeSpan timeLimitPerElement = TimeSpan.FromMilliseconds(200); // Set the time limit per element traversal to 900 milliseconds

        //     TraverseElementWithTreeWalker(window, elements, treeWalker, stopwatch, timeLimitPerElement);
        //     jsontime.Stop();
        //     double elapsedSeconds = jsontime.Elapsed.TotalSeconds;
        //     long elapsedTime = jsontime.ElapsedMilliseconds;
        //     Console.WriteLine($"Elapsed time: {elapsedSeconds} seconds for *** ELEMENT *** in milliseconds: {elapsedTime}");
        //     System.Console.WriteLine("this is no. of children : " + counting);
        //     return JsonConvert.SerializeObject(elements, Formatting.Indented);
        // }

        // Method to check if an element is within the window bounds
        static bool IsElementWithinWindow(System.Windows.Rect elementRect)
        {
            return
                elementRect.Bottom - windowRect.Bottom <= 0 &&
                elementRect.Left - windowRect.Left >= 0 &&
                elementRect.Right - windowRect.Right <= 0 &&
                elementRect.Top - windowRect.Top >= 0 &&
                elementRect.Width <= window_width - 1 &&
                elementRect.Height <= window_height - 1;
        }

        // Recursive method to traverse elements using UI Automation ...
        // static void TraverseElementWithTreeWalker(AutomationElement element, List<Element_info> elements, TreeWalker treeWalker, Stopwatch stopwatch, TimeSpan timeLimit)
        // {
        //     stopwatch.Restart();

        //     System.Windows.Rect elementRect = element.Current.BoundingRectangle;

        //     if (IsElementWithinWindow(elementRect))
        //     {
        //         counting++;
        //         Element_info elementInfo = new Element_info
        //         {
        //             Element_Name = element.Current.Name,
        //             Element_ControlType = element.Current.LocalizedControlType,
        //             ActionElementHandle = _elementResolver.GetElementXPath(element),
        //             left = (double)(elementRect.Left - windowRect.Left),
        //             top = (double)(elementRect.Top - windowRect.Top),
        //             width = (double)elementRect.Width,
        //             height = (double)elementRect.Height
        //         };
        //         //Console.WriteLine("This is the element's name: " + element.Current.Name);
        //         elements.Add(elementInfo);
        //     }

        //     AutomationElement child = treeWalker.GetFirstChild(element);

        //     // Check if the elapsed time exceeds the time limit
        //     if (stopwatch.Elapsed > timeLimit)
        //     {
        //         Console.WriteLine("********** Skipping ***********");
        //         return; // Return if the time limit is exceeded
        //     }

        //     while (child != null)
        //     {
        //         TraverseElementWithTreeWalker(child, elements, treeWalker, stopwatch, timeLimit); // Recursive call to traverse child elements
        //         child = treeWalker.GetNextSibling(child);
        //     }
        // }

        static void _FindWindow(IntPtr handle)
        {

            IntPtr calculatorHandle = FindWindowEx(handle, IntPtr.Zero, "Windows.UI.Core.CoreWindow", null);

            if (calculatorHandle != IntPtr.Zero)
            {
                if (!GetWindowRect(calculatorHandle, out windowRect))
                {
                    Console.WriteLine($"Window position: ( l:{windowRect.Left}\n, T: {windowRect.Top})\n,R: {windowRect.Right}\n B:{windowRect.Bottom}");

                }
                else
                {
                    Console.WriteLine("Failed to get window rectangle");
                }
            }
        }

        // Method to capture visible elements using UI Automation with help of FindAll
        static string CaptureVisibleElements_(AutomationElement window)
        {
            try
            {
                List<Element_info> elements = new List<Element_info>();
                // give the condtition on which Element should be captured ...
                Condition condition = new AndCondition(
                    new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                    new PropertyCondition(AutomationElement.IsOffscreenProperty, false),
                    new PropertyCondition(AutomationElement.IsControlElementProperty, true),
                    new PropertyCondition(AutomationElement.IsContentElementProperty, true)
                );
                Stopwatch method = new Stopwatch();
                method.Start();
                AutomationElementCollection Descendants = window.FindAll(TreeScope.Descendants, condition);
                method.Stop();
                double elapsedSeconds = method.Elapsed.TotalSeconds;
                long elapsedTime = method.ElapsedMilliseconds;
                Console.WriteLine($"Elapsed time: {elapsedSeconds} seconds for *** ELEMENT *** in milliseconds: {elapsedTime}");
                System.Console.WriteLine("this is no. of children : " + Descendants.Count);
                Console.WriteLine("\n ------ Done rendering --------\n");

                foreach (AutomationElement child in Descendants)
                {
                    TraverseElement(child, elements);
                }
                return JsonConvert.SerializeObject(elements, Formatting.Indented);
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("faild while capturing element rendering/element information ...");
                return "";
            }
        }

        static void TraverseElement(AutomationElement element, List<Element_info> elements)
        {
            System.Windows.Rect elementRect = element.Current.BoundingRectangle;

            if (IsElementWithinWindow(elementRect))
            {
                Element_info elementInfo = new Element_info
                {
                    Element_Name = element.Current.Name,
                    Element_ControlType = element.Current.LocalizedControlType,
                    left = (double)(elementRect.Left - windowRect.Left),
                    top = (double)(elementRect.Top - windowRect.Top),
                    width = (double)elementRect.Width,
                    height = (double)elementRect.Height
                };
                elements.Add(elementInfo);
            }

        }

        // Method to capture the screenshot and UI elements
        public static Bitmap TakeScreenshot(int processId)
        {
            
            string jsonElements;
           
            
           
            Process activeProcess = new Process();
            SetForegroundWindow(activeProcess.MainWindowHandle);
            IntPtr hWnd = GetForegroundWindow();
            System.Console.WriteLine("this is hndle : " + hWnd);
            List<string> browserProcessNames = new List<string> {
            "chrome",
            "firefox",
            "edge",
            "opera",
            "safari",
            // Add other browser process names here if needed
        };

            // DetectApplicationType is Application is web-browser or Not ....
            if (browserProcessNames.Contains(activeProcess.ProcessName.ToLower()))
            {
                MessageBox.Show("why don't we skip for now :");
            }

           
            window_width = windowRect.Right - windowRect.Left;
            window_height = windowRect.Bottom - windowRect.Top;

            Console.WriteLine($"Process ID: {processId} ");

            // funtion for capturing Image ...
            Bitmap source = CaptureWindow(window_width, window_height, ImageFormat.Png);
            return source;

            // Capture the UI elements using UI Automation
            AutomationElement targetWindow = AutomationElement.FromHandle(hWnd);

            // Console.WriteLine("****** method ...........1 treeWalker *****");
            // jsonElements = CaptureVisibleElements(targetWindow, elements);
            // string jsonFilePath_1 = Path.Combine(logFileDirectory, $"{FileName}.json");
            // File.WriteAllText(jsonFilePath_1, jsonElements);
            // GeneratedMessage(activeProcess, jsonFilePath_1, FileName, false);


            //Console.WriteLine("****** method ...........2 scope find all *****");
          /*  jsonElements =  CaptureVisibleElements_(targetWindow);
            if (jsonElements == "")
            {
                return;
            }
            File.WriteAllText(jsonFilePath, jsonElements);*/
        }
      
    }
}