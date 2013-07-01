using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AcidLookup.Entities;

namespace AcidLookup {
    public partial class EditPhoneNumbers : Form {

        Contact contact;

        public EditPhoneNumbers() {
            InitializeComponent();
        }

        public EditPhoneNumbers(Contact c) {
            InitializeComponent();
            this.contact = c;
            phonesBindingBindingSource.DataSource = c.PhoneNumbers;
            phonesBindingBindingSource.AllowNew = true;
        }

        protected override void OnClosing(CancelEventArgs e) {

            foreach (PhoneNumber pn in phonesBindingBindingSource.List) {
                
                if (!contact.PhoneNumbers.Contains(pn) && !String.IsNullOrEmpty(pn.Number) ) {
                    contact.PhoneNumbers.Add(pn);
                }

                if (pn.Contact == null)
                    pn.Contact = this.contact;

            }

            base.OnClosing(e);
        }
    }
}
