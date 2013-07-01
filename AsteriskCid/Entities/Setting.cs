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
