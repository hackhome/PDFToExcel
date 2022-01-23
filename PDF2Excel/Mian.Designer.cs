
namespace PDF2Excel
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_Change = new System.Windows.Forms.Button();
            this.btn_Check = new System.Windows.Forms.Button();
            this.grb_ShowError = new System.Windows.Forms.GroupBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.txt_ShowError = new System.Windows.Forms.TextBox();
            this.lb_Change = new System.Windows.Forms.ListBox();
            this.grb_ShowError.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Change
            // 
            this.btn_Change.Location = new System.Drawing.Point(12, 12);
            this.btn_Change.Name = "btn_Change";
            this.btn_Change.Size = new System.Drawing.Size(65, 23);
            this.btn_Change.TabIndex = 6;
            this.btn_Change.Text = "打开PDF";
            this.btn_Change.UseVisualStyleBackColor = true;
            this.btn_Change.Click += new System.EventHandler(this.btn_Change_Click);
            // 
            // btn_Check
            // 
            this.btn_Check.Enabled = false;
            this.btn_Check.Location = new System.Drawing.Point(83, 12);
            this.btn_Check.Name = "btn_Check";
            this.btn_Check.Size = new System.Drawing.Size(72, 23);
            this.btn_Check.TabIndex = 8;
            this.btn_Check.Text = "开始转换";
            this.btn_Check.UseVisualStyleBackColor = true;
            this.btn_Check.Click += new System.EventHandler(this.btn_Check_Click);
            // 
            // grb_ShowError
            // 
            this.grb_ShowError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grb_ShowError.Controls.Add(this.btnClear);
            this.grb_ShowError.Controls.Add(this.txt_ShowError);
            this.grb_ShowError.Location = new System.Drawing.Point(12, 228);
            this.grb_ShowError.Name = "grb_ShowError";
            this.grb_ShowError.Size = new System.Drawing.Size(560, 210);
            this.grb_ShowError.TabIndex = 52;
            this.grb_ShowError.TabStop = false;
            this.grb_ShowError.Text = "输出消息";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(63, -4);
            this.btnClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(68, 22);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "清空输出";
            this.btnClear.UseVisualStyleBackColor = true;
            // 
            // txt_ShowError
            // 
            this.txt_ShowError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_ShowError.Location = new System.Drawing.Point(16, 20);
            this.txt_ShowError.Multiline = true;
            this.txt_ShowError.Name = "txt_ShowError";
            this.txt_ShowError.ReadOnly = true;
            this.txt_ShowError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_ShowError.Size = new System.Drawing.Size(528, 164);
            this.txt_ShowError.TabIndex = 0;
            // 
            // lb_Change
            // 
            this.lb_Change.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_Change.FormattingEnabled = true;
            this.lb_Change.ItemHeight = 12;
            this.lb_Change.Location = new System.Drawing.Point(12, 61);
            this.lb_Change.Name = "lb_Change";
            this.lb_Change.Size = new System.Drawing.Size(560, 148);
            this.lb_Change.TabIndex = 53;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 450);
            this.Controls.Add(this.lb_Change);
            this.Controls.Add(this.grb_ShowError);
            this.Controls.Add(this.btn_Change);
            this.Controls.Add(this.btn_Check);
            this.Name = "Form1";
            this.Text = "PDF转Excel";
            this.grb_ShowError.ResumeLayout(false);
            this.grb_ShowError.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_Change;
        private System.Windows.Forms.Button btn_Check;
        private System.Windows.Forms.GroupBox grb_ShowError;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txt_ShowError;
        private System.Windows.Forms.ListBox lb_Change;
    }
}

