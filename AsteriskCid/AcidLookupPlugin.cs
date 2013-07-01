using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using AcidLookup.Entities;
using NHibernate.Tool.hbm2ddl;
using Google.GData.Client;
using System.IO;
using Asterisk.NET.Manager;
using MediaPortal.GUI.Library;
using System.Drawing;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using System.Reflection;
using log4net.Repository.Hierarchy;
using log4net.Filter;
using System.Windows.Forms;
using MediaPortal.Player;
using log4net.Core;
using System.Threading;
using Asterisk.NET.Manager.Action;
using Asterisk.NET.Manager.Response;
using System.Drawing.Imaging;

namespace AcidLookup {

    [PluginIcons(
    "AcidLookup.Icons.AcidLookupIcon.png",
    "AcidLookup.Icons.AcidLookupIconDisabled.png")]
    public class AcidLookupPlugin : IPlugin, ISetupForm {

        #region General Functions & Properties

        delegate void ShowIncomingCallDialogDelagate(string name, string number, Bitmap photo);
        delegate void HideIncomingCallDialogDelagate();

        System.Threading.Timer connectTimer;

        System.Threading.Timer checkMessagesTimer;

        log4net.ILog log = log4net.LogManager.GetLogger("AcidLookupPlugin.Core");
        
        HashSet<string> monitoredChannels = new HashSet<string>();

        ISessionFactory SessionFactory { get; set; }

        ManagerConnection AsteriskManager { get; set; }

        string ThumbFolder { get; set; }

        bool InitComplete { get; set; }

        /// <summary>
        /// Sets up the logging facility for the Plugin, which is powered by .NET
        /// </summary>
        void ConfigureLogging() {

            //Create a rolling log file in the MediaPortal log folder
            log4net.Appender.RollingFileAppender rfa = new log4net.Appender.RollingFileAppender();
            rfa.File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Team MediaPortal\MediaPortal\log\AcidLookup.log");
            rfa.MaxFileSize = 10 * 1024 * 1024;
            rfa.MaxSizeRollBackups = 5;
            rfa.Layout = new log4net.Layout.PatternLayout("%date [%thread] %-5level %logger - %message%newline");

            //add a log match filter to our appender above
            //that will only log AcidLookupPlugin.* loggers
            LoggerMatchFilter sourceFilter = new LoggerMatchFilter();
            sourceFilter.LoggerToMatch = "AcidLookupPlugin";
            rfa.AddFilter(sourceFilter);
            // now add the deny all filter to end of the chain
            DenyAllFilter denyAllFilter = new DenyAllFilter();
            rfa.AddFilter(denyAllFilter);
            // activate the options
            rfa.ActivateOptions();

            ((Logger)log4net.LogManager.GetLogger("AcidLookupPlugin").Logger).AddAppender(rfa);

            //Some plugins may already be using log4net so
            //don't configure it unless it hasn't been configured before
            if (!log4net.LogManager.GetRepository().Configured) {
                log4net.Config.BasicConfigurator.Configure();
            }

            //drop the NHibernate level to Notice
            ((Logger)log4net.LogManager.GetLogger("NHibernate").Logger).Level = Level.Notice;
        }

        /// <summary>
        /// Overall initialiser function which setups logging, creates the 
        /// NHibernate session factory and connects to the Asterisk AMI interface
        /// </summary>
        void Init() {

            ConfigureLogging();

            log.Debug("Asterisk Caller ID Lookup starting...");

            try {

                //Initalise caller thumbnail folder and create if neccessary 
                ThumbFolder = MediaPortal.Configuration.Config.GetFolder(Config.Dir.Thumbs) + @"\Acid\";
                if (!Directory.Exists(ThumbFolder))
                    Directory.CreateDirectory(ThumbFolder);

                //create the NHibernate session factory wrapped around the Acid SQLite database
                SessionFactory = CreateSessionFactory();


                //setup the various Asterisk notifications loading
                //the settings from the database
                using (ISession session = SessionFactory.OpenSession()) {

                    AsteriskManager = new ManagerConnection(GetSetting(session, "ami_host", "localhost"),
                                                            5038,
                                                            GetSetting(session, "ami_user", "admin"),
                                                            GetSetting(session, "ami_password", "amp111"));

                    AsteriskManager.UnhandledEvent += new ManagerEventHandler(AsteriskManager_UnhandledEvent);
                    AsteriskManager.NewChannel += new NewChannelEventHandler(AsteriskManager_NewChannel);
                    AsteriskManager.Hangup += new HangupEventHandler(AsteriskManager_Hangup);
                    AsteriskManager.NewState += new NewStateEventHandler(AsteriskManager_NewState);
                    AsteriskManager.KeepAlive = true;
                    AsteriskManager.KeepAliveAfterAuthenticationFailure = true;

                    AsteriskManagerConnect(null);
                }

                log.Debug("Asterisk Caller ID Looukp started");

            } catch (Exception e) {
                log.Error("Failed to start Asterisk Caller ID Lookup", e);
            }

            InitComplete = true;
        }

