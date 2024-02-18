using System.Collections.ObjectModel;
using System.Diagnostics;


namespace Spy
{
    internal class Process_list
    {
        public static void process_list(ObservableCollection<p_Entries> p_Entries)
        {
            var processes = Process.GetProcesses();
            p_Entries.Clear();
            foreach (var process in processes)
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && (process.ProcessName != "Spy") )
                {
                    p_Entries.Add(new p_Entries
                    {
                        ProcessName = process.ProcessName,
                        MainWindowTitle = process.MainWindowTitle,
                        Id = process.Id
                        // Add more properties as needed
                    });
                }
            }
        }



    }
}
