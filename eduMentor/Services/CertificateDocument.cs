using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

namespace eduMentor.Services
{
    public class CertificateDocument : IDocument
    {
        private readonly string _studentName;
        private readonly string _courseTitle;
        private readonly DateTime _fecha;
        private readonly string _issuerName;

        public CertificateDocument(string studentName, string courseTitle, DateTime fecha, string issuerName = "eduMentor")
        {
            _studentName = studentName;
            _courseTitle = courseTitle;
            _fecha = fecha;
            _issuerName = issuerName;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                // ===== Header =====
                page.Header()
                    .Height(80)
                    .AlignMiddle()
                    .Row(row =>
                    {
                        row.RelativeColumn().Stack(stack =>
                        {
                            stack.Item().Text("eduMentor")
                                .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            stack.Item().Text("Centro de aprendizaje demo")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                        // logo simple de relleno
                        row.ConstantColumn(100)
                           .AlignCenter()
                           .AlignMiddle()
                           .Element(e =>
                           {
                               e.Background(Colors.Grey.Lighten2)
                                .Height(50)
                                .Width(70);
                           });
                    });

                // ===== Content =====
                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().AlignCenter().Text("CERTIFICADO DE FINALIZACIÓN")
                            .FontSize(24).Bold().FontColor(Colors.Blue.Darken2);

                        column.Item().AlignCenter().Text("Se certifica que")
                            .FontSize(12).FontColor(Colors.Grey.Darken2);

                        column.Item().AlignCenter().Text(_studentName)
                            .FontSize(20).Bold().FontColor(Colors.Black);

                        column.Item().AlignCenter().Text("ha completado satisfactoriamente el curso")
                            .FontSize(12).FontColor(Colors.Grey.Darken2);

                        column.Item().AlignCenter().Text(_courseTitle)
                            .FontSize(16).Bold().FontColor(Colors.Black);

                        column.Item().PaddingTop(15)
                            .AlignCenter()
                            .Text($"Emitido el {_fecha:dd/MM/yyyy}")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken1);

                        column.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeColumn().AlignCenter().Column(c =>
                            {
                                c.Item().Text("__________________________").FontColor(Colors.Grey.Darken1);
                                c.Item().Text("Instructor (Demo)").FontSize(10).FontColor(Colors.Grey.Darken2);
                            });

                            // Espacio entre firmas
                            row.ConstantColumn(30).Element(e => e.Padding(0));

                            row.RelativeColumn().AlignCenter().Column(c =>
                            {
                                c.Item().Text("__________________________").FontColor(Colors.Grey.Darken1);
                                c.Item().Text("Director (Demo)").FontSize(10).FontColor(Colors.Grey.Darken2);
                            });
                        });

                        column.Item().PaddingTop(20).AlignCenter()
                            .Text("Este certificado es una demostración generada automáticamente por eduMentor.")
                            .FontSize(9).FontColor(Colors.Grey.Lighten1);
                    });

                // ===== Footer =====
                page.Footer()
                    .Height(40)
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.Span("eduMentor • ").FontSize(9);
                        t.Span(DateTime.UtcNow.Year.ToString()).FontSize(9);
                    });
            });
        }
    }
}
