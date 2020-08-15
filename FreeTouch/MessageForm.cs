using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FreeTouch
{
    public partial class MessageForm : Form
    {
        public MessageForm()
        {
            InitializeComponent();
        }

        private void MessageForm_Load(object sender, EventArgs e)
        {
            MaximizeBox = false;
            MinimizeBox = false;
            label1.Text = Properties.Resources.Description1;
            label2.Text = Properties.Resources.Description2;
            label3.Text = Properties.Resources.Version;
            label4.Text = Properties.Resources.Copyright;
            int x = (this.Width - label1.Width) / 2;
            label1.Location = new Point(x - 6, label1.Location.Y);
            label2.Location = new Point(x - 6, label1.Location.Y + label1.Height + 6);
            label3.Location = new Point(x - 6, label2.Location.Y + label2.Height + 24);
            label4.Location = new Point(x - 6, label3.Location.Y + label3.Height + 6);
            linkLabel1.Location = new Point(x - 6, label4.Location.Y + label4.Height + 40);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            Process.Start("msedge.exe", "https://github.com/guang-lin/FreeTouch");
        }
    }
}
