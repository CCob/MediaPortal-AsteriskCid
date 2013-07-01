/*    This file is part of AsteriskCid.

    AsteriskCid is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AsteriskCid is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AsteriskCid.  If not, see <http://www.gnu.org/licenses/>*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NHibernate;
using AcidLookup.Entities;
using System.IO;

namespace AcidLookup {
    public partial class AcidLookupConfigurationDialog : Form {

        #region Properties 

        BindingList<Contact> contactsBinding;
        public BindingList<Contact> Contacts {
            get { return contactsBinding; }
        }

        public string GoogleLogin {
            get { return textGoogleLogin.Text; }
        }
    
        public string GooglePassword {
            get { return textGooglePassword.Text; }
        }

        public bool GoogleSync {
            get { return chkSyncGoogleContacts.Checked; }
        }

        public string AmiUser {
            get { return textAMIUser.Text; }
        }

        public string AmiPassword {
            get { return textAMIPassword.Text; }
        }

        public string AmiHost {
            get { return textAMIHost.Text; }
        }

        public string ChannelFilter {
            get { return textChannelFilter.Text; }
        }

        public string MonitorMailbox {
            get { return textMailbox.Text; }
        }

        #endregion

        #region Constructors

        public AcidLookupConfigurationDialog(string amiUser, string amiPassword, string amiHost, string channelFilter,
                                             string googleLogin,string googlePassword, bool googleSync, string mailboxExt, IList<Contact> contacts) {
            InitializeComponent();

            this.contactsBinding = new BindingList<Contact>(contacts);
            this.contactsBinding.AllowEdit = true;
            this.contactsBinding.AllowNew = true;
            this.contactsBinding.AllowRemove = true;

            this.nhDataSource.DataSource = this.contactsBinding;

            this.textAMIUser.Text = amiUser;
            this.textAMIPassword.Text = amiPassword;
            this.textAMIHost.Text = amiHost;
            this.textChannelFilter.Text = channelFilter;

            this.textGoogleLogin.Text = googleLogin;
            this.textGooglePassword.Text = googlePassword;
            this.chkSyncGoogleContacts.Checked = googleSync;

            this.textMailbox.Text = mailboxExt;
        }

        #endregion

        #region Events

        private void label1_Click(object sender, EventArgs e) {

        }

        private void chkSyncGoogleContacts_CheckedChanged(object sender, EventArgs e) {
            textGooglePassword.Enabled = textGoogleLogin.Enabled = chkSyncGoogleContacts.Checked;
        }

        private void btnSave_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        private void gvContacts_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if(e.ColumnIndex == 3){
                if (this.ofdSelectPhoto.ShowDialog(this) == DialogResult.OK) {                    
                    BinaryReader br = new BinaryReader(new FileStream(ofdSelectPhoto.FileName,FileMode.Open,FileAccess.Read));
                    ((Contact)nhDataSource.Current).Photo = br.ReadBytes((int)br.BaseStream.Length);
                }
            } else if (e.ColumnIndex == 2) {
                EditPhoneNumbers epn = new EditPhoneNumbers((Contact)nhDataSource.Current);
                epn.ShowDialog(this);
            }
        }

        private void gvContacts_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {
            //DataGridViewComboBoxCell box = gvContacts.Rows[e.RowIndex].Cells[2] as DataGridViewComboBoxCell;
            //box.DataSource = ((Contact)nhDataSource.Current).PhoneNumbersBinding;
        }

        private void chkSyncGoogleContacts_CheckedChanged_1(object sender, EventArgs e) {
            textGooglePassword.Enabled = textGoogleLogin.Enabled = chkSyncGoogleContacts.Checked;
        }
    }
}
