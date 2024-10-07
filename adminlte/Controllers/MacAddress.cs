using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;

namespace E658.Controllers
{
    public class MacAddress
    {
        public string GetMacAddress()
        {
            string macAddresses = string.Empty;

            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        macAddresses += nic.GetPhysicalAddress().ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return macAddresses;
        }
    }
}