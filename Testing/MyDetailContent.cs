using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UDTDetailContentBase;

namespace Testing
{
    public partial class MyDetailContent:FISCA.Presentation.DetailContent
    {
        private SingleUDCDecorator<MyUDT> decDetailBase;
        public MyDetailContent()
        {
            InitializeComponent();
        }

        private void MyDetailContent_Load(object sender, EventArgs e)
        {
            this.decDetailBase = new UDTDetailContentBase.SingleUDCDecorator<MyUDT>(this, "StudentID");
            this.decDetailBase.AfterDataLoaded += new SingleUDCDecorator<MyUDT>.UDTDetailContentEventHandler(decDetailBase_OnBindingData);
            this.decDetailBase.OnValidatingData += new SingleUDCDecorator<MyUDT>.UDTDetailContentEventHandler(decDetailBase_OnValidatingData);
            this.decDetailBase.OnReadDataError += new SingleUDCDecorator<MyUDT>.UDTDetailContentErrorEventHandler(decDetailBase_OnReadDataError);
            this.decDetailBase.OnSaveDataError += new SingleUDCDecorator<MyUDT>.UDTDetailContentErrorEventHandler(decDetailBase_OnSaveDataError);
            this.decDetailBase.OnSaveActionCanceled += new EventHandler(decDetailBase_OnSaveActionCanceled);
            this.decDetailBase.AfterDataSaved += new EventHandler(decDetailBase_AfterDataSaved);

            this.radioButton1.CheckedChanged    +=new EventHandler(this.decDetailBase.something_changed);
        }

        void decDetailBase_AfterDataSaved(object sender, EventArgs e)
        {
            MessageBox.Show("儲存資料完成，可以進行 Log");
            this.decDetailBase.ReloadData();    //重新載入資料
        }

        void decDetailBase_OnSaveActionCanceled(object sender, EventArgs e)
        {
            MessageBox.Show("儲存動作被取消了！");
        }

        void decDetailBase_OnSaveDataError(object sender, UDTDetailContentErrorEventArgs<MyUDT> e)
        {
            MessageBox.Show("儲存資料錯誤！" + e.Error.Message);
        }

        void decDetailBase_OnReadDataError(object sender, UDTDetailContentErrorEventArgs<MyUDT> e)
        {
            if (e.Error != null)
                MessageBox.Show("讀取資料錯誤！" + e.Error.Message);
        }

        /// <summary>
        /// 當按下儲存按鈕時，要在此事件驗證資料，並將UI資料放入 UDT 物件。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void decDetailBase_OnValidatingData(object sender, UDTDetailContentEventArgs<MyUDT> e)
        {
            if (string.IsNullOrEmpty(this.FirstName.Text))
            {
                MessageBox.Show("請輸入 FirstName");
                e.Canceled = true;  //取消儲存
            }
            else
                e.CurrentTargets[0].FirstName = this.FirstName.Text;

            e.CurrentTargets[0].Gender = this.Gender.Checked;
            e.CurrentTargets[0].Country = this.Country.Text;
            e.CurrentTargets[0].StudentID = int.Parse(this.PrimaryKey);
        }

        /// <summary>
        /// 讀取完資料後會觸發此事件，以將UDT 資料填入 UI。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void decDetailBase_OnBindingData(object sender, UDTDetailContentEventArgs<MyUDT> e)
        {
            MyUDT myData = e.CurrentTargets[0];
            if (!string.IsNullOrEmpty(myData.UID))
            {
                this.FirstName.Text = myData.FirstName;
                this.Gender.Checked = myData.Gender;
                this.Country.Text = myData.Country;
            }
            else
            {
                this.FirstName.Text = "";
                this.Gender.Checked = false;
                this.Country.Text = "";
            }
        }

    }
}
