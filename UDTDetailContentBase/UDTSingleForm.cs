using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using FISCA.UDT;

namespace UDTDetailContentBase
{
    public partial class UDTSingleForm : BaseForm 
    {
        public delegate void UDTSingleFormEventHandler(object sender, string[] uids);
        public event UDTSingleFormEventHandler AfterSaved;
        protected ActiveRecord _target;

        protected bool isDirty = false;     //使用者是否有修改？

        public UDTSingleForm()
        {
            InitializeComponent();

            this.Load += new EventHandler(UDTSingleForm_Load);
        }

        private void UDTSingleForm_Load(object sender, EventArgs e)
        {
            //if (this.DesignMode)
            //    return;
        }

        public  void SetUDT(ActiveRecord rec)
        {
            this._target = rec;
            this.FillData();
        }

        /// <summary>
        /// 填入資料。當此表單被開啟時，就會呼叫此函數填入資料。
        /// 開發者必須自行複寫此函數。
        /// </summary>
        protected  virtual void FillData()
        {

        }

        /// <summary>
        /// 收集資料。當按下儲存按鈕，且驗証資料正確後，就會呼叫此函數以取得要儲存的UDT物件。
        /// 開發者必須自行複寫此函數。
        /// </summary>
        protected virtual void GatherData()
        {

        }

        /// <summary>
        /// 驗正資料是否正確。在按下儲存按鈕時會先呼叫此函數。
        /// 如果有錯誤時，請開發者自行顯示錯誤訊息，並回傳 false。預設回傳 true。
        /// 開發者必須自行複寫此函數。
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateData()
        {
            return true;
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            string msg = (this.isDirty) ? "" : "是否不儲存直接離開？";

            if (this.isDirty)
            {
                if (MessageBox.Show(msg, "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                    this.Close();
            }
            else
                this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!this.ValidateData())
                return;

            this.GatherData();

            List<ActiveRecord> recs = new List<ActiveRecord>();
            recs.Add(this._target);
            FISCA.UDT.AccessHelper ah = new AccessHelper();
            try
            {
                List<string> ids = ah.SaveAll(recs);                          

                if (this.AfterSaved != null)
                {
                    this.AfterSaved(this, ids.ToArray<string>());
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "儲存資料時發生錯誤！", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
