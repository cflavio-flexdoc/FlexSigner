using FlexSignerService;
using System;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        Cert myCert;
        PDFSigner pdfSigner;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string configFile = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\CONFIG.XML";
            string cnpjCertificate = IniFile.IniReadValue(configFile, "CERTIFICATE", "cnpj");
            string nameCertificate = IniFile.IniReadValue(configFile, "CERTIFICATE", "name");
            string thumbCertificate = IniFile.IniReadValue(configFile, "CERTIFICATE", "thumb");

            lblCnpj.Text = cnpjCertificate;
            lblNome.Text = nameCertificate;
            lblThumb.Text = thumbCertificate;
        }
                
        private void button2_Click(object sender, EventArgs e)
        {
            //Testar Certificado
            button2.Enabled = false;
            MessageBox.Show("Este teste irá listar todos os certificados acessíveis no Computador [Store.LocalComputer].");
            myCert = new Cert(lblCnpj.Text, lblNome.Text, lblThumb.Text);
            myCert.TestCertificate(); 
            button2.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Testar Certificado
            MessageBox.Show("Este teste irá testar se o certificado com os dados informados está acessível e válido.");
            button3.Enabled = false;
            myCert = new Cert(lblCnpj.Text, lblNome.Text, lblThumb.Text);
            if(myCert.LocateCert())
            {
                MessageBox.Show("Certificado válido!");
            }
            else
            {
                MessageBox.Show("Nenhum certificado válido encontrado com os dados acima. Verifique se está instalado para Local.Computer (e não Current.User) e se está dentro do prazo de validade. Verifique ainda se o usuário local e também o [LOCAL SERVICE] possuem acesso à chave privada do certificado!");
            }
            button3.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string inputFile = "C:\\TEMP\\TEST.PDF";
            string signedFile = "C:\\TEMP\\SIGNED.PDF";

            button4.Enabled = false;
            MessageBox.Show(@"Este teste irá assinar o documento " + inputFile + " e gerar um documento assinado " + signedFile);
            if(!System.IO.File.Exists(inputFile))
            {
                MessageBox.Show(@"Arquivo " + inputFile + " não encontrado!");
            }
            else
            {                
                if(System.IO.File.Exists(signedFile))
                {
                    System.IO.File.Delete(signedFile);
                }

                myCert = new Cert(lblCnpj.Text, lblNome.Text, lblThumb.Text);
                if (myCert.LocateCert())
                {
                    pdfSigner = new PDFSigner();
                    if(pdfSigner.Sign(inputFile, signedFile, myCert, null, null, null, null))
                    {
                        MessageBox.Show("Documento assinado com sucesso!");
                    }
                    else
                    {
                        MessageBox.Show("Erro ao assinar documento!");
                    }
                }
                else
                {
                    MessageBox.Show("Certificado não encontrado!");
                }
            }
            button4.Enabled = true;
        }
    }
}