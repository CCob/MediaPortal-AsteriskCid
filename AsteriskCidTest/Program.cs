using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcidLookup.Entities;
using System.Drawing;

namespace AsteriskCidTest {
    class Program {
        [STAThread]
        static void Main(string[] args) {

            //log4net.Config.XmlConfigurator.Configure();

            AcidLookup.AcidLookupPlugin plugin = new AcidLookup.AcidLookupPlugin();

            //plugin.ShowPlugin();

            //plugin.Start();

            //plugin.SyncGoogleContacts();

            plugin.ShowPlugin();

            Console.ReadLine();
        }
    }
}
