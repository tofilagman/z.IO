using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.IO
{
    public class SerialFinder
    {
        private string rname;

        /// <summary>
        /// Set the name of the serial to find
        /// </summary>
        /// <param name="regname">\Device\ProlificSerial0</param>
        public SerialFinder(string regname)
        {
            rname = regname;
        }

        //Search for the virtual serial port created by usbclient.
        public bool SearchforCom(ref string sCom)//modify by Darcy on Nov.26 2009
        {
            string sComValue;
            RegistryKey myReg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("HARDWARE\\DEVICEMAP\\SERIALCOMM");
            string[] sComNames = myReg.GetValueNames();//strings array composed of the key name holded by the subkey "SERIALCOMM"
            for (int i = 0; i < sComNames.Length; i++)
            {
                sComValue = "";
                sComValue = myReg.GetValue(sComNames[i]).ToString();//obtain the key value of the corresponding key name
                if (sComValue == "")
                {
                    continue;
                }

                sCom = "";
                if (sComNames[i] == rname)//find the virtual serial port created by usbclient
                {
                    sCom = sComValue;
                    return true;
                }
            }
            return false;//add by Darcy on Nov.26 2009
        }
    }
}
