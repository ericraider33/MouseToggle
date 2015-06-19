using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using MouseToggle.Properties;

namespace MouseToggle
{
    public class MouseApp : ApplicationContext
    {
        private const String SWAP_KEY = "SwapMouseButtons";

        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private bool leftHanded;
        private DateTime lastClick = DateTime.MinValue;

        public MouseApp()
        {
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", Resources.exit, handleExitClick);            

            leftHanded = readLeft();

            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "Toggle mouse button";
            notifyIcon.Icon = leftHanded ? Resources.mouse_right : Resources.mouse_left;
            notifyIcon.MouseClick += handleMouseClick;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void handleMouseClick(object sender, MouseEventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan lastSpan = now - lastClick;

            toggle();

            if (lastSpan < TimeSpan.FromSeconds(0.5))
            {
                // Shows context menu
                MethodInfo showContext = notifyIcon.GetType().GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                showContext.Invoke(notifyIcon, null);

                lastClick = DateTime.MinValue;
            }
            else
            {
                // Makes sure that single click doesn't show context menu
                if (contextMenu.Visible)
                    contextMenu.Visible = false;

                lastClick = now;
            }
        }

        private void toggle()
        {
            leftHanded = !leftHanded;
            notifyIcon.Icon = leftHanded ? Resources.mouse_right : Resources.mouse_left;
            SwapMouseButton(leftHanded);
        }

        private void handleExitClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private bool readLeft()
        {
            using (RegistryKey mouseRegistryKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse", true))
            {
                String value = mouseRegistryKey.GetValue(SWAP_KEY) as String;
                return value == "1";
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SwapMouseButton([param: MarshalAs(UnmanagedType.Bool)] bool fSwap);

    }
}
