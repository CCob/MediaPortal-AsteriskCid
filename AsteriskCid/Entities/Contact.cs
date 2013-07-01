using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;

namespace AcidLookup.Entities {
    public class Contact {

        static Image.GetThumbnailImageAbort notUsed =
                        new Image.GetThumbnailImageAbort(ThumbnailCallback);

        static Bitmap largePhotoNone = (Bitmap)Bitmap.FromStream(Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream("AcidLookup.Icons.NoPhoto.png"));
        public static Bitmap LargePhotoNone{
            get { return largePhotoNone; }
        }

        static Bitmap smallPhotoNone = (Bitmap)largePhotoNone.GetThumbnailImage(48, 48, notUsed, IntPtr.Zero);
        public static Bitmap SmallPhotoNone {
            get { return smallPhotoNone; }
        }

        static bool ThumbnailCallback() {
            return false;
        }
        
        public Contact() {
        }

        public Contact(string fullName) {
            this.FullName = fullName;
        }

        public virtual long Id { get; set; }

        public virtual string FullName {get; set;}

        byte[] photoBytes;
        public virtual byte[] Photo {

            get {
                return photoBytes;
            }
            set {
                photoBytes = value;
                if (photoBytes != null) {
                    MemoryStream ms = new MemoryStream(photoBytes);

                    largePhoto = (Bitmap)Bitmap.FromStream(ms);
                    smallPhoto = (Bitmap)largePhoto.GetThumbnailImage(48,48,notUsed,IntPtr.Zero);
                }
            }
        }

        Bitmap smallPhoto;
        public virtual Bitmap SmallPhoto {
            get {
                if (smallPhoto != null)
                    return smallPhoto;
                else
                    return smallPhotoNone;
            }
        }

        Bitmap largePhoto;
        public virtual Bitmap PhotoLarge {
            get {
                if (largePhoto != null)
                    return largePhoto;
                else
                    return largePhotoNone;
            }
        }

        public virtual BindingList<PhoneNumber> PhoneNumbersBinding {
            get {
                BindingList<PhoneNumber> pnbl = new BindingList<PhoneNumber>(phoneNumbers.ToList());
                pnbl.AllowEdit = true;
                pnbl.AllowNew = true;
                pnbl.AllowRemove = true;
                return pnbl;
            }
        }

        ICollection<PhoneNumber> phoneNumbers = new HashSet<PhoneNumber>();
        public virtual ICollection<PhoneNumber> PhoneNumbers {
            get { return phoneNumbers; }
            set { phoneNumbers = value; }
        }

        public virtual string PhoneNumbersString {
            get {
                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (PhoneNumber pn in phoneNumbers) {
                    sb.Append(String.Format("{0}{1}", first ? "" : ", ", pn.Number));
                    first = false;
                }
                return sb.ToString();
            }
        }

        public virtual void SetPhoto(Stream photoStream) {
            byte[] buffer = new byte[1024];
            using (MemoryStream ms = new MemoryStream()) {
                int read;
                while ((read = photoStream.Read(buffer, 0, buffer.Length)) > 0) {
                    ms.Write(buffer, 0, read);
                }
                Photo = ms.ToArray();
            }
        }
    }
}
