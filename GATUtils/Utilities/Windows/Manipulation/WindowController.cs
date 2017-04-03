using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using GATUtils.Types.Custom;
using WindowsInput;
using GATUtils.Logger;

namespace GATUtils.Utilities.Windows.Manipulation
{
    public static class WindowController
    {
        public static bool WindowExists(string windowFullName)
        {
            return GetWindow(windowFullName) != IntPtr.Zero;
        }

        public static IntPtr GetWindow(string windowFullName)
        {
            if (windowFullName.Equals(_currentWindowName))
            {
                GatLogger.Instance.AddMessage(string.Format("Window [{0}] is current window.", windowFullName));
                return _currentWinHandle;
            }

            _currentWindowName = windowFullName;
            GatLogger.Instance.AddMessage(string.Format("Attempting to find window [{0}]", _currentWindowName));
            Callback myCallBack = new Callback(s_EnumChildGetValue);
            s_ClearLists();

            _currentWinHandle = Win32Handle.SearchForWindow(null, _currentWindowName);
            Win32Handle.EnumChildWindows((int)_currentWinHandle, myCallBack, 0);

            GatLogger.Instance.AddMessage(_currentWinHandle == IntPtr.Zero
                                              ? string.Format("Window [{0}] found", _currentWindowName)
                                              : string.Format("Warning Window [{0}] not found", _currentWindowName));

            return _currentWinHandle;
        }

        public static Dictionary<int, string> GetWindowButtons(string windowFullName)
        {
            IntPtr hWnd = GetWindow(windowFullName);
            return ro_DictButtons;
        }

        public static Dictionary<int, string> GetTextFields(string windowFullName)
        {
            IntPtr hwnd = GetWindow(windowFullName);
            return ro_DictEdit;
        }

        public static bool TryGetWindow(string windowFullName, out IntPtr winHandle)
        {
            winHandle = GetWindow(windowFullName);
            return winHandle != IntPtr.Zero;
        }

        public static void ClickButtonOnWindow(string windowFullName, string buttonCaption)
        {
            GetWindow(windowFullName);
            s_Go(buttonCaption);
        }

        public static void ClickButtonOnWindow(string windowFullName, int buttonIndex)
        {
            GetWindow(windowFullName);
            ButtonClick(buttonIndex);
        }

        public static void WriteToTextBox(string windowFullName, string textBoxName, string message)
        {
            GatLogger.Instance.AddMessage(string.Format("Attempting to write text to Window[{0}]->TextField[{1}]", windowFullName, textBoxName), LogMode.LogAndScreen);
            GetWindow(windowFullName);

            int txtBoxIdx = ro_DictEdit.FindIndexByValue(textBoxName);

            GatLogger.Instance.AddMessage(string.Format("\tSleeping {0} seconds", (_mediumWaitDuration/1000f)));
            Thread.Sleep(_mediumWaitDuration);

            int hWnd = WriteText(txtBoxIdx, message);
        }

        public static void WriteToTextBox(string windowFullName, int textBoxIndex, string message)
        {
            GatLogger.Instance.AddMessage(string.Format("Attempting to write text to Window[{0}]->TextField[{1}]", windowFullName, textBoxIndex), LogMode.LogAndScreen);
            GetWindow(windowFullName);
            
            GatLogger.Instance.AddMessage(string.Format("\tSleeping {0} seconds", (_mediumWaitDuration / 1000f)));
            Thread.Sleep(_mediumWaitDuration);

            int hWnd = WriteText(textBoxIndex, message);
        }

        public static void ButtonClick(int buttonIdx)
        {
            int hWnd = ro_ListButtons[buttonIdx];
            s_ClickButton(hWnd);
        }

        public static void ButtonClick(string buttonCaption)
        {
            s_Go(buttonCaption);
        }

