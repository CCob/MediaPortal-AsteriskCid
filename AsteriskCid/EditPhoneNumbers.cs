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
