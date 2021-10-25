using TicketBai.Client.Schemas.TicketBaiRequest;
using TicketBai.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography.X509Certificates;

namespace TicketBai.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CallTest()
        {
            //HERE: Fill with your certificate
            using (var certificate = new X509Certificate2(".\\0Files\\cert.pfx", "FILL_PASSWORD"))
            {
                var document = GenerateDemo();
                var ticketBaiClient = new TicketBaiClient(false, true);
                var response = ticketBaiClient.SendInvoice(certificate,document);

            }
        }

        private TicketBaiRequest GenerateDemo()
        {
            var demo = new TicketBaiRequest();
            demo.Cabecera = new Cabecera()
            {
                IDVersionTBAI = IDVersionTicketBaiType.Item12
            };

            demo.HuellaTBAI = new HuellaTBAI()
            {
                Software = new SoftwareFacturacionType()
                {
                    EntidadDesarrolladora = new EntidadDesarrolladoraType()
                    {
                        Item = "BXXXXXXXX"
                    },
                    LicenciaTBAI = "TBAIGIPREXXXXXXXXXXX",
                    Nombre = "XXXXXXXXX",
                    Version = "1.0"
                }
            };

            demo.Sujetos = new Sujetos()
            {
                Emisor = new Emisor()
                {
                    NIF = "BXXXXXXXX",
                    ApellidosNombreRazonSocial = "CIUDADANO FICTICIO ACTIVO",
                },
                VariosDestinatarios = SiNoType.N,
                VariosDestinatariosSpecified = true,
                EmitidaPorTercerosODestinatario = EmitidaPorTercerosType.T,
                EmitidaPorTercerosODestinatarioSpecified = true
            };

            demo.Factura = new Factura()
            {
                CabeceraFactura = new CabeceraFacturaType()
                {
                    SerieFactura = "TB-2021-S",
                    NumFactura = "24",
                    FechaExpedicionFactura = DateTime.Now.ToString("dd-MM-yyyy"),
                    HoraExpedicionFactura = DateTime.Now.ToString("HH:mm:ss"),
                    FacturaSimplificada = SiNoType.S,
                    FacturaEmitidaSustitucionSimplificadaSpecified = true,
                    FacturaEmitidaSustitucionSimplificada = SiNoType.N,
                    FacturaSimplificadaSpecified = true
                },
                DatosFactura = new DatosFacturaType()
                {
                    FechaOperacion = DateTime.Now.ToString("dd-MM-yyyy"),
                    DescripcionFactura = "Factura de test",
                    DetallesFactura = new IDDetalleFacturaType[]
                    {
                    new IDDetalleFacturaType()
                    {
                        DescripcionDetalle = "primera linea",
                        Cantidad = "1.0",
                        ImporteUnitario = "111.0",
                        Descuento = "12.21",
                        ImporteTotal = "119.54"
                    },
                    new IDDetalleFacturaType()
                    {
                        DescripcionDetalle = "segunda linea",
                        Cantidad = "2.0",
                        ImporteUnitario = "222.0",
                        Descuento = "97.68",
                        ImporteTotal = "419.05"
                    }
                    },
                    ImporteTotalFactura = "538.59",
                    Claves = new IDClaveType[]
                    { new IDClaveType()
                    {
                        ClaveRegimenIvaOpTrascendencia = IdOperacionesTrascendenciaTributariaType.Item01
                    }
                    }
                },
                TipoDesglose = new TipoDesgloseType()
                {
                    Item = new DesgloseFacturaType()
                    {
                        Sujeta = new SujetaType()
                        {
                            NoExenta = new DetalleNoExentaType[]
                            {
                            new DetalleNoExentaType()
                            {
                                TipoNoExenta = TipoOperacionSujetaNoExentaType.S1,
                                DesgloseIVA = new DetalleIVAType[]
                                {
                                    new DetalleIVAType()
                                    {
                                        BaseImponible = "445.11",
                                        TipoImpositivo = "21.0",
                                        CuotaImpuesto = "93.48",
                                        OperacionEnRecargoDeEquivalenciaORegimenSimplificado = SiNoType.N,
                                        OperacionEnRecargoDeEquivalenciaORegimenSimplificadoSpecified = true
                                    }
                                }
                            }
                            }
                        }
                    }
                }
            };
            return demo;

        }
    }
}
