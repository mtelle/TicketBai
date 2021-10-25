
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TicketBai.Client.Schemas;
using TicketBai.Client.Schemas.TicketBaiRequest;

namespace TicketBai.Client
{
    public class TicketBaiClient
    {
        private const string _preUrl = "https://tbai-prep.egoitza.gipuzkoa.eus/WAS/HACI/HTBRecepcionFacturasWEB/rest/recepcionFacturas/alta";
        private const string _realUrl = "https://tbai-z.egoitza.gipuzkoa.eus/sarrerak/alta";
        private string _url;
        private readonly bool _logFiles;
        private readonly string _path = ".\\0Files\\";

        public TicketBaiClient(bool realEnviroment, bool logFiles)
        {
            if (realEnviroment)
                _url = _realUrl;
            else
                _url = _preUrl;
            _logFiles = logFiles;
        }

        public TicketBaiResponse SendInvoice(X509Certificate2 certificate, TicketBaiRequest ticketBaiRequest)
        {
            var dateTime = DateTime.Now;
            XmlDocument xmlDoc;
            var serializerTicket = new XmlSerializer(typeof(TicketBaiRequest));

            using (var memStm = new MemoryStream())
            {
                serializerTicket.Serialize(memStm, ticketBaiRequest);

                memStm.Position = 0;

                using (var xtr = new StreamReader(memStm, Encoding.UTF8))
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.PreserveWhitespace = true;
                    xmlDoc.Load(xtr);
                }
            }

            var attr = xmlDoc.CreateAttribute("xmlns:ds");
            attr.Value = "http://www.w3.org/2000/09/xmldsig#";
            xmlDoc.DocumentElement.Attributes.Append(attr);

            SignXml(xmlDoc, certificate.GetRSAPrivateKey(), certificate);
            if (_logFiles)
                xmlDoc.Save($"{_path}{ticketBaiRequest.Factura.CabeceraFactura.SerieFactura}-{ticketBaiRequest.Factura.CabeceraFactura.NumFactura}.{dateTime.Ticks}.xml");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
            request.ClientCertificates = new X509CertificateCollection(new X509CertificateCollection() { certificate });
            request.ContentType = "application/xml;charset=UTF-8";
            request.Method = "POST";
            using (Stream stream = request.GetRequestStream())
            {
                xmlDoc.Save(stream);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    var serializer = new XmlSerializer(typeof(TicketBaiResponse));
                    using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        var responseTicket = (TicketBaiResponse)serializer.Deserialize(reader);
                        if (_logFiles)
                        {
                            using (var fileStream = File.CreateText($"{_path}{ticketBaiRequest.Factura.CabeceraFactura.SerieFactura}-{ticketBaiRequest.Factura.CabeceraFactura.NumFactura}.{dateTime.Ticks}.Response.xml"))
                            {
                                serializer.Serialize(fileStream, responseTicket);
                            }
                        }
                        return responseTicket;
                    }
                }
            }
        }

        private void SignXml(XmlDocument xmlDoc, RSA rsaKey, X509Certificate certificate)
        {
            if (xmlDoc == null)
                throw new ArgumentException(nameof(xmlDoc));
            if (rsaKey == null)
                throw new ArgumentException(nameof(rsaKey));
            SignedXml signedXml = new SignedXml(xmlDoc);
            signedXml.SigningKey = rsaKey;

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate));
            signedXml.KeyInfo = keyInfo;

            Reference reference = new Reference();
            reference.Uri = "";

            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);
            signedXml.AddReference(reference);
            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        }
    }
}
