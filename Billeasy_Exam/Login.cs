using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.Common;

namespace Billeasy_Exam
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var conn = new SQLiteConnection("Data Source=db/Exam_db.db"))
            {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = @"Select * from login where UserName=@username AND Password=@password";
                command.Parameters.AddWithValue("@username", txtUserName.Text);
                command.Parameters.AddWithValue("@password", txtPassword.Text);
                int result = 0;
                result = command.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("User Logged in successfully!");
                Form1 form = new Form1();
                form.Show();
                this.Hide();

                //string a = "abcd";
                //string b = "abcd";
                //if(a==txtUserName.Text && b == txtPassword.Text)
                //{
                //    MessageBox.Show("User Logged in successfully!");
                //    form.Show();
                //    this.Hide();
                //}
                //else
                //{
                //    MessageBox.Show("Invalid User");
                //}
            }

        }
    }
}
