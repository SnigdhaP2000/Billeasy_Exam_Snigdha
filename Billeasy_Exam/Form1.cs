using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
            if (openFileDialog1.ShowDialog() == DialogResult.OK) 
            {
                string FileName = openFileDialog1.FileName;
                currFile = FileName;
                string extension = Path.GetExtension(openFileDialog1.FileName);
                DateTime dateTime = DateTime.Now;
                string name = dateTime.ToString("dd_MM_yyyy_HH_MM_ss");
                string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\";
                newformat = FileName + path + name + extension;
                CreatePreview();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK) 
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
                }
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
            p.Size = new Size(filePreview.ClientSize.Width - 15, 50);
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
            if (CheckConnectivity && !exceptionOccured)
            {
                if (sender is Control control)
                {
                    Panel parentPanel = control.Parent as Panel;
                    MessageBox.Show("Connected");
                    await UploadFileToDropbox(parentPanel.Name);
                }
                
            }
            else
            {
                MessageBox.Show("Not Connected");
                CacheLocally();

            }

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
            if (!exceptionOccured) { 
                using (var dbx = new DropboxClient(AccessToken))
            {
                var fileName = Path.GetFileName(filePath);
                var fileContent = File.ReadAllBytes(filePath);


                using (var mem = new MemoryStream(fileContent))
                {
                    try
                    {   
                        var updated = await dbx.Files.UploadAsync(
                        "/" + fileName,
                        WriteMode.Overwrite.Instance,
                        body: mem);
                        MessageBox.Show("Uploaded file: " + updated.Name);

                    }
                    catch (Exception ex)
                    {
                        exceptionOccured = true;
                            try
                            {
                                using (StreamWriter sw = new StreamWriter(TokenFile, false))
                                {
                                    sw.Write(string.Empty);
                                }

                                Console.WriteLine($"Content inside the text file '{filePath}' deleted.");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error deleting content: {e.Message}");
                            }
                            MessageBox.Show("Token Expired, please contact administrator");
                    }
                    

                }
            }
            }
        }
        bool exceptionOccured=false;
        static readonly string TokenFile = AppDomain.CurrentDomain.BaseDirectory + @"AccessToken\Token.txt";
        string AccessToken = "";

        public async Task CacheLocally()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory+@"cache\"+DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")+ Path.GetExtension(currFile);

            if (File.Exists(currFile))
            {
                File.Copy(currFile, appDirectory, true);
            }
        }
        

        string CacheFolder = AppDomain.CurrentDomain.BaseDirectory + @"cache\";

       
        private async void timer1_Tick(object sender, EventArgs e)
        {
            bool checkConnectivity = IsConnectedToInternet();
            if (checkConnectivity)
            {
                string[] files = Directory.GetFiles(CacheFolder);

                HashSet<string> fileNames = new HashSet<string>(files.Length);
                if (files.Length > 0 && !exceptionOccured)
                {
                    foreach (var filePath in files)
                    {
                        await UploadFileToDropbox(filePath);
                        File.Delete(filePath);
                    }
                    
                }

            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (File.Exists(TokenFile))
            {
                AccessToken = File.ReadAllText(TokenFile);
            }
            else
            {
                string folderPath = @"AccessToken"; // Replace with the desired folder path

                try
                {
                    // Check if the folder doesn't exist, then create it
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        MessageBox.Show("Folder created successfully!");
                    }
               
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating folder: {ex.Message}");
                }
            }
                if (AccessToken == null || AccessToken != "")
                {
                    exceptionOccured = false;
                }
           

        }
    }

}
