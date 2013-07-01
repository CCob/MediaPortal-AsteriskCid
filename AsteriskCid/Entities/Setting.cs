using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AcidLookup.Entities {
    class Setting {

        public Setting() {
        }

        public Setting(string name, string value) {
            this.name = name;
            this.value = value;
        }

        string name;
        public virtual string Name {
            get { return name; }
            set { name = value; }
        }

        string value;
        public virtual string Value {
            get { return this.value; }
            set { this.value = value; }
        }
    }
}
