using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GATUtils.Utilities.Windows.Manipulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class WindowControllerTests
    {
        [TestMethod]
        public void Test01_DoesWindowExist()
        {
            Assert.IsTrue(WindowController.WindowExists("GAT_Initiator"));

            Assert.IsTrue(WindowController.WindowExists("Inbox"));

            Assert.IsFalse(WindowController.WindowExists("Internet Explorer"));
        }

        [TestMethod]
        public void Test02_GetWindow()
        {
            IntPtr Hwnd = WindowController.GetWindow("GAT_Initiator");
            Assert.AreNotEqual(IntPtr.Zero, Hwnd);
        }

        [TestMethod]
        public void Test03_ListWindowTextFields()
        {
            string windowName = "Documents";
            if (WindowController.WindowExists(windowName))
            {
                Dictionary<int, string> textFields = WindowController.GetTextFields(windowName);
                Assert.AreNotEqual(0, textFields.Count);
            }
        }

        [TestMethod]
        public void Test04_ListWindowButtons()
        {
            string windowName = "Calc";
            if (WindowController.WindowExists(windowName))
            {
                Dictionary<int, string> buttons = WindowController.GetWindowButtons(windowName);
                Assert.AreNotEqual(0, buttons.Count);
            }
        }

        [TestMethod]
        public void Test05_ClickWindowButtons()
        {
            string windowName = "Calc";
            if (WindowController.WindowExists(windowName))
            {
                Dictionary<int, string> buttons = WindowController.GetWindowButtons(windowName);
                Assert.AreNotEqual(0, buttons.Count);

                WindowController.ClickButtonOnWindow(windowName, "Grads"); //multiplication button
            }
        }

        [TestMethod]
        public void Test06_WriteToWindowTextFields()
        {
            string windowName = "Document1";
            if (WindowController.WindowExists(windowName))
            {
                Dictionary<int, string> textFields = WindowController.GetTextFields(windowName);
                Assert.AreNotEqual(0, textFields.Count);

                for (int i = 0; i < textFields.Count; i++)
                {
                    WindowController.WriteToTextBox(windowName, i, string.Format("Test Write {0}", i.ToString("15")));
                }
            }
        }
    }
}
