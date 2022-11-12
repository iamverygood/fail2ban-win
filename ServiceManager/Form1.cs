using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Drawing;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace ServiceManager
{
    public partial class Form1 : Form
    {
        string m_servicePath = $"{Application.StartupPath}\\fail2ban-win.exe";
        string m_serviceName = "fail2ban-win";

        public Form1()
        {
            InitializeComponent();
            textBoxServiceName.Text = m_serviceName;
            textBoxServicePath.Text = m_servicePath;
        }

        private void UpdateParams()
        {
            m_serviceName = textBoxServiceName.Text;
            m_servicePath = textBoxServicePath.Text;
        }

        //判断服务是否存在
        private bool IsServiceExisted(string serviceName)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController sc in services)
                {
                    if (sc.ServiceName.ToLower() == serviceName.ToLower())
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        //安装服务
        private void InstallService(string serviceFilePath)
        {
            try
            {
                using (AssemblyInstaller installer = new AssemblyInstaller())
                {
                    installer.UseNewContext = true;
                    installer.Path = serviceFilePath;
                    IDictionary savedState = new Hashtable();
                    installer.Install(savedState);
                    installer.Commit(savedState);
                }
                MessageBox.Show("服务安装成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show("服务安装失败！" + e.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        //卸载服务
        private void UninstallService(string serviceFilePath)
        {
            try
            {
                using (AssemblyInstaller installer = new AssemblyInstaller())
                {
                    installer.UseNewContext = true;
                    installer.Path = serviceFilePath;
                    installer.Uninstall(null);
                }
                MessageBox.Show("服务卸载成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show("服务卸载失败！" + e.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //启动服务
        private void ServiceStart(string serviceName)
        {
            try
            {
                using (ServiceController control = new ServiceController(serviceName))
                {
                    if (control.Status == ServiceControllerStatus.Stopped)
                    {
                        control.Start();
                        MessageBox.Show("服务启动成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    } 
                    else
                    {
                        MessageBox.Show("服务已经启动！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("服务启动失败！" + e.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //停止服务
        private void ServiceStop(string serviceName)
        {
            try
            {
                using (ServiceController control = new ServiceController(serviceName))
                {
                    if (control.Status == ServiceControllerStatus.Running)
                    {
                        control.Stop();
                        MessageBox.Show("服务停止成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("服务已经停止！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("服务停止失败！" + e.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            UpdateParams();
            if (!File.Exists(m_servicePath))
            {
                MessageBox.Show("选择的程序文件不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            ChangeButtonState(false);
            if (this.IsServiceExisted(m_serviceName))
            {
                MessageBox.Show("服务已经存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            this.InstallService(m_servicePath);
            ChangeButtonState(true);
        }


        private void buttonStart_Click(object sender, EventArgs e)
        {
            ChangeButtonState(false);
            UpdateParams();
            if (this.IsServiceExisted(m_serviceName))
            {
                this.ServiceStart(m_serviceName);
            } 
            else
            {
                MessageBox.Show("服务不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            ChangeButtonState(true);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            ChangeButtonState(false);
            UpdateParams();
            if (this.IsServiceExisted(m_serviceName))
            {
                this.ServiceStop(m_serviceName);
            } 
            else
            {
                MessageBox.Show("服务不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            ChangeButtonState(true);
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            UpdateParams();
            if (!File.Exists(m_servicePath))
            {
                MessageBox.Show("选择的程序文件不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            ChangeButtonState(false);
            if (this.IsServiceExisted(m_serviceName))
            {
                this.ServiceStop(m_serviceName);
                this.UninstallService(m_servicePath);
            } 
            else
            {
                MessageBox.Show("服务不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            ChangeButtonState(true);
        }

        private void buttonSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //打开的文件选择对话框上的标题
            openFileDialog.Title = "请选择程序文件";
            //设置文件类型
            openFileDialog.Filter = "程序文件(*.exe)|*.exe|所有文件(*.*)|*.*";
            //设置默认文件类型显示顺序
            openFileDialog.FilterIndex = 1;
            //保存对话框是否记忆上次打开的目录
            openFileDialog.RestoreDirectory = true;
            //设置是否允许多选
            openFileDialog.Multiselect = false;
            //按下确定选择的按钮
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //获得文件路径
                string localFilePath = openFileDialog.FileName.ToString();
                textBoxServicePath.Text = localFilePath;
            }
        }

        private void ChangeButtonState(bool state)
        {
            buttonSelectFile.Enabled = state;
            buttonInstall.Enabled = state;
            buttonStart.Enabled = state;
            buttonStop.Enabled = state;
            buttonRemove.Enabled = state;
        }

    }
}