        /// <summary>
        /// Helper function to fetch a name/value pair from the database
        /// </summary>
        /// <param name="session">The NHIbernate session to use</param>
        /// <param name="name">The name of the setting</param>
        /// <param name="value">The corresponding value for the setting</param>
        string GetSetting(ISession session, string name, string def) {
            Setting setting = session.Get<Setting>(name);

            if (setting != null) {
                log.DebugFormat("Configuration setting {1}:", name, setting.Value);
                return setting.Value;
            } else {
                log.DebugFormat("Configuration setting {1} not set, using default value {2}", name, def);
                return def;
            }
        }

        /// <summary>
        /// Helper function to save a name/value pair to the database
        /// </summary>
        /// <param name="session">The NHIbernate session to use</param>
        /// <param name="name">The name of the setting</param>
        /// <param name="value">The corresponding value for the setting</param>
        void SaveSetting(ISession session, string name, string value) {

            Setting setting = session.Get<Setting>(name);

            if (setting != null) {
                setting.Value = value;
                session.Update(setting);
            } else {
                setting = new Setting(name, value);
                session.Save(setting);
            }

            log.DebugFormat("Saved configuration setting {1}:", name, setting.Value);
        }

        /// <summary>
        /// Simply hides the incoming call dialog if it's the current active window
        /// </summary>
        void HideIncomingCallDialog() {

            if (GUIGraphicsContext.form.InvokeRequired) {
                HideIncomingCallDialogDelagate d = HideIncomingCallDialog;
                GUIGraphicsContext.form.Invoke(d);
                return;
            }
            
            GUIDialogNotify window = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
            if (window != null) {
                window.PageDestroy();
                log.DebugFormat("Hiden the call notification dialog");
            }
        }

        /// <summary>
        /// Shows the incoming call dialog inside MediaPortal.  Playback is paused whilst
        /// the dialog has focus, and resumed once closed
        /// </summary>
        /// <param name="name">The caller name</param>
        /// <param name="number">The telephone number for the caller</param>
        /// <param name="photo">The photo to be used for the caller</param>
        void ShowIncomingCallDialog(string name, string number, Bitmap photo) {

            if (GUIGraphicsContext.form.InvokeRequired) {
                ShowIncomingCallDialogDelagate d = ShowIncomingCallDialog;
                GUIGraphicsContext.form.Invoke(d, name, number, photo);
                return;
            }

            Log.Info("AcidLookup: Incoming call - Name: {0}, Number:{1}", name, number);

            try {
                GUIDialogNotify incomingCallDlg = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
                if (null != incomingCallDlg) {

                    if (g_Player.Playing) {
                        g_Player.Pause();
                    }

                    string thumbPath = ThumbFolder + number + ".png";

                    if(!File.Exists(thumbPath))
                        photo.Save(thumbPath,ImageFormat.Png);

                    incomingCallDlg.Reset();
                    incomingCallDlg.SetImage(thumbPath);
                    incomingCallDlg.SetImageDimensions(new Size(96, 96), true, true);
                    incomingCallDlg.SetHeading("Incoming Call");
                    incomingCallDlg.SetText(String.Format("Name: {0}, Number:{1}", name, number));
                    incomingCallDlg.TimeOut = 10;
                    incomingCallDlg.DoModal(GUIWindowManager.ActiveWindow);

                    if (g_Player.Paused) {
                        //resume playerback of paused player
                        g_Player.Pause();
                    }
                }
            } catch (Exception e) {
                log.Error("Failed to show incoming call notification window", e);
            }
        }

