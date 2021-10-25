using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HIDInterface;
using MFIT_Command;

namespace MFIT_Client
{

    public class MFIT_Client
    {
        private const ushort TEENSY_VID = 0x16c0;
        private const ushort TEENSY_PID = 0x0486;
        private HIDDevice device;
        public static bool verbose { get; set; }
        public static bool mock { get; set; }

        public MFIT_Client(string serialNumber)
        {
            device = GetMFITDevice(serialNumber);
        }

        public MFIT_Client(): this("")
        {
        }

        public bool WasMFITFound()
        {
            return device != null;
        }

        ~MFIT_Client()
        {
            if (device != null)
            {
                device.close();
            }
        }

        private void device_write(byte[] bytes)
        {
            if (verbose)
            {
                Console.Write("write={0:d}: ", bytes.Length);
                foreach (byte b in bytes)
                {
                    Console.Write("0x{0,2:s},", b.ToString("x2"));
                }
                Console.WriteLine("");
            }

            if (mock)
            {
                Console.WriteLine("This is a mock!");
                return;
            }
            device.write(bytes);
        }

        private void device_write_cmd(MFIT_Cmd_Base cmd)
        {
            device_write(cmd.getCMDBytes());
        }

        private byte[] device_read()
        {
            byte [] bytes = device.read();
            if (verbose)
            {
                Console.Write("read={0:d}: ", bytes.Length);
                foreach (byte b in bytes)
                {
                    Console.Write("0x{0,2:s},", b.ToString("x2"));
                }
                Console.WriteLine("");
            }
            return bytes;
        }

        public void CommandSetHigh(byte pin)
        {
            device_write_cmd(new MFIT_Cmd_SetHigh(pin));
        }

        public void CommandSetLow(byte pin)
        {
            device_write_cmd(new MFIT_Cmd_SetLow(pin));
        }

