using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcidLookup.Entities;
using FluentNHibernate.Mapping;

namespace AcidLookup.Mappings {
    class PhoneNumberMap : ClassMap<PhoneNumber> {
        public PhoneNumberMap() {
            Id(pn => pn.Number).Not.Nullable();
            References(pn => pn.Contact);
        }
    }
}