        #endregion

        #region NHibernate Functions

        /// <summary>
        /// Exposes an extra function to allow the NHibernate layer to
        /// be configured during the Fluent Nhibernate configuration below
        /// </summary>
        void NHibernateConfig(global::NHibernate.Cfg.Configuration cfg) {
            new SchemaUpdate(cfg)
              .Execute(false, true);
        }

        /// <summary>
        /// This plugin relies on Fluent NHibernate as an ORM layer between an SQLite
        /// database.  Fluent NHibernate is configured here
        /// </summary>
        private ISessionFactory CreateSessionFactory() {
            return Fluently.Configure()
                .Database(
                SQLiteConfiguration.Standard
                .UsingFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Team MediaPortal\MediaPortal\database\AcidLookup.db3"))
                )
                .Mappings(m =>
                            m.FluentMappings.AddFromAssemblyOf<Contact>())
                .ExposeConfiguration(NHibernateConfig)
                .BuildSessionFactory();
        }

        #endregion

        #region Asterisk Callback Functions

        /// <summary>
        /// Uses the Asterisk AMI interface to check a monitored
        /// extension mailbox for new messages if configured to do so.
        /// Sets up two MediaPortal properties that can be used by skins
        /// </summary>
        void AsteriskManagerCheckMessages(Object state) {

            using(ISession session = SessionFactory.OpenSession()){

                string mailbox = GetSetting(session, "mailbox_ext", "");
                string newMessages = "0";
                string oldMessages = "0";

                if(!String.IsNullOrEmpty(mailbox) && AsteriskManager.IsConnected() ){
                    MailboxCountAction mca = new MailboxCountAction(mailbox);
                    ManagerResponse response = AsteriskManager.SendAction(mca);

                    if (response is MailboxCountResponse) {
                        if (response.IsSuccess()) {
                            newMessages = ((MailboxCountResponse)response).NewMessages.ToString();
                            oldMessages = ((MailboxCountResponse)response).OldMessages.ToString();
                        }
                    } else if (response is ManagerError) {
                        log.ErrorFormat("Error whilst trying to fetch extension messages: {0}",((ManagerError)response).Message);
                    }
                }
            
                // Save some GUI properties for mailbox counts 
                GUIPropertyManager.SetProperty("#ACID.MailboxNew", newMessages);
                GUIPropertyManager.SetProperty("#ACID.MailboxOld", oldMessages);               
            }
        }

        /// <summary>
        /// Attempts to connect to the AMI interface on the configured
        /// Asterisk host.  If the connection is not successful, a timer is 
        /// started to try again within a 30s interval to this function.  On successful
        /// connection a message check timer is started with 60s intervals
        /// </summary>
        void AsteriskManagerConnect(Object state) {

            try {
                log.DebugFormat("Attempting to connect to asterisk AMI at {0} with user {1} and pass '***'",
                    AsteriskManager.Hostname, AsteriskManager.Username);

                AsteriskManager.Login();

                log.Info("Successfully connected to Asterisk server");
                this.checkMessagesTimer = new System.Threading.Timer(new System.Threading.TimerCallback(AsteriskManagerCheckMessages), null, 0, 60000);


            } catch (Asterisk.NET.Manager.TimeoutException te) {
                log.Error("Failed to connect to the Asterisk server", te);
                //try again in 30 seconds
                this.connectTimer = new System.Threading.Timer(new System.Threading.TimerCallback(AsteriskManagerConnect), null, 30000, Timeout.Infinite);
            }
            
        }

        void AsteriskManager_NewState(object sender, Asterisk.NET.Manager.Event.NewStateEvent e) {            
        }

        /// <summary>
        /// Listens for channel hangup, if the channel is associated with the incoming call dialog 
        /// then it hides the incoming call dialog  
        /// </summary>
        void AsteriskManager_Hangup(object sender, Asterisk.NET.Manager.Event.HangupEvent e) {
            if (monitoredChannels.Contains(e.Channel)) {
                monitoredChannels.Remove(e.Channel);
                HideIncomingCallDialog();
            }
        }

