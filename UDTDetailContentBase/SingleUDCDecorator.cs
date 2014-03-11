using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation;
using FISCA.UDT ;
using System.ComponentModel;
using System.Reflection;
using DevComponents.DotNetBar.Controls;

namespace UDTDetailContentBase
{
    public class SingleUDCDecorator<T> where T: ActiveRecord, new()
    {
        private DetailContent usercontrol;
        private BackgroundWorker backgroundWorker1;
        private BackgroundWorker backgroundWorker2;

        protected T target;    //代表目前使用中的 UDT 記錄。新增或修改的判斷應從 target.UID 是否有值來判斷
        protected bool needReload = false;     //判斷當 background1_RunAsyncComplete 後是否還要需要重載資料一次？
        protected Dictionary<string, Control> dicFieldControls;  //開發者要設定屬性名稱和對應控制項

        private string pkFieldName = "";    //UDT 上做為參考 PK 的屬性名稱
        private string filterField = "";    // pkFieldName 對應的 DB 欄位名稱

        private bool saveActionCanceled = false;    //是否取消儲存動作

        public delegate void UDTDetailContentEventHandler(object sender, UDTDetailContentEventArgs<T> e);
        public delegate void UDTDetailContentErrorEventHandler(object sender, UDTDetailContentErrorEventArgs<T> e);

        /// <summary>
        /// 讀取完資料後，將資料填入 UI Controls 時觸發。
        /// 若沒有設定，就會按照預設的方式處理。
        /// </summary>
        public event UDTDetailContentEventHandler AfterDataLoaded; 
        /// <summary>
        /// 在儲存資料前會先驗證資料正確性
        /// </summary>
        public event UDTDetailContentEventHandler OnValidatingData;

        /// <summary>
        /// 當儲存資料完成後觸發
        /// </summary>
        public event EventHandler AfterDataSaved;

        /// <summary>
        /// 當儲存資料發生錯誤時。
        /// </summary>
        public event UDTDetailContentErrorEventHandler OnSaveDataError;
        /// <summary>
        /// 當讀取資料發生錯誤時。
        /// </summary>
        public event UDTDetailContentErrorEventHandler OnReadDataError;

        /// <summary>
        /// 當取消儲存動作時。
        /// </summary>
        public event EventHandler OnSaveActionCanceled;

        /// <summary>
        /// 當『取消』按鈕被按下的時候。
        /// </summary>
        public event EventHandler OnCancelButtonClicked;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="detailContent"></param>
        /// <param name="pkFieldName">當作 Refernce 的 UDT 屬性名稱</param>
        /// <param name="autoBindChangeEvent">是否自動 Bind Onchange 事件，如果是 true，就會自動處理 顯示 save, cancel 按鈕。預設是 true</param>
        public SingleUDCDecorator(DetailContent detailContent, string pkFieldName, bool autoBindChangeEvent)
        {
            this.pkFieldName = pkFieldName;
            this.filterField = GetFilterField(pkFieldName);
            this.usercontrol = detailContent;
            this.usercontrol.PrimaryKeyChanged += new EventHandler(UDTDetailContentBase_PrimaryKeyChanged);
            this.usercontrol.SaveButtonClick += new EventHandler(UDTDetailContentBase_SaveButtonClick);
            this.usercontrol.CancelButtonClick += new EventHandler(UDTDetailContentBase_CancelButtonClick);
            
            this.backgroundWorker1 = new BackgroundWorker();
            this.backgroundWorker1.DoWork +=new DoWorkEventHandler(backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);

            this.backgroundWorker2 = new BackgroundWorker();
            this.backgroundWorker2.DoWork +=new DoWorkEventHandler(backgroundWorker2_DoWork);
            this.backgroundWorker2.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(backgroundWorker2_RunWorkerCompleted);

            this.MatchUDTPropertyToControl();   //將控制項和 UDT 的屬性配對起來

            if (autoBindChangeEvent)
                AttachUIEventListener();
        }
        public SingleUDCDecorator(DetailContent detailContent, string filterField)
            : this(detailContent, filterField, true)
        {            
        }

