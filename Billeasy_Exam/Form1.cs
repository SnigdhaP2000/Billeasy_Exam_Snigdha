using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Billeasy_Exam
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

            string FileName = openFileDialog1.FileName;
            string extension = Path.GetExtension(openFileDialog1.FileName);
            DateTime dateTime = DateTime.Now;
            string name = dateTime.ToString("dd_MM_yyyy_HH_MM_ss");
            string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\";
            File.Copy(FileName, path + name + extension);
            MessageBox.Show("File saved in your documents folder successfully.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowDialog();
            int count = 0;
            foreach (string item in openFileDialog1.FileNames)
            {
                count++;
                string FileName = item;
                string extension = Path.GetExtension(item);
                DateTime dateTime = DateTime.Now;
                string name = dateTime.ToString("dd_MM_yyyy_HH_MM_ss");
                string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\";
                File.Copy(item, path + name + "_" + count + extension);
            }
            MessageBox.Show("All Files saved in your documents folder successfully.");
        }
    }
}
