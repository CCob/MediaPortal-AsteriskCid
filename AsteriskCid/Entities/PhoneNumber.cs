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
