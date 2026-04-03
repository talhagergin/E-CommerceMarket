using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ToptanciECommerce.Application.DTOs.Cart;
using ToptanciECommerce.Application.Interfaces.Services;

namespace ToptanciECommerce.Infrastructure.Services;

public class OrderPdfService : IOrderPdfService
{
    public byte[] GenerateOrderList(CartDto cart, string? customerName, string? companyName)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                // ── Header ────────────────────────────────────────────────
                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item()
                                .Text("ToptanciB2B")
                                .FontSize(22).Bold().FontColor("#2563eb");
                            left.Item()
                                .Text("Profesyonel Toptan Alışveriş Platformu")
                                .FontSize(9).FontColor("#64748b");
                        });

                        row.ConstantItem(170).Column(right =>
                        {
                            right.Item().AlignRight()
                                .Text("SİPARİŞ LİSTESİ")
                                .FontSize(14).Bold().FontColor("#1e293b");
                            right.Item().AlignRight()
                                .Text($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(9).FontColor("#64748b");
                            right.Item().AlignRight()
                                .Text($"No: TL-{DateTime.Now:yyyyMMddHHmm}")
                                .FontSize(9).FontColor("#64748b");
                        });
                    });

                    header.Item().PaddingTop(10).LineHorizontal(1).LineColor("#e2e8f0");
                });

                // ── Content ───────────────────────────────────────────────
                page.Content().PaddingTop(16).Column(body =>
                {
                    // Customer info block (only if logged in)
                    if (!string.IsNullOrEmpty(customerName) || !string.IsNullOrEmpty(companyName))
                    {
                        body.Item().Background("#f8fafc").Padding(12).Column(info =>
                        {
                            info.Item()
                                .Text("MÜŞTERİ BİLGİLERİ")
                                .FontSize(8).Bold().FontColor("#94a3b8");

                            info.Item().PaddingTop(4).Row(r =>
                            {
                                if (!string.IsNullOrEmpty(companyName))
                                    r.RelativeItem()
                                        .Text($"Firma: {companyName}")
                                        .Bold();
                                if (!string.IsNullOrEmpty(customerName))
                                    r.RelativeItem()
                                        .Text($"Yetkili: {customerName}");
                            });
                        });

                        body.Item().PaddingBottom(12);
                    }

                    // Section label
                    body.Item().PaddingBottom(6)
                        .Text("ÜRÜN LİSTESİ")
                        .FontSize(8).Bold().FontColor("#94a3b8");

                    // Product table
                    body.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(28);   // #
                            c.ConstantColumn(72);   // SKU
                            c.RelativeColumn(3);    // Name
                            c.ConstantColumn(82);   // Unit price
                            c.ConstantColumn(52);   // Qty
                            c.ConstantColumn(90);   // Total
                        });

                        // Header row
                        table.Header(h =>
                        {
                            h.Cell().Background("#1e3a8a").Padding(7)
                                .Text("#").Bold().FontColor(Colors.White).FontSize(9);
                            h.Cell().Background("#1e3a8a").Padding(7)
                                .Text("SKU").Bold().FontColor(Colors.White).FontSize(9);
                            h.Cell().Background("#1e3a8a").Padding(7)
                                .Text("ÜRÜN ADI").Bold().FontColor(Colors.White).FontSize(9);
                            h.Cell().Background("#1e3a8a").Padding(7).AlignRight()
                                .Text("BİRİM FİYAT").Bold().FontColor(Colors.White).FontSize(9);
                            h.Cell().Background("#1e3a8a").Padding(7).AlignCenter()
                                .Text("MİKTAR").Bold().FontColor(Colors.White).FontSize(9);
                            h.Cell().Background("#1e3a8a").Padding(7).AlignRight()
                                .Text("TOPLAM").Bold().FontColor(Colors.White).FontSize(9);
                        });

                        // Data rows
                        int rowNum = 1;
                        foreach (var item in cart.Items)
                        {
                            string bg = rowNum % 2 == 0 ? "#f8fafc" : Colors.White;

                            table.Cell()
                                .Background(bg).BorderBottom(1).BorderColor("#e2e8f0")
                                .PaddingVertical(7).PaddingHorizontal(6)
                                .Text(rowNum.ToString()).FontColor("#94a3b8");

                            table.Cell()
                                .Background(bg).BorderBottom(1).BorderColor("#e2e8f0")
                                .PaddingVertical(7).PaddingHorizontal(6)
                                .Text(item.SKU).FontSize(9);

                            table.Cell()
                                .Background(bg).BorderBottom(1).BorderColor("#e2e8f0")
                                .PaddingVertical(7).PaddingHorizontal(6)
                                .Column(c =>
                                {
                                    c.Item().Text(item.ProductName).Bold();
                                    if (item.MinOrderQuantity > 1)
                                        c.Item()
                                            .Text($"Min. {item.MinOrderQuantity} adet")
                                            .FontSize(8).FontColor("#94a3b8");
                                });

                            table.Cell()
                                .Background(bg).BorderBottom(1).BorderColor("#e2e8f0")
                                .PaddingVertical(7).PaddingHorizontal(6)
                                .AlignRight()
                                .Text($"{item.UnitPrice:N2} ₺");

                            table.Cell()
                                .Background(bg).BorderBottom(1).BorderColor("#e2e8f0")
                                .PaddingVertical(7).PaddingHorizontal(6)
                                .AlignCenter()
                                .Text(item.Quantity.ToString()).Bold();

                            table.Cell()
                                .Background(bg).BorderBottom(1).BorderColor("#e2e8f0")
                                .PaddingVertical(7).PaddingHorizontal(6)
                                .AlignRight()
                                .Text($"{item.LineTotal:N2} ₺").Bold().FontColor("#2563eb");

                            rowNum++;
                        }
                    });

                    // Totals
                    body.Item().PaddingTop(16).AlignRight().Width(240).Column(totals =>
                    {
                        totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Ara Toplam:");
                            r.ConstantItem(100).AlignRight()
                                .Text($"{cart.SubTotal:N2} ₺");
                        });

                        totals.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text("KDV Toplam:");
                            r.ConstantItem(100).AlignRight()
                                .Text($"{cart.TaxAmount:N2} ₺");
                        });

                        totals.Item().PaddingTop(6).Column(grand =>
                        {
                            grand.Item().LineHorizontal(2).LineColor("#2563eb");
                            grand.Item().PaddingTop(6).Row(r =>
                            {
                                r.RelativeItem()
                                    .Text("GENEL TOPLAM")
                                    .FontSize(12).Bold();
                                r.ConstantItem(120).AlignRight()
                                    .Text($"{cart.Total:N2} ₺")
                                    .FontSize(14).Bold().FontColor("#2563eb");
                            });
                        });

                        totals.Item().PaddingTop(4).AlignRight()
                            .Text($"(KDV Dahil · {cart.TotalQuantity} adet ürün)")
                            .FontSize(8).FontColor("#94a3b8");
                    });

                    // Info note
                    body.Item().PaddingTop(24)
                        .Background("#eff6ff")
                        .Border(1).BorderColor("#bfdbfe")
                        .Padding(12).Column(note =>
                        {
                            note.Item()
                                .Text("NOT:")
                                .Bold().FontColor("#1e40af").FontSize(9);
                            note.Item().PaddingTop(4)
                                .Text("Bu belge bir sipariş listesidir. Fiyatlar toptan fiyatları " +
                                      "yansıtmaktadır ve KDV dahildir. Siparişinizi onaylamak için " +
                                      "müşteri temsilcinizle iletişime geçiniz.")
                                .FontColor("#1e40af").FontSize(9);
                        });
                });

                // ── Footer ────────────────────────────────────────────────
                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor("#e2e8f0");
                    footer.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem()
                            .Text("ToptanciB2B  |  info@toptanci.com  |  0850 000 00 00")
                            .FontSize(8).FontColor("#94a3b8");

                        row.ConstantItem(80).AlignRight().Text(x =>
                        {
                            x.Span("Sayfa ").FontSize(8).FontColor("#94a3b8");
                            x.CurrentPageNumber().FontSize(8).FontColor("#94a3b8");
                            x.Span(" / ").FontSize(8).FontColor("#94a3b8");
                            x.TotalPages().FontSize(8).FontColor("#94a3b8");
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
