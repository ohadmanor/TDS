using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TDSServer
{
    public partial class Form1 : Form
    {
        public SessionManager sessionManager = null;
        public Form1()
        {
            InitializeComponent();
            notifyIcon1.ContextMenu = new ContextMenu();


            MenuItem MenuItemClose = new MenuItem();
            MenuItemClose.Text = "Close TDS Server";
            MenuItemClose.Click += new EventHandler(MenuItemClose_Click);
            notifyIcon1.ContextMenu.MenuItems.Add(MenuItemClose);
        }
        void MenuItemClose_Click(object sender, EventArgs e)
        {           
            if (MessageBox.Show("Are You Sure", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                this.Close();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Text = "TDS Server";
            notifyIcon1.Icon = Properties.Resources.Chess48;
            WindowState = FormWindowState.Minimized;
            this.ClientSize = new System.Drawing.Size(292, 266);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            sessionManager.Dispose();
        }
    }
}
