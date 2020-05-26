using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace TplGui
{
    public static class ColorUtil
    {
        public static System.Drawing.Brush RGB(byte r, byte g, byte b, byte a = 255)
        {
            return new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(a, r, g, b));
        }
    }
}
