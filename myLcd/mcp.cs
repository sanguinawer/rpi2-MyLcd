// Creado por: SkUaTeR
// Basado en el codigo de: https://github.com/symptog/rpi_lcd/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    class mcp
    {
        private const byte MCP23017_IODIRA = 0x00;
        private const byte MCP23017_IODIRB = 0x01;
        private const byte MCP23017_GPIOA = 0x12;
        private const byte MCP23017_GPIOB = 0x13;
        private const byte MCP23017_GPPUA = 0x0C;
        private const byte MCP23017_GPPUB = 0x0D;

        private const byte MCP23017_OLATA = 0x14;
        private const byte MCP23017_OLATB = 0x15;
        rpii2c i2c;
        public mcp(rpii2c i)
        {
            i2c = i;
        }
        public void mcpSetup()
        {
            //i2c = _i2c;
            /*if (addr==0)
                i2c.rpiI2cSetup();
            else
                i2c.rpiI2cSetup(addr);*/
         
            i2c.rpiI2cWrite(MCP23017_IODIRA, 0xFF); // all A as INPUT
            i2c.rpiI2cWrite(MCP23017_IODIRB, 0xFF); // all B as INPUT
            short direction = 0;
            direction = i2c.rpiI2cRead8(MCP23017_IODIRA);
            direction |= (short)((short)i2c.rpiI2cRead8(MCP23017_IODIRB) << 8);

            i2c.rpiI2cWrite(MCP23017_GPPUA, 0x00); // write A Latch
            i2c.rpiI2cWrite(MCP23017_GPPUB, 0x00); // write B Latch
        }
        /*
         * 
         * name: mcpChangebit
         * @param __u8 bitmap (bitmap to change), __u8 bit (bitposition to change), __u8 value (new value)
         * @return __u8 new bitmap
         * 
         * example: bitmap: 00011100 , bit: 3, value: 0
         * -> new bitmap: 00010100
         * 
         */
        byte mcpChangebit(byte bitmap, byte bit, byte value)
        {
            if (value == 0)
            {
                return (byte)(bitmap & ~(1 << bit));
            }
            else if (value == 1)
            {
                return (byte)(bitmap | (1 << bit));
            }
            else
            {
                return bitmap;
            }
        }
        void mcpReadChangePin(byte port, byte pin, byte value, byte currvalue, byte set)
        {
            if (set == 0)
                currvalue = i2c.rpiI2cRead8(port);
            byte newvalue = mcpChangebit(currvalue, pin, value);
            i2c.rpiI2cWrite(port, newvalue);
        }
        public void mcpPullup(byte pin, byte value)
        {
            if (pin < 8)
                mcpReadChangePin(MCP23017_GPPUA, pin, value, 0, 0);
            else
                mcpReadChangePin(MCP23017_GPPUB, (byte)(pin - 8), value, 0, 0);
        }
        public void mcpConfig(byte pin, byte mode)
        {
            if (pin < 8)
                mcpReadChangePin(MCP23017_IODIRA, pin, mode, 0, 0);
            else
                mcpReadChangePin(MCP23017_IODIRB, (byte)(pin - 8), mode, 0, 0);
        }
        public void mcpOutput(byte pin, byte value)
        {
            if (pin < 8)
                mcpReadChangePin(MCP23017_GPIOA, pin, value, i2c.rpiI2cRead8(MCP23017_OLATA), 1);
            else
                mcpReadChangePin(MCP23017_GPIOB, (byte)(pin - 8), value, i2c.rpiI2cRead8(MCP23017_OLATB), 1);
        }
        public byte mcpInput(byte pin)
        {
            short value = i2c.rpiI2cRead16(MCP23017_GPIOA);
            short temp = (short)(value >> 8);
            value <<= 8;
            value |= temp;

            return (byte)(value & (1 << pin));
        }
        public short mcpRead16()
        {
            short lo = i2c.rpiI2cRead8(MCP23017_OLATA);
            short hi = i2c.rpiI2cRead8(MCP23017_OLATB);
            return (short)( (short)(hi << 8) | lo);
        }
        public void mcpWrite16(byte value)
        {
            i2c.rpiI2cWrite(MCP23017_OLATA, (byte)(value & (byte)0xFF));
            i2c.rpiI2cWrite(MCP23017_OLATB, (byte) ((value >> (byte)8) & (byte)0xFF));
        }
        public void mcpClose()
        {
            i2c.rpiI2cClose();
        }
    }

