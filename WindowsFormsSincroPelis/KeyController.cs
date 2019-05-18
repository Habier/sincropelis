using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SincroPelis
{
    static class KeyController
    {
        const UInt32 WM_KEYDOWN = 0x0100;
        const int VK_F5 = 0x20;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        public static void SendKey2Process(string name)
        {
            Process[] processes = Process.GetProcessesByName(name);

            foreach (Process proc in processes)
                PostMessage(proc.MainWindowHandle, WM_KEYDOWN, VK_F5, 0);

        }

        public static void logThis(string msg)
        {
            Console.WriteLine(msg);
            Program.myForm.SendDebug(msg);

        }
    }
}
