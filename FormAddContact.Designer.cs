namespace ChatTwo
{
    partial class FormAddContact
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblContactName = new System.Windows.Forms.Label();
            this.tbxContactName = new System.Windows.Forms.TextBox();
            this.btnSendRequest = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lblContactName
            // 
            this.lblContactName.AutoSize = true;
            this.lblContactName.Location = new System.Drawing.Point(12, 9);
            this.lblContactName.Name = "lblContactName";
            this.lblContactName.Size = new System.Drawing.Size(78, 13);
            this.lblContactName.TabIndex = 0;
            this.lblContactName.Text = "Contact Name:";
            // 
            // tbxContactName
            // 
            this.tbxContactName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxContactName.Location = new System.Drawing.Point(12, 25);
            this.tbxContactName.Name = "tbxContactName";
            this.tbxContactName.Size = new System.Drawing.Size(210, 20);
            this.tbxContactName.TabIndex = 1;
            // 
            // btnSendRequest
            // 
            this.btnSendRequest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSendRequest.Location = new System.Drawing.Point(112, 77);
            this.btnSendRequest.Name = "btnSendRequest";
            this.btnSendRequest.Size = new System.Drawing.Size(110, 23);
            this.btnSendRequest.TabIndex = 2;
            this.btnSendRequest.Text = "Send Request";
            this.btnSendRequest.UseVisualStyleBackColor = true;
            this.btnSendRequest.Click += new System.EventHandler(this.btnSendRequest_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 77);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(94, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblResult
            // 
            this.lblResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblResult.Location = new System.Drawing.Point(12, 48);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(210, 26);
            this.lblResult.TabIndex = 6;
            this.lblResult.Text = "lblResult";
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormAddContact
            // 
            this.AcceptButton = this.btnSendRequest;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(234, 112);
            this.ControlBox = false;
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSendRequest);
            this.Controls.Add(this.tbxContactName);
            this.Controls.Add(this.lblContactName);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(250, 150);
            this.Name = "FormAddContact";
            this.Text = "Add Contact";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblContactName;
        private System.Windows.Forms.TextBox tbxContactName;
        private System.Windows.Forms.Button btnSendRequest;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Timer timer1;
    }
}