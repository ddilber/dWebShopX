using dWebShop.Domain.Entities.Products;
using dWebShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.DataMigration;

/// <summary>
/// Restores SKU option values that the original migration skipped.
///
/// The old database left ProductOptionValues.ProductId and SkuOptionValues.ProductId
/// NULL, and stored a SKU's option in SkuOptionValues.ProductOptionId (not
/// ProductOptionsId). The migration required both, so nearly all values and
/// SKU→value links were dropped. This reads them from the old SQL dump instead.
///
/// Only adds missing rows: products and SKUs are matched (product by slug, SKU by
/// code within its product) and never created or changed. Old data has one option
/// row per SKU; options are merged by name per product, as the admin expects.
/// A SKU that already has a value for an option keeps it.
/// </summary>
public class OptionsRepair(string dumpPath, string targetConnStr, bool apply)
{
    private record OldOption(int ProductId, string Name, bool IsNamePart);
    private record Link(int SkuId, int ProductId, string Option, string Value);

    public async Task RunAsync()
    {
        Console.WriteLine($"Reading old data from {Path.GetFileName(dumpPath)}…");
        var dump = new SqlDumpReader(dumpPath);
        var oldProductSlug = dump.Rows("Products").ToDictionary(r => Int(r["Id"]), r => r["Slug"]!);
        var oldSkus = dump.Rows("ProductsSkus").ToDictionary(r => Int(r["Id"]), r => (ProductId: NInt(r["ProductId"]), Sku: r["SKU"]!));
        var oldOptions = dump.Rows("ProductOptions").ToDictionary(r => Int(r["Id"]),
            r => new OldOption(Int(r["ProductId"]), r["Name"]!.Trim(), r["IsNamePart"] == "1"));
        var oldValues = dump.Rows("ProductOptionValues").ToDictionary(r => Int(r["Id"]),
            r => (OptionId: NInt(r["ProductOptionId"]), Name: r["Name"]!.Trim()));
        var oldLinks = dump.Rows("SkuOptionValues");
        Console.WriteLine($"  old: {oldOptions.Count} options, {oldValues.Count} values, {oldLinks.Count} SKU links");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(targetConnStr, new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;
        await using var db = new AppDbContext(options);

        var productBySlug = await db.Products.AsNoTracking().ToDictionaryAsync(p => p.Slug, p => p.Id);
        var skuByKey = (await db.ProductSkus.AsNoTracking().Where(s => s.ProductId != null)
                .Select(s => new { s.Id, ProductId = s.ProductId!.Value, s.SKU }).ToListAsync())
            .GroupBy(s => (s.ProductId, s.SKU)).ToDictionary(g => g.Key, g => g.Min(s => s.Id));

        int? NewProduct(int oldProductId) =>
            oldProductSlug.TryGetValue(oldProductId, out var slug) && productBySlug.TryGetValue(slug, out var id) ? id : null;

        // ── What the old data says should exist ─────────────────────────────
        var wantOptions = new Dictionary<(int ProductId, string Name), bool>();   // → IsNamePart
        var wantValues = new HashSet<(int ProductId, string Option, string Value)>();
        var wantLinks = new List<Link>();
        int unmatchedProducts = 0, unmatchedSkus = 0, brokenRows = 0;

        foreach (var (_, v) in oldValues)
        {
            if (v.OptionId is not int oid || !oldOptions.TryGetValue(oid, out var o)) { brokenRows++; continue; }
            if (NewProduct(o.ProductId) is not int pid) { unmatchedProducts++; continue; }
            wantOptions.TryAdd((pid, o.Name), o.IsNamePart);
            wantValues.Add((pid, o.Name, v.Name));
        }

        foreach (var r in oldLinks)
        {
            var oid = NInt(r["ProductOptionId"]) ?? NInt(r["ProductOptionsId"]);
            var vid = NInt(r["ProductOptionValueId"]);
            var sid = NInt(r["ProductSkuId"]);
            if (oid is null || vid is null || sid is null
                || !oldOptions.TryGetValue(oid.Value, out var o)
                || !oldValues.TryGetValue(vid.Value, out var v)
                || !oldSkus.TryGetValue(sid.Value, out var s)) { brokenRows++; continue; }
            if (NewProduct(o.ProductId) is not int pid) { unmatchedProducts++; continue; }
            if (!skuByKey.TryGetValue((pid, s.Sku), out var newSkuId)) { unmatchedSkus++; continue; }
            wantLinks.Add(new Link(newSkuId, pid, o.Name, v.Name));
        }

        // ── Compare with the current database ───────────────────────────────
        var optionId = await db.ProductOptions.AsNoTracking().Where(o => o.ProductId != null)
            .ToDictionaryAsync(o => (o.ProductId!.Value, o.Name), o => o.Id);
        var missingOptions = wantOptions.Where(kv => !optionId.ContainsKey(kv.Key)).ToList();

        Console.WriteLine();
        Console.WriteLine($"Options to add:  {missingOptions.Count} (of {wantOptions.Count} needed)");
        if (apply && missingOptions.Count > 0)
        {
            db.ProductOptions.AddRange(missingOptions.Select(kv =>
                new ProductOption { ProductId = kv.Key.ProductId, Name = kv.Key.Name, IsNamePart = kv.Value }));
            await db.SaveChangesAsync();
            optionId = await db.ProductOptions.AsNoTracking().Where(o => o.ProductId != null)
                .ToDictionaryAsync(o => (o.ProductId!.Value, o.Name), o => o.Id);
        }

        var valueId = (await db.ProductOptionValues.AsNoTracking().Where(v => v.ProductOptionId != null)
                .Select(v => new { v.Id, OptionId = v.ProductOptionId!.Value, v.Name }).ToListAsync())
            .GroupBy(v => (v.OptionId, v.Name)).ToDictionary(g => g.Key, g => g.Min(v => v.Id));
        // In a dry run, options that would be added have no id yet; count their values as missing.
        var missingValues = wantValues.Where(w =>
            !optionId.TryGetValue((w.ProductId, w.Option), out var oid) || !valueId.ContainsKey((oid, w.Value))).ToList();

        Console.WriteLine($"Values to add:   {missingValues.Count} (of {wantValues.Count} needed)");
        if (apply && missingValues.Count > 0)
        {
            db.ProductOptionValues.AddRange(missingValues.Select(w => new ProductOptionValue
            {
                ProductId = w.ProductId, ProductOptionId = optionId[(w.ProductId, w.Option)], Name = w.Value,
            }));
            await db.SaveChangesAsync();
            valueId = (await db.ProductOptionValues.AsNoTracking().Where(v => v.ProductOptionId != null)
                    .Select(v => new { v.Id, OptionId = v.ProductOptionId!.Value, v.Name }).ToListAsync())
                .GroupBy(v => (v.OptionId, v.Name)).ToDictionary(g => g.Key, g => g.Min(v => v.Id));
        }

        // A SKU keeps any value it already has for an option.
        var linkedOptionsBySku = (await db.SkuOptionValues.AsNoTracking()
                .Where(l => l.ProductSkuId != null && l.ProductOptionsId != null)
                .Select(l => new { Sku = l.ProductSkuId!.Value, Opt = l.ProductOptionsId!.Value }).ToListAsync())
            .Select(l => (l.Sku, l.Opt)).ToHashSet();
        var linksToAdd = new List<SkuOptionValue>();
        int alreadySet = 0, wouldAdd = 0;
        foreach (var l in wantLinks.DistinctBy(l => (l.SkuId, l.Option)))
        {
            if (!optionId.TryGetValue((l.ProductId, l.Option), out var oid)) { wouldAdd++; continue; } // option added only on --apply
            if (linkedOptionsBySku.Contains((l.SkuId, oid))) { alreadySet++; continue; }
            wouldAdd++;
            if (valueId.TryGetValue((oid, l.Value), out var vid))
                linksToAdd.Add(new SkuOptionValue { ProductId = l.ProductId, ProductSkuId = l.SkuId, ProductOptionsId = oid, ProductOptionValueId = vid });
        }

        Console.WriteLine($"SKU links to add: {wouldAdd} ({alreadySet} SKUs already had a value for that option)");
        if (apply && linksToAdd.Count > 0)
        {
            db.SkuOptionValues.AddRange(linksToAdd);
            await db.SaveChangesAsync();
        }

        Console.WriteLine();
        Console.WriteLine($"Skipped: {unmatchedProducts} rows whose product slug isn't in the new DB, " +
                          $"{unmatchedSkus} links whose SKU code isn't in the new DB, {brokenRows} incomplete old rows.");
        Console.WriteLine(apply ? "Applied." : "Dry run — nothing written. Re-run with --apply to write these rows.");
    }

    private static int Int(string? s) => int.Parse(s!);
    private static int? NInt(string? s) => s is null ? null : int.Parse(s);
}
