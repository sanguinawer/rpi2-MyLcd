// Creado por: SkUaTeR
// Basado en el codigo de: https://github.com/symptog/rpi_lcd/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

    class lcd
    {
        public enum lcdBotones
        {
            SELECT = 0,
            RIGHT = 1,
            DOWN = 2,
            UP = 3,
            LEFT = 4,
        }
        public enum lcdColores
        {
            NONE =(byte)0x00,
            RED = 0x01,
            GREEN = 0x02,
            BLUE = 0x04,
            YELLOW = RED + GREEN,
            TEAL = GREEN + BLUE,
            VIOLET = RED + BLUE,
            WHITE = RED + GREEN + BLUE,
         }
        private const byte OUTPUT = 0;
        private const byte INPUT = 1;
        private const byte pin_rs = 15;
        private const byte pin_e = 13;
        private const byte pin_rw = 14;
        private byte[] pin_db = new[] { (byte)12, (byte)11, (byte)10, (byte)9 };
        private byte displaycontrol, displayfunction, displaymode, numlines = 2, currline;

        private const byte LCD_DISPLAYON = 0x04;
        private const byte LCD_DISPLAYOFF = 0x00;
        private const byte LCD_CURSORON = 0x02;
        private const byte LCD_CURSOROFF = 0x00;
        private const byte LCD_BLINKON = 0x01;
        private const byte LCD_BLINKOFF = 0x00;
        // flags for function set
        private const byte LCD_8BITMODE = 0x10;
        private const byte LCD_4BITMODE = 0x00;
        private const byte LCD_2LINE = 0x08;
        private const byte LCD_1LINE = 0x00;
        private const byte LCD_5x10DOTS = 0x04;
        private const byte LCD_5x8DOTS = 0x00;
        // flags for display entry mode
        private const byte LCD_ENTRYRIGHT = 0x00;
        private const byte LCD_ENTRYLEFT = 0x02;
        private const byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        private const byte LCD_ENTRYSHIFTDECREMENT = 0x00;
        // commands
        private const byte LCD_CLEARDISPLAY = 0x01;
        private const byte LCD_RETURNHOME = 0x02;
        private const byte LCD_ENTRYMODESET = 0x04;
        private const byte LCD_DISPLAYCONTROL = 0x08;
        private const byte LCD_CURSORSHIFT = 0x10;
        private const byte LCD_FUNCTIONSET = 0x20;
        private const byte LCD_SETCGRAMADDR = 0x40;
        private const byte LCD_SETDDRAMADDR = 0x80;
        // flags for display/cursor shift
        private const byte LCD_DISPLAYMOVE = 0x08;
        private const byte LCD_CURSORMOVE = 0x00;
        // flags for display/cursor shift
        private const byte LCD_MOVERIGHT = 0x04;
        private const byte LCD_MOVELEFT = 0x00;
        //colores
        
        // botones
       
        private mcp mymcp = null;
        private rpii2c i2c = null;
        // Funciones del display
        public lcd(I2cDevice portExpander)
        {
            i2c = new rpii2c(portExpander);
            mymcp = new mcp(i2c);
            mymcp.mcpSetup();
            mymcp.mcpConfig(pin_e, OUTPUT);
            mymcp.mcpConfig(pin_rs, OUTPUT);
            mymcp.mcpConfig(pin_rw, OUTPUT);
            mymcp.mcpOutput(pin_rw, 0);
            mymcp.mcpOutput(pin_e, 0);
            for (int i = 0; i < 4; i++)
                mymcp.mcpConfig(pin_db[i], OUTPUT);
            lcdWriteBits(0x33, 0);
            lcdWriteBits(0x32, 0);
            lcdWriteBits(0x28, 0);
            lcdWriteBits(0x0C, 0);
            lcdWriteBits(0x06, 0);

            displaycontrol = LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF;
            displayfunction = LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS;
            displayfunction |= LCD_2LINE;

            displaymode = LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT;
            lcdWriteBits((sbyte)(LCD_ENTRYMODESET | displaymode), 0);

            mymcp.mcpConfig(6, OUTPUT);
            mymcp.mcpConfig(7, OUTPUT);
            mymcp.mcpConfig(8, OUTPUT);
            mymcp.mcpOutput(6, 0);
            mymcp.mcpOutput(7, 0);
            mymcp.mcpOutput(8, 0);

            mymcp.mcpPullup((byte)lcdBotones.SELECT, 1);
            mymcp.mcpPullup((byte)lcdBotones.LEFT, 1);
            mymcp.mcpPullup((byte)lcdBotones.RIGHT, 1);
            mymcp.mcpPullup((byte)lcdBotones.UP, 1);
            mymcp.mcpPullup((byte)lcdBotones.DOWN, 1);
            mymcp.mcpConfig((byte)lcdBotones.SELECT, INPUT);
            mymcp.mcpConfig((byte)lcdBotones.LEFT, INPUT);
            mymcp.mcpConfig((byte)lcdBotones.RIGHT, INPUT);
            mymcp.mcpConfig((byte)lcdBotones.UP, INPUT);
            mymcp.mcpConfig((byte)lcdBotones.DOWN, INPUT);
            lcdBackLight(0);
            lcdLedColor(lcdColores.NONE);
            lcdClear();
        }
        private void lcdPulseEnable()
        {
            mymcp.mcpOutput(pin_e, 1);
            mymcp.mcpOutput(pin_e, 0);
        }

        //CHECK_BIT(var, pos) ((var & (1 << pos)) == (1 << pos))
        private void lcdWriteBits(sbyte bits, byte char_mode)
        {
            mymcp.mcpOutput(pin_rs, char_mode);
            bool z;
            int i;
            for (i = 7; i >= 4; i--)
            {
                //z = CHECK_BIT(bits, i);
                z = ((bits & (1 << i)) == (1 << i));
                if (z)
                    mymcp.mcpOutput(pin_db[i - 4], 1);
                else
                    mymcp.mcpOutput(pin_db[i - 4], 0);
            }
            lcdPulseEnable();

            for (i = 3; i >= 0; i--)
            {
                //z = CHECK_BIT(bits, i);
                z = ((bits & (1 << i)) == (1 << i));
                if (z)
                    mymcp.mcpOutput(pin_db[i], 1);
                else
                    mymcp.mcpOutput(pin_db[i], 0);
            }
            lcdPulseEnable();
        }
        public void lcdMessage(string text)
        {
            int i = 0;
            int len = text.Length;

            while (i < len)
            {
                if (text[i] == '\n')
                {
                    int x = 0xc0;
                    lcdWriteBits((sbyte)x, 0);
                }
                else
                {
                    lcdWriteBits((sbyte)text[i], 1);
                }

                i++;
            }
        }
        public void lcdClear()
        {
            lcdWriteBits((sbyte)LCD_CLEARDISPLAY, 0);
        }
        public bool lcdClose()
        {
            try {
                lcdClear();
                lcdBackLight(0);
                lcdLedColor(lcdColores.NONE);
                mymcp.mcpClose();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool lcdButtonPressed(lcdBotones bt)
        {
            byte boton = (byte)bt;
            if (boton > (byte)lcdBotones.LEFT)
                return false;
            else
            {
                int valor = mymcp.mcpInput(boton);
                if (valor == 0)
                    return true;
                else
                    return false;


            }
        }
        public void lcdLedColor( lcdColores color)
        {
            mymcp.mcpConfig(6, 1);
            mymcp.mcpConfig(7, 1);
            mymcp.mcpConfig(8, 1);
            if (color != lcdColores.NONE)
            {
                if ((color & lcdColores.RED) == lcdColores.RED)
                    mymcp.mcpConfig(6, 0);
                if ((color & lcdColores.GREEN) == lcdColores.GREEN)
                    mymcp.mcpConfig(7, 0);
                if ((color & lcdColores.BLUE) == lcdColores.BLUE)
                    mymcp.mcpConfig(8, 0);
            }
        }
        public void lcdBackLight(int state)
        {
            if (state == 1)
                mymcp.mcpConfig(5, 0);
            if (state == 0)
                mymcp.mcpConfig(5, 1);
        }
        public void lcdBegin(byte lines)
        {
            if (lines > 1)
            {
                numlines = lines;
                displayfunction |= LCD_2LINE;
            }
            currline = 0;
            lcdClear();
        }

        public void lcdHome()
        {
            lcdWriteBits((sbyte)LCD_RETURNHOME, 0);
        }

        public void lcdSetCurser(byte col, byte row)
        {
            byte[] rowoffsets = new byte[] { 0x00, 0x40, 0x14, 0x54 };
            if (row > numlines)
                row = (byte)(numlines - 1);
            lcdWriteBits((sbyte)(LCD_SETDDRAMADDR | (col + rowoffsets[row])), 0);
        }

        public void lcdNoDisplay()
        {
            int x = ~LCD_DISPLAYON;
            displaycontrol &= (byte)x;
            lcdWriteBits((sbyte)(LCD_DISPLAYCONTROL | displaycontrol), 0);
        }

        public void lcdDisplay()
        {
            displaycontrol |= LCD_DISPLAYON;
            lcdWriteBits((sbyte)(LCD_DISPLAYCONTROL | displaycontrol), 0);
        }

        public void lcdNoCursor()
        {
            int x = ~LCD_CURSORON;
            displaycontrol &= (byte)x;
            lcdWriteBits((sbyte)(LCD_DISPLAYCONTROL | displaycontrol), 0);
        }

        public  void lcdCursor()
        {
            displaycontrol |= LCD_CURSORON;
            lcdWriteBits((sbyte)(LCD_DISPLAYCONTROL | displaycontrol), 0);
        }

        public void lcdNoBlink()
        {
            int x = ~LCD_BLINKON;
            displaycontrol &= (byte)x;
            lcdWriteBits((sbyte)(LCD_DISPLAYCONTROL | displaycontrol), 0);
        }

        public void lcdBlink()
        {
            displaycontrol |= LCD_BLINKON;
            lcdWriteBits((sbyte)(LCD_DISPLAYCONTROL | displaycontrol), 0);
        }

        public void lcdScrollDisplayLeft()
        {
            lcdWriteBits((sbyte)(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT), 0);
        }

        public void lcdScrollDisplayRight()
        {
            lcdWriteBits((sbyte)(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT), 0);
        }

        public void lcdLeftToRight()
        {
            displaymode |= LCD_ENTRYLEFT;
            lcdWriteBits((sbyte)(LCD_ENTRYMODESET | displaymode), 0);
        }

        public void lcdRightToLeft()
        {
            int x = ~LCD_ENTRYLEFT;
            displaymode &= (byte)x;
            lcdWriteBits((sbyte)(LCD_ENTRYMODESET | displaymode), 0);
        }

        public void lcdAutoscroll()
        {
            displaymode |= LCD_ENTRYSHIFTINCREMENT;
            lcdWriteBits((sbyte)(LCD_ENTRYMODESET | displaymode), 0);
        }

        public void lcdNoAutoscroll()
        {
            int x = ~LCD_ENTRYSHIFTINCREMENT;
            displaymode &= (byte)x;
            lcdWriteBits((sbyte)(LCD_ENTRYMODESET | displaymode), 0);
        }
       
       
        
    }