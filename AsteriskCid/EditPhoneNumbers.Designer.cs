namespace AcidLookup {
    partial class EditPhoneNumbers {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.gvNumbers = new System.Windows.Forms.DataGridView();
            this.numberDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contactDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.phonesBindingBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gvNumbers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.phonesBindingBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // gvNumbers
            // 
            this.gvNumbers.AutoGenerateColumns = false;
            this.gvNumbers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gvNumbers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvNumbers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.numberDataGridViewTextBoxColumn,
            this.contactDataGridViewTextBoxColumn});
            this.gvNumbers.DataSource = this.phonesBindingBindingSource;
            this.gvNumbers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gvNumbers.Location = new System.Drawing.Point(0, 0);
            this.gvNumbers.Name = "gvNumbers";
            this.gvNumbers.Size = new System.Drawing.Size(374, 293);
            this.gvNumbers.TabIndex = 0;
            // 
            // numberDataGridViewTextBoxColumn
            // 
            this.numberDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.numberDataGridViewTextBoxColumn.DataPropertyName = "Number";
            this.numberDataGridViewTextBoxColumn.HeaderText = "Number";
            this.numberDataGridViewTextBoxColumn.Name = "numberDataGridViewTextBoxColumn";
            // 
            // contactDataGridViewTextBoxColumn
            // 
            this.contactDataGridViewTextBoxColumn.DataPropertyName = "Contact";
            this.contactDataGridViewTextBoxColumn.HeaderText = "Contact";
            this.contactDataGridViewTextBoxColumn.Name = "contactDataGridViewTextBoxColumn";
            this.contactDataGridViewTextBoxColumn.Visible = false;
            // 
            // phonesBindingBindingSource
            // 
            this.phonesBindingBindingSource.DataSource = typeof(AcidLookup.Bindings.PhonesBinding);
            // 
            // EditPhoneNumbers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 293);
            this.Controls.Add(this.gvNumbers);
            this.Name = "EditPhoneNumbers";
            this.Text = "Edit Contact Phone Numbers";
            ((System.ComponentModel.ISupportInitialize)(this.gvNumbers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.phonesBindingBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gvNumbers;
        private System.Windows.Forms.DataGridViewTextBoxColumn numberDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn contactDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource phonesBindingBindingSource;
    }
}