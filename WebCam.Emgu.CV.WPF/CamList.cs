using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WebCam_Emgu_CV_WPF
{
    //Source: https://sourceforge.net/projects/csharp-web-cam-list/
    public class CamList
    {
        internal static readonly Guid SystemDeviceEnum = new Guid(0x62BE5D10, 0x60EB, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);
        internal static readonly Guid VideoInputDevice = new Guid(0x860BB310, 0x5D01, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

        [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPropertyBag
        {
            [PreserveSig]
            int Read(
                [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
                [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pVar,
                [In] IntPtr pErrorLog);
            [PreserveSig]
            int Write(
                [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
                [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
        }

        [ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICreateDevEnum
        {
            [PreserveSig]
            int CreateClassEnumerator([In] ref Guid type, [Out] out IEnumMoniker enumMoniker, [In] int flags);
        }

        public class Device
        {
            public int Index { get; set; }
            public IMoniker Mon { get; set; }
            public string Name { get; set; }
            public string DevicePath { get; set; }
            public Guid ClassID { get; set; }

            public Device(int index, string name, IMoniker mon, string devicePath, Guid classID)
            {
                this.Index = index;
                this.Name = name;
                this.ClassID = classID;
                this.Mon = mon;
                this.DevicePath = devicePath;
            }

            public Device() { }
        }

        public static List<Device> FindCameras()
        {
            List<Device> devices = new List<Device>();

            Object bagObj = null;
            object comObj = null;
            ICreateDevEnum enumDev = null;
            IEnumMoniker enumMon = null;
            IMoniker[] moniker = new IMoniker[100];
            IPropertyBag bag = null;
            try
            {
                // Get the system device enumerator
                Type srvType = Type.GetTypeFromCLSID(SystemDeviceEnum);
                // create device enumerator
                comObj = Activator.CreateInstance(srvType);
                enumDev = (ICreateDevEnum)comObj;
                // Create an enumerator to find filters of specified category
                enumDev.CreateClassEnumerator(VideoInputDevice, out enumMon, 0);
                Guid bagId = typeof(IPropertyBag).GUID;
                int index = 0;
                while (enumMon.Next(1, moniker, IntPtr.Zero) == 0)
                {
                    // get property bag of the moniker
                    moniker[0].BindToStorage(null, null, ref bagId, out bagObj);
                    bag = (IPropertyBag)bagObj;
                    // read FriendlyName
                    object friendlyName = "";
                    object devicePath = "";
                    object classID = "";
                    bag.Read("FriendlyName", ref friendlyName, IntPtr.Zero);
                    bag.Read("DevicePath", ref devicePath, IntPtr.Zero);
                    bag.Read("ClassID", ref classID, IntPtr.Zero);
                    //list in box
                    devices.Add(
                        new Device()
                        {
                            Index = index,
                            Name = (string)friendlyName,
                            DevicePath = (string)devicePath
                            //,
                            //ClassID = (Guid)classID
                        }
                   );
                    index++;
                }

            }
            catch (Exception)
            {
            }
            finally
            {
                bag = null;
                if (bagObj != null)
                {
                    Marshal.ReleaseComObject(bagObj);
                    bagObj = null;
                }
                enumDev = null;
                enumMon = null;
                moniker = null;
            }

            return devices;
        }
    }
}





