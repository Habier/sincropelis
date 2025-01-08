using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SincroPelis
{
    class PlayerFinder
    {

        private static string[] videoPlayerNames = { "vlc", "mpc-hc64", };
        // "wmplayer"  uses ctrl+P

        public static string find()
        {
            foreach (string name in videoPlayerNames)
            {
                Process[] processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                {
                    return name;

                }
            }

            return "";
        }



    }
}
