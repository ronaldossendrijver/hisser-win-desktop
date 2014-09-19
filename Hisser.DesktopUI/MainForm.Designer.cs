namespace Hisser.DesktopUI
{
    partial class MainForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._messageTextBox = new System.Windows.Forms.TextBox();
            this.contactGridView = new System.Windows.Forms.DataGridView();
            this.invitationContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.acceptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkMessageTimer = new System.Windows.Forms.Timer(this.components);
            this.acceptButton = new System.Windows.Forms.Button();
            this.buttonAttachment = new System.Windows.Forms.Button();
            this._clearServerButton = new System.Windows.Forms.Button();
            this.settingsButton = new System.Windows.Forms.Button();
            this.inviteButton = new System.Windows.Forms.Button();
            this._sendButton = new System.Windows.Forms.Button();
            this.errorPictureBox = new System.Windows.Forms.PictureBox();
            this.messagePanel = new Hisser.DesktopUI.MessagePanel();
            ((System.ComponentModel.ISupportInitialize)(this.contactGridView)).BeginInit();
            this.invitationContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // _messageTextBox
            // 
            this._messageTextBox.AcceptsReturn = true;
            this._messageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._messageTextBox.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._messageTextBox.Location = new System.Drawing.Point(270, 407);
            this._messageTextBox.Multiline = true;
            this._messageTextBox.Name = "_messageTextBox";
            this._messageTextBox.Size = new System.Drawing.Size(450, 22);
            this._messageTextBox.TabIndex = 0;
            this._messageTextBox.Visible = false;
            this._messageTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._messageTextBox_KeyPress);
            // 
            // contactGridView
            // 
            this.contactGridView.AllowUserToAddRows = false;
            this.contactGridView.AllowUserToDeleteRows = false;
            this.contactGridView.AllowUserToResizeColumns = false;
            this.contactGridView.AllowUserToResizeRows = false;
            this.contactGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.contactGridView.BackgroundColor = System.Drawing.Color.DimGray;
            this.contactGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.contactGridView.CausesValidation = false;
            this.contactGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.contactGridView.ColumnHeadersVisible = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(215)))), ((int)(((byte)(215)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.contactGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this.contactGridView.Location = new System.Drawing.Point(12, 12);
            this.contactGridView.MultiSelect = false;
            this.contactGridView.Name = "contactGridView";
            this.contactGridView.ReadOnly = true;
            this.contactGridView.RowHeadersVisible = false;
            this.contactGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.contactGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.contactGridView.Size = new System.Drawing.Size(244, 391);
            this.contactGridView.TabIndex = 5;
            this.contactGridView.SelectionChanged += new System.EventHandler(this.contactGridView_SelectionChanged);
            // 
            // invitationContextMenuStrip
            // 
            this.invitationContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.acceptToolStripMenuItem});
            this.invitationContextMenuStrip.Name = "invitationContextMenuStrip";
            this.invitationContextMenuStrip.Size = new System.Drawing.Size(112, 26);
            // 
            // acceptToolStripMenuItem
            // 
            this.acceptToolStripMenuItem.Name = "acceptToolStripMenuItem";
            this.acceptToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.acceptToolStripMenuItem.Text = "Accept";
            this.acceptToolStripMenuItem.Click += new System.EventHandler(this.acceptToolStripMenuItem_Click);
            // 
            // checkMessageTimer
            // 
            this.checkMessageTimer.Interval = 10000;
            this.checkMessageTimer.Tick += new System.EventHandler(this.checkMessageTimer_Tick);
            // 
            // acceptButton
            // 
            this.acceptButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("acceptButton.BackgroundImage")));
            this.acceptButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.acceptButton.FlatAppearance.BorderSize = 0;
            this.acceptButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.acceptButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.acceptButton.Location = new System.Drawing.Point(64, 409);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Size = new System.Drawing.Size(20, 20);
            this.acceptButton.TabIndex = 21;
            this.acceptButton.UseVisualStyleBackColor = true;
            this.acceptButton.Visible = false;
            this.acceptButton.Click += new System.EventHandler(this.acceptButton_Click);
            // 
            // buttonAttachment
            // 
            this.buttonAttachment.BackgroundImage = global::Hisser.DesktopUI.Properties.Resources.camera;
            this.buttonAttachment.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buttonAttachment.FlatAppearance.BorderSize = 0;
            this.buttonAttachment.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.buttonAttachment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAttachment.Location = new System.Drawing.Point(752, 407);
            this.buttonAttachment.Name = "buttonAttachment";
            this.buttonAttachment.Size = new System.Drawing.Size(20, 17);
            this.buttonAttachment.TabIndex = 19;
            this.buttonAttachment.UseVisualStyleBackColor = true;
            this.buttonAttachment.Visible = false;
            this.buttonAttachment.Click += new System.EventHandler(this.buttonAttachment_Click);
            // 
            // _clearServerButton
            // 
            this._clearServerButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_clearServerButton.BackgroundImage")));
            this._clearServerButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this._clearServerButton.FlatAppearance.BorderSize = 0;
            this._clearServerButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this._clearServerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._clearServerButton.Location = new System.Drawing.Point(210, 409);
            this._clearServerButton.Name = "_clearServerButton";
            this._clearServerButton.Size = new System.Drawing.Size(20, 20);
            this._clearServerButton.TabIndex = 4;
            this._clearServerButton.UseVisualStyleBackColor = true;
            this._clearServerButton.Visible = false;
            this._clearServerButton.Click += new System.EventHandler(this._clearServerButton_Click);
            // 
            // settingsButton
            // 
            this.settingsButton.BackgroundImage = global::Hisser.DesktopUI.Properties.Resources.settings;
            this.settingsButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.settingsButton.FlatAppearance.BorderSize = 0;
            this.settingsButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsButton.Location = new System.Drawing.Point(12, 409);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(20, 20);
            this.settingsButton.TabIndex = 2;
            this.settingsButton.UseVisualStyleBackColor = true;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // inviteButton
            // 
            this.inviteButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("inviteButton.BackgroundImage")));
            this.inviteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.inviteButton.FlatAppearance.BorderSize = 0;
            this.inviteButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.inviteButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this.inviteButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.inviteButton.Location = new System.Drawing.Point(38, 409);
            this.inviteButton.Name = "inviteButton";
            this.inviteButton.Size = new System.Drawing.Size(20, 20);
            this.inviteButton.TabIndex = 3;
            this.inviteButton.UseVisualStyleBackColor = true;
            this.inviteButton.Click += new System.EventHandler(this.inviteButton_Click);
            // 
            // _sendButton
            // 
            this._sendButton.BackgroundImage = global::Hisser.DesktopUI.Properties.Resources.email;
            this._sendButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this._sendButton.FlatAppearance.BorderSize = 0;
            this._sendButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DimGray;
            this._sendButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._sendButton.Location = new System.Drawing.Point(726, 407);
            this._sendButton.Name = "_sendButton";
            this._sendButton.Size = new System.Drawing.Size(20, 20);
            this._sendButton.TabIndex = 1;
            this._sendButton.UseVisualStyleBackColor = true;
            this._sendButton.Visible = false;
            this._sendButton.Click += new System.EventHandler(this._sendButton_Click);
            // 
            // errorPictureBox
            // 
            this.errorPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.errorPictureBox.Image = global::Hisser.DesktopUI.Properties.Resources.stop;
            this.errorPictureBox.Location = new System.Drawing.Point(236, 409);
            this.errorPictureBox.Name = "errorPictureBox";
            this.errorPictureBox.Size = new System.Drawing.Size(20, 20);
            this.errorPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.errorPictureBox.TabIndex = 23;
            this.errorPictureBox.TabStop = false;
            this.errorPictureBox.Visible = false;
            // 
            // messagePanel
            // 
            this.messagePanel.AutoScroll = true;
            this.messagePanel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messagePanel.Location = new System.Drawing.Point(270, 12);
            this.messagePanel.Name = "messagePanel";
            this.messagePanel.Size = new System.Drawing.Size(502, 389);
            this.messagePanel.TabIndex = 20;
            this.messagePanel.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(268, 441);
            this.Controls.Add(this.errorPictureBox);
            this.Controls.Add(this.acceptButton);
            this.Controls.Add(this.messagePanel);
            this.Controls.Add(this.buttonAttachment);
            this.Controls.Add(this._clearServerButton);
            this.Controls.Add(this.settingsButton);
            this.Controls.Add(this.inviteButton);
            this.Controls.Add(this.contactGridView);
            this.Controls.Add(this._sendButton);
            this.Controls.Add(this._messageTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hisser";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.contactGridView)).EndInit();
            this.invitationContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _messageTextBox;
        private System.Windows.Forms.Button _sendButton;
        private System.Windows.Forms.DataGridView contactGridView;
        private System.Windows.Forms.Button inviteButton;
        private System.Windows.Forms.Button settingsButton;
        private System.Windows.Forms.ContextMenuStrip invitationContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem acceptToolStripMenuItem;
        private System.Windows.Forms.Timer checkMessageTimer;
        private System.Windows.Forms.Button _clearServerButton;
        private System.Windows.Forms.Button buttonAttachment;
        private MessagePanel messagePanel;
        private System.Windows.Forms.Button acceptButton;
        private System.Windows.Forms.PictureBox errorPictureBox;
    }
}

