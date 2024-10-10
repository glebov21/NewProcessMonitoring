using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace NewProcessMonitoring
{
    internal class MinimizeToTray: IDisposable
    {
        private Window _window;
        private NotifyIcon _notifyIcon;
        //private bool _balloonShown;

        /// <summary>
        /// Initializes a new instance of the MinimizeToTray class.
        /// </summary>
        /// <param name="window">Window instance to attach to.</param>
        public MinimizeToTray(Window window)
        {
            Debug.Assert(window != null, "window parameter is null.");
            _window = window;
            _window.StateChanged += HandleStateChanged;
        }

        /// <summary>
        /// Handles the Window's StateChanged event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleStateChanged(object sender, EventArgs e)
        {
            if (_notifyIcon == null)
            {
                // Initialize NotifyIcon instance "on demand"
                var contextMenu = new System.Windows.Forms.ContextMenu();
                var menuItem_Exit = new System.Windows.Forms.MenuItem();
                contextMenu.MenuItems.AddRange(
                new System.Windows.Forms.MenuItem[] { menuItem_Exit });
                menuItem_Exit.Index = 0;
                menuItem_Exit.Text = "E&xit";
                menuItem_Exit.Click += (_sender, _e) => System.Windows.Application.Current.Shutdown();


                _notifyIcon = new NotifyIcon();
                _notifyIcon.ContextMenu = contextMenu;
                _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                _notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconOrBalloonClicked);
                _notifyIcon.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
            }
            // Update copy of Window Title in case it has changed
            _notifyIcon.Text = _window.Title;

            // Show/hide Window and NotifyIcon
            var minimized = (_window.WindowState == WindowState.Minimized);
            _window.ShowInTaskbar = !minimized;
            _notifyIcon.Visible = minimized;
            //if (minimized && !_balloonShown)
            //{
            //    // If this is the first time minimizing to the tray, show the user what happened
            //    _notifyIcon.ShowBalloonTip(1000, null, _window.Title, ToolTipIcon.None);
            //    _balloonShown = true;
            //}
        }

        /// <summary>
        /// Handles a click on the notify icon or its balloon.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
        {
            // Restore the Window
            _window.WindowState = WindowState.Normal;
            _window.Activate();
        }



        bool disposed = false;
        ~MinimizeToTray()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            _notifyIcon.Visible = false;
            _notifyIcon.Icon = null;
            _notifyIcon.Dispose();
            _notifyIcon = null;

            disposed = true;
        }
    }
}
