using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatchManager
{
    public class RichTextBoxRedrawHandler
    {
        RichTextBox rtb;

        public RichTextBoxRedrawHandler(RichTextBox _rtb)
        {
            rtb = _rtb;
        }
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, ref Point lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, IntPtr lParam);

        const int WM_USER = 1024;
        const int WM_SETREDRAW = 11;
        const int EM_GETEVENTMASK = WM_USER + 59;
        const int EM_SETEVENTMASK = WM_USER + 69;
        const int EM_GETSCROLLPOS = WM_USER + 221;
        const int EM_SETSCROLLPOS = WM_USER + 222;

        private Point _ScrollPoint;
        private bool _Painting = true;
        private IntPtr _EventMask;
        private int _SuspendIndex = 0;
        private int _SuspendLength = 0;

        public void SuspendPainting()
        {
            if (_Painting)
            {
                _SuspendIndex = rtb.SelectionStart;
                _SuspendLength = rtb.SelectionLength;
                SendMessage(rtb.Handle, EM_GETSCROLLPOS, 0, ref _ScrollPoint);
                SendMessage(rtb.Handle, WM_SETREDRAW, 0, IntPtr.Zero);
                _EventMask = SendMessage(rtb.Handle, EM_GETEVENTMASK, 0, IntPtr.Zero);
                _Painting = false;
            }
        }

        public void ResumePainting()
        {
            if (!_Painting)
            {
                rtb.Select(_SuspendIndex, _SuspendLength);
                SendMessage(rtb.Handle, EM_SETSCROLLPOS, 0, ref _ScrollPoint);
                SendMessage(rtb.Handle, EM_SETEVENTMASK, 0, _EventMask);
                SendMessage(rtb.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
                _Painting = true;
                rtb.Invalidate();
            }

        }
    }
}