        /// <summary>
        /// 取得目前的 Active Record 物件
        /// </summary>
        /// <returns></returns>
        public T GetCurrentRecord()
        {
            return this.target;
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
        /// 顯示/隱藏 『儲存』和『取消』按鈕
        /// </summary>
        /// <param name="result"></param>
        public void showSaveButton(bool result)
        {
            this.usercontrol.SaveButtonVisible = result;
            this.usercontrol.CancelButtonVisible = result;
        }

        /// <summary>
        /// 重新讀取資料
        /// </summary>
        public void ReloadData()
        {
            this.backgroundWorker1.RunWorkerAsync();
        }

        /// <summary>
        /// 當 UI 上的任何動作需要顯示『儲存』、『取消』按鈕時，可以繫結此 Event Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void something_changed(object sender, EventArgs e)
        {
            this.showSaveButton(true);
        }


        #region =============== Event Handlers ======================
        //當使用者改選了其它記錄時
        void UDTDetailContentBase_PrimaryKeyChanged(object sender, EventArgs e)
        {
            this.usercontrol.Loading = true;

            this.showSaveButton(false);
            if (!this.backgroundWorker1.IsBusy)
                this.backgroundWorker1.RunWorkerAsync();
            else
            {
                this.needReload = true;
            }
        }

        void UDTDetailContentBase_CancelButtonClick(object sender, EventArgs e)
        {
            this.showSaveButton(false); //hide save button
            if (this.OnCancelButtonClicked != null)
                this.OnCancelButtonClicked(this, new EventArgs());
        }

        void UDTDetailContentBase_SaveButtonClick(object sender, EventArgs e)
        {
            if (this.OnValidatingData != null)
            {
                //呼叫使用者自訂函數填好物件
                UDTDetailContentEventArgs<T> arg = new UDTDetailContentEventArgs<T>();
                List<T> recs = new List<T>();
                recs.Add(this.target);
                arg.CurrentTargets = recs;
                arg.Canceled = false;
                this.OnValidatingData(this, arg);   //教由使用者

                if (arg.Canceled)
                {
                    if (this.OnSaveActionCanceled != null)
                        this.OnSaveActionCanceled(this, new EventArgs());
                }
                else
                {
                    this.usercontrol.Loading = true;
                    this.backgroundWorker2.RunWorkerAsync();
                }
            }
            else
            {
                MessageBox.Show("請先設定 OnValidatingData 的事件處理器！");
            }            
        }

        //Read Data
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Get  Info from UDT "T"
            FISCA.UDT.AccessHelper ah = new FISCA.UDT.AccessHelper();
            List<T> records = ah.Select<T>(string.Format("{0}='{1}'", this.filterField , this.usercontrol.PrimaryKey));
            if (records.Count > 0)
                this.target = records[0];
            else
            {
                this.target = new T();
                this.setFilterFieldValue(this.target);
            }
        }

