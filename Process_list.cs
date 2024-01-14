using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Spy
{
    internal class Process_list
    {
        public static void process_list(ObservableCollection<object> p_Entries)
        {

            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle)  && !string.IsNullOrEmpty(process.ProcessName))
                {
                    Console.WriteLine($"{process.MainWindowTitle} (PID: {process.Id})");
                    //p_Entries.Add($"{process.ProcessName}\t{process.MainWindowTitle}\t {process.Id}");
                    //string process_name = process.ProcessName;

                    p_Entries.Add(new p_Entries() { ProcessName = process.ProcessName, MainWindowTitle = process.MainWindowTitle, Id = process.Id });
                }
            }
        }



    }
}
