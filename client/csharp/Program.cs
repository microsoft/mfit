using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using HIDInterface;
using MFIT_Command;
using MFIT_Client;
using System.Data;
using System.Globalization;
using System.IO.Ports;
using System.Threading;

namespace MFIT_Client
{
    class Program
    {
        private const byte PIN_INJ0_REF = 1;
        private const byte PIN_ENABLE_ALERTN = 2;

        static void print_usage(string binary)
        {
            Console.WriteLine("{0:s} [-m|-v|-s SER#OF#DEV|-S] COMMAND [params]", binary);

            Console.WriteLine("COMMAND-fi");
            Console.WriteLine("\tdisable-alertn");
            Console.WriteLine("\tenable-alertn");
            Console.WriteLine("\tnoref TIMESPEC [COM# uefi_delay_ms|pre-trigger|trigger|trigger-cnt CNT|trigger-loop CNT|trigger-loop-fix CNT]");
            Console.WriteLine("\t\tTIMESPEC:");
            Console.WriteLine("\t\t\tNUMBER{u,m,s} -- default is s");
            Console.WriteLine("COMMAND-util");
            Console.WriteLine("\tget-version");
            Console.WriteLine("\tidentify [1|2] -- toggle some LEDs");
            Console.WriteLine("\tupdate");
            Console.WriteLine("");
            Console.WriteLine("EXAMPLES");
            Console.WriteLine("\tnoref 100u COM6 500");
            Console.WriteLine("\t\tsend the trigger on COM6 port, then wait 500ms and then suppress ref for 100 microseconds");
            Console.WriteLine("\tnoref 100u pre-trigger");
            Console.WriteLine("\t\tarm mFIT with the external trigger (a signal connected to mFIT). After the signal is given supress refreshes for 100 microseconds.");
            Console.WriteLine("\tnoref 100u trigger");
            Console.WriteLine("\t\tarm mFIT with the external trigger. After the signal is given supress refreshes for at least 100 microseconds until the trigger re-occurs.");
            Console.WriteLine("\tnoref 100m");
            Console.WriteLine("\t\tsupress refreshes for 100 miilliseconds, right away");
            Console.WriteLine("\tnoref 20u trigger-cnt 5");
            Console.WriteLine("\t\tarm mFIT with the external trigger and loop. (1) Wait for the signal. (2) Supress refreshes for at least 50 us (HARDCODED) until the trigger re-occurs.\n\t\tThen wait 20 us and go to (2) 4 times.");
            Console.WriteLine("\tnoref 100u trigger-loop 5");
            Console.WriteLine("\t\tarm mFIT with the external trigger and loop. (1) Wait for the signal. (2) Supress refreshes for at least 50 us (HARDCODED) until the trigger re-occurs.\n\t\tThen wait 100 us (at least 50 us) and go to (1) 4 times.");
            Console.WriteLine("\tnoref 100u trigger-loop-fix 5");
            Console.WriteLine("\t\tarm mFIT with the external trigger and loop. (1) Wait for the signal. (2) Supress refreshes for at least 50 us (HARDCODED) until the trigger re-occurs.\n\t\tThen wait 100 us (at least 50 us), run the mFIT Fixup Sequence, and go to (1) 4 times.");
            Console.WriteLine("\n");
        }

        static UInt64 parse_timespec_to_us(string timespec)
        {
            if (timespec[timespec.Length - 1] == 'u')
            {
                return UInt64.Parse(timespec.TrimEnd('u'), NumberStyles.AllowExponent);
            }
            if (timespec[timespec.Length - 1] == 'm')
            {
                return 1000UL * UInt64.Parse(timespec.TrimEnd('m'), NumberStyles.AllowExponent);
            }
            if (timespec[timespec.Length - 1] == 's')
            {
                return 1000_000UL * UInt64.Parse(timespec.TrimEnd('s'), NumberStyles.AllowExponent);
            }

            // default is just seconds
            return 1000_000UL * UInt64.Parse(timespec, NumberStyles.AllowExponent);
        }

