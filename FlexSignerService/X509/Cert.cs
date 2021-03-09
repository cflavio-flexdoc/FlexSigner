using iTextSharp.text.pdf;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace FlexSignerService
{
    public class Cert
    {
        private readonly Log _log = GenericSingleton<Log>.GetInstance();

        #region Attributes

        private string path = "";
        private string password = "";
        private AsymmetricKeyParameter akp;
        private Org.BouncyCastle.X509.X509Certificate[] chain;

        private string certNum = "";
        private string certName = "";
        private string certThumb = "";

        public int NumberOfCertificatesFound = 0;

        X509Store x509Store;
        //IList<Org.BouncyCastle.X509.X509Certificate> chain;
        IOcspClient ocspClient;
        ITSAClient tsaClient;
        //IList<ICrlClient> crlList;
        X509Certificate2 pk;

        #endregion


        #region Accessors
        public Org.BouncyCastle.X509.X509Certificate[] Chain
        {
            get { return chain; }
        }
        public AsymmetricKeyParameter Akp
        {
            get { return akp; }
        }

        public string Path
        {
            get { return path; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string CertNum
        {
            get { return certNum; }
            set { certNum = value; }
        }

        public string CertName
        {
            get { return certName; }
            set { certName = value; }
        }

        public string CertThumb
        {
            get { return certThumb; }
            set { certThumb = value; }
        }

        #endregion


        public static AsymmetricKeyParameter TransformRSAPrivateKey(AsymmetricAlgorithm privateKey)
        {
            RSACryptoServiceProvider prov = privateKey as RSACryptoServiceProvider;
            RSAParameters parameters = prov.ExportParameters(true);

            return new RsaPrivateCrtKeyParameters(
                new BigInteger(1, parameters.Modulus),
                new BigInteger(1, parameters.Exponent),
                new BigInteger(1, parameters.D),
                new BigInteger(1, parameters.P),
                new BigInteger(1, parameters.Q),
                new BigInteger(1, parameters.DP),
                new BigInteger(1, parameters.DQ),
                new BigInteger(1, parameters.InverseQ));
        }


        public void TestCertificate()
        {
            X509Store store = new X509Store("MY", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "Test Certificate Select", "Select a certificate from the following list to get information on that certificate", X509SelectionFlag.MultiSelection);
            Console.WriteLine("Number of certificates: {0}{1}", scollection.Count, Environment.NewLine);

            foreach (X509Certificate2 x509 in scollection)
            {
                try
                {
                    byte[] rawdata = x509.RawData;
                    Console.WriteLine("Content Type: {0}{1}", X509Certificate2.GetCertContentType(rawdata), Environment.NewLine);
                    Console.WriteLine("Friendly Name: {0}{1}", x509.FriendlyName, Environment.NewLine);
                    Console.WriteLine("Certificate Verified?: {0}{1}", x509.Verify(), Environment.NewLine);
                    Console.WriteLine("Simple Name: {0}{1}", x509.GetNameInfo(X509NameType.SimpleName, true), Environment.NewLine);
                    Console.WriteLine("Signature Algorithm: {0}{1}", x509.SignatureAlgorithm.FriendlyName, Environment.NewLine);
                    Console.WriteLine("Public Key: {0}{1}", x509.PublicKey.Key.ToXmlString(false), Environment.NewLine);
                    Console.WriteLine("Certificate Archived?: {0}{1}", x509.Archived, Environment.NewLine);
                    Console.WriteLine("Length of Raw Data: {0}{1}", x509.RawData.Length, Environment.NewLine);
                    X509Certificate2UI.DisplayCertificate(x509);
                    x509.Reset();
                }
                catch (CryptographicException)
                {
                    Console.WriteLine("Information could not be written out for this certificate.");
                }
            }
            store.Close();
        }

        public Cert(string certNum, string certName, string certThumb)
        {
            this.certNum = certNum;
            this.certName = certName;
            this.certThumb = certThumb;
        }

        public bool LocateCert()
        {
            x509Store = new X509Store(StoreLocation.LocalMachine);
            x509Store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificates = x509Store.Certificates;
            chain = new List<Org.BouncyCastle.X509.X509Certificate>().ToArray();
            List<Org.BouncyCastle.X509.X509Certificate> tempChain = new List<Org.BouncyCastle.X509.X509Certificate>();
            pk = null;
                        
            NumberOfCertificatesFound = certificates.Count;

            bool found = false;

            if (certificates.Count > 0)
            {
                X509Certificate2Enumerator certificatesEn = certificates.GetEnumerator();
                int contCert = 0;
                while (true)
                {
                    contCert++;
                    certificatesEn.MoveNext();
                    pk = certificatesEn.Current;
                    //_log.Debug(pk.FriendlyName.ToString());

                    if ((certNum.Trim() != "" && pk.Subject.Contains(certNum)) ||
                        (certName.Trim() != "" && pk.Subject.Contains(certName)) ||
                        (certThumb.Trim() != "" && pk.Subject.Contains(certThumb)))
                    {
                        DateTime dt = pk.NotAfter;
                        if (dt > System.DateTime.UtcNow)
                        {
                            this.akp = TransformRSAPrivateKey(pk.PrivateKey);

                            found = true;
                            break;
                        }
                        else
                        {
                            if(certNum.Trim()!="")
                                _log.Error("Certificado: Num [" + this.certNum + "] expirado!");
                            if (certNum.Trim() != "")
                                _log.Error("Certificado: Name [" + this.certName + "] expirado!");
                            if (certNum.Trim() != "")
                                _log.Error("Certificado: Thumb [" + this.certThumb + "] expirado!");
                        }
                    }

                    if (contCert >= certificates.Count)
                        break;
                }

                if (!found)
                {
                    if (certNum.Trim() != "")
                        _log.Error("Certificado: Num [" + this.certNum + "] não encontrado!");
                    if (certNum.Trim() != "")
                        _log.Error("Certificado: Name [" + this.certName + "] não encontrado!");
                    if (certNum.Trim() != "")
                        _log.Error("Certificado: Thumb [" + this.certThumb + "] não encontrado");
                    return false;
                }

                X509Chain x509chain = new X509Chain();
                x509chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                x509chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                x509chain.Build(pk);

                foreach (X509ChainElement x509ChainElement in x509chain.ChainElements)
                {
                    tempChain.Add(DotNetUtilities.FromX509Certificate(x509ChainElement.Certificate));
                }

                chain = tempChain.ToArray();
            }
            x509Store.Close();

            //ocspClient = new OcspClientBouncyCastle();

            tsaClient = null;
            //for (int i = 0; i < chain.Count; i++)
            //{
            //    Org.BouncyCastle.X509.X509Certificate cert = chain[i];
            //    String tsaUrl = CertificateUtil.GetTSAURL(cert);
            //    if (tsaUrl != null)
            //    {
            //        tsaClient = new TSAClientBouncyCastle(tsaUrl);
            //        break;
            //    }
            //}
            //crlList = new List<ICrlClient>();
            //crlList.Add(new CrlClientOnline(chain));

            //crlList.Clear();

            return true;
        }
    }
}
