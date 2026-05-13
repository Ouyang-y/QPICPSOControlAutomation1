using Aerotech.Automation1.DotNet;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QPICPSOControlAutomation1
{
    public partial class FormMain : Form
    {
        private Controller controller = Controller.Connect();  // 定义controller变量，并链接设备进行初始化
        public FormMain()
        {
            InitializeComponent();  // 初始化窗体
            this.FormClosing += FormMain_FormClosing;  // 链接窗体关闭句柄
        }

        #region 窗口打开/关闭
        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Visible = false;  // 先隐藏窗口，防止闪烁
            try
            {
                // 打开时链接控制箱报错
                // We can also subscribe to the ExceptionOccurred event to be notified of any errors that occur
                // while the controller is retrieving status. If this event is raised, it will mean the status retrieval has been paused.
                controller.Runtime.Status.Automatic.ExceptionOccurred += controller_ExceptionOccurred;

                // 打开时设定PsoOutputPin，依据硬件连接(当前为XR3，Output2，链接引脚10&12)
                // https://help.aerotech.com/automation1/hardware-manuals/Automation1-iXR3-and-XR3-web/Chapter-2-Installation-and-Configuration/Position-Synchronized-Output-XR3.htm
                controller.Runtime.Commands.Pso.PsoOutputConfigureOutput("X", PsoOutputPin.XR3PsoOutput2);
                DataCollectionConfiguration configuration = new DataCollectionConfiguration(1, DataCollectionFrequency.Frequency1kHz);
                configuration.Axis.Add(AxisDataSignal.PsoStatus, "X");
                DataCollectionResults result = controller.Runtime.DataCollection.CollectSnapshot(configuration);
                double rawValue = result.AllResults.First().Points[0]; ;
                int value = (int)rawValue;  // double → int

                // 用位掩码判断
                if ((value & 1024) != 0)
                {
                    //1039
                    Button1.Text = "Laser On";
                    Button1.BackColor = Color.PaleGreen;
                }
                else
                {
                    //527
                    Button1.Text = "Laser Off";
                    Button1.BackColor = SystemColors.Control;  // 恢复默认
                }
                this.Visible = true;  // 成功后显示窗口
            }
            catch (Exception ex)
            {
                MessageBox.Show($"控制器连接/启动失败: {ex.Message}", "错误");
                Application.Exit();  // 终止程序
            }
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.Runtime.Status.Automatic.ExceptionOccurred -= controller_ExceptionOccurred;
            try
            {
                controller?.Disconnect(); // 断连接
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"关闭时出错: {ex.Message}");
                Application.Exit();  // 终止程序
            }
        }
        #endregion

        private void Button1_Click(object sender, EventArgs e)
        {
            if (Button1.Text == "Laser On")
            {
                controller.Runtime.Commands.Pso.PsoOutputOff("X");
                Button1.Text = "Laser Off";
                Button1.BackColor = SystemColors.Control;  // 恢复默认
            }
            else
            {
                controller.Runtime.Commands.Pso.PsoOutputOn("X");
                Button1.Text = "Laser On";
                Button1.BackColor = Color.PaleGreen;
            }
        }
        private void controller_ExceptionOccurred(object sender, ExceptionOccurredEventArgs e)
        {
            // Since we want to use the new status from this event to update elements in the UI, we
            // must use Invoke() to have the code that processes the new status run on the main UI thread.
            // See the following Microsoft documentation for details on making thread-safe calls to WinForms controls.
            // https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-make-thread-safe-calls-to-windows-forms-controls
            Invoke(new Action(() =>
            {
                MessageBox.Show($"{e.Exception.Message}", "Controller Error");
                Application.Exit();  // 终止程序
            }));
        }
    }
}
