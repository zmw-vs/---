using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace 京威_定时删除文件夹内文件
{
    public partial class Form1 : Form
    {
     
        string selectedFolderPath;
        public Form1()
        {
            InitializeComponent();
          
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 选择文件夹并记录路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //选择文件夹，记录路径
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFolderPath = folderDialog.SelectedPath;
                    textBox1.Text = folderDialog.SelectedPath;
                }
            }
        }
        
        //声明一个timer
        private System.Timers.Timer timer;
        //声明用户输入的要删除的间隔时间
        private int days;
        private void button2_Click(object sender, EventArgs e)
        {
            // 用户输入的间隔时间，每隔多久执行一次检查和删除的方法  
            int min;
            if (int.TryParse(textBox2.Text, out min))
            {
                // 将用户输入的分钟转化为毫秒  
                min = min * 60 * 1000;
            }
            else
            {
                // 如果转换失败，则提示用户  
                MessageBox.Show("请输入有效的检查间隔时间(分钟)!");
                return;
            }

            // 检查用户输入的间隔时间  
            if (int.TryParse(textBox3.Text, out days))
            {
                // 这里不需要直接使用 thresholdDate，因为它在 OnTimedEvent 中会被重新计算  
            }
            else
            {
                MessageBox.Show("请输入有效的删除间隔时间(天)!", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 如果文件夹路径不为空，则开启 timer  
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                timer = new System.Timers.Timer(min);
                //注册删除事件
                timer.Elapsed += OnTimedEvent; 
                timer.AutoReset = true; 
                timer.Start();
                textBox4.Text += "开始监控文件夹: " + selectedFolderPath + Environment.NewLine;
                button2.Text = "监控中";
                button2.Enabled = false;
                label5.Text = "监控中";
                textBox1.ReadOnly = true;
                textBox2.ReadOnly = true;
                textBox3.ReadOnly = true;
                textBox4.ReadOnly = true;
                button3.Enabled = true;
                button3.Visible = true;
                button1.Enabled = false;
            }
            else
            {
                MessageBox.Show("请先选择要监控的文件夹");
            }
        }

        /// <summary>  
        /// 检查用户路径符合条件的文件名。对符合大于指定天数的文件进行删除，并且向数据库插入删除记录  
        /// </summary>  
        /// <param name="source"></param>  
        /// <param name="e"></param>  
        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                var files = Directory.GetFiles(selectedFolderPath, textBox5.Text.ToString());
                DateTime thresholdDate = DateTime.Now.AddDays(-days);

                foreach (var file in files)
                {
                    if (File.GetCreationTime(file) < thresholdDate)
                    {
                        File.Delete(file); // 删除文件  
                        string fileName = Path.GetFileName(file); // 获取文件名称  
                        string filePath = file; // 文件路径  
                        DateTime deleteTime = DateTime.Now; // 删除时间  
                        // 向数据库插入记录  
                        InsertDeleteRecord(filePath, fileName, deleteTime);

                        // 更新文本框  
                        UpdateTextBox(file + " 已删除");

                        // 等待5秒  
                        await Task.Delay(5000); // 5000毫秒 = 5秒  
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生错误: " + ex.Message);
            }
        }
        private void UpdateTextBox(string message)
        {
            if (textBox4.InvokeRequired)
            {
                textBox4.Invoke(new Action<string>(UpdateTextBox), message);
            }
            else
            {
                textBox4.AppendText(message + Environment.NewLine); 
            }
        }


       

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            button3.Enabled = false;
            label5.Text = "已停止监控";
            button2.Text = "开启监控";
            button2.Enabled = true;
            textBox1.ReadOnly = false;
            textBox2.ReadOnly = false;
            textBox3.ReadOnly = false;
            button1.Enabled = true;
            timer.Stop();
        }
        private void InsertDeleteRecord(string filePath, string fileName, DateTime deleteTime)
        {
            string connectionString = "Server=127.0.0.1;Database=PLM;User Id=caxa;Password=JwSap20200806;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO DeleteRec (path, FileName, DeleteTime) VALUES (@Path, @FileName, @DeleteTime)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // 添加参数  
                    command.Parameters.AddWithValue("@Path", filePath);
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@DeleteTime", deleteTime);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button3.Visible = false;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
        }
    }
}
