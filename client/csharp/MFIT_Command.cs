using System;
using System.Collections.Generic;
using System.IO;


namespace MFIT_Command
{
    internal class MFIT_Cmd_Base
    {
        private struct MFIT_Cmd_Raw
        {
            public byte cmd_op;
            public byte cmd_pin;
            // param 1
            public UInt32 cmd_delay;
            // param 2
            public UInt32 cmd_count;
        }

        private MFIT_Cmd_Raw cmd;
        public MFIT_Cmd_Base(byte op, byte pin, UInt32 delay, UInt32 count)
        {
            cmd.cmd_op = op;
            cmd.cmd_pin = pin;
            // param 1
            cmd.cmd_delay = delay;
            // param 2
            cmd.cmd_count = count;
        }

        public MFIT_Cmd_Base(byte op) : this(op, 0)
        {
        }

        public MFIT_Cmd_Base(byte op, byte pin) : this(op, pin, 0)
        {
        }
        public MFIT_Cmd_Base(byte op, byte pin, UInt32 delay) : this(op, pin, delay, 0)
        {
        }

        public byte[] getCMDBytes()
        {
            List<byte[]> list = new List<byte[]>();

            // GetBytes does not work for the byte type
            list.Add(new byte[] { cmd.cmd_op });
            list.Add(new byte[] { cmd.cmd_pin });

            list.Add(BitConverter.GetBytes(cmd.cmd_delay));
            list.Add(BitConverter.GetBytes(cmd.cmd_count));

            int s = 0;
            int idx = 0;
            foreach (byte[] e in list)
            {
                //Console.WriteLine("sizeof(cmd[{0:d}])={1:d}", idx, e.Length);
                s += e.Length;
                idx++;
            }

            byte[] result = new byte[s];
            using (var stream = new MemoryStream(result))
            {
                foreach (byte[] bytes in list)
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            return result;
        }
    }
    internal class MFIT_Cmd_SetHigh : MFIT_Cmd_Base
    {
        public MFIT_Cmd_SetHigh(byte pin) : base((byte)'h', pin)
        {

        }
    }

    internal class MFIT_Cmd_SetLow : MFIT_Cmd_Base
    {
        public MFIT_Cmd_SetLow(byte pin) : base((byte)'l', pin)
        {

        }
    }

    // delay in microseconds
    internal class MFIT_Cmd_ToggleDelay_us : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelay_us(byte pin, UInt32 delay_us) : base((byte)'D', pin, delay_us)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreTrigger_us : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreTrigger_us(byte pin, UInt32 delay_us) : base((byte)'T', pin, delay_us)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreAndPostTrigger_us : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreAndPostTrigger_us(byte pin, UInt32 delay_us) : base((byte)'A', pin, delay_us)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreAndPostTriggerCount_us : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreAndPostTriggerCount_us(byte pin, UInt32 delay_us, UInt32 count) : base((byte)'M', pin, delay_us, count)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerCount_us : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerCount_us(byte pin, UInt32 delay_us, UInt32 count) : base((byte)'R', pin, delay_us, count)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerFixCount_us : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerFixCount_us(byte pin, UInt32 delay_us, UInt32 count) : base((byte)'F', pin, delay_us, count)
        {

        }
    }

    // delay in milliseconds
    internal class MFIT_Cmd_ToggleDelay_ms : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelay_ms(byte pin, UInt32 delay_ms) : base((byte)'d', pin, delay_ms)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreTrigger_ms : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreTrigger_ms(byte pin, UInt32 delay_ms) : base((byte)'t', pin, delay_ms)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreAndPostTrigger_ms : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreAndPostTrigger_ms(byte pin, UInt32 delay_ms) : base((byte)'a', pin, delay_ms)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreAndPostTriggerCount_ms : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreAndPostTriggerCount_ms(byte pin, UInt32 delay_ms, UInt32 count) : base((byte)'m', pin, delay_ms, count)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerCount_ms : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerCount_ms(byte pin, UInt32 delay_ms, UInt32 count) : base((byte)'r', pin, delay_ms, count)
        {

        }
    }
    internal class MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerFixCount_ms : MFIT_Cmd_Base
    {
        public MFIT_Cmd_ToggleDelayPreAndPostLoopTriggerFixCount_ms(byte pin, UInt32 delay_ms, UInt32 count) : base((byte)'f', pin, delay_ms, count)
        {

        }
    }

    internal class MFIT_Cmd_GetVersion : MFIT_Cmd_Base
    {
        public MFIT_Cmd_GetVersion() : base((byte)'v', 1)
        {

        }
    }

    internal class MFIT_Cmd_Update : MFIT_Cmd_Base
    {
        public MFIT_Cmd_Update() : base((byte)'U', 1)
        {

        }
    }
    internal class MFIT_Cmd_Identify : MFIT_Cmd_Base
    {
        public MFIT_Cmd_Identify(int id_cmd) : base((byte)'i', (byte)id_cmd)
        {

        }
        public MFIT_Cmd_Identify() : base((byte)'i', (byte)1)
        {

        }
    }
}