        //Read Data Complete
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.needReload)
            {
                this.backgroundWorker1.RunWorkerAsync();
                this.needReload = false;
            }
            else
            {
                if (e.Error == null)
                {
                    if (this.AfterDataLoaded != null)
                    {
                        UDTDetailContentEventArgs<T> arg = new UDTDetailContentEventArgs<T>();
                        List<T> recs = new List<T>();
                        recs.Add(this.target);
                        arg.CurrentTargets = recs;
                        arg.Canceled = false;
                        this.AfterDataLoaded(this, arg);
                    }
                    //else
                    //this.BindData();    //把 UDT 欄位資料 Bind 到 UI Control 上，預設行為。後來決定讓使用者自行處理。

                    this.usercontrol.Loading = false;
                }
                else
                {
                    UDTDetailContentErrorEventArgs<T> arg = new UDTDetailContentErrorEventArgs<T>();
                    List<T> recs = new List<T>();
                    recs.Add(this.target);
                    arg.CurrentTargets = recs;
                    arg.Error = e.Error;
                    if (this.OnReadDataError != null)
                        this.OnReadDataError(this, arg);
                }
            }
            this.showSaveButton(false);
        }
        
        //Save Data
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            FISCA.UDT.AccessHelper ah = new FISCA.UDT.AccessHelper();
            List<FISCA.UDT.ActiveRecord> rec = new List<FISCA.UDT.ActiveRecord>();

            //PutDataToObject(); //把 UI 上的資料放到 this.target 物件上。後來決議讓使用者自行處理。
                       
            rec.Add(this.target);
            ah.SaveAll(rec);
        }
        
        //Save Data Complete
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.usercontrol.Loading = false;
            if (e.Error == null)
            {
                this.showSaveButton(false);
                if (this.AfterDataSaved != null)
                    this.AfterDataSaved(this, new EventArgs());

                //this.backgroundWorker1.RunWorkerAsync();  //如要 Refresh 資料可由Developer 自行處理。
            }
            else
            {
                UDTDetailContentErrorEventArgs<T> arg = new UDTDetailContentErrorEventArgs<T>();
                List<T> recs = new List<T>();
                recs.Add(this.target);
                arg.CurrentTargets = recs;
                arg.Error = e.Error;
                if (this.OnSaveDataError != null)
                    this.OnSaveDataError(this, arg);
            }
        }

        #endregion


        #region ===============  private function =======================

        /// <summary>
        /// 將 UDT 屬性名稱和對應的控制項匹配起來
        /// </summary>
        private void MatchUDTPropertyToControl()
        {
            this.dicFieldControls = new Dictionary<string, Control>();
            Type type = typeof(T);
            foreach (PropertyInfo f in type.GetProperties())
            {
                if (isUDTField(f))  //如果是 UDT 欄位
                {
                    string fieldName = f.Name;  //取得屬性欄位名稱
                    //看畫面是否有此控制項
                    if (this.usercontrol.Controls.ContainsKey(fieldName))
                    {
                        //如果有，就把屬性和控制項繫結起來
                        this.dicFieldControls.Add(fieldName, this.usercontrol.Controls[fieldName]);
                    }
                    else
                        this.dicFieldControls.Add(fieldName, null); //加入 null 的目地可以知道那些屬性沒有對應的控制項。
                }
            } // end for
        }


        //將畫面上的 Control 加上 OnChange 的事件處理，目地是顯示 Save 按鈕。
        //找出 UDT 中所有 Property 對應的
        private void AttachUIEventListener()
        {
            foreach (Control c in this.dicFieldControls.Values)
            {
                if (c != null)
                {
                    if (c.GetType() == typeof(TextBox))
                        ((TextBox)c).TextChanged += new EventHandler(something_changed);
                    else if (c.GetType() == typeof(TextBoxX))
                        ((TextBoxX)c).TextChanged += new EventHandler(something_changed);
                    else if (c.GetType() == typeof(ComboBox))
                    {
                        ((ComboBox)c).TextChanged += new EventHandler(something_changed);
                    }
                    else if (c.GetType() == typeof(DateTimePicker))
                    {
                        ((DateTimePicker)c).ValueChanged += new EventHandler(something_changed);
                    }
                    else if (c.GetType() == typeof(CheckBox))
                    {
                        ((CheckBox)c).CheckedChanged += new EventHandler(something_changed);
                    }
                }
            }
        }
       
        /*
        /// <summary>
        /// 把 UDT 欄位資料 Bind 到 UI Control 上。
        /// Developer 可 override 此 method 以改變預設行為
        /// </summary>
        protected virtual void BindData()
        {
            Type type = typeof(T);
            foreach (FieldInfo f in type.GetFields(System.Reflection.BindingFlags.Public))
            {
                if (isUDTField(f))  //如果是 UDT 欄位
                {
                    string fieldName = f.Name;  //取得屬性欄位名稱
                    if (this.dicFieldControls.ContainsKey(fieldName))
                    {
                        Control c = this.dicFieldControls[fieldName];
                        if (c != null)
                        {
                            SetControlValue(c, f.GetValue(this.target));
                        }
                            //c.Text = f.GetValue(this.target).ToString(); //將 UDT 的欄位值設定到 UI 控制項上
                    }
                }
            } // end for
        }
        */
        /*
        private void SetControlValue(Control c, object value)
        {
             if (c.GetType() == typeof(TextBox))    {            
                 ((TextBox)c).Text = (value == null) ? "" : value.ToString();
             }            
            else if (c.GetType() == typeof(ComboBox))
            {
                ComboBox cbo = (ComboBox)c;


            }
            else if (c.GetType() == typeof(DateTimePicker))
            {
                ((DateTimePicker)c).Value = (value == null) ? DateTime.MinValue : DateTime.Parse(value.ToString());
            }
            else if (c.GetType() == typeof(CheckBox))
            {
                ((CheckBox)c).Checked = (value == null) ? false : bool.Parse(value.ToString());
            }            
        }
         * */

        //判斷是否是 UDT 欄位
        private bool isUDTField(PropertyInfo p)
        {
            bool result = false;
            foreach (object attribute in p.GetCustomAttributes(true))
            {
                FISCA.UDT.FieldAttribute field = (attribute as FISCA.UDT.FieldAttribute);
                if (field != null)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

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
            }

            return result;
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

        /*
        //把 UI 上的資料放到 this.target 物件上
        protected virtual void PutDataToObject()
        {
                Type type = typeof(T);
                foreach (PropertyInfo f in type.GetProperties())
                {
                    if (isUDTField(f))  //如果是 UDT 欄位
                    {
                        string fieldName = f.Name;  //取得屬性欄位名稱

                        if (fieldName.ToUpper() == this.FilterField.ToUpper())
                        {
                            f.SetValue(this.target, this.usercontrol.PrimaryKey);
                        }
                        else if (fieldName.ToUpper() != "UID")
                        {
                            if (this.dicFieldControls.ContainsKey(fieldName))
                            {
                                Control c = this.dicFieldControls[fieldName];
                                f.SetValue(this.target, c.Text);    //把控制項的值放到
                            }
                        }
                    } //end if
                } // end for
        }
        */
        #endregion   

    } //end class

}
