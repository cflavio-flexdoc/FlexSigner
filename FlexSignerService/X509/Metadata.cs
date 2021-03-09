using System;
using System.Collections.Generic;
using System.Text;


using System.Collections;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.xml.xmp;
using iTextSharp.text;


using System.Linq;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

using System.Security.Cryptography.Pkcs;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Math;

///
/// <summary>
/// This Library allows you to sign a PDF document using iTextSharp
/// </summary>
/// <author>Alaa-eddine KADDOURI</author>
///
///

namespace FlexSignerService
{
    /// <summary>
    /// This is a holder class for PDF metadata
    /// </summary>
    public class MetaData
    {
        private Hashtable info = new Hashtable();

        public Hashtable Info
        {
            get { return info; }
            set { info = value; }
        }

        public string Author
        {
            get { return (string)info["Author"]; }
            set { info.Add("Author", value); }
        }
        public string Title
        {
            get { return (string)info["Title"]; }
            set { info.Add("Title", value); }
        }
        public string Subject
        {
            get { return (string)info["Subject"]; }
            set { info.Add("Subject", value); }
        }
        public string Keywords
        {
            get { return (string)info["Keywords"]; }
            set { info.Add("Keywords", value); }
        }
        public string Producer
        {
            get { return (string)info["Producer"]; }
            set { info.Add("Producer", value); }
        }

        public string Creator
        {
            get { return (string)info["Creator"]; }
            set { info.Add("Creator", value); }
        }

        public Hashtable getMetaData()
        {
            return this.info;
        }
        public byte[] getStreamedMetaData()
        {
            MemoryStream os = new System.IO.MemoryStream();
            XmpWriter xmp = new XmpWriter(os, this.info);
            xmp.Close();
            return os.ToArray();
        }

    }

}