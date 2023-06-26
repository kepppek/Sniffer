namespace Main
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.cb = new System.Windows.Forms.ComboBox();
            this.lb = new System.Windows.Forms.ListBox();
            this.but = new System.Windows.Forms.Button();
            this.dgv = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // cb
            // 
            this.cb.FormattingEnabled = true;
            this.cb.Location = new System.Drawing.Point(86, 11);
            this.cb.Name = "cb";
            this.cb.Size = new System.Drawing.Size(135, 21);
            this.cb.TabIndex = 0;
            // 
            // lb
            // 
            this.lb.FormattingEnabled = true;
            this.lb.Location = new System.Drawing.Point(4, 317);
            this.lb.Name = "lb";
            this.lb.ScrollAlwaysVisible = true;
            this.lb.Size = new System.Drawing.Size(605, 121);
            this.lb.TabIndex = 4;
            // 
            // but
            // 
            this.but.Location = new System.Drawing.Point(5, 11);
            this.but.Name = "but";
            this.but.Size = new System.Drawing.Size(75, 21);
            this.but.TabIndex = 5;
            this.but.Text = "Старт";
            this.but.UseVisualStyleBackColor = true;
            this.but.Click += new System.EventHandler(this.but_Click);
            // 
            // dgv
            // 
            this.dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.BackgroundColor = System.Drawing.Color.White;
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv.GridColor = System.Drawing.SystemColors.Control;
            this.dgv.Location = new System.Drawing.Point(4, 38);
            this.dgv.Name = "dgv";
            this.dgv.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dgv.Size = new System.Drawing.Size(604, 273);
            this.dgv.TabIndex = 6;
            this.dgv.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_RowHeaderMouseClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 441);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.but);
            this.Controls.Add(this.lb);
            this.Controls.Add(this.cb);
            this.Name = "Form1";
            this.Text = "Sniffer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cb;
        private System.Windows.Forms.ListBox lb;
        private System.Windows.Forms.Button but;
        private System.Windows.Forms.DataGridView dgv;
    }
}

