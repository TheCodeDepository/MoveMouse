using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoveMouse
{
    public partial class KeepUnlocked : Form
    {


        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(String sClassname, String sAppname);

        internal class NativeMethods
        {
            // Import SetThreadExecutionState Win32 API and necessary flags
            [DllImport("kernel32.dll")]
            public static extern uint SetThreadExecutionState(uint esFlags);
            public const uint ES_CONTINUOUS = 0x80000000;
            public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        }

       

        public enum fsModifiers
        {
            alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Window = 0x0008
        }

        private IntPtr thisWindow;
        private uint fPreviousExecutionState;

        private void KeepUnlocked_Load(object sender, EventArgs e)
        {
            thisWindow = FindWindow(null, "KeepUnlocked");
            RegisterHotKey(thisWindow, 1, (uint)fsModifiers.alt, (uint)Keys.T);
        }


        public KeepUnlocked()
        {
            InitializeComponent();

            // Set new state to prevent system sleep
            fPreviousExecutionState = NativeMethods.SetThreadExecutionState(
                NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);
            if (fPreviousExecutionState == 0)
            {
                MessageBox.Show("Failed");
                Console.WriteLine("SetThreadExecutionState failed. Do something here...");
                Close();
            }

        }





        private void Start_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Timer.Interval = (int)(1000 * numericUpDown1.Value);
            Timer.Tick += new EventHandler(MoveCursor);
            Timer.Start();

        }
        bool tf = true;

        private void MoveCursor(object sender, EventArgs e)
        {
            tf = !tf;
            this.Cursor = new Cursor(Cursor.Current.Handle);

            if (tf)
            {
                Cursor.Position = new Point(Cursor.Position.X - 5, Cursor.Position.Y - 5);
            }
            else
            {
                Cursor.Position = new Point(Cursor.Position.X + 5, Cursor.Position.Y + 5);
            }

        }

        private void KeepUnlocked_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnregisterHotKey(thisWindow, 1);
        }

        protected override void WndProc(ref Message m)
        {
            if(m.Msg == 0x0312)
            {
                stop();
            }
            base.WndProc(ref m);
        }

        private void stop()
        {
            Timer.Stop();
            this.Visible = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (NativeMethods.SetThreadExecutionState(fPreviousExecutionState) == 0)
            {
                MessageBox.Show("Failed to restore state");
            }
        }
    }
}
