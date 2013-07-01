using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using AcidLookup.Entities;

namespace AcidLookup.Mappings {
    class SettingMap : ClassMap<Setting> {
        public SettingMap() {
            Id(s => s.Name);
            Map(s => s.Value);
        }
    }
}
