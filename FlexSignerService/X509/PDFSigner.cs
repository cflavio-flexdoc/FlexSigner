using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexSignerService
{
    public class PDFSigner
    {
        private readonly Log _log = GenericSingleton<Log>.GetInstance();
        public bool Sign(string pdfFileInput, string pdfFileOutput, Cert myCert, MetaData metadata, string SigReason, string SigContact, string SigLocation, bool visible = false, float x = 0, float y = 0, float x1 = 0, float y1 = 0, bool val1 = false, bool val2 = false, bool val3 = false, bool def = false)
        {
            try
            {
                PdfReader reader = new PdfReader(pdfFileInput);
                //Activate MultiSignatures
                PdfStamper st = PdfStamper.CreateSignature(reader, new FileStream(pdfFileOutput, FileMode.Create, FileAccess.Write), '\0', null, true);
                //To disable Multi signatures uncomment this line : every new signature will invalidate older ones !
                //PdfStamper st = PdfStamper.CreateSignature(reader, new FileStream(this.outputPDF, FileMode.Create, FileAccess.Write), '\0'); 

                if (metadata != null)
                {
                    st.MoreInfo = metadata.getMetaData();
                    st.XmpMetadata = metadata.getStreamedMetaData();
                }

                PdfSignatureAppearance sap = st.SignatureAppearance;
                sap.Acro6Layers = true;
                sap.SetCrypto(myCert.Akp, myCert.Chain, null, PdfSignatureAppearance.WINCER_SIGNED);
                if (def == false)
                {
                    if (val1 == false)
                    {
                        sap.Reason = SigReason;
                    }
                    if (val2 == false)
                    {
                        sap.Contact = SigContact;
                    }
                    if (val3 == false)
                    {
                        sap.Location = SigLocation;
                    }
                }

                if (visible)
                {
                    sap.SetVisibleSignature(new iTextSharp.text.Rectangle(x, y, x1, y1), 1, null);

                    if (def == true)
                    {

                        Font fontSig = FontFactory.GetFont(FontFactory.HELVETICA, (float)7, Font.NORMAL);
                        sap.Layer2Font = fontSig;
                        //sap.Render = PdfSignatureAppearance.SignatureRender.Description;

                        string signerName = PdfPKCS7.GetSubjectFields(myCert.Chain[0]).GetField("CN");
                        PdfTemplate template = st.SignatureAppearance.GetLayer(2);
                        template.MoveTo(0, 200);
                        template.LineTo(500, 0);
                        template.Stroke();
                        template.BeginText();
                        BaseFont bf1 = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        template.SetFontAndSize(bf1, 10);
                        template.SetTextMatrix(1, 1);
                        template.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "" + signerName + "", 0, 40, 0);

                        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        template.SetFontAndSize(bf, 7);
                        template.SetTextMatrix(1, 1);
                        template.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "" + System.DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss zzz") + "", 50, 30, 0);

                        template.SetFontAndSize(bf, 7);
                        template.SetTextMatrix(1, 1);
                        template.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Signer:", 0, 25, 0);

                        template.SetFontAndSize(bf, 7);
                        template.SetTextMatrix(1, 1);
                        template.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "" + "CN=" + PdfPKCS7.GetSubjectFields(myCert.Chain[0]).GetField("CN") + "", 10, 17, 0);


                        template.SetFontAndSize(bf, 7);
                        template.SetTextMatrix(1, 1);
                        template.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "" + "C=" + PdfPKCS7.GetSubjectFields(myCert.Chain[0]).GetField("C") + "", 10, 10, 0);

                        template.EndText();
                    }
                }
                st.Close();
                return true;
            }
            catch (Exception e)
            {
                _log.Error("Sign:" + e.Message + " / " + e.StackTrace);
            }

            return false;
        }
    }
}