        public static int WriteText(int index, string text)
        {
            int hWnd = ro_ListEdit[index];
            StringBuilder sb = new StringBuilder(1024);
            sb.Append(text);
            Win32Handle.SendMessage(hWnd, Win32Handle.WM_WRITE, 0, sb);
            Thread.Sleep(_mediumWaitDuration);

            return hWnd;
        }

        /// <summary>
        /// Adjusts the wait times in miliseconds between control acquisition and attempting to use them.  Providing the value 0 for a particular field
        /// will reassign the default value of the wait period.  It is better to adjust all times uniformly from their defaults than individually. 
        /// </summary>
        /// <param name="shortWait">The short wait. Default (500)</param>
        /// <param name="mediumWait">The medium wait. Default (1000)</param>
        /// <param name="longWait">The long wait. Default (2000)</param>
        /// <param name="extendedWait">The extended wait. Default (2500)</param>
        public static void AdjustWaitTimes(int shortWait, int mediumWait, int longWait, int extendedWait)
        {
            _shortWaitDuration = shortWait == 0 ? 500 : shortWait;
            _mediumWaitDuration = mediumWait == 0 ? 1000 : mediumWait;
            _longWaitDuration = longWait == 0 ? 2000 : longWait;
            _extendedWaitDuration = extendedWait == 0 ? 2500 : extendedWait;
        }

        private static int s_EnumChildGetValue(int hWnd, int lParam)
        {
            StringBuilder formDetails = new StringBuilder(256);
            int txtValue;
            string editText = "";

            txtValue = Win32Handle.GetWindowText(hWnd, formDetails, 256);
            editText = formDetails.ToString().Trim();
            if (editText == "")
            {
                Win32Handle.SendMessage(hWnd, Win32Handle.WM_GETTEXT, 256, formDetails);
                editText = formDetails.ToString().Trim();
            }
            ro_DictChildWindows.Add(hWnd, editText);

            StringBuilder cb = new StringBuilder(1024);
            Win32Handle.GetClassName(hWnd, cb, cb.Capacity);

            if (cb.ToString().Contains("Button"))
            {
                ro_ListButtons.Add(hWnd);
                ro_DictButtons.Add(hWnd, editText);
            }
            if (cb.ToString().Contains("Edit"))
            {
                ro_ListEdit.Add(hWnd);
                ro_ListEditValue.Add(editText);
                ro_DictEdit.Add(hWnd, editText);
            }
            if(!ro_ListClass.Contains(cb.ToString())) ro_ListClass.Add(cb.ToString());
            ro_DictChildWindowsValue.Add(hWnd + "_" + cb.ToString(), editText);

            return 1;
        }
        
        private static bool s_Go(string strButtonText)
        {
            try
            {
                int hWnd = ro_DictChildWindows.FindKeyByValue(strButtonText);
                s_ClickButton(hWnd);
                return true;
            }
            catch (Exception 
                
                )
            {
                return false;
            }
        }

        private static void s_ClickButton(int hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            Win32Handle.SendMessage(hWnd, Win32Handle.BN_CLICKED, 0, sb);
        }

        private static void s_ClearLists()
        {
            ro_DictChildWindows.Clear();
            ro_DictChildWindowsValue.Clear();
            ro_DictEdit.Clear();
            ro_DictButtons.Clear();

            ro_ListButtons.Clear();
            ro_ListEdit.Clear();
            ro_ListClass.Clear();
            ro_ListEditValue.Clear();
        }

        private static string _currentWindowName;
        private static IntPtr _currentWinHandle;
        
        private delegate int Callback(int hWnd, int lParam);
        private static readonly Dictionary<int, string> ro_DictChildWindows = new Dictionary<int, string>();
        private static readonly Dictionary<string, string> ro_DictChildWindowsValue = new Dictionary<string, string>();

        private static readonly List<int> ro_ListButtons = new List<int>();
        private static readonly List<int> ro_ListEdit = new List<int>();
        private static readonly List<string> ro_ListEditValue = new List<string>();

