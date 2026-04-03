using Microsoft.EntityFrameworkCore;
using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // Only seed if empty
        if (await db.Categories.AnyAsync()) return;

        // ── Categories ────────────────────────────────────────────────────────
        var temizlik   = new Category { Name = "Temizlik Ürünleri",   Slug = "temizlik-urunleri",   DisplayOrder = 1,  IsActive = true };
        var ofis       = new Category { Name = "Ofis Malzemeleri",    Slug = "ofis-malzemeleri",    DisplayOrder = 2,  IsActive = true };
        var elektrik   = new Category { Name = "Elektrik & Aydınlatma", Slug = "elektrik-aydinlatma", DisplayOrder = 3, IsActive = true };
        var ambalaj    = new Category { Name = "Ambalaj & Paketleme", Slug = "ambalaj-paketleme",   DisplayOrder = 4,  IsActive = true };
        var mutfak     = new Category { Name = "Mutfak & Restoran",   Slug = "mutfak-restoran",     DisplayOrder = 5,  IsActive = true };
        var kisisel    = new Category { Name = "Kişisel Bakım",       Slug = "kisisel-bakim",       DisplayOrder = 6,  IsActive = true };

        // Sub-categories
        var yuzeyTemizlik = new Category { Name = "Yüzey Temizleyiciler", Slug = "yuzey-temizleyiciler", IsActive = true };
        var deterjan      = new Category { Name = "Deterjan & Çamaşır",   Slug = "deterjan-camasir",     IsActive = true };
        var kagitUrun     = new Category { Name = "Kâğıt Ürünler",        Slug = "kagit-urunler",        IsActive = true };

        temizlik.SubCategories.Add(yuzeyTemizlik);
        temizlik.SubCategories.Add(deterjan);
        temizlik.SubCategories.Add(kagitUrun);

        var yaziMalz   = new Category { Name = "Yazı & Çizim",      Slug = "yazi-cizim",      IsActive = true };
        var dosyalama  = new Category { Name = "Dosyalama",          Slug = "dosyalama",       IsActive = true };
        ofis.SubCategories.Add(yaziMalz);
        ofis.SubCategories.Add(dosyalama);

        db.Categories.AddRange(temizlik, ofis, elektrik, ambalaj, mutfak, kisisel);
        await db.SaveChangesAsync();

        // ── Products ─────────────────────────────────────────────────────────
        var products = new List<Product>
        {
            // Temizlik
            new() {
                Name = "Domestos Çamaşır Suyu 1.5L",
                Slug = "domestos-camasir-suyu-15l",
                SKU = "TEM-001",
                ShortDescription = "Güçlü beyazlatma ve dezenfeksiyon etkisi. 6'lı koli.",
                Description = "Domestos Çamaşır Suyu, yüzey temizliğinde üstün etki sağlar. Bakterileri, virüsleri ve mantarları %99.9 oranında yok eder. 1.5 litre kapasiteli şişe, 6'lı koli halinde satılmaktadır.",
                Price = 89.90m,
                WholesalePrice = 54.00m,
                MinOrderQuantity = 6,
                QuantityStep = 6,
                StockQuantity = 240,
                TaxRate = 18m,
                CategoryId = temizlik.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "Fairy Bulaşık Deterjanı 750ml",
                Slug = "fairy-bulasik-deterjani-750ml",
                SKU = "TEM-002",
                ShortDescription = "Yağ kesici formula, 12'li koli.",
                Description = "Fairy Sıvı Bulaşık Deterjanı, güçlü yağ kesici formülü ile karbonlaşmış lekeleri bile kolayca çözer. 750 ml şişe, 12'li koli.",
                Price = 44.90m,
                WholesalePrice = 28.50m,
                MinOrderQuantity = 12,
                QuantityStep = 12,
                StockQuantity = 360,
                TaxRate = 18m,
                CategoryId = yuzeyTemizlik.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "Flash Banyo Temizleyici 750ml",
                Slug = "flash-banyo-temizleyici-750ml",
                SKU = "TEM-003",
                ShortDescription = "Kireç ve sabun birikintisi çözücü, sprey.",
                Description = "Flash Banyo Temizleyici, kireç birikintileri ve sabun kiri üzerinde etkili formül ile banyonuzu parlatır.",
                Price = 38.50m,
                WholesalePrice = 24.00m,
                MinOrderQuantity = 12,
                QuantityStep = 12,
                StockQuantity = 180,
                TaxRate = 18m,
                CategoryId = yuzeyTemizlik.Id,
                IsActive = true,
                IsFeatured = false
            },
            new() {
                Name = "Ariel Toz Deterjan 6KG",
                Slug = "ariel-toz-deterjan-6kg",
                SKU = "TEM-004",
                ShortDescription = "Ağır lekelere karşı üstün beyazlık. 3'lü koli.",
                Description = "Ariel Active çamaşır tozu deterjanı, en inatçı lekeleri bile tek yıkamada temizler. 6 kg'lık pakette, 3'lü koli.",
                Price = 449.00m,
                WholesalePrice = 290.00m,
                MinOrderQuantity = 3,
                QuantityStep = 3,
                StockQuantity = 90,
                TaxRate = 18m,
                CategoryId = deterjan.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "Selpak Kağıt Havlu 24'lü Paket",
                Slug = "selpak-kagit-havlu-24lu",
                SKU = "TEM-005",
                ShortDescription = "3 katlı emici kağıt havlu, 24 rulo.",
                Description = "Selpak Kağıt Havlu yüksek emicilik kapasitesiyle hem evde hem de iş yerinde pratik kullanım sağlar. 3 katlı, 24 rulo paket.",
                Price = 369.00m,
                WholesalePrice = 235.00m,
                MinOrderQuantity = 1,
                QuantityStep = 1,
                StockQuantity = 150,
                TaxRate = 18m,
                CategoryId = kagitUrun.Id,
                IsActive = true,
                IsFeatured = false
            },
            new() {
                Name = "Papia Tuvalet Kağıdı 32'li Paket",
                Slug = "papia-tuvalet-kagidi-32li",
                SKU = "TEM-006",
                ShortDescription = "4 katlı, 32 rulo ekonomik paket.",
                Description = "Papia 4 Katlı Tuvalet Kağıdı yumuşak dokusu ve yüksek kalitesiyle konfor sağlar. 32 rulo ekonomik paket.",
                Price = 289.00m,
                WholesalePrice = 185.00m,
                MinOrderQuantity = 2,
                QuantityStep = 2,
                StockQuantity = 200,
                TaxRate = 18m,
                CategoryId = kagitUrun.Id,
                IsActive = true,
                IsFeatured = false
            },

            // Ofis
            new() {
                Name = "Bic Tükenmez Kalem Mavi 50'li Kutu",
                Slug = "bic-tukenmez-kalem-mavi-50li",
                SKU = "OFI-001",
                ShortDescription = "Klasik Bic Round Stic, mavi, 50'li kutu.",
                Description = "Bic Round Stic tükenmez kalem, akıcı yazım deneyimi ve uzun ömürlü kartuşuyla ofis standartlarının vazgeçilmezidir. Mavi, 50'li kutu.",
                Price = 125.00m,
                WholesalePrice = 78.00m,
                MinOrderQuantity = 1,
                QuantityStep = 1,
                StockQuantity = 500,
                TaxRate = 18m,
                CategoryId = yaziMalz.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "A4 Fotokopi Kağıdı 80gr (500 Yaprak)",
                Slug = "a4-fotokopi-kagidi-80gr-500",
                SKU = "OFI-002",
                ShortDescription = "Beyaz, 80 gr/m², 500 sayfalık 1 top.",
                Description = "Yüksek beyazlıkta (163 CIE) A4 fotokopi kağıdı. Lazer ve mürekkep püskürtmeli yazıcılarla uyumlu. 80 gr/m², 500 yaprak.",
                Price = 159.00m,
                WholesalePrice = 105.00m,
                MinOrderQuantity = 5,
                QuantityStep = 5,
                StockQuantity = 1000,
                TaxRate = 18m,
                CategoryId = yaziMalz.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "Plastik Şeffaf Dosya A4 (100'lü Paket)",
                Slug = "plastik-seffaf-dosya-a4-100lu",
                SKU = "OFI-003",
                ShortDescription = "Şeffaf L tipi dosya, A4, 100 adet.",
                Description = "Belgeleri korumak için ideal L tipi şeffaf dosya. Çizilmeye ve yırtılmaya dayanıklı kalın PP malzeme. 100'lü paket.",
                Price = 95.00m,
                WholesalePrice = 62.00m,
                MinOrderQuantity = 1,
                QuantityStep = 1,
                StockQuantity = 300,
                TaxRate = 18m,
                CategoryId = dosyalama.Id,
                IsActive = true,
                IsFeatured = false
            },
            new() {
                Name = "Ataç Zımba Teli No:10 (1000 Adet)",
                Slug = "atac-zimba-teli-no10-1000",
                SKU = "OFI-004",
                ShortDescription = "No:10 zımba teli, 10'lu kutu (toplam 10.000 adet).",
                Description = "Standart No:10 zımba tellerine uyumlu. Her kutuda 1000 tel, toplu satışta 10'lu kutu.",
                Price = 58.00m,
                WholesalePrice = 36.00m,
                MinOrderQuantity = 10,
                QuantityStep = 10,
                StockQuantity = 400,
                TaxRate = 18m,
                CategoryId = ofis.Id,
                IsActive = true,
                IsFeatured = false
            },

            // Elektrik & Aydınlatma
            new() {
                Name = "Philips LED Ampul 9W E27 (10'lu Paket)",
                Slug = "philips-led-ampul-9w-e27-10lu",
                SKU = "ELK-001",
                ShortDescription = "Soğuk beyaz (6500K), 10'lu ekonomik paket.",
                Description = "Philips CorePro LED ampul, 9W gücünde 806 lümen ışık sağlar. E27 duy, 15.000 saat ömür, soğuk beyaz ışık. 10'lu koli.",
                Price = 349.00m,
                WholesalePrice = 220.00m,
                MinOrderQuantity = 10,
                QuantityStep = 10,
                StockQuantity = 200,
                TaxRate = 18m,
                CategoryId = elektrik.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "Pil Duracell AA Alkalin (40'lı Paket)",
                Slug = "pil-duracell-aa-40li",
                SKU = "ELK-002",
                ShortDescription = "Duracell Plus Power AA kalem pil, 40'lı büyük paket.",
                Description = "Duracell Plus Power alkalin AA pil, uzun ömürlü performans için tasarlanmıştır. Uzaktan kumandalar, oyuncaklar ve fenerler için ideal. 40'lı paket.",
                Price = 289.00m,
                WholesalePrice = 185.00m,
                MinOrderQuantity = 1,
                QuantityStep = 1,
                StockQuantity = 150,
                TaxRate = 18m,
                CategoryId = elektrik.Id,
                IsActive = true,
                IsFeatured = false
            },

            // Ambalaj
            new() {
                Name = "Streç Film 500m (6'lı Koli)",
                Slug = "strec-film-500m-6li",
                SKU = "AMB-001",
                ShortDescription = "45cm genişlik, 500m uzunluk, 6 rulo koli.",
                Description = "Endüstriyel streç film, palet sarma ve ürün ambalajlama için idealdir. 45 cm genişliğinde, 500 metre uzunluğunda. 6 rulo koli.",
                Price = 420.00m,
                WholesalePrice = 270.00m,
                MinOrderQuantity = 6,
                QuantityStep = 6,
                StockQuantity = 120,
                TaxRate = 18m,
                CategoryId = ambalaj.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "Bant Şeffaf 48mm x 50m (24'lü Koli)",
                Slug = "bant-seffaf-48mm-50m-24lu",
                SKU = "AMB-002",
                ShortDescription = "Koli bandı, 48mm x 50m, 24'lü koli.",
                Description = "Yüksek yapışkanlıklı şeffaf koli bandı. Karton kutu kapatma ve paketleme için uygun. 48mm genişlik, 50 metre uzunluk. 24'lü koli.",
                Price = 385.00m,
                WholesalePrice = 245.00m,
                MinOrderQuantity = 24,
                QuantityStep = 24,
                StockQuantity = 96,
                TaxRate = 18m,
                CategoryId = ambalaj.Id,
                IsActive = true,
                IsFeatured = false
            },

            // Mutfak
            new() {
                Name = "Nescafe Gold Teneke 200g (6'lı Koli)",
                Slug = "nescafe-gold-teneke-200g-6li",
                SKU = "MUT-001",
                ShortDescription = "Ofis ve işyeri için Nescafe Gold 200g teneke, 6'lı.",
                Description = "Nescafe Gold kahvenin kendine özgü aroması ve pürüzsüz tadıyla her ortama mükemmel kahve deneyimi sunar. 200 gram teneke, 6'lı koli.",
                Price = 1620.00m,
                WholesalePrice = 1050.00m,
                MinOrderQuantity = 6,
                QuantityStep = 6,
                StockQuantity = 60,
                TaxRate = 8m,
                CategoryId = mutfak.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "Bardak Karton 7oz 180ml (1000 Adet)",
                Slug = "bardak-karton-7oz-180ml-1000",
                SKU = "MUT-002",
                ShortDescription = "Tek kullanımlık karton bardak, 180ml, 1000 adet.",
                Description = "Bükülerek sıkıştırılmış gıda sınıfı karton malzemeden üretilen tek kullanımlık su bardağı. Sıcak ve soğuk içecekler için uygun. 1000 adet.",
                Price = 485.00m,
                WholesalePrice = 310.00m,
                MinOrderQuantity = 1,
                QuantityStep = 1,
                StockQuantity = 100,
                TaxRate = 8m,
                CategoryId = mutfak.Id,
                IsActive = true,
                IsFeatured = false
            },

            // Kişisel Bakım
            new() {
                Name = "Dove Sabun 90g (72'li Koli)",
                Slug = "dove-sabun-90g-72li",
                SKU = "KIS-001",
                ShortDescription = "Nemlendirici formüllü Dove sabun, 72'li koli.",
                Description = "Dove katı sabun, ¼ nemlendirici krem formülü ile derinin nem dengesini korur. 90g, 72'li koli (6x12).",
                Price = 1890.00m,
                WholesalePrice = 1220.00m,
                MinOrderQuantity = 12,
                QuantityStep = 12,
                StockQuantity = 144,
                TaxRate = 18m,
                CategoryId = kisisel.Id,
                IsActive = true,
                IsFeatured = true
            },
            new() {
                Name = "Pamukkale Sıvı Sabun 1L (12'li Koli)",
                Slug = "pamukkale-sivi-sabun-1l-12li",
                SKU = "KIS-002",
                ShortDescription = "Hassas formüllü sıvı el sabunu, 1L, 12'li koli.",
                Description = "Pamukkale Sıvı El Sabunu yumuşatıcı krem içeriği ile ellerin nemini korur. Tüm cilt tipleri için uygundur. 1 litre dispenser şişe, 12'li koli.",
                Price = 720.00m,
                WholesalePrice = 460.00m,
                MinOrderQuantity = 12,
                QuantityStep = 12,
                StockQuantity = 120,
                TaxRate = 18m,
                CategoryId = kisisel.Id,
                IsActive = true,
                IsFeatured = false
            },
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}
