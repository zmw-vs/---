using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 京威_定时删除文件夹内文件
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
       private static Mutex mutex = null;  
        [STAThread]
        static void Main()
        {
            const string appName = "京威-定时删除文件夹内文件.exe"; // 应用程序唯一标识符  
            bool createdNew;

            // 尝试创建一个新的Mutex  
            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // 如果已存在相同名称的Mutex，则表示程序已在运行  
                MessageBox.Show("程序已在运行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // 退出应用程序  
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