        private static readonly Dictionary<int, string> ro_DictEdit = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> ro_DictButtons = new Dictionary<int, string>();
        private static readonly List<string> ro_ListClass = new List<string>();
        
        private static int _shortWaitDuration = 500;
        private static int _mediumWaitDuration = 1000;
        private static int _longWaitDuration = 2000;
        private static int _extendedWaitDuration = 2500;

        // CODE BEYOND THIS POINT IS OBSOLET AND HERE FOR REFERENCE OF USAGE OF THE ABOVE PUBLIC METHODS
        private static string strMain = "TW Today's Treasury Market";
        private static string strQT = "TW  (Q-TKT)";
        private static string strTicket = "TW  (TKT)";
        private static string strTrade = "TW  (NEG)";

        private static void _Initialize()
        {
            GetWindow(strMain);

            ButtonClick(19); // The little magnify glass button
            Thread.Sleep(_shortWaitDuration);
            GetWindow(strQT);

            s_Go("All Markets");
            Thread.Sleep(_shortWaitDuration);
        }

        private static decimal _Trade(string cuspid, string side, int quantity)
        {
            GetWindow(strQT);
            int hWnd = 0;
            Thread.Sleep(_mediumWaitDuration);

            hWnd = WriteText(0, cuspid);
            Win32Handle.SetForegroundWindow(hWnd);

            Thread.Sleep(_shortWaitDuration);
            s_Go(side);
            Thread.Sleep(_mediumWaitDuration);

            IntPtr hWnd2 = GetWindow(strTicket);

            if (hWnd2 != IntPtr.Zero)
            {
                hWnd = WriteText(5, quantity.ToString());
                Win32Handle.SetForegroundWindow(hWnd);
                InputSimulator.SimulateKeyPress(VirtualKeyCode.RETURN);

                s_Go("Send");
            }

            Thread.Sleep(_extendedWaitDuration);
            IntPtr hWnd3 = GetWindow(strTrade);

            decimal val1 = 0;
            decimal val2 = 0;

            if ((int)hWnd3 != 0)
            {
                decimal price1 = 0;
                decimal price2 = 0;
                decimal price3 = 0;

                if (side == "SELL") // In the even that a broker doesnt list a proce, set it high som it ont get chosen.
                {
                    price1 = 1000;
                    price2 = 1000;
                    price3 = 1000;
                }

                if (ro_ListEditValue[1] != "") price1 = Convert.ToDecimal(ro_ListEditValue[1]);
                if (ro_ListEditValue[3] != "") price2 = Convert.ToDecimal(ro_ListEditValue[3]);
                if (ro_ListEditValue[5] != "") price3 = Convert.ToDecimal(ro_ListEditValue[5]);

                int button1 = 10;
                int button2 = 12;
                int button3 = 14;

                if (side == "BUY")
                {
                    val1 = Math.Max(price1, price2);
                    val2 = Math.Max(val1, price3);
                }
                else
                {
                    val1 = Math.Min(price1, price2);
                    val2 = Math.Min(val1, price3);
                }
                int buttonPress = 0;

                if (val2 == price1) buttonPress = button1;
                else if (val2 == price2) buttonPress = button2;
                else if (val2 == price3) buttonPress = button3;

                // HIT/LIFT button needs to be clicked twice for the trade to go thru
                ButtonClick(buttonPress);
                Thread.Sleep(_mediumWaitDuration);
                ButtonClick(buttonPress);

                // If Trade was not accepted, end trading
                Thread.Sleep(_longWaitDuration);
                hWnd3 = GetWindow(strTrade);
                if (s_Go("END TRADING")) val2 = 0;
            }

            // close last open window
            IntPtr hWndClose = hWnd3;
            if ((int)hWndClose == 0) hWndClose = hWnd2;
            StringBuilder sb = new StringBuilder();
            Win32Handle.SendMessage((int)hWndClose, Win32Handle.WM_CLOSE, 0, sb);

            return val2;
        }
    }
}
