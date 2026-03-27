using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SincroPelis
{
    // Minimal AxHost wrapper to host Windows Media Player ActiveX control at runtime
    public class WindowsMediaPlayerHost : AxHost
    {
        public WindowsMediaPlayerHost() : base("6BF52A52-394A-11D3-B153-00C04F79FAA6")
        {
        }

        public object? Ocx
        {
            get
            {
                try { return this.GetOcx(); } catch { return null; }
            }
        }
    }
}
