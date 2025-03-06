using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Logikal.Killer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\OFCAS";

            // Komut satırında çalıştırılacak komut
            string command = $"rmdir /s /q \"{folderPath}\"";

            // Komut satırı işlemi başlat
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command, // /c parametresi komutun çalıştırılacağını belirtir
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // İşlemi başlat
            using (Process process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();

                // İşlem tamamlanana kadar bekleyin
                process.WaitForExit();

                // Çıktıyı oku
                string output = process.StandardOutput.ReadToEnd();
                listBox1.Items.Add(output);
                listBox1.Items.Add("AppData Temizlendi.");
                listBox1.Refresh();
            }
            this.Cursor = Cursors.Default;
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }
        public string GetProcessUser(Process process)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + process.Id;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                object[] argList = new object[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    return $"{argList[1]}\\{argList[0]}"; // Domain\Username
                }
            }

            return "SYSTEM";
        }
        private void button1_Click(object sender, EventArgs e)
        {

            var select = Boolean.Parse(radioGroup1.EditValue.ToString());
            if (select)
            {
                Process[] processes = Process.GetProcesses();

                // Mevcut kullanıcının tüm çalışan uygulamalarını bul ve sonlandır
                foreach (Process process in processes)
                {
                    try
                    {
                        if (process.MainModule != null)
                        {
                            string path = process.MainModule.FileName;
                            FileInfo fileInfo = new FileInfo(path);
                            FileVersionInfo myFileVersionInfo =
                            FileVersionInfo.GetVersionInfo(path);
                            var pn = myFileVersionInfo.ProductName;
                            if (!string.IsNullOrEmpty(pn))
                            {
                                if (pn.Contains("LogiKal") || pn.Contains("ORGADATA"))
                                {
                                    string processUser = GetProcessUser(process);
                                    process.Kill();
                                    listBox1.Items.Add($"{processUser} Kullanıcısının sonlandırıldı: " + process.ProcessName);
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        //listBox1.Items.Add("Hata: " + ex.Message);
                    }
                }
            }
            else
            {
                Process[] processes = Process.GetProcesses();

                // Mevcut kullanıcının tüm çalışan uygulamalarını bul ve sonlandır
                foreach (Process process in processes)
                {
                    // Sadece mevcut kullanıcının oturumuna ait işlemleri kontrol et
                    if (process.SessionId == Process.GetCurrentProcess().SessionId)
                    {
                        try
                        {
                            if (process.MainModule != null)
                            {
                                string path = process.MainModule.FileName;
                                FileInfo fileInfo = new FileInfo(path);
                                FileVersionInfo myFileVersionInfo =
                                FileVersionInfo.GetVersionInfo(path);
                                var pn = myFileVersionInfo.ProductName;
                                if (!string.IsNullOrEmpty(pn))
                                {
                                    if (pn.Contains("LogiKal") || pn.Contains("ORGADATA"))
                                    {
                                        string processUser = GetProcessUser(process);
                                        process.Kill();
                                        listBox1.Items.Add($"{processUser} Kullanıcısının sonlandırıldı: " + process.ProcessName);
                                    }
                                }
                                
                            }
                        }
                        catch (Exception ex)
                        {
                            //listBox1.Items.Add("Hata: " + ex.Message);
                        }
                    }
                }
            }

            listBox1.Items.Add("Logikal uygulamları sonlandırıldı.");
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }
    }
}
