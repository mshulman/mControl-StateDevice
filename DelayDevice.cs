//========================================================================================
/// <copyright from='2005' to='2008' company='Embedded Automation, Inc.'>
///
///  mHome Automation Server
///
///  Copyright (c) Embedded Automation, Inc.  All rights reserved.
///  Copyright (c) Gert-Jan Niewenhuijse.  All rights reserved.
///
/// </copyright>
//
//  $Id$
//
//========================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Net;
using System.Text;
using System.Threading;

namespace EmbeddedAutomation.mServer.Adapters
{
    using EmbeddedAutomation.mHome.Api;
    using EmbeddedAutomation.mServer.Api;
    using System.Xml;
    using System.Xml.XPath;

    public class DelayDevice : AbstractDevice
    {
        public override string PluginName { get { return "Delay Device"; } }
        public override Version PluginVersion { get { return new Version(1, 0, 0); } }

        private AutoResetEvent _evRespRcvd; // response received
        protected int currentState;
        protected string interval = "0";
        static System.Timers.Timer _timer;


        public DelayDevice(AbstractAdapterDriver mngr, IKnownModule module) : base(mngr, module)
        {
        }

        public override void UninitDevice()
        {
            SrvUtil.DisposeWaitHandle(ref _evRespRcvd);
        }

        public override void InitDevice()
        {
            string options = this.Options;
            if (string.IsNullOrEmpty(options))
            {
                throw new InvalidDataException(String.Format("{0}: error - Options string is empty", this.Adapter));
            }
            else
            {
                string[] parts = options.Split('|');

                // interval
                if (parts.Length > 0)
                {
                    interval = parts[0];
                }  
            }
            

            _evRespRcvd = new AutoResetEvent(false);
        }

        protected bool getState()
        {
            return (currentState != 0);
        }

        public void start()
        {
            _timer = new System.Timers.Timer();
            _timer.Interval = (int.Parse(interval) * 1000);
            _timer.AutoReset = true;
            _timer.Start();
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true;            // Enable it
            
            updateState(true);            
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender == _timer)
            {
                _timer.Stop();
                
                updateState(2);
            }
        }

        public void stop()
        {
            _timer.Stop();
            
            updateState(false);
        }

        private void updateState(bool newState)
        {
            int newValue;
            if (newState == true)
            {
                newValue = 1;
            }
            else
            {
                newValue = 0;
            }
            updateState(newValue);
        }

        private void updateState(int newValue)
        {
            currentState = newValue;
            tellServer("Current_State", newValue.ToString());
            base.CurrentStatus = CurrentStatusAsString(); // this will announce to the clients;
        }

        private void tellServer(string message, string state)
        {
            DeviceCommandReceived(new DeviceChangeEvent(message, state));
        }

        public string CurrentStatusAsString()
        {
            string currentStateAsString = "";
            if (currentState == 0)
            {
                currentStateAsString = "Stopped";
            }
            else if (currentState == 1)
            {
                currentStateAsString = String.Format("Running ({0})", interval);
            }
            else if (currentState == 2)
            {
                currentStateAsString = "Elapsed";
            }
            return currentStateAsString;
        }
    }
}