        public void CommandTooglePin_us(byte pin, uint microseconds)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelay_us(pin, microseconds));
        }
        public void CommandTooglePinPreTrigger_us(byte pin, uint microseconds)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreTrigger_us(pin, microseconds));
        }
        public void CommandTooglePinPreAndPostTrigger_us(byte pin, uint microseconds)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreAndPostTrigger_us(pin, microseconds));
        }
        public void CommandTooglePinPreAndPostTriggerCount_us(byte pin, uint microseconds, uint cnt)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreAndPostTriggerCount_us(pin, microseconds, cnt));
        }
        public void CommandTooglePinPreAndPostLoopTriggerCount_us(byte pin, uint microseconds, uint cnt)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerCount_us(pin, microseconds, cnt));
        }
        public void CommandTooglePinPreAndPostLoopTriggerFixCount_us(byte pin, uint microseconds, uint cnt)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerFixCount_us(pin, microseconds, cnt));
        }

        public void CommandTooglePin_ms(byte pin, uint milliseconds)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelay_ms(pin, milliseconds));
        }
        public void CommandTooglePinPreTrigger_ms(byte pin, uint milliseconds)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreTrigger_ms(pin, milliseconds));
        }
        public void CommandTooglePinPreAndPostTrigger_ms(byte pin, uint milliseconds)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreAndPostTrigger_ms(pin, milliseconds));
        }
        public void CommandTooglePinPreAndPostTriggerCount_ms(byte pin, uint milliseconds, uint cnt)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreAndPostTriggerCount_ms(pin, milliseconds, cnt));
        }
        public void CommandTooglePinPreAndPostLoopTriggerCount_ms(byte pin, uint milliseconds, uint cnt)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerCount_ms(pin, milliseconds, cnt));
        }
        public void CommandTooglePinPreAndPostLoopTriggerFixCount_ms(byte pin, uint milliseconds, uint cnt)
        {
            device_write_cmd(new MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerFixCount_ms(pin, milliseconds, cnt));
        }



        public string CommandGetVersion()
        {
            device_write_cmd(new MFIT_Cmd_GetVersion());
            byte[] bytes = device_read();

            // this is an extra byte, not sure why
            bytes = bytes.Skip(1).ToArray();

            byte result = bytes[0];
            bytes = bytes.Skip(1).ToArray();

            byte payload_len = bytes[0];
            bytes = bytes.Skip(1).ToArray();

            // the rest is just the version
            return Encoding.ASCII.GetString(bytes, 0, payload_len);
        }

        public void CommandUpdate()
        {
            device_write_cmd(new MFIT_Cmd_Update());
        }

        public void CommandIdentify(int id_cmd)
        {
            device_write_cmd(new MFIT_Cmd_Identify(id_cmd));
        }

        private static int ListDevices(HIDDevice.interfaceDetails[] devices, ushort VID, ushort PID, ushort interfaceNum)
        {
            int idx = -1;
            int dev_cnt = 0;
            for (idx = 0; idx < devices.Length; ++idx)
            {
                HIDDevice.interfaceDetails dev = devices[idx];
                //Console.WriteLine("vid={0,4:X} pid={1,4:X} path={2:s}", dev.VID, dev.PID, dev.devicePath);
                if (dev.VID != VID || dev.PID != PID || !dev.devicePath.ToLower().Contains("&mi_" + String.Format("{0:x2}", interfaceNum)))
                {
                    if (verbose)
                    {
                        Console.WriteLine("skip SN='{0:s}' ver=0x{1:x4} prod='{2:s}' path='{3:s}' manuf='{4:s}'", dev.serialNumber, dev.versionNumber, dev.product, dev.devicePath, dev.manufacturer);
                    }
                    continue;
                }
                Console.WriteLine("SN='{0:s}' ver=0x{1:x4} prod='{2:s}' path='{3:s}' manuf='{4:s}'", dev.serialNumber, dev.versionNumber, dev.product, dev.devicePath, dev.manufacturer);
                ++dev_cnt;
            }
            return dev_cnt;
        }

        private static int FindDevice(HIDDevice.interfaceDetails[] devices, ushort VID, ushort PID, ushort interfaceNum, string serialNumber)
        {
            int idx = -1;
            for (idx = 0; idx < devices.Length; ++idx)
            {
                HIDDevice.interfaceDetails dev = devices[idx];

                if (verbose)
                {
                    Console.WriteLine("SN='{0:s}' ver=0x{1:x4} prod='{2:s}' path='{3:s}' manuf='{4:s}'", dev.serialNumber, dev.versionNumber, dev.product, dev.devicePath, dev.manufacturer);
                }
                //For Teensy
                //BAD------ >>> Match, SN = 6460490 ver = 631 prod = Teensyduino RawHID path =\\?\hid#vid_16c0&pid_0486&mi_01#7&50d2197&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030} manuf=Teensyduino
                //GOOD --->> Match, SN = 6460490 ver = 631 prod = Teensyduino RawHID path =\\?\hid#vid_16c0&pid_0486&mi_00#7&34f26416&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030} manuf=Teensyduino

                //34f26416 is revision number

                if (dev.VID == VID && dev.PID == PID)
                {
                    /*
                     * https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/hidclass-hardware-ids-for-top-level-collections
                     *
                     * On composide USB devices (USB devices with more than one interface), select only the first interface
                     */
                    
                    if (dev.devicePath.ToLower().Contains("&mi_" + String.Format("{0:x2}", interfaceNum)))
                    {
                        if (serialNumber != "")
                        {
                            // we have a serial number requested to match
                            if (String.Equals(dev.serialNumber, serialNumber, StringComparison.OrdinalIgnoreCase))
                            {
                                return idx;
                            }
                        } else
                        {
                            // we do not have a serial number requestd to match, so return the first instance
                            return idx;
                        }
                    }
                }
            }
            return -1;
        }

        private static HIDDevice GetMFITDevice(string serialNumber)
        {
            //Get the details of all connected USB HID devices
            HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();

            int selectedDeviceIndex = MFIT_Client.FindDevice(devices, MFIT_Client.TEENSY_VID, MFIT_Client.TEENSY_PID, 0, serialNumber);

            if (selectedDeviceIndex == -1)
            {
                Console.WriteLine("mFIT not found :-(");
                return null;
            }

            return new HIDDevice(devices[selectedDeviceIndex].devicePath, false);
        }

        public static int ListMFITDevices()
        {
            HIDDevice.interfaceDetails[] devices = HIDDevice.getConnectedDevices();

            return MFIT_Client.ListDevices(devices, MFIT_Client.TEENSY_VID, MFIT_Client.TEENSY_PID, 0);
        }

        //Whenever a report comes in, this method will be called and the data will be available! Like magic...
        private void display_bytes(byte[] message)
        {
            Console.Write("MFIT_read={0:d}: ", message.Length);
            foreach (byte b in message)
            {
                Console.Write("0x{0,2:X},", b);
            }
            Console.WriteLine("");
        }
    }
}
