﻿using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Diagnostics;
using System;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;
using System.Drawing.Drawing2D;
using static Spy.MainWindow;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;


namespace Spy;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    Bitmap? screenshot ;

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_SHOWMINNOACTIVE = 7;

    private const int SW_MAXIMIZE = 3;

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }


    public MainWindow()
    {
        DataContext = this;
        p_Entries = new ObservableCollection<object>();
        Process_list obj = new Process_list();
        obj.process_list(p_Entries);
        // TakeScreenshot.process_list(p_Entries);
        InitializeComponent();
    }

    public ObservableCollection<object> p_Entries;

    public ObservableCollection<object> P_Entries
    {
        get { return p_Entries; }
        set { p_Entries = value; }
        // pname.ItemSource = p_Entries;
    }


    private void Hndl_Save(object sender, RoutedEventArgs e)
    {
        // Check if the image source is set
        if ( screenshot != null)
        {
            // Save the transparent bitmap to a file (PNG supports transparency)
            if ("please select the Path" != Folder.Text)
            {
                screenshot.Save($"{Folder.Text}\\{DateTime.Now.ToString("yyyy/MM/dd hh-mm-ss")}.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            else
            {
                screenshot.Save($"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Saved Pictures\\{DateTime.Now.ToString("yyyy/MM/dd \n hh-mm-ss")}.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            System.Windows.MessageBox.Show("Image saved successfully! ");
            ScreenShot.Source = null;


        }
        else
        {
            System.Windows.MessageBox.Show("Please make sure you captured the Image first");
        }
    }

    private void Hndl_CaptureScreen(object sender, RoutedEventArgs e)
    {
        if (ProcessList.SelectedItem != null)
        {
            int processId = ((p_Entries)ProcessList.SelectedItem).Id;

            Process process = Process.GetProcessById(processId);

            if (process.MainWindowHandle != IntPtr.Zero)
            {
                IntPtr hWnd = process.MainWindowHandle;
                RECT windowRect;

                ShowWindow(hWnd, SW_MAXIMIZE);
                Thread.Sleep(1000);
                SetForegroundWindow(hWnd);
                GetWindowRect(hWnd, out windowRect);

                int borderWidth = windowRect.Right - windowRect.Left;
                int borderHeight = windowRect.Bottom - windowRect.Top;

                // Capture screenshot
                screenshot = new Bitmap(borderWidth, borderHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (Graphics graphics = Graphics.FromImage(screenshot))
                {
                    graphics.CopyFromScreen(windowRect.Left, windowRect.Top, 0, 0, new System.Drawing.Size(borderWidth, borderHeight), CopyPixelOperation.SourceCopy);
                }

                ScreenShot.Source = ConvertBitmapToImageSource(screenshot);
               
                // Send the window to the back
                ShowWindow(hWnd, SW_SHOWMINNOACTIVE);
            }
        }
    }

    private BitmapImage ConvertBitmapToImageSource(Bitmap bitmap)
    {
        using (MemoryStream memory = new MemoryStream())
        {
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

    }

    private void Hndl_Highlight_win(object sender, RoutedEventArgs e)
    {
        // Assuming each item in the ListView has an 'id' property
        int processId = ((p_Entries)ProcessList.SelectedItem).Id;


        Process process = Process.GetProcessById(processId);

        if (ProcessList.SelectedItem != null)
        {
            if (process != null)
            {
                IntPtr hWnd = process.MainWindowHandle;
                ShowWindow(hWnd, SW_MAXIMIZE);
                Thread.Sleep(500);
                SetForegroundWindow(hWnd);

                RECT windowRect;


                GetWindowRect(hWnd, out windowRect);
                int Width = windowRect.Right - windowRect.Left;
                int Height = windowRect.Bottom - windowRect.Top;
                using (Graphics screenGraphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Red, 4.0F);
                    myPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    System.Drawing.Rectangle theRectangle = new System.Drawing.Rectangle(windowRect.Left, windowRect.Top, Width, Height);

                    screenGraphics.DrawRectangle(myPen, theRectangle);

                    // Delay for 1 seconds
                    Thread.Sleep(1000);

                    // Redraw the rectangle with a transparent pen to make it vanish
                    myPen.Color = System.Drawing.Color.Transparent;
                    screenGraphics.DrawRectangle(myPen, theRectangle);

                    screenGraphics.Dispose();
                    myPen.Dispose();
                    Thread.Sleep(1000);
                    ShowWindow(hWnd, SW_SHOWMINNOACTIVE);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Sorry couldn't able to find the right Process");
                Console.WriteLine("Process not found.");
            }
        }


    }
    private void OpenDirectoryDialog()
    {
        using (var folderDialog = new FolderBrowserDialog())
        {
            folderDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            DialogResult result = folderDialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
            {
                // Use the selected folder path
                string selectedPath = folderDialog.SelectedPath;
                Folder.Text = selectedPath;
            }
        }
    }

    private void Hndl_SetDirectoryDialog(object sender, RoutedEventArgs e)
    {
        OpenDirectoryDialog();
    }

}

public class p_Entries
{
    public String? ProcessName { get; set; }
    public String? MainWindowTitle { get; set; }
    public int Id { get; set; }
}