        /// <summary>
        /// Listens for new channels, if a channel filter has been specified then it is
        /// compared against the new channel.  With no filter or a filter match, then an
        /// attempt is made to fetch the correponding Contact and then display the incoming
        /// call dialog within MediaPortal
        /// </summary>
        void AsteriskManager_NewChannel(object sender, Asterisk.NET.Manager.Event.NewChannelEvent e) {

            log.Info("New Channel: " + e.Channel + ", Caller Number: " + e.CallerIdNum);

            //Setup defaults for unknown contacts and phone numbers
            Bitmap contactPhoto = (Bitmap)Contact.LargePhotoNone.Clone();
            string contactNumber = e.CallerIdNum;
            string contactName = "Unknown";

            using (ISession session = SessionFactory.OpenSession()) {

                string chanFilter = GetSetting(session, "channel_filter", "");

                //if the channel filter is empty (dont filer) or the new channel starts
                //with the filter string the show the notification, otherwise ignore
                if (String.IsNullOrEmpty(chanFilter) || e.Channel.StartsWith(chanFilter, StringComparison.CurrentCultureIgnoreCase)) {

                    //See if we can find a phone number within the database
                    PhoneNumber phoneNumber = session.Get<PhoneNumber>(e.CallerIdNum);

                    if (phoneNumber != null) {
                        //We have found a phone number, so use the contacts name
                        //and photo for the notification dialog
                        log.Info("Found contact with name " + phoneNumber.Contact.FullName);
                        contactName = phoneNumber.Contact.FullName;
                        contactPhoto = (Bitmap)phoneNumber.Contact.PhotoLarge.Clone();
                    }

                    //Show the dialog
                    monitoredChannels.Add(e.Channel);
                    ShowIncomingCallDialog(contactName, contactNumber, contactPhoto);
                } else {
                    log.Debug("New channel ignored due to channel filter");
                }
            }
        }

        void AsteriskManager_UnhandledEvent(object sender, Asterisk.NET.Manager.Event.ManagerEvent e) {
            log.Info("Event : " + e.GetType().Name);
        }

        #endregion

        #region Contact Functions

        public IList<Contact> GetContacts() {
            using (ISession session = SessionFactory.OpenSession()) {
                IList<Contact> contacts = session.CreateCriteria<Contact>().List<Contact>();
                return contacts;
            }
        }

