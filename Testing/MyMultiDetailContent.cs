using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Testing
{
    public partial class MyMultiDetailContent : FISCA.Presentation.DetailContent
    {
        public MyMultiDetailContent()
        {
            InitializeComponent();
        }

        private void MyMultiDetailContent_Load(object sender, EventArgs e)
        {
            this.Group = "多筆 UDT Grid 測試";
        }
    }
}
