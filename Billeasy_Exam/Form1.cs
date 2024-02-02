using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Billeasy_Exam
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        string newformat = "";
        string currFile = "";
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) // Test result.
            {
                string FileName = openFileDialog1.FileName;
                currFile = FileName;
                string extension = Path.GetExtension(openFileDialog1.FileName);
                DateTime dateTime = DateTime.Now;
                string name = dateTime.ToString("dd_MM_yyyy_HH_MM_ss");
                string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\";
                newformat = FileName + path + name + extension;
                CreatePreview();
                //File.Copy(FileName, path + name + extension);
            }
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK) // Test result.
            {
                int count = 0;
                foreach (string item in openFileDialog1.FileNames)
                {
                    count++;
                    string FileName = item;
                    currFile = item;
                    string extension = Path.GetExtension(item);
                    DateTime dateTime = DateTime.Now;
                    string name = dateTime.ToString("dd_MM_yyyy_HH_MM_ss");
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\";
                    newformat = item + path + name + extension;
                    CreatePreview();
                    //File.Copy(item, path + name + "_" + count + extension);
                }
                MessageBox.Show("All Files saved in your documents folder successfully.");
            }
        }

        public void CreatePreview()
        {
            Button button = new Button();
            button.Size = new Size(50, 50);
            button.Dock = DockStyle.Right;
            button.Text = "Save";
            button.Font = Font = new Font(Label.DefaultFont, FontStyle.Bold);
            button.ForeColor = Color.White;
            button.Click += new EventHandler(save_button_Click);

            Random R = new Random();
            Panel p = new Panel();
            p.Name = currFile;
            p.BackColor = Color.FromArgb(123, R.Next(222), R.Next(222));
            p.Size = new Size(filePreview.ClientSize.Width-15, 50);
            filePreview.Controls.Add(p);
            filePreview.Controls.SetChildIndex(p, 0);
            p.Paint += (ss, ee) => { ee.Graphics.DrawString(p.Name, Font, Brushes.White, 22, 11); };
            p.Controls.Add(button);
            filePreview.Invalidate();
        }

        protected async void save_button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;

            bool CheckConnectivity = IsConnectedToInternet();
            if (CheckConnectivity)
            {
                MessageBox.Show("Connected");
                await UploadFileToDropbox(currFile);
            }
            else
            {
                MessageBox.Show("Not Connected");
            }
            
            //MessageBox.Show(newformat + currFile);
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        private async Task UploadFileToDropbox(string filePath)
        {
            using (var dbx = new DropboxClient("sl.Bu0C52oO45zDjIR9zGOaNNUlXZqfbvbZHTxRdNQJGR7QmOvoYloZ7Sl9zk4Uc4Zi-1LTQ4NdGVkJMIYQWgvOl4eOfBHjT8wf88_8x6ymZwc56PXW5pWBps4JTdbCXfewN6M6zZ9WHBT8"))
            {
                var fileName = Path.GetFileName(filePath);
                var fileContent = File.ReadAllBytes(filePath);

                using (var mem = new MemoryStream(fileContent))
                {
                    var updated = await dbx.Files.UploadAsync(
                        "/" + fileName,
                        WriteMode.Overwrite.Instance,
                        body: mem);

                    Console.WriteLine("Uploaded file: " + updated.Name);
                }
            }
        }

    }

}
