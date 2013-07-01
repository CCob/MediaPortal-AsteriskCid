using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcidLookup.Entities;
using FluentNHibernate.Mapping;

namespace AcidLookup.Mappings {
    class ContactMap : ClassMap<Contact> {
        public ContactMap() {
            Id(c => c.Id).GeneratedBy.Native();
            Map(c => c.FullName);
            Map(c => c.Photo);
            HasMany(c => c.PhoneNumbers).AsSet().Cascade.AllDeleteOrphan();
        }
    }
}
