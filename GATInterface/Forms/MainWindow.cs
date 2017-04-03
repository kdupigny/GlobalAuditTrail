using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GATInterface.FirmModel;
using GATInterface.Forms.OrderLogData;

namespace GATInterface.Forms
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            //EntityTest test = new EntityTest();
            //List <FirmExecution> firmFills = test.TodaysFills;
            

    
            InitializeComponent();

            GatInterfaceDevXDbModel window = new GatInterfaceDevXDbModel();
            window.Show();
        }
    }
}
