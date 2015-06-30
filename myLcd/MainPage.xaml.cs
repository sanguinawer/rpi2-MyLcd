/*
    Copyright(c) Microloft Open Technologies, Inc. All rights reserved.

    The MIT License(MIT)

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files(the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions :

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Data.Xml.Dom;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using System.Net.NetworkInformation;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace myLcd
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const double TIMER_INTERVAL = 500; // value is milliseconds and denotes the timer interval
        private const double BUTTON_STATUS_CHECK_TIMER_INTERVAL = 50;
        private DispatcherTimer ledTimer;
        private DispatcherTimer buttonStatusCheckTimer;
        private bool isLedOn = false;
        private bool isButtonPressed = false;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        // use these constants for controlling how the I2C bus is setup
        private const byte PORT_EXPANDER_I2C_ADDRESS = 0x20; // 7-bit I2C address of the port expander
        // create a null LCD object, this object must be initialize with an initialized i2device        
        lcd mylcd = null;
        // create menu object
        menu mymenu = null;

        public MainPage()
        {
            this.InitializeComponent();
            // Register for the unloaded event so we can clean up upon exit 
            Unloaded += MainPage_Unloaded;

            // Initialize the system.
            InitializeSystem();
        }

        private async void InitializeSystem()
        {
            // initialize I2C communications
            string deviceSelector = I2cDevice.GetDeviceSelector();
            I2cDevice i2cPortExpander=null;

            var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
            if (i2cDeviceControllers.Count == 0)
            {
                ButtonStatusText.Text = "No I2C controllers were found on this system.";
                return;
            }
            
            var i2cSettings = new I2cConnectionSettings(PORT_EXPANDER_I2C_ADDRESS);
            i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
            i2cPortExpander = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);
            if (i2cPortExpander == null)
            {
                ButtonStatusText.Text = string.Format("Slave address {0} is currently in use on {1}. Please ensure that no other applications are using I2C.",i2cSettings.SlaveAddress,i2cDeviceControllers[0].Id);
                return;
            }

            // initialize the LCD and MENU.
            try
            {
                // Initialize LCD class controller
                mylcd = new lcd(i2cPortExpander);
                mymenu = new menu(mylcd, null);
                mylcd.lcdMessage(mymenu.display());
            }
            catch (Exception e)
            {
                ButtonStatusText.Text = "Failed to initialize I2C port expander: " + e.Message;
                return;
            }

            // setup our timers, one for the LED blink interval, the other for checking button status
            ledTimer = new DispatcherTimer();
            ledTimer.Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL);
            ledTimer.Tick += LedTimer_Tick;
            ledTimer.Start();

            buttonStatusCheckTimer = new DispatcherTimer();
            buttonStatusCheckTimer.Interval = TimeSpan.FromMilliseconds(BUTTON_STATUS_CHECK_TIMER_INTERVAL);
            buttonStatusCheckTimer.Tick += ButtonStatusCheckTimer_Tick;
            buttonStatusCheckTimer.Start();
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            /* Cleanup */
    
            mylcd.lcdClose(); 
        }

        
        // Control de botones del display
        private void CheckButtonStatus()
        {
            buttonStatusCheckTimer.Stop();
            if (mylcd.lcdButtonPressed(lcd.lcdBotones.SELECT))
            {
                mymenu.select();
            }

            if (mymenu.bFinaliza)
            {
                mylcd.lcdClose(); 
                App.Current.Exit();
                return;
            }

            if (mylcd.lcdButtonPressed(lcd.lcdBotones.RIGHT) )
            {
                mymenu.right();
                mymenu.display();
            }
            if (mylcd.lcdButtonPressed(lcd.lcdBotones.LEFT))
            {
                mymenu.left();
                mymenu.display();
            }
            if (mylcd.lcdButtonPressed(lcd.lcdBotones.UP))
            {
                mymenu.up();
                mymenu.display();
            }
            if (mylcd.lcdButtonPressed(lcd.lcdBotones.DOWN))
            {
                mymenu.down();
                mymenu.display();
            }
            buttonStatusCheckTimer.Start();
        }

        private void FlipLED()
        {
            if (isLedOn == true)
            {
                // turn off the LED
                isLedOn = false;
                Led.Fill = grayBrush;
            }
            else
            {
                // turn on the LED
                isLedOn = true;
                Led.Fill = redBrush;
            }
        }

        private void TurnOffLED()
        {
            isLedOn = false;
            Led.Fill = grayBrush;
        }

        private void LedTimer_Tick(object sender, object e)
        {
            if (isButtonPressed == false)
            {
                FlipLED();
            }
        }

        private void ButtonStatusCheckTimer_Tick(object sender, object e)
        {
            CheckButtonStatus();
        }

        private void Delay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ledTimer == null)
            {
                return;
            }
            if (e.NewValue == Delay.Minimum)
            {
                DelayText.Text = "Stopped";
                ledTimer.Stop();
                TurnOffLED();
            }
            else
            {
                DelayText.Text = e.NewValue + "ms";
                ledTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
                ledTimer.Start();
            }
        }
        public string LocalHostName()
        {

            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp != null && icp.NetworkAdapter != null)
            {
                foreach (var n in NetworkInformation.GetHostNames())
                {
                    if (n.IPInformation!= null)
                    {
                        var j = n.IPInformation.NetworkAdapter;
                    }
                }
                var x = NetworkInformation.GetHostNames()[1];
                var hostname =
                    NetworkInformation.GetHostNames()
                        .SingleOrDefault(
                            hn =>
                            hn.IPInformation != null && hn.IPInformation.NetworkAdapter == null
                            && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId );

                if (hostname != null)
                {
                    // the ip address
                    return hostname.CanonicalName;
                }
            }
            return "";
        }
        
        // Screen cotrol
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            /*mymenu.up();
            string texto = mymenu.display();
            DelayText.Text = texto;*/

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            mymenu.down();
            string texto = mymenu.display();
            DelayText.Text = texto;

        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            mymenu.left();
            string texto = mymenu.display();
            DelayText.Text = texto;

        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            mymenu.right();
            string texto = mymenu.display();
            DelayText.Text = texto;
           
        }
    }
}
/*

doc.LoadXml(
                        "<application>" +
                            "<settings lcdColor=\"White\" lcdBackground = \"On\"/>" +
                            "<folder text=\"System Info\" >" +
                                "<widget text=\"Show Date/Time\" function=\"ShowDateTime\"/>" +
                                "<widget text=\"Set Date/Time\" function=\"SetDateTime\"/>" +
                                "<folder text=\"Network\"> " +
                                    "<widget text=\"IP Address\" function=\"ShowIPAddress\"/> " +
                                    "<widget text=\"Use 10.1.1.2\" function=\"Use10Network\"/> " +
                                    "<widget text=\"Use DHCP\" function=\"UseDHCP\"/>" +
                                "</folder>" +
                                "<folder text =\"LCD Color\">" +
                                    "<widget text=\"Red\" function=\"LcdRed\"/>"+
                                    "<widget text=\"Green\" function=\"LcdGreen\"/>"+
                                    "<widget text=\"Blue\" function=\"LcdBlue\" />"+
                                    "<widget text=\"Yellow\" function=\"LcdYellow\"/>"+
                                    "<widget text=\"Teal\" function=\"LcdTeal\" />"+
                                    "<widget text=\"Violet\" function=\"LcdViolet\"/>"+
                                    "<widget text=\"White\" function=\"LcdWhite\"/>"+
                                    "<widget text=\"none\" function= \"LcdNone\"/>"+
                                "</folder>"+
                                "<run text=\"Run ls\">ls</run>" +
                                "<widget text=\"Enter a word\" function=\"EnterWord\"/>" +
                            "</folder>" +
                            "<folder text=\"Fruty\">"+
                                "<service text=\"Wifi\" function=\"Wifi\" tag=\"start|stop\"/>" +
                                "<service text=\"Karma\" function=\"Karma\" tag=\"start|stop\"/>" +
                            "</folder>" +
                            "<widget text=\"Quit\" function=\"DoQui\t\"/>" +
                            "<widget text=\"Reboot\" function=\"DoReboot\" />" +
                            "<widget text=\"Shutdown\" function=\"DoShutdown\" />" +
                        "</application>"
                       );
*/