using Dapper;
using dWebShop.Domain.Entities.Products;
using dWebShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace dWebShop.DataMigration;

public class MigrationRunner(
    string sourceConnStr,
    string targetConnStr,
    string? srcImagesPath,
    string? srcDocsPath,
    string? tgtImagesPath,
    string? tgtDocsPath)
{
    public async Task RunAsync()
    {
        Console.WriteLine("Opening source MySQL connection…");
        await using var src = new MySqlConnection(sourceConnStr);
        await src.OpenAsync();

        Console.WriteLine("Opening target DB context…");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(targetConnStr, new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;
        await using var tgt = new AppDbContext(options);

        // ── 1. Brands ──────────────────────────────────────────────────────────
        Console.WriteLine("Migrating brands…");
        var srcBrands = (await src.QueryAsync<SrcBrand>("SELECT Id, Name, Slug, Description, LogoImage, SliderImage FROM ProductBrands")).ToList();
        var brandMap = new Dictionary<int, int>(); // old id → new id

        foreach (var sb in srcBrands)
        {
            if (await tgt.Brands.AnyAsync(b => b.Slug == sb.Slug))
            {
                var existing = await tgt.Brands.FirstAsync(b => b.Slug == sb.Slug);
                brandMap[sb.Id] = existing.Id;
                Console.WriteLine($"  Brand '{sb.Name}' already exists (id={existing.Id}), skipping.");
                continue;
            }

            var brand = new Brand
            {
                Name = sb.Name,
                Slug = sb.Slug,
                Description = sb.Description,
                LogoImage = sb.LogoImage,
                SliderImage = sb.SliderImage,
            };
            tgt.Brands.Add(brand);
            await tgt.SaveChangesAsync();
            brandMap[sb.Id] = brand.Id;
            Console.WriteLine($"  Brand '{sb.Name}' → id={brand.Id}");
        }

        // ── 2. Categories ──────────────────────────────────────────────────────
        Console.WriteLine("Migrating categories (pass 1 — root categories)…");
        var srcCats = (await src.QueryAsync<SrcCategory>(
            "SELECT Id, Name, Slug, Description, CategoryId, BrandId FROM ProductCategories ORDER BY CategoryId IS NOT NULL, CategoryId"))
            .ToList();
        var catMap = new Dictionary<int, int>(); // old id → new id

        // Two passes: roots first, then children
        foreach (var pass in new[] { false, true })
        {
            foreach (var sc in srcCats.Where(c => (c.CategoryId == null) == !pass))
            {
                if (await tgt.Categories.AnyAsync(c => c.Slug == sc.Slug))
                {
                    var existing = await tgt.Categories.FirstAsync(c => c.Slug == sc.Slug);
                    catMap[sc.Id] = existing.Id;
                    Console.WriteLine($"  Category '{sc.Name}' already exists, skipping.");
                    continue;
                }

                var cat = new Category
                {
                    Name = sc.Name,
                    Slug = sc.Slug,
                    Description = sc.Description,
                    BrandId = sc.BrandId.HasValue && brandMap.TryGetValue(sc.BrandId.Value, out var bid) ? bid : null,
                    CategoryId = sc.CategoryId.HasValue && catMap.TryGetValue(sc.CategoryId.Value, out var pid) ? pid : null,
                };
                tgt.Categories.Add(cat);
                await tgt.SaveChangesAsync();
                catMap[sc.Id] = cat.Id;
                Console.WriteLine($"  Category '{sc.Name}' → id={cat.Id}");
            }
        }

        // ── 3. Products ────────────────────────────────────────────────────────
        Console.WriteLine("Migrating products…");
        var srcProducts = (await src.QueryAsync<SrcProduct>(
            "SELECT Id, Name, SKU, ExtRef, Slug, Description, IsActive, BrandId FROM Products")).ToList();
        var srcProductCats = (await src.QueryAsync<SrcProductCategory>(
            "SELECT ProductsId AS ProductId, CategoriesId AS CategoryId FROM CategoryProduct")).ToList();

        // Fallback to alternate join table names if needed
        if (!srcProductCats.Any())
        {
            try { srcProductCats = (await src.QueryAsync<SrcProductCategory>("SELECT ProductId, CategoryId FROM ProductCategories")).ToList(); }
            catch { /* table name may differ — will skip category assignments */ }
        }

        var productMap = new Dictionary<int, int>(); // old id → new id

        foreach (var sp in srcProducts)
        {
            if (await tgt.Products.AnyAsync(p => p.Slug == sp.Slug))
            {
                var existing = await tgt.Products.FirstAsync(p => p.Slug == sp.Slug);
                productMap[sp.Id] = existing.Id;
                Console.WriteLine($"  Product '{sp.Name}' already exists, skipping.");
                continue;
            }

            var catIds = srcProductCats
                .Where(pc => pc.ProductId == sp.Id && catMap.ContainsKey(pc.CategoryId))
                .Select(pc => catMap[pc.CategoryId])
                .ToList();
            var cats = tgt.Categories.Where(c => catIds.Contains(c.Id)).ToList();

            var product = new Product
            {
                Name = sp.Name,
                SKU = sp.SKU,
                ExtRef = sp.ExtRef,
                Slug = sp.Slug,
                Description = sp.Description,
                Status = sp.IsActive ? ProductStatus.Active : ProductStatus.Draft,
                BrandId = sp.BrandId.HasValue && brandMap.TryGetValue(sp.BrandId.Value, out var b) ? b : null,
                Categories = cats,
                ProductDetails = new ProductDetails { DetailDescription = string.Empty },
            };
            tgt.Products.Add(product);
            await tgt.SaveChangesAsync();
            productMap[sp.Id] = product.Id;
            Console.WriteLine($"  Product '{sp.Name}' → id={product.Id}");
        }

        // ── 4. ProductDetails ─────────────────────────────────────────────────
        Console.WriteLine("Migrating product details, images, documents…");
        var srcDetails = (await src.QueryAsync<SrcProductDetails>(
            "SELECT Id, ProductId, DetailDescription FROM ProductDetails")).ToList();
        var detailsMap = new Dictionary<int, int>(); // old id → new id

        foreach (var sd in srcDetails)
        {
            if (!productMap.TryGetValue(sd.ProductId, out var newProductId)) continue;

            var details = await tgt.ProductDetails.FirstOrDefaultAsync(pd => pd.ProductId == newProductId);
            if (details is null)
            {
                details = new ProductDetails { ProductId = newProductId, DetailDescription = sd.DetailDescription };
                tgt.ProductDetails.Add(details);
            }
            else
            {
                details.DetailDescription = sd.DetailDescription;
            }
            await tgt.SaveChangesAsync();
            detailsMap[sd.Id] = details.Id;
        }

        // ProductInfos
        var srcInfos = (await src.QueryAsync<SrcProductInfo>(
            "SELECT Id, ProductDetailsId, `Key`, Data FROM ProductInfos")).ToList();
        foreach (var si in srcInfos)
        {
            if (!detailsMap.TryGetValue(si.ProductDetailsId, out var newDetailsId)) continue;
            if (await tgt.ProductInfos.AnyAsync(i => i.ProductDetailsId == newDetailsId && i.Key == si.Key)) continue;
            tgt.ProductInfos.Add(new ProductInfo { ProductDetailsId = newDetailsId, Key = si.Key, Data = si.Data });
        }
        await tgt.SaveChangesAsync();

        // ProductImages
        var srcImages = (await src.QueryAsync<SrcProductImage>(
            "SELECT Id, ProductDetailsId, Path, Description FROM ProductImages")).ToList();
        foreach (var si in srcImages)
        {
            if (!detailsMap.TryGetValue(si.ProductDetailsId, out var newDetailsId)) continue;
            if (await tgt.ProductImages.AnyAsync(i => i.ProductDetailsId == newDetailsId && i.Path == si.Path)) continue;
            tgt.ProductImages.Add(new ProductImage { ProductDetailsId = newDetailsId, Path = si.Path, Description = si.Description });
        }
        await tgt.SaveChangesAsync();

        // ProductDocuments
        /*
        var srcDocs = (await src.QueryAsync<SrcProductDocument>(
            "SELECT Id, ProductDetailsId, Name, Path, Description FROM ProductDocuments")).ToList();
        foreach (var sd in srcDocs)
        {
            if (!detailsMap.TryGetValue(sd.ProductDetailsId, out var newDetailsId)) continue;
            if (await tgt.ProductDocuments.AnyAsync(d => d.ProductDetailsId == newDetailsId && d.Path == sd.Path)) continue;
            tgt.ProductDocuments.Add(new ProductDocument { ProductDetailsId = newDetailsId, Name = sd.Name, Path = sd.Path, Description = sd.Description });
        }
        await tgt.SaveChangesAsync();
        */

        // ── 5. ProductSkus / Options ──────────────────────────────────────────
        Console.WriteLine("Migrating SKUs and options…");

        var srcSkus = (await src.QueryAsync<SrcProductSku>(
            "SELECT Id, ProductId, SKU, ExtRef, Name, Price, Tax, ImagePath FROM ProductsSkus")).ToList();
        var skuMap = new Dictionary<int, int>();

        foreach (var ss in srcSkus)
        {
            if (!productMap.TryGetValue(ss.ProductId, out var newProductId)) continue;
            if (await tgt.ProductSkus.AnyAsync(s => s.ProductId == newProductId && s.SKU == ss.SKU))
            {
                var existing = await tgt.ProductSkus.FirstAsync(s => s.ProductId == newProductId && s.SKU == ss.SKU);
                skuMap[ss.Id] = existing.Id;
                continue;
            }
            var sku = new ProductSku
            {
                ProductId = newProductId, SKU = ss.SKU, ExtRef = ss.ExtRef,
                Name = ss.Name, Price = ss.Price, Tax = ss.Tax, ImagePath = ss.ImagePath,
            };
            tgt.ProductSkus.Add(sku);
            await tgt.SaveChangesAsync();
            skuMap[ss.Id] = sku.Id;
        }

        var srcOptions = (await src.QueryAsync<SrcProductOption>(
            "SELECT Id, ProductId, Name, IsNamePart FROM ProductOptions")).ToList();
        var optionMap = new Dictionary<int, int>();

        foreach (var so in srcOptions)
        {
            if (!productMap.TryGetValue(so.ProductId, out var newProductId)) continue;
            if (await tgt.ProductOptions.AnyAsync(o => o.ProductId == newProductId && o.Name == so.Name))
            {
                var existing = await tgt.ProductOptions.FirstAsync(o => o.ProductId == newProductId && o.Name == so.Name);
                optionMap[so.Id] = existing.Id;
                continue;
            }
            var opt = new ProductOption { ProductId = newProductId, Name = so.Name, IsNamePart = so.IsNamePart };
            tgt.ProductOptions.Add(opt);
            await tgt.SaveChangesAsync();
            optionMap[so.Id] = opt.Id;
        }

        // The old DB leaves ProductOptionValues.ProductId NULL — take it from the option.
        var srcOptionValues = (await src.QueryAsync<SrcProductOptionValue>(
            @"SELECT v.Id, o.ProductId, v.ProductOptionId, v.Name
              FROM ProductOptionValues v JOIN ProductOptions o ON o.Id = v.ProductOptionId")).ToList();
        var optValMap = new Dictionary<int, int>();

        foreach (var sv in srcOptionValues)
        {
            if (!productMap.TryGetValue(sv.ProductId, out var newProductId)) continue;
            if (!optionMap.TryGetValue(sv.ProductOptionId, out var newOptionId)) continue;
            if (await tgt.ProductOptionValues.AnyAsync(v => v.ProductOptionId == newOptionId && v.Name == sv.Name))
            {
                var existing = await tgt.ProductOptionValues.FirstAsync(v => v.ProductOptionId == newOptionId && v.Name == sv.Name);
                optValMap[sv.Id] = existing.Id;
                continue;
            }
            var val = new ProductOptionValue { ProductId = newProductId, ProductOptionId = newOptionId, Name = sv.Name };
            tgt.ProductOptionValues.Add(val);
            await tgt.SaveChangesAsync();
            optValMap[sv.Id] = val.Id;
        }

        // The old DB leaves SkuOptionValues.ProductId NULL and stores the option in
        // ProductOptionId (ProductOptionsId is NULL) — take the product from the SKU.
        var srcSkuOptVals = (await src.QueryAsync<SrcSkuOptionValue>(
            @"SELECT sov.Id, s.ProductId, sov.ProductSkuId,
                     COALESCE(sov.ProductOptionsId, sov.ProductOptionId) AS ProductOptionsId, sov.ProductOptionValueId
              FROM SkuOptionValues sov JOIN ProductsSkus s ON s.Id = sov.ProductSkuId
              WHERE sov.ProductOptionValueId IS NOT NULL")).ToList();

        foreach (var sv in srcSkuOptVals)
        {
            if (!productMap.TryGetValue(sv.ProductId, out var newProductId)) continue;
            if (!skuMap.TryGetValue(sv.ProductSkuId, out var newSkuId)) continue;
            if (!optionMap.TryGetValue(sv.ProductOptionsId, out var newOptId)) continue;
            if (!optValMap.TryGetValue(sv.ProductOptionValueId, out var newOptValId)) continue;
            if (await tgt.SkuOptionValues.AnyAsync(s => s.ProductSkuId == newSkuId && s.ProductOptionsId == newOptId && s.ProductOptionValueId == newOptValId)) continue;
            tgt.SkuOptionValues.Add(new SkuOptionValue
            {
                ProductId = newProductId,
                ProductSkuId = newSkuId,
                ProductOptionsId = newOptId,
                ProductOptionValueId = newOptValId,
            });
        }
        await tgt.SaveChangesAsync();

        // ── 6. Copy static files ───────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(srcImagesPath) && !string.IsNullOrWhiteSpace(tgtImagesPath) &&
            Directory.Exists(srcImagesPath))
        {
            Console.WriteLine("Copying product images…");
            CopyDirectory(srcImagesPath, tgtImagesPath);
        }

        if (!string.IsNullOrWhiteSpace(srcDocsPath) && !string.IsNullOrWhiteSpace(tgtDocsPath) &&
            Directory.Exists(srcDocsPath))
        {
            Console.WriteLine("Copying product documents…");
            CopyDirectory(srcDocsPath, tgtDocsPath);
        }

        Console.WriteLine();
        Console.WriteLine("Migration complete.");
        Console.WriteLine($"  Brands    : {brandMap.Count}");
        Console.WriteLine($"  Categories: {catMap.Count}");
        Console.WriteLine($"  Products  : {productMap.Count}");
        Console.WriteLine($"  SKUs      : {skuMap.Count}");
    }

    private static void CopyDirectory(string src, string tgt)
    {
        Directory.CreateDirectory(tgt);
        foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(src, file);
            var dest = Path.Combine(tgt, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
    }
}
