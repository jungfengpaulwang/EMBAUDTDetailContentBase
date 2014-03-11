using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation;
using System.ComponentModel;
using System.Reflection;
using FISCA.UDT;

namespace UDTDetailContentBase
{
    /// <summary>
    /// 專門處理多筆資料的資料項目，
    /// 畫面上必須由 一個 DataGridView，及新增、修改、刪除等按鈕處理完
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultipleUDCDecorator<T, F>
        where T : FISCA.UDT.ActiveRecord, new()
        where F : UDTSingleForm, new()
    {
        //private Button btnAddNew;
        //private Button btnUpdate;
        //private Button btnDelete;
        private DataGridView dg;
        private DetailContent usercontrol;
        private BackgroundWorker backgroundWorker1;
        private BackgroundWorker backgroundWorker2;

        private string pkFieldName = "";
        private string filterField = "";
        private bool autoFillData = true;

        protected List<T> records;    //代表目前取得的 UDT 記錄。
        //protected bool needReload = false;     //判斷當 background1_RunAsyncComplete 後是否還要需要重載資料一次？
        private string currentRunningID = "";

        #region ================== Event Declaration =========
        public delegate void UDTDetailContentEventHandler(object sender, UDTDetailContentEventArgs<T> e);
        public delegate void UDTDetailContentErrorEventHandler(object sender, UDTDetailContentErrorEventArgs<T> e);

        /// <summary>
        /// 讀取完資料後，將資料填入 UI Controls 時觸發。
        /// 若沒有設定，就會按照預設的方式處理。
        /// </summary>
        public event EventHandler AfterDataLoaded;

        /// <summary>
        /// 當讀取資料發生錯誤時。
        /// </summary>
        public event UDTDetailContentErrorEventHandler OnReadDataError;

        /// <summary>
        /// 在成功刪除資料之後，在這事件做 Log
        /// </summary>
        public event UDTDetailContentEventHandler AfterDeleted;

        /// <summary>
        /// 在刪除資料時發生錯誤
        /// </summary>
        public event UDTDetailContentErrorEventHandler OnDeleteError;

        /// <summary>
        /// 正確儲存資料之後
        /// </summary>
        public event UDTDetailContentEventHandler AfterSaved;



        #endregion

        public MultipleUDCDecorator(DetailContent dc, DataGridView dg, string pkFieldName)
            : this(dc, dg, pkFieldName, true)
        {
        }

        public MultipleUDCDecorator(DetailContent dc, DataGridView dg, string pkFieldName, bool autoFillData)
        {
            this.pkFieldName = pkFieldName;
            this.dg = dg;
            this.usercontrol = dc;
            this.autoFillData = autoFillData;

            this.filterField = GetFilterField(pkFieldName);

            this.usercontrol.PrimaryKeyChanged += new EventHandler(UDTDetailContentBase_PrimaryKeyChanged);

            this.backgroundWorker1 = new BackgroundWorker();
            this.backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);

            this.backgroundWorker2 = new BackgroundWorker();
            this.backgroundWorker2.DoWork += new DoWorkEventHandler(backgroundWorker2_DoWork);
            this.backgroundWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker2_RunWorkerCompleted);

