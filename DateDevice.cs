//========================================================================================
/// <copyright from='2005' to='2008' company='Embedded Automation, Inc.'>
///
///  mHome Automation Server
///
///  Copyright (c) Embedded Automation, Inc.  All rights reserved.
///  Copyright (c) Michael Shulman.  All rights reserved.
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

    public class DateDevice : AbstractDevice
    {
        public override string PluginName { get { return "Date Device"; } }
        public override Version PluginVersion { get { return new Version(1, 0, 0); } }

        private AutoResetEvent _evRespRcvd; // response received
           
        protected DateTime currentDate;

        public DateDevice(AbstractAdapterDriver mngr, IKnownModule module) : base(mngr, module)
        {
        }

        public override void UninitDevice()
        {
            SrvUtil.DisposeWaitHandle(ref _evRespRcvd);
        }

        public override void InitDevice()
        {
            tellServerDate();
            _evRespRcvd = new AutoResetEvent(false);
            
        }

        public void tellServerDate()
        {
            currentDate = DateTime.Now;

            tellServer("Year", currentDate.Year.ToString());
            tellServer("Month", currentDate.Month.ToString());
            tellServer("Day", currentDate.Day.ToString());
            tellServer("DayOfWeekNum", Convert.ToInt32(currentDate.DayOfWeek).ToString()); // sunday = 0
            tellServer("DateIsOdd", Convert.ToString(((currentDate.Day & 1) == 1)).ToUpper());

            base.CurrentStatus = CurrentStatusAsString(); // this will announce to the clients;
        }


        public void processMidnight()
        {
            tellServerDate();
        }

        private void tellServer(string message, string state)
        {
            DeviceCommandReceived(new DeviceChangeEvent(message, state));
        }


        public string CurrentStatusAsString()
        {
            return currentDate.ToShortDateString() +" "+ currentDate.ToShortTimeString();
        }
    }
}