        static void Main(string[] args)
        {
            uint i = 0;

            void check_arglen(string exit_message)
            {
                if (i >= args.Length)
                {
                    Console.WriteLine(exit_message);
                    print_usage(System.AppDomain.CurrentDomain.FriendlyName);
                    System.Environment.Exit(1);
                }
            }

            check_arglen("Invalid number of args");


            MFIT_Client.verbose = false;
            MFIT_Client.mock = false;
            string serialNumber = "";
            bool hasListSwitch = false;

            /* get switches */
            while (i < args.Length && args[i].StartsWith("-"))
            {
                if (String.Equals(args[i], "-v", StringComparison.OrdinalIgnoreCase))
                {
                    MFIT_Client.verbose = true;
                } else if (String.Equals(args[i], "-m", StringComparison.OrdinalIgnoreCase))
                {
                    MFIT_Client.mock = true;
                } else if (String.Equals(args[i], "-s"))
                {
                    // consumed
                    ++i;
                    check_arglen("Missing serial number");
                    serialNumber = args[i];
                } else if (String.Equals(args[i], "-S"))
                {
                    // do just the listing
                    hasListSwitch = true;
                }
                ++i;
            }

            if (hasListSwitch)
            {
                int count = MFIT_Client.ListMFITDevices();
                Console.WriteLine("Found {0:d} mFIT device(s).", count);
                return;
            }

            MFIT_Client c = new MFIT_Client(serialNumber);
            if (!c.WasMFITFound())
            {
                return;
            }

            check_arglen("Missing command");

            if (String.Equals(args[i], "get-version", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("mFIT_version={0:s}", c.CommandGetVersion());
                return;
            }
            if (String.Equals(args[i], "identify", StringComparison.OrdinalIgnoreCase))
            {
                int id_cmd = 1;
                uint next = i + 1;
                if (next < args.Length)
                {
                    // we have an extra paramreter that we could use
                    id_cmd = Convert.ToInt32(args[next], 10);
                    if (id_cmd < 1 || id_cmd > 2)
                    {
                        Console.WriteLine("Invalid parameter to identify cmd");
                        id_cmd = 1;
                    }
                }
                c.CommandIdentify(id_cmd);
                return;
            }
            if (String.Equals(args[i], "update", StringComparison.OrdinalIgnoreCase))
            {
                c.CommandUpdate();
                return;
            }
            if (String.Equals(args[i], "disable-alertn", StringComparison.OrdinalIgnoreCase))
            {
                c.CommandSetLow(PIN_ENABLE_ALERTN);
                Console.WriteLine("disabled ALERTn");
                return;
            }
            if (String.Equals(args[i], "enable-alertn", StringComparison.OrdinalIgnoreCase))
            {
                c.CommandSetHigh(PIN_ENABLE_ALERTN);
                Console.WriteLine("enabled ALERTn");
                return;
            }

            if (String.Equals(args[i], "noref", StringComparison.OrdinalIgnoreCase))
            {
                ++i;
                check_arglen("Missing TIMESPEC parameter to 'noref'");

                UInt64 delay_in_us = parse_timespec_to_us(args[i]);
                bool do_ms = delay_in_us % 1000 == 0;
                UInt64 delay_value = do_ms ? delay_in_us / 1000 : delay_in_us;

                /* supply parameters to the COM port */
                bool trigger_uefi_via_com_port = false;
                SerialPort port = null;
                string port_string = null;
                UInt32 trigger_uefi_delay_ms = 1;

                /* should the noref wait for the external trigger? */
                bool do_trigger_ext_pre = false;
                bool do_trigger_ext_post_and_pre = false;
                bool do_trigger_ext_post_and_pre_cnt = false;
                bool do_trigger_ext_post_and_pre_loop_cnt = false;
                bool do_trigger_ext_post_and_pre_loop_fix_cnt = false;

                UInt32 trigger_ext_post_and_pre_cnt = 1;
                UInt32 trigger_ext_post_and_pre_loop_cnt = 1;
                UInt32 trigger_ext_post_and_pre_loop_fix_cnt = 1;


                if (delay_value > UInt32.MaxValue)
                {
                    Console.WriteLine("Delay is too large {0:d} (us)", delay_in_us);
                    return;
                }

                ++i;
                if (i < args.Length)
                {
                    if (args[i].StartsWith("COM"))
                    {
                        trigger_uefi_via_com_port = true;
                        port_string = args[i];
                    }
                    else if (String.Equals(args[i], "pre-trigger", StringComparison.OrdinalIgnoreCase))
                    {
                        do_trigger_ext_pre = true;
                    }
                    else if (String.Equals(args[i], "trigger", StringComparison.OrdinalIgnoreCase))
                    {
                        do_trigger_ext_post_and_pre = true;
                    }
                    else if (String.Equals(args[i], "trigger-cnt", StringComparison.OrdinalIgnoreCase))
                    {
                        do_trigger_ext_post_and_pre_cnt = true;
                        ++i;
                        if (i >= args.Length)
                        {
                            Console.WriteLine("Missing parameter to trigger-cnt", args[i]);
                            print_usage(System.AppDomain.CurrentDomain.FriendlyName);
                            System.Environment.Exit(1);
                        }
                        trigger_ext_post_and_pre_cnt = UInt32.Parse(args[i], NumberStyles.AllowExponent);
                    }
                    else if (String.Equals(args[i], "trigger-loop", StringComparison.OrdinalIgnoreCase))
                    {
                        do_trigger_ext_post_and_pre_loop_cnt = true;
                        ++i;
                        if (i >= args.Length)
                        {
                            Console.WriteLine("Missing parameter to trigger-loop", args[i]);
                            print_usage(System.AppDomain.CurrentDomain.FriendlyName);
                            System.Environment.Exit(1);
                        }
                        trigger_ext_post_and_pre_loop_cnt = UInt32.Parse(args[i], NumberStyles.AllowExponent);
                    }
                    else if (String.Equals(args[i], "trigger-loop-fix", StringComparison.OrdinalIgnoreCase))
                    {
                        do_trigger_ext_post_and_pre_loop_fix_cnt = true;
                        ++i;
                        if (i >= args.Length)
                        {
                            Console.WriteLine("Missing parameter to trigger-loop-fix", args[i]);
                            print_usage(System.AppDomain.CurrentDomain.FriendlyName);
                            System.Environment.Exit(1);
                        }
                        trigger_ext_post_and_pre_loop_fix_cnt = UInt32.Parse(args[i], NumberStyles.AllowExponent);
                    }
                    else
                    {
                        Console.WriteLine("Unknown parameter to noref: '{0:s}'", args[i]);
                        print_usage(System.AppDomain.CurrentDomain.FriendlyName);
                        System.Environment.Exit(1);
                    }
                }

                // done parsing args, lets do something now
                if (trigger_uefi_via_com_port)
                {
                    ++i;
                    check_arglen("Missing parameter to delay via COM port");
                    trigger_uefi_delay_ms = UInt32.Parse(args[i], NumberStyles.AllowExponent);
                }

                // do the trigger uefi and wait
                if (trigger_uefi_via_com_port)
                {
                    Console.WriteLine("Gonna trigger UEFI on port {0:s} trigger_delay is {1:d} ms", port_string, trigger_uefi_delay_ms);
                    port = new SerialPort(port_string, 115200, Parity.None, 8, StopBits.One);
                    port.Open();

                    // Trigger before
                    Console.WriteLine("First UEFI-trigger and then NOREF");
                    port.Write(new byte[] { (byte)'X' }, 0, 1);
                    Thread.Sleep((int)trigger_uefi_delay_ms);

                    //Console.WriteLine("First NOREF and then UEFI-trigger");
                }

                // do the actual command
                if (do_ms)
                {
                    // delay is in ms
                    if (do_trigger_ext_pre)
                    {
                       c.CommandTooglePinPreTrigger_ms(PIN_INJ0_REF, (UInt32)delay_value);
                    }
                    else if (do_trigger_ext_post_and_pre)
                    {
                        c.CommandTooglePinPreAndPostTrigger_ms(PIN_INJ0_REF, (UInt32)delay_value);
                    }
                    else if (do_trigger_ext_post_and_pre_cnt)
                    {
                        c.CommandTooglePinPreAndPostTriggerCount_ms(PIN_INJ0_REF, (UInt32)delay_value, (UInt32)trigger_ext_post_and_pre_cnt);
                    }
                    else if (do_trigger_ext_post_and_pre_loop_cnt)
                    {
                        c.CommandTooglePinPreAndPostLoopTriggerCount_ms(PIN_INJ0_REF, (UInt32)delay_value, (UInt32)trigger_ext_post_and_pre_loop_cnt);
                    }
                    else if (do_trigger_ext_post_and_pre_loop_fix_cnt)
                    {
                        c.CommandTooglePinPreAndPostLoopTriggerFixCount_ms(PIN_INJ0_REF, (UInt32)delay_value, (UInt32)trigger_ext_post_and_pre_loop_fix_cnt);
                    }
                    else
                    {
                        c.CommandTooglePin_ms(PIN_INJ0_REF, (UInt32)delay_value);
                    }
                }
                else
                {
                    // delay is us
                    if (do_trigger_ext_pre)
                    {
                        c.CommandTooglePinPreTrigger_us(PIN_INJ0_REF, (UInt32)delay_value);
                    }
                    else if (do_trigger_ext_post_and_pre)
                    {
                        c.CommandTooglePinPreAndPostTrigger_us(PIN_INJ0_REF, (UInt32)delay_value);
                    }
                    else if (do_trigger_ext_post_and_pre_cnt)
                    {
                        c.CommandTooglePinPreAndPostTriggerCount_us(PIN_INJ0_REF, (UInt32)delay_value, (UInt32)trigger_ext_post_and_pre_cnt);
                    }
                    else if (do_trigger_ext_post_and_pre_loop_cnt)
                    {
                        c.CommandTooglePinPreAndPostLoopTriggerCount_us(PIN_INJ0_REF, (UInt32)delay_value, (UInt32)trigger_ext_post_and_pre_loop_cnt);
                    }
                    else if (do_trigger_ext_post_and_pre_loop_fix_cnt)
                    {
                        c.CommandTooglePinPreAndPostLoopTriggerFixCount_us(PIN_INJ0_REF, (UInt32)delay_value, (UInt32)trigger_ext_post_and_pre_loop_fix_cnt);
                    }
                    else
                    {
                        c.CommandTooglePin_us(PIN_INJ0_REF, (UInt32)delay_value);
                    }
                }

                if (port != null)
                {
                    port.Close();
                }

                return;
            }
            Console.WriteLine("nothing to do :-)");
         }
    }
}
