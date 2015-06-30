// Creado por: SkUaTeR
// Basado en el codigo de: https://github.com/symptog/rpi_lcd/
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

    #region Ejemplo:Crear acceso a i2c 
    /*public async void rpiI2cSetup(byte addr = PORT_EXPANDER_I2C_ADDRESS)
    {
        string deviceSelector = I2cDevice.GetDeviceSelector();
        var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
        if (i2cDeviceControllers.Count == 0)
        {
            error= "No I2C controllers were found on this system.";
            return;
        }

        var i2cSettings = new I2cConnectionSettings(addr);
        i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
        i2cPortExpander = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);
        if (i2cPortExpander == null)
        {
            error = string.Format("Slave address {0} is currently in use on {1}. " +
                "Please ensure that no other applications are using I2C.",
                i2cSettings.SlaveAddress,
                i2cDeviceControllers[0].Id);
            return;
        }
    }
    */
    #endregion

    class rpii2c
    {
        private const byte PORT_EXPANDER_I2C_ADDRESS = 0x20; // 7-bit I2C address of the port expander
        public I2cDevice i2cPortExpander;
        string error = "";
        public rpii2c(I2cDevice portExpander)
        {
            i2cPortExpander = portExpander;
        }

        public void rpiI2cWrite(byte reg, byte value)
        {
            byte[] i2CWriteBuffer;
            i2CWriteBuffer = new byte[] { reg, value };
            i2cPortExpander.Write(i2CWriteBuffer);
        }
        public byte rpiI2cRead8(byte reg)
        {
            byte[] i2CReadBuffer;
            i2CReadBuffer = new byte[1];
            i2cPortExpander.WriteRead(new byte[] { reg }, i2CReadBuffer);
            return i2CReadBuffer[0];
        }
        public short  rpiI2cRead16(byte reg)
        {

            byte[] i2CReadBuffer=new byte[1];

            i2cPortExpander.WriteRead(new byte[] { reg }, i2CReadBuffer);
            short hi = i2CReadBuffer[0];
            reg++;
            i2cPortExpander.WriteRead(new byte[] { reg }, i2CReadBuffer);
            short lo = i2CReadBuffer[0];
            return (short)((hi << 8) + lo);
        }
        public void rpiI2cClose()
        {
            i2cPortExpander.Dispose();
        }
    }

