﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Management;
using System.Drawing;
using PopCom.Properties;

/**********************************Simple Tray Icon sample DOTNET 2.0***********************************************
 * This class creates the notification icon that dotnet 2.0 offers.
 * It will be displaying the status of the application with appropiate icons.
 * It will have a contextmenu that enables the user to open the form or exit the application.
 * The form could be used to change settings of the app which in turn are saved in the app.config or some other file.
 * This formless, useless, notification sample does only chane the icon and balloontext.
 * NOTE:Chacker is a Singleton class so it will only allow to be instantiated once, and therefore only one instance.
 * I have done this to prevent more then one icon on the tray and to share data with the form (if any)
 *
 ******************************************************************************************************************/

namespace PopCom
{
    class Popper : IDisposable
    {
        //Popper is a singleton
        private static readonly Popper popper = new Popper();

        //notify icon: prepare the icons we may use in the notification
        private NotifyIcon notify;
        private ContextMenu contextMenu = new ContextMenu();
        private MenuItem enablePlugInMenuItem;
        private MenuItem enablePlugOutMenuItem;

        // Events enabled
        bool enablePlugIn = true;
        bool enablePlugOut = true;

        public static Popper GetInstance()
        {
            return popper;
        }

        private Popper() //singleton so private constructor!
        {
            // Create menu
            enablePlugInMenuItem = new MenuItem("Plug-in Notifications", togglePlugInNotif);
            enablePlugOutMenuItem = new MenuItem("Plug-out Notifications", togglePlugOutNotif);
            contextMenu.MenuItems.Add(enablePlugInMenuItem);
            contextMenu.MenuItems.Add(enablePlugOutMenuItem);
            enablePlugInMenuItem.Checked = true;
            //add a exit submenu item
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add(new MenuItem("Exit", new EventHandler(Menu_OnExit)));

            // notifyicon
            notify = new NotifyIcon();
            notify.Icon = PopCom.popcom;
            notify.Text = "PopCom Plug-in notifier";
            notify.ContextMenu = contextMenu;
            notify.Visible = true;

            enablePlugInNotif = true;
            enablePlugOutNotif = false;


            // Create a WMI query for device insertion
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler((object sender, EventArrivedEventArgs args) => DeviceEvent(sender, args, true));
            insertWatcher.Start();

            // Create a WMI query for device removal
            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");

            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler((object sender, EventArrivedEventArgs args) => DeviceEvent(sender, args, false));
            removeWatcher.Start();

        }

        private void DeviceEvent(object sender, EventArrivedEventArgs e, bool insertion)
        {
            if ((insertion && enablePlugIn) || (!insertion && enablePlugOut))
            {
                ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

                var guid = instance["ClassGuid"];
                if (guid != null && guid.ToString() == "{4d36e978-e325-11ce-bfc1-08002be10318}")
                {
                    notify.ShowBalloonTip(20000,
                        insertion ? "Port Plugged in" : "Port plugged out",
                        instance["Caption"].ToString(), ToolTipIcon.Info);
                }
            }
        }

        ~Popper()
        {
            Dispose();
        }

        public void Dispose()
        {
        }

        void Menu_OnExit(Object sender, EventArgs e)
        {
            //be sure to call Application.Exit
            Dispose();
            Application.Exit();
        }

        public bool enablePlugInNotif
        {
            get
            {
                return enablePlugIn;
            }
            set
            {
                enablePlugIn = value;
                enablePlugInMenuItem.Checked = value;
            }
        }

        public bool enablePlugOutNotif
        {
            get
            {
                return enablePlugOut;
            }
            set
            {
                enablePlugOut = value;
                enablePlugOutMenuItem.Checked = value;
            }
        }

        private void togglePlugInNotif(object sender, EventArgs args)
        {
            enablePlugInNotif = !enablePlugInNotif;
        }

        private void togglePlugOutNotif(object sender, EventArgs args)
        {
            enablePlugOutNotif = !enablePlugOutNotif;
        }

    }
}
