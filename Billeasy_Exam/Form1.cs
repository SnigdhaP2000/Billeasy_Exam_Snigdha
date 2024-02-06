using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace Billeasy_Exam
{
    public partial class Form1 : Form
    {
        private string childFormValue;
        private const string RedirectUri = "https://SnigdhaP.temboolive.com/callback/dropbox";
        private const string AuthorizationEndpoint = "https://www.dropbox.com/oauth2/authorize";
        private const string TokenEndpoint = "https://api.dropbox.com/oauth2/token";
        public static string accessToken;


        private const string ClientId = "g177qj3a7zdn6qi"; // Replace with your actual client ID
        private const string ClientSecret = "9xyohgc4zyyxbak"; // Replace with your actual client secret
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
                button3.Visible = true;
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
                button3.Visible = true;
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
            if (!exceptionOccured)
            {
                using (var dbx = new DropboxClient(accessToken))
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
        private async Task UploadFileToDropboxFromCache(string filePath)
        {
            if (!exceptionOccured)
            {
                using (var dbx = new DropboxClient(accessToken))
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
        bool exceptionOccured = false;
        static readonly string TokenFile = AppDomain.CurrentDomain.BaseDirectory + @"AccessToken\Token.txt";

        public async Task CacheLocally()
        {
            string appDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\cache_files\" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + Path.GetExtension(currFile);

            if (File.Exists(currFile))
            {
                File.Copy(currFile, appDirectory, true);
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            string CacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\cache_files";
            bool checkConnectivity = IsConnectedToInternet();
            if (checkConnectivity)
            {
                string[] files = Directory.GetFiles(CacheFolder);

                HashSet<string> fileNames = new HashSet<string>(files.Length);
                if (files.Length > 0 && !exceptionOccured)
                {
                    foreach (var filePath in files)
                    {
                        await UploadFileToDropboxFromCache(filePath);
                        File.Delete(filePath);
                    }
                    MessageBox.Show("All files from cache folder are uploaded successfully.");
                }

            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            //if (File.Exists(TokenFile))
            //{
            //    AccessToken = File.ReadAllText(TokenFile);
            //}
            //else
            //{
            //    string folderPath = @"AccessToken";

            //    try
            //    {
            //        if (!Directory.Exists(folderPath))
            //        {
            //            Directory.CreateDirectory(folderPath);
            //            MessageBox.Show("Folder created successfully!");
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Error creating folder: {ex.Message}");
            //    }
            //}
            //if (AccessToken == null || AccessToken != "")
            //{
            //    exceptionOccured = false;
            //}
            string cacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents\cache_files";
            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
                using (StreamWriter sw = File.CreateText(cacheFolder + @"\hello.txt")) ;
                timer1.Enabled = false;

            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            filePreview.Controls.Clear();
            button3.Visible = false;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            bool success = false;

            while (!success)
            {
                try
                {
                    // Generate a random state for security
                    string state = Guid.NewGuid().ToString();

                    // Build the authorization URL
                    string authorizationUrl = $"{AuthorizationEndpoint}?client_id={ClientId}&response_type=code&redirect_uri={RedirectUri}&state={state}";

                    // Open the URL in the default system web browser
                    System.Diagnostics.Process.Start(authorizationUrl);

                    // Wait for the user to complete the authentication (you may need to implement a mechanism for this)

                    // Simulate user input (replace with your actual authorization code)
                    Form formBackground = new Form();
                    using (Login login = new Login())
                    {
                        formBackground.StartPosition = FormStartPosition.CenterParent;
                        formBackground.FormBorderStyle = FormBorderStyle.None;
                        formBackground.Opacity = .90d;
                        formBackground.BackColor = System.Drawing.Color.Gray;
                        formBackground.TransparencyKey = System.Drawing.Color.Gray;
                        formBackground.Height = Screen.PrimaryScreen.WorkingArea.Height;
                        formBackground.Width = Screen.PrimaryScreen.WorkingArea.Width;
                        formBackground.Location = Screen.PrimaryScreen.WorkingArea.Location;
                        formBackground.TopMost = false;
                        formBackground.ShowInTaskbar = false;
                        formBackground.Show();
                        login.Owner = formBackground;
                        login.ShowDialog();
                        childFormValue = login.TextBoxValue;
                    }

                    string authorizationCode = childFormValue;

                    accessToken = await ExchangeCodeForTokenAsync(authorizationCode);
                    if(accessToken != null)
                    {
                        success = true;
                        MessageBox.Show("Access token obtained successfully.");
                        timer1.Enabled = true;
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("Not a proper acess token, please enter the code properly.");
                        this.Hide();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }

        }

        private async Task<string> ExchangeCodeForTokenAsync(string code)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("client_secret", ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", RedirectUri)
                });

                var response = await client.PostAsync(TokenEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize JSON response
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                // Access the properties from the deserialized object
                string accessToken = tokenResponse.access_token;
                int expiresIn = tokenResponse.expires_in;

                return accessToken;
            }
        }

    }
    public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        // Add other properties as needed
    }


}
