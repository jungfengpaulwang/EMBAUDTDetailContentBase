namespace Testing
{
    partial class MyDetailContent
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.FirstName = new System.Windows.Forms.TextBox();
            this.Gender = new System.Windows.Forms.CheckBox();
            this.Country = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // FirstName
            // 
            this.FirstName.Location = new System.Drawing.Point(91, 17);
            this.FirstName.Name = "FirstName";
            this.FirstName.Size = new System.Drawing.Size(100, 25);
            this.FirstName.TabIndex = 0;
            // 
            // Gender
            // 
            this.Gender.AutoSize = true;
            this.Gender.Location = new System.Drawing.Point(91, 59);
            this.Gender.Name = "Gender";
            this.Gender.Size = new System.Drawing.Size(69, 21);
            this.Gender.TabIndex = 1;
            this.Gender.Text = "is Male";
            this.Gender.UseVisualStyleBackColor = true;
            // 
            // Country
            // 
            this.Country.FormattingEnabled = true;
            this.Country.Items.AddRange(new object[] {
            "ROC",
            "China",
            "USA"});
            this.Country.Location = new System.Drawing.Point(290, 17);
            this.Country.Name = "Country";
            this.Country.Size = new System.Drawing.Size(121, 25);
            this.Country.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "FirstName :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(242, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Country";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(29, 91);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(107, 21);
            this.radioButton1.TabIndex = 5;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "radioButton1";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(29, 118);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(107, 21);
            this.radioButton2.TabIndex = 6;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "radioButton2";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // MyDetailContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Country);
            this.Controls.Add(this.Gender);
            this.Controls.Add(this.FirstName);
            this.Name = "MyDetailContent";
            this.Size = new System.Drawing.Size(550, 210);
            this.Load += new System.EventHandler(this.MyDetailContent_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FirstName;
        private System.Windows.Forms.CheckBox Gender;
        private System.Windows.Forms.ComboBox Country;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
    }
}
