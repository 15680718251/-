using System.Windows.Forms;

namespace 全球典型要素数据库增量更新系统
{
    public partial class FormMeasureResult : Form
    {
        //声明运行结果关闭事件
        public delegate void FormClosedEventHandler();
        public event FormClosedEventHandler frmClosed = null;

        public FormMeasureResult()
        {
            InitializeComponent();
        }

        //窗口关闭时引发委托事件
        private void FormMeasureResult_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (frmClosed != null)
            {
                frmClosed();
            }
        }
    }
}
