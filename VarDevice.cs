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
using System.Threading;

namespace EmbeddedAutomation.mServer.Adapters
{
    using EmbeddedAutomation.mServer.Api;

    public class VarDevice : AbstractDevice
    {
        public override string PluginName { get { return "Var Device"; } }
        public override Version PluginVersion { get { return new Version(3, 0, 0); } }

        private AutoResetEvent _evRespRcvd; // response received
        protected int currentState;

        public VarDevice(AbstractAdapterDriver mngr, IKnownModule module) : base(mngr, module)
        {
        }

        public override void UninitDevice()
        {
            SrvUtil.DisposeWaitHandle(ref _evRespRcvd);
        }

        public override void InitDevice()
        {
            string tmpState = ((StateDeviceMgr)this.AdapterDriver).stateData.getSavedState(this.Address);
            if (tmpState != "")
                currentState = int.Parse(tmpState);

            tellServer("Current_State", currentState.ToString());
            base.CurrentStatus = CurrentStatusAsString(); // this will announce to the clients;
            _evRespRcvd = new AutoResetEvent(false);
        }

        protected bool getState()
        {
            return (currentState != 0);
        }

        public void toggle()
        {
            updateState(!getState());
        }

        public void turnOn()
        {
            updateState(true);
        }

        public void turnOff()
        {
            updateState(false);
        }

        public void increment()
        {
            int newValue = currentState + 1;
            updateState(newValue);
        }

        public void decrement()
        {
            int newValue = currentState - 1;
            if (newValue < 0)
            {
                newValue = 0;
            }
            updateState(newValue);
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
            ((StateDeviceMgr)this.AdapterDriver).stateData.saveState(this.Address, newValue.ToString());
            tellServer("Change_State", newValue.ToString());
            tellServer("Current_State", newValue.ToString());
            base.CurrentStatus = CurrentStatusAsString(); // this will announce to the clients;
        }

        private void tellServer(string message, string state)
        {
            DeviceCommandReceived(new DeviceChangeEvent(message, state));
        }

        public string CurrentStatusAsString()
        {
            string currentStateBoolAsString = (currentState == 0) ? "False" : "True";
            return String.Format("{0} ({1:00#})", currentStateBoolAsString, currentState);
        }
    }
}