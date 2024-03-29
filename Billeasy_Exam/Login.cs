﻿using System;
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
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Billeasy_Exam
{
    public partial class Login : Form
    {
        public string auth="";
        public Login()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            DataTable dt = Authenticate(txtUserName.Text, txtPassword.Text);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("User Logged in successfully!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Please check your credentials.");
            }
        }

        public string TextBoxValue
        {
            get { return txtCode.Text; }
        }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            Form1 form = new Form1();
            if (e.KeyChar == (char)Keys.Enter)
            {
                DataTable dt = Authenticate(txtUserName.Text, txtPassword.Text);
                if (dt.Rows.Count > 0)
                {
                    MessageBox.Show("User Logged in successfully!");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please check your credentials.");
                }
            }
        }

        public DataTable Authenticate(string username, string password)
        {
            DataTable dt = new DataTable();
            string connectionString = @"db\Exam_db.db";

            SQLiteConnection con = new SQLiteConnection("Data Source=" + Path.Combine(Directory.GetCurrentDirectory(), connectionString));
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "select * from login where UserName=@username and Password=@password";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
            sda.Fill(dt);
            return dt;


        }
        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel2.Visible = true;
        }

        private void closeNoti_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
        }
    }

}
