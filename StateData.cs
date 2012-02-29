using System;
using System.Xml;
using System.Xml.XPath;
using System.IO;

public class StateData
{
    #region Variables

    private string _fname = String.Empty;
    private string _sname = String.Empty;

    #endregion


    #region Functions

    public string FileName
    {
        get { return _fname; }
        set { _fname = value; }
    }

    public string ModuleName
    {
        get { return _sname; }
        set { _sname = value; }
    }

    public string getSavedState(string deviceName)
    {
        string retval = string.Empty;

        try
        {
            XmlDocument x = new XmlDocument();
            XPathNavigator xpn = x.CreateNavigator();

            if (File.Exists(_fname) & (_sname != String.Empty))
            {
                x.Load(_fname);
                string xpath = String.Format("/{0}/device[@name='{1}']/currentState", _sname, deviceName);
                XPathNavigator currentStateNode = xpn.SelectSingleNode(xpath);
                if (currentStateNode != null)
                {
                    retval = currentStateNode.InnerXml;
                }
            }
        }
        catch { }

        return retval;
    }

    //public void saveState(string deviceName, string[] state, DateTime[] stateDate, int iPacketSize)
    //{
    //    saveState(deviceName, ArraytoString(state, stateDate, iPacketSize) );
    //}

    public void saveState(string deviceName, string deviceState)
    {
        if (_sname != String.Empty)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XPathNavigator xpn = xmlDoc.CreateNavigator();

                if (!File.Exists(_fname))
                {
                    CreateXmlDoc(xmlDoc, xpn, deviceName);
                }
                else
                {
                    xmlDoc.Load(_fname);
                }
                string xpath = String.Format("/{0}/device[@name='{1}']/currentState", _sname, deviceName);
                XPathNavigator currentStateNode = xpn.SelectSingleNode(xpath);

                if (currentStateNode == null)
                {
                    AddDeviceToXmlDoc(xmlDoc, xpn, deviceName);
                    currentStateNode = xpn.SelectSingleNode(xpath);
                }
                currentStateNode.SetValue(deviceState);
                xmlDoc.Save(_fname);
            }
            catch { }
        }
    }

    private static string ArraytoString(string[] state, DateTime[] stateDate, int iPacketSize)
    {

        string tmpString = string.Empty;
        for (int i = 0; i < iPacketSize; i++)
        {
            tmpString += state[i] + "|" + stateDate[i] + "~";
        }
        return tmpString.Trim('~').Trim();
    }

    private void AddDeviceToXmlDoc(XmlDocument xmlDoc, XPathNavigator xpn, string deviceName)
    {
        XmlNode root = xmlDoc.DocumentElement;

        // create the device (by name)
        XmlElement deviceNode = xmlDoc.CreateElement("device");
        deviceNode.SetAttribute("name", deviceName);
        root.AppendChild(deviceNode);

        XmlElement currentStateNode = xmlDoc.CreateElement("currentState");
        //Add the node to the document.
        deviceNode.AppendChild(currentStateNode);
    }


    private void CreateXmlDoc(XmlDocument x, XPathNavigator xpn, string deviceName)
    {
        //Create a new root
        XmlElement rootElement = x.CreateElement(_sname);
        x.AppendChild(rootElement);

        AddDeviceToXmlDoc(x, xpn, deviceName);
    }



    #endregion
}