using System;
using System.Xml;
using System.IO;

namespace FlexSignerService
{
    public class FlexSigner
    {
        SignX509 signx509;

        private string cnpjCertificate = "";

        private string signInputPath = "";
        private string signOutputPath = "";
        private string signTempPath = "";

        private string signCycle = "";
        private string signDelay = "";

        private readonly Log _log = new Log();

        private System.Timers.Timer timer;
        public void Init()
        {
            _log.Debug("Init::Begin: " + System.DateTime.Now.ToString());

            string configFile = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\CONFIG.XML";

            if (!File.Exists(configFile))
            {
                IniFile.IniWriteValue(configFile, "SIGN", "Cycle", "10");
                IniFile.IniWriteValue(configFile, "SIGN", "Delay", "15");
                IniFile.IniWriteValue(configFile, "SIGN", "SignInputPath", @"C:\sign\input\");
                IniFile.IniWriteValue(configFile, "SIGN", "SignOutputPath", @"C:\sign\output\");
                IniFile.IniWriteValue(configFile, "SIGN", "SignTempPath", @"C:\sign\temp\");

                IniFile.IniWriteValue(configFile, "CERTIFICATE", "cnpj", "10583028000152");                
            }

            _log.Debug("Init: [1]");

            signCycle = IniFile.IniReadValue(configFile, "SIGN", "Cycle");
            if (signCycle == "")
                signCycle = "10";

            signDelay = IniFile.IniReadValue(configFile, "SIGN", "Delay");
            if (signDelay == "")
                signDelay = "10";

            signInputPath = IniFile.IniReadValue(configFile, "SIGN", "SignInputPath");
            signOutputPath = IniFile.IniReadValue(configFile, "SIGN", "SignOutputPath");
            signTempPath = IniFile.IniReadValue(configFile, "SIGN", "SignTempPath");

            _log.Debug("Init: [2]");

            cnpjCertificate = IniFile.IniReadValue(configFile, "CERTIFICATE", "cnpj");

            _log.Debug("Init: [3]");

            try
            {
                System.IO.Directory.CreateDirectory(signInputPath);
                System.IO.Directory.CreateDirectory(signOutputPath);
                System.IO.Directory.CreateDirectory(signTempPath);
            }
            catch (Exception e)
            {
                _log.Debug("Init: Error CreateDir:" + e.Message);
            }
        
            _log.Debug("SignInputPath:" + signInputPath);
            _log.Debug("SignOutputPath:" + signOutputPath);
            _log.Debug("SignTempPath:" + signTempPath);
            _log.Debug("SignCycle:" + signCycle);
            _log.Debug("SignDelay:" + signDelay);

            _log.Debug("Checking certificate");
            signx509 = new SignX509(cnpjCertificate);

            if (signx509.NumberOfCertificatesFound == 0)
            {
                _log.Debug("No certificates found: " + cnpjCertificate);
                return;
            }

            _log.Debug("Certificate Ok : [" + cnpjCertificate + "]");

            int cycle = (Convert.ToInt32("0" + signCycle))*1000;

            this.timer = new System.Timers.Timer(cycle);  // 30000 milliseconds = 30 seconds
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            this.timer.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            _log.Debug("Timer: " + System.DateTime.Now.ToString());
            ProcessSign();
            timer.Start();
        }

        private void ProcessSign()
        {
            try
            {
                string[] files = System.IO.Directory.GetFiles(signInputPath);

                _log.Debug("ProcessSign: Total Files [IN]: " + files.Length.ToString());

                foreach (string file in files)
                {
                    DateTime fileTime = File.GetLastWriteTime(file);
                    DateTime nowDate = DateTime.Now;

                    _log.Debug("ProcessSign:Processing: " + file);

                    //use timespan to get the number of seconds
                    TimeSpan span = nowDate - fileTime;
                    var seconds = (int)span.TotalSeconds;

                    int delay = (Convert.ToInt32("0" + signCycle));

                    if (seconds >= delay)    //Delay
                    {
                        string fileout = signOutputPath + "\\" + System.IO.Path.GetFileName(file);
                        string fileTmp = signTempPath + "\\" + System.IO.Path.GetFileName(file);

                        try
                        {
                            if (System.IO.File.Exists(fileout))
                                System.IO.File.Delete(fileout);

                            if (System.IO.File.Exists(fileTmp))
                                System.IO.File.Delete(fileTmp);

                            if (signx509.SignPDF(file, fileTmp))
                            {
                                System.IO.File.Delete(file);
                                System.IO.File.SetCreationTime(fileTmp, System.DateTime.Now);
                                System.IO.File.SetLastWriteTime(fileTmp, System.DateTime.Now);
                                System.IO.File.Move(fileTmp, fileout);
                                _log.Debug("Processed: " + file);
                            }
                        
                        }
                        catch (Exception e)
                        {
                            _log.Debug("**ERROR: " + e.Message);
                            _log.Debug("ERROR: " + file);
                            System.IO.File.Delete(file);

                        }
                    }
                    else
                    {
                        _log.Debug("Delay:" + file);                        
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("ProcessSIGN::Error:" + e.Message);
            }

            _log.Debug("ProcessSign: End");
        }
                               
        private static string ReadConfigTag(string tag)
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\CONFIG\\CONFIG.XML";
            string ret = "";

            if (File.Exists(fileName))
            {
                var xtr = new XmlTextReader(fileName);
                xtr.WhitespaceHandling = WhitespaceHandling.None;
                var xml = new XmlDocument();
                xml.Load(xtr);
                xtr.Close();

                XmlNode element = xml.SelectSingleNode("/ROOT/" + tag);
                if (element != null)
                    ret = element.InnerText;
            }            
            return ret;
        }
    }
}
