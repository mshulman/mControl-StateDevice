//========================================================================================
/// <copyright from='2005' to='2008' company='Embedded Automation, Inc.'>
///
///  mHome Automation Server
///
///  Copyright (c) Embedded Automation, Inc.  All rights reserved.
///
/// </copyright>
//
//  $Id: WebServiceManager.cs,v 1.2 2008/03/18 17:42:13 george Exp $
//
//========================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace EmbeddedAutomation.mServer.Adapters
{
    using EmbeddedAutomation.mHome.Api;
    using EmbeddedAutomation.mServer.Api;

    [NameAndVersion("STATEDEVICE", "1.0.0")]
    [SupportedProtocols("PROT_STATEDEVICE")]

    /// <summary>
    /// This is a class that manages all aspects of
    /// WebService data source and feeds devices
    /// </summary>
    public sealed class StateDeviceMgr : AbstractAdapterDriver
    {
        private const string _driverProt = "PROT_STATEDEVICE";

        public StateData stateData = new StateData();

        public override AbstractDevice CreateDeviceForModule(IKnownModule devModule)
        {
            if (devModule.BaseName.Equals("VARDEVICEMODULE"))
            {
                return new VarDevice(this, devModule);
            }
            else if (devModule.BaseName.Equals("DATEDEVICEMODULE"))
            {
                return new DateDevice(this, devModule);
            }
            else if (devModule.BaseName.Equals("DELAYDEVICEMODULE"))
            {
                return new DelayDevice(this, devModule);
            }
            else
            {
                throw new InvalidDataException(String.Format(
                    "{0}: error - unknown device requested", _driverProt));
            }
        }

        public override void DeviceAdded(AbstractDevice devrec)
        {
            if ( (devrec is VarDevice) | (devrec is DateDevice) | (devrec is DelayDevice) )
            {
                devrec.InitDevice();
            }
        }

        public override void DeviceRemoved(AbstractDevice devrec)
        {
            if ((devrec is VarDevice) | (devrec is DateDevice) | (devrec is DelayDevice))
            {
                devrec.UninitDevice();
            }
        }

        protected override void InternalShutdown()
        {
            StopMyDevices();
        }

        protected override void InternalStartup()
        {
            lock (this)
            {
                State = EAdapterState.Initializing;
                try
                {
                    stateData.FileName = ApiUtil.GetAssemblyPath(System.Reflection.Assembly.GetExecutingAssembly()) + ".xml";
                    stateData.ModuleName = "StateDevice";
                    InitMyDevices();
                    State = EAdapterState.Initialized;
                }
                catch (Exception ex)
                {
                    ApiUtil.LogException(ex, "{0} Initialization Error!", PluginName);
                    State = EAdapterState.NotInitialized;
                }
            }
        }

        /// <summary>
        /// Initialize local devices instances of each WebService 
        /// device found in mServer
        /// </summary>
        private void InitMyDevices()
        {
            foreach (AbstractDevice devrec in base.GetMyDevicesList())
            {
                if (DebugLevel >= 8)
                    ApiUtil.LogDebug("No options available");
                
                devrec.InitDevice();
            }
        }

        private void StopMyDevices()
        {
            foreach (AbstractDevice devrec in GetMyDevicesList())
            {
                if ( (devrec is VarDevice) | (devrec is DateDevice) | (devrec is DelayDevice) )
                {
                    devrec.UninitDevice();
                }
            }
        }

        public void writeLog(string iMessage)
        {
            if (DebugLevel >= 8)
            {
                {
                    ApiUtil.LogDebug(iMessage);
                }
            }
        }

        protected override bool SendCommand(AbstractDevice devrec, string command, string[] cmdParams)
        {
            writeLog("Command: " + command);
            foreach (string param in cmdParams)
            {
                writeLog("Param: " + param);
            }

            if (
                ( (devrec is VarDevice) | (devrec is DateDevice) | (devrec is DelayDevice) ) && 
                devrec.Adapter == PluginName &&
                devrec.DeviceModule.ModuleClass == EModuleClass.UNDEFINED
               )
            {
                if (devrec is DateDevice)
                {
                    DateDevice device = devrec as DateDevice;
                    switch (command)
                    {
                        case "CURRENT":
                            device.tellServerDate();
                            break;

                        default:
                            break;
                    }
                }
                else if (devrec is VarDevice)
                {
                    VarDevice device = devrec as VarDevice;
                    switch (command)
                    {
                        case "TOGGLE":
                            device.toggle();
                            break;

                        case "TURNON":
                            device.turnOn();
                            break;

                        case "TURNOFF":
                        case "SETZERO":
                            device.turnOff();
                            break;

                        case "INCREMENT":
                            device.increment();
                            break;

                        case "DECREMENT":
                            device.decrement();
                            break;

                        default:
                            break;
                    }
                }
                else if (devrec is DelayDevice)
                {
                    DelayDevice device = devrec as DelayDevice;
                    switch (command)
                    {
                        case "START":
                            device.start();
                            break;

                        case "STOP":
                            device.stop();
                            break;

                        default:
                            break;
                    }
                }
            }
            return false;
        }
    }
}
