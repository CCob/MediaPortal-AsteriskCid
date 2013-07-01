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
using System.Linq;
using System.Text;

namespace AcidLookup.Entities {
    public class PhoneNumber {

        public virtual string Number { get; set; }

        public virtual Contact Contact { get; set; }

        public PhoneNumber() {
        }

        public PhoneNumber(string phoneNumber, Contact contact) {
            this.Number = phoneNumber;
            this.Contact = contact;
        }

        public override string ToString() {
            return this.Number;
        }
    }
}