            this.records = new List<T>();
        }

        /// <summary>
        /// Reference Primary Key 的欄位名稱
        /// </summary>
        public string FilterField
        {
            get { return this.filterField; }
            set { this.filterField = value; }
        }

        /// <summary>
        /// 目前取得的紀錄清單
        /// </summary>
        /// <returns></returns>
        public List<T> GetRecords()
        {
            return this.records;
        }

        /// <summary>
        /// 重新讀取資料
        /// </summary>
        public void ReloadData()
        {
            this.usercontrol.Loading = true;

            if (!this.backgroundWorker1.IsBusy)
            {
                currentRunningID = this.usercontrol.PrimaryKey;
                this.backgroundWorker1.RunWorkerAsync();
            }
        }

        /// <summary>
        /// 開啟編輯畫面
        /// </summary>
        public void OpenUpdateForm()
        {
            if (this.dg.SelectedRows.Count > 0)
            {
                T obj = this.dg.SelectedRows[0].Tag as T;
                if (obj != null)
                {
                    this.OpenForm(obj);
                }
            }
        }

        //開啟新增畫面
        public void OpenAddNewForm()
        {
            T obj = new T();
            this.setFilterFieldValue(obj);
            this.OpenForm(obj);
        }

        public void Delete()
        {
            btnDelete_Click(null, null);
        }

        #region =============== Event Handlers ======================

        void btnDelete_Click(object sender, EventArgs e)
        {
            //先檢查
            if (this.dg == null) return;

            List<ActiveRecord> recs = new List<ActiveRecord>();
            foreach (DataGridViewRow dr in this.dg.SelectedRows)
            {
                T obj = dr.Tag as T;
                if (obj != null)
                    recs.Add(obj);
            }
            if (recs.Count == 0)
            {
                MessageBox.Show("請先選擇要刪除的資料！");
                return;
            }
            if (MessageBox.Show(string.Format("您確定要刪除這 {0} 筆紀錄嗎？", recs.Count.ToString()), "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes)
                return;

            //沒問題才開始
            this.backgroundWorker2.RunWorkerAsync();

        }


        //當使用者改選了其它記錄時
        void UDTDetailContentBase_PrimaryKeyChanged(object sender, EventArgs e)
        {
            this.ReloadData();
            //else
            //{
            //this.needReload = true;
            //}
        }

        //Read Data
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Get  Info from UDT "T"
            try
            {
                FISCA.UDT.AccessHelper ah = new FISCA.UDT.AccessHelper();
                List<T> the_records = ah.Select<T>(string.Format("{0}='{1}'", this.filterField, currentRunningID));
                this.records = the_records;
            }
            catch (Exception ex)
            {
                e.Result = new Exception(ex.Message);
            }
        }

        //Read Data Complete
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (this.needReload)
            //{
            //    this.backgroundWorker1.RunWorkerAsync();
            //    this.needReload = false;
            //}
            //if (e.Result != null && e.Result.GetType().Equals(Type.GetType("System.Exception")))
            //{
            //    UDTDetailContentErrorEventArgs<T> arg = new UDTDetailContentErrorEventArgs<T>();
            //    arg.CurrentTargets = new List<T>();
            //    arg.Error = e.Error;
            //    if (this.OnReadDataError != null)
            //        this.OnReadDataError(this, arg);

            //    return;
            //}
            if (currentRunningID != this.usercontrol.PrimaryKey)
            {
                currentRunningID = this.usercontrol.PrimaryKey;
                this.backgroundWorker1.RunWorkerAsync();                
            }
            else
            {
                if (e.Error == null)
                {
                    if (this.autoFillData)                    
                        FillUDTDataToGrid();   //將 UDT 資料填到GRid 上面
                    
                    if (this.AfterDataLoaded != null)                    
                        this.AfterDataLoaded(this, new EventArgs());    //觸發交由Developer 自行填資料？

                    this.usercontrol.Loading = false;
                }
                else
                {
                    UDTDetailContentErrorEventArgs<T> arg = new UDTDetailContentErrorEventArgs<T>();
                    arg.CurrentTargets = new List<T>();
                    arg.Error = e.Error;
                    if (this.OnReadDataError != null)
                        this.OnReadDataError(this, arg);
                }
            }
        }

        //Delete Data
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            List<ActiveRecord> recs = new List<ActiveRecord>();
            foreach (DataGridViewRow dr in this.dg.SelectedRows)
            {
                T obj = dr.Tag as T;
                if (obj != null)
                    recs.Add(obj);
            }

            AccessHelper ah = new AccessHelper();
            ah.DeletedValues(recs);

        }

        //Delete Data Complete
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.usercontrol.Loading = false;
            //取得要刪除的資料，當作事件參數，便於做 Log
            List<T> recs = new List<T>();
            foreach (DataGridViewRow dr in this.dg.SelectedRows)
            {
                T obj = dr.Tag as T;
                if (obj != null)
                    recs.Add(obj);
            }

            if (e.Error == null)
            {
                if (this.AfterDeleted != null)
                {
                    UDTDetailContentEventArgs<T> args = new UDTDetailContentEventArgs<T>();
                    args.CurrentTargets = recs;
                    this.AfterDeleted(this, args);
                }
            }
            else
            {
                UDTDetailContentErrorEventArgs<T> arg = new UDTDetailContentErrorEventArgs<T>();
                arg.CurrentTargets = recs;
                arg.Error = e.Error;
                if (this.OnDeleteError != null)
                    this.OnDeleteError(this, arg);
            }
        }

        void form_AfterSaved(object sender, string[] uids)
        {
            if (this.AfterSaved != null)
            {
                UDTDetailContentEventArgs<T> args = new UDTDetailContentEventArgs<T>();
                AccessHelper ah = new AccessHelper();
                List<T> recs = ah.Select<T>(uids);
                
                args.CurrentTargets = recs;
                this.AfterSaved(this, args);
            }
        }

        #endregion

        /// <summary>
        /// 根據 UDT 上的欄位名稱取得齊對應的資料表欄位名稱
        /// </summary>
        /// <param name="pkFieldName"></param>
        /// <returns></returns>
        private string GetFilterField(string PropertyName)
        {
            string result = "";
            Type type = typeof(T);
            foreach (PropertyInfo f in type.GetProperties())
            {
                string fieldName = f.Name;  //取得屬性欄位名稱
                if (PropertyName.Equals(fieldName))
                {  //取得
                    foreach (object attribute in f.GetCustomAttributes(true))
                    {
                        FISCA.UDT.FieldAttribute field = (attribute as FISCA.UDT.FieldAttribute);
                        if (field != null)
                        {
                            result = field.Field;
                            break;
                        }
                    }
                    break;
                }
            } // end for

            return result;
        }

        private void OpenForm(T obj)
        {
            F form = new F();
            form.SetUDT(obj);
            form.AfterSaved += new UDTSingleForm.UDTSingleFormEventHandler(form_AfterSaved);
            form.ShowDialog();
        }

        /// <summary>
        /// 對於新建立物件，將 pkField 的值指定上去
        /// </summary>
        /// <param name="obj"></param>
        void setFilterFieldValue(T obj)
        {
            Type type = typeof(T);
            foreach (PropertyInfo f in type.GetProperties())
            {
                string fieldName = f.Name;  //取得屬性欄位名稱
                if (this.pkFieldName.Equals(fieldName))
                {
                    if (f.PropertyType == typeof(System.Int32))
                        f.SetValue(obj, int.Parse(this.usercontrol.PrimaryKey), null);
                    else
                        f.SetValue(obj, this.usercontrol.PrimaryKey, null);

                    break;
                }
            }
        }

        //TODO : 尚未寫完
        void FillUDTDataToGrid()
        {
            this.dg.Rows.Clear();
            Dictionary<string, PropertyInfo> allUDTProperties = this.GetAllUDTProperties();  //取得 UDT 理的所有屬性
            //1. 對於每一筆記錄
            foreach (T obj in this.records)
            {
                //根據 DataGridViewColumn 的順序建立 object[]
                List<object> rawData = new List<object>();
                foreach (DataGridViewColumn dc in this.dg.Columns)
                {
                    if (allUDTProperties.ContainsKey(dc.Name))
                        rawData.Add(allUDTProperties[dc.Name].GetValue(obj, null));
                    else
                        rawData.Add(null);
                }
                if (rawData.Count > 0)
                {
                    int index = this.dg.Rows.Add(rawData.ToArray());
                    DataGridViewRow row = this.dg.Rows[index];
                    row.Tag = obj;
                }
            }
        }

        private Dictionary<string, PropertyInfo> GetAllUDTProperties()
        {
            Dictionary<string, PropertyInfo> result = new Dictionary<string, PropertyInfo>();
            Type type = typeof(T);
            foreach (PropertyInfo p in type.GetProperties())
            {
                result.Add(p.Name, p);
            }
            return result;
        }
    }
}