        public void SyncGoogleContacts() {

            using (ISession session = SessionFactory.OpenSession()) {

                if (bool.Parse(GetSetting(session, "google_sync", "false"))) {

                    using (ITransaction trx = session.BeginTransaction()) {

                        try {

                            Google.Contacts.ContactsRequest cr = new Google.Contacts.ContactsRequest(
                                new RequestSettings("AsteriskCid", new GDataCredentials(
                                                                    GetSetting(session, "google_login", ""),
                                                                    GetSetting(session, "google_password", ""))));

                            Feed<Google.Contacts.Contact> f = cr.GetContacts();
                            f.AutoPaging = true;
                            foreach (Google.Contacts.Contact entry in f.Entries) {

                                if (entry.Name != null && entry.Name.FullName != null) {

                                    Contact acidContact = new Contact(entry.Name.FullName);

                                    if (entry.PhotoEtag != null) {
                                        acidContact.SetPhoto(cr.Service.Query(entry.PhotoUri));
                                    }

                                    foreach (Google.GData.Extensions.PhoneNumber number in entry.Phonenumbers) {

                                        if (session.Get<PhoneNumber>(number.Value) != null) {
                                            log.WarnFormat("Phone number {0} already exist, skipping this number", number.Value);
                                            continue;
                                        }

                                        PhoneNumber phoneNumber = new PhoneNumber(number.Value, acidContact);
                                        acidContact.PhoneNumbers.Add(phoneNumber);
                                        session.Save(phoneNumber);
                                    }

                                    if (acidContact.PhoneNumbers.Count > 0)
                                        session.Save(acidContact);
                                }
                            }

                            trx.Commit();

                        } catch (Exception e) {
                            log.Error("Failed to synchronize google contacts", e);
                            trx.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        #endregion

        #region IPlugin Members

        public void Start() {
            Init();
        }

        public void Stop() {
            AsteriskManager.Logoff();
        }

        #endregion

        #region ISetupForm Members

        public string Author() {
            return "CCob";
        }

        public bool CanEnable() {
            return true;
        }

        public bool DefaultEnabled() {
            return true;
        }

        public string Description() {
            return "Notifies MediaPortal users of incoming calls, with caller id and photo if available.";
        }

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) {
            strButtonText = String.Empty;
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return false;
        }

        public int GetWindowId() {
            return (int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY;
        }

        public bool HasSetup() {
            return true;
        }

        public string PluginName() {
            return "Asterisk Caller ID Notify";
        }

        /// <summary>
        /// Initalises the config dialog with settings stored in the database, 
        /// shows the configuration dialog, once the Save button is selected all
        /// settings are saved back to database.  User has a choice ti synchronise contacts  
        /// </summary>
        public void ShowPlugin() {

            if (InitComplete == false) {
                Init();
            }

            using (ISession configSession = SessionFactory.OpenSession()) {

                AcidLookupConfigurationDialog config = new AcidLookupConfigurationDialog(
                    GetSetting(configSession, "ami_user", "admin"),
                    GetSetting(configSession, "ami_password", "amp111"),
                    GetSetting(configSession, "ami_host", "localhost"),
                    GetSetting(configSession, "channel_filter", ""),
                    GetSetting(configSession, "google_login", "me@gmail.com"),
                    GetSetting(configSession, "google_password", ""),
                    bool.Parse(GetSetting(configSession, "google_sync", "false")),
                    GetSetting(configSession, "mailbox_ext",""),
                    configSession.CreateCriteria<Contact>().List<Contact>());

                using (ITransaction trx = configSession.BeginTransaction()) {

                    if (config.ShowDialog() == System.Windows.Forms.DialogResult.OK) {

                        //go through the BindingList contacts and
                        //check which contacts are new.
                        foreach (Contact c in config.Contacts) {
                            if (c.Id == 0) {
                                configSession.Save(c);
                            }
                        }
                        
                        SaveSetting(configSession, "ami_user", config.AmiUser);
                        SaveSetting(configSession, "ami_password", config.AmiPassword);
                        SaveSetting(configSession, "ami_host", config.AmiHost);
                        SaveSetting(configSession, "channel_filter", config.ChannelFilter);
                        SaveSetting(configSession, "google_login", config.GoogleLogin);
                        SaveSetting(configSession, "google_password", config.GooglePassword);
                        SaveSetting(configSession, "google_sync", config.GoogleSync.ToString());
                        SaveSetting(configSession, "mailbox_ext", config.MonitorMailbox);
                        trx.Commit();

                        //Now the settings have been saved, lets try and sync
                        //all the users google contacts locally
                        if (MessageBox.Show("Settings saved, do you want to synchronise your google contacts now?", "Asterisk Caller ID Sync", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) {
                            SyncGoogleContacts();
                        }

                    } else {
                        trx.Rollback();
                    }
                }

                try {

                    //Update Asterisk host information and reconnect
                    AsteriskManager.Hostname = GetSetting(configSession, "ami_host", "localhost");
                    AsteriskManager.Username = GetSetting(configSession, "ami_user", "admin");
                    AsteriskManager.Password = GetSetting(configSession, "ami_password", "amp111");
                    AsteriskManager.Logoff();
                    AsteriskManager.Login();

                    MessageBox.Show(String.Format("Asterisk & Google sync complete, you now have {0} contacts in your database", GetContacts().Count),
                                       "Asterisk Caller ID Lookup", MessageBoxButtons.OK, MessageBoxIcon.Information);

                } catch (LoggedException e) {
                    MessageBox.Show("Failed to synchronize google contacts, check credentials and try again: " + e.Message, "Asterisk Caller ID Lookup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } catch (AuthenticationFailedException) {
                    MessageBox.Show("Asterisk authentication failed, check credentials and try again", "Asterisk Caller ID Lookup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } catch (Asterisk.NET.Manager.TimeoutException) {
                    MessageBox.Show("Asterisk connection failed, please check host details and try again", "Asterisk Caller ID Lookup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    #endregion
}


