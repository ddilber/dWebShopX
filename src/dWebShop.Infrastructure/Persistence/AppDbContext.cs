using dWebShop.Domain.Entities.Blog;
using dWebShop.Domain.Entities.Inspirations;
using dWebShop.Domain.Entities.Orders;
using dWebShop.Domain.Entities.Partners;
using dWebShop.Domain.Entities.Pricing;
using dWebShop.Domain.Entities.Products;
using dWebShop.Domain.Entities.ShoppingCart;
using dWebShop.Domain.Entities.Social;
using dWebShop.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>, dWebShop.Application.Common.Interfaces.IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Products
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<UoM> UoMs => Set<UoM>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductDetails> ProductDetails => Set<ProductDetails>();
    public DbSet<ProductInfo> ProductInfos => Set<ProductInfo>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductDocument> ProductDocuments => Set<ProductDocument>();
    public DbSet<ProductSku> ProductSkus => Set<ProductSku>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductOptionValue> ProductOptionValues => Set<ProductOptionValue>();
    public DbSet<SkuOptionValue> SkuOptionValues => Set<SkuOptionValue>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Partners
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<AddressType> AddressTypes => Set<AddressType>();

    // Pricing
    public DbSet<Pricelist> Pricelists => Set<Pricelist>();
    public DbSet<PricelistItem> PricelistItems => Set<PricelistItem>();
    public DbSet<ClientPricelist> ClientPricelists => Set<ClientPricelist>();
    public DbSet<ClientDiscount> ClientDiscounts => Set<ClientDiscount>();
    public DbSet<CustomerProductPrice> CustomerProductPrices => Set<CustomerProductPrice>();
    public DbSet<VatRate> VatRates => Set<VatRate>();
    public DbSet<PaymentTerms> PaymentTerms => Set<PaymentTerms>();
    public DbSet<DiscountDefinition> DiscountDefinitions => Set<DiscountDefinition>();
    public DbSet<DiscountVersion> DiscountVersions => Set<DiscountVersion>();
    public DbSet<DiscountRule> DiscountRules => Set<DiscountRule>();

    // Shopping Cart
    public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();

    // Inspirations
    public DbSet<Inspiration> Inspirations => Set<Inspiration>();

    // Blog
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<PostMeta> PostMetas => Set<PostMeta>();
    public DbSet<PostComment> PostComments => Set<PostComment>();

    // Social (homepage "Društvene mreže" block)
    public DbSet<SocialProfile> SocialProfiles => Set<SocialProfile>();
    public DbSet<SocialVideo> SocialVideos => Set<SocialVideo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity table renames
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        // Brand
        builder.Entity<Brand>(e =>
        {
            e.ToTable("Brands");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
        });

        // Category → Brand (many-to-one), Category self-ref
        builder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Brand)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        });

        // Tag many-to-many with Product and Category
        builder.Entity<Tag>(e =>
        {
            e.ToTable("Tags");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        });

        builder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasOne(x => x.Brand)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Categories)
                .WithMany(x => x.Products);
            e.HasMany(x => x.Tags)
                .WithMany(x => x.Products)
                .UsingEntity("ProductTags");
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.Name).HasMaxLength(300).IsRequired();
            e.Property(x => x.SKU).HasMaxLength(100);
            e.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        });

        builder.Entity<ProductDetails>(e =>
        {
            e.ToTable("ProductDetails");
            e.HasOne<Product>()
                .WithOne(x => x.ProductDetails)
                .HasForeignKey<ProductDetails>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductInfo>(e =>
        {
            e.ToTable("ProductInfos");
            e.HasOne<ProductDetails>()
                .WithMany(x => x.Information)
                .HasForeignKey(x => x.ProductDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductImage>(e =>
        {
            e.ToTable("ProductImages");
            e.HasOne<ProductDetails>()
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProductDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductDocument>(e =>
        {
            e.ToTable("ProductDocuments");
            e.HasOne<ProductDetails>()
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ProductDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductSku>(e =>
        {
            e.ToTable("ProductSkus");
            e.HasOne<Product>()
                .WithMany(x => x.ProductSkus)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Gtin).HasMaxLength(14);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.CompareAtPrice).HasPrecision(18, 4);
            e.Property(x => x.CostPrice).HasPrecision(18, 4);
            e.Property(x => x.Tax).HasPrecision(18, 4);
        });

        builder.Entity<ProductOption>(e =>
        {
            e.ToTable("ProductOptions");
            e.HasOne<Product>()
                .WithMany(x => x.ProductOptions)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductOptionValue>(e =>
        {
            e.ToTable("ProductOptionValues");
            e.HasOne<ProductOption>()
                .WithMany(x => x.ProductOptionValues)
                .HasForeignKey(x => x.ProductOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SkuOptionValue>(e =>
        {
            e.ToTable("SkuOptionValues");
            e.HasOne(x => x.ProductOption)
                .WithMany(x => x.SkuOptionValues)
                .HasForeignKey(x => x.ProductOptionsId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ProductOptionValue)
                .WithMany(x => x.SkuOptionValues)
                .HasForeignKey(x => x.ProductOptionValueId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ProductSku>()
                .WithMany(x => x.SkuOptionValues)
                .HasForeignKey(x => x.ProductSkuId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Orders
        builder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.DeliveryAddress)
                .WithMany()
                .HasForeignKey(x => x.DeliveryAddressId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Status).HasConversion<int>();
        });

        builder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasOne(x => x.Sku)
                .WithMany()
                .HasForeignKey(x => x.SkuId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.Tax).HasPrecision(18, 4);
            e.Property(x => x.Discount).HasPrecision(18, 4);
            e.Property(x => x.Quantity).HasPrecision(18, 4);
        });

        // Partners
        builder.Entity<Partner>(e =>
        {
            e.ToTable("Partners");
            e.HasOne(x => x.Address)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.DeliveryAddresses)
                .WithMany();
        });

        builder.Entity<Address>(e =>
        {
            e.ToTable("Addresses");
            e.HasOne(x => x.AddressType)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AddressType>().ToTable("AddressTypes");

        // Pricing
        builder.Entity<Pricelist>(e =>
        {
            e.ToTable("Pricelists");
            e.HasMany(x => x.Items)
                .WithOne(x => x.Pricelist)
                .HasForeignKey(x => x.PricelistId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.ClientPricelists)
                .WithOne(x => x.Pricelist)
                .HasForeignKey(x => x.PricelistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PricelistItem>(e =>
        {
            e.ToTable("PricelistItems");
            e.HasOne(x => x.ProductSku)
                .WithMany()
                .HasForeignKey(x => x.ProductSkuId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.MinQuantity).HasPrecision(18, 4);
        });

        builder.Entity<ClientPricelist>(e =>
        {
            e.ToTable("ClientPricelists");
            e.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ClientDiscount>(e =>
        {
            e.ToTable("ClientDiscounts");
            e.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        });

        // Shopping Cart
        builder.Entity<ShoppingCartItem>(e =>
        {
            e.ToTable("ShoppingCartItems");
            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.Tax).HasPrecision(18, 4);
            e.Property(x => x.Discount).HasPrecision(18, 4);
            e.Property(x => x.Quantity).HasPrecision(18, 4);
        });

        // Blog
        builder.Entity<Post>(e =>
        {
            e.ToTable("Posts");
            e.HasOne(x => x.Parent)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Categories)
                .WithMany(x => x.Posts);
            e.HasMany(x => x.Tags)
                .WithMany(x => x.Posts);
            e.HasMany(x => x.Metas)
                .WithOne(x => x.Post)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Comments)
                .WithOne(x => x.Post)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PostCategory>(e =>
        {
            e.ToTable("PostCategories");
            e.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PostTag>().ToTable("PostTags");
        builder.Entity<PostMeta>().ToTable("PostMetas");
        builder.Entity<PostComment>().ToTable("PostComments");
        builder.Entity<UoM>().ToTable("UoMs");

        // VatRate
        builder.Entity<VatRate>(e =>
        {
            e.ToTable("VatRates");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Rate).HasPrecision(5, 2);
        });

        // PaymentTerms
        builder.Entity<PaymentTerms>(e =>
        {
            e.ToTable("PaymentTerms");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CashDiscountPercent).HasPrecision(5, 2);
        });

        // Partner → PaymentTerms
        builder.Entity<Partner>()
            .HasOne(x => x.PaymentTerms)
            .WithMany()
            .HasForeignKey(x => x.PaymentTermsId)
            .OnDelete(DeleteBehavior.SetNull);

        // ProductSku → VatRate
        builder.Entity<ProductSku>()
            .HasOne(x => x.VatRate)
            .WithMany()
            .HasForeignKey(x => x.VatRateId)
            .OnDelete(DeleteBehavior.SetNull);

        // CustomerProductPrice
        builder.Entity<CustomerProductPrice>(e =>
        {
            e.ToTable("CustomerProductPrices");
            e.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ProductSku)
                .WithMany()
                .HasForeignKey(x => x.ProductSkuId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Price).HasPrecision(18, 4);
            e.Property(x => x.MinQuantity).HasPrecision(18, 4);
            e.HasIndex(x => new { x.PartnerId, x.ProductSkuId });
        });

        // Pricelist extensions
        builder.Entity<PricelistItem>(e =>
        {
            e.Property(x => x.Currency).HasMaxLength(3);
        });

        // DiscountDefinition
        builder.Entity<DiscountDefinition>(e =>
        {
            e.ToTable("DiscountDefinitions");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Code).HasMaxLength(100).IsRequired();
            e.Property(x => x.Type).HasConversion<int>();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasMany(x => x.Versions)
                .WithOne(x => x.DiscountDefinition)
                .HasForeignKey(x => x.DiscountDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DiscountVersion
        builder.Entity<DiscountVersion>(e =>
        {
            e.ToTable("DiscountVersions");
            e.HasMany(x => x.Rules)
                .WithOne(x => x.DiscountVersion)
                .HasForeignKey(x => x.DiscountVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DiscountRule
        builder.Entity<DiscountRule>(e =>
        {
            e.ToTable("DiscountRules");
            e.Property(x => x.TargetType).HasConversion<int>();
            e.Property(x => x.Value).HasPrecision(18, 4);
            e.Property(x => x.MinQuantity).HasPrecision(18, 4);
            e.Property(x => x.MinOrderAmount).HasPrecision(18, 4);
            e.HasIndex(x => new { x.TargetType, x.TargetId });
        });

        // OrderItem snapshot columns
        builder.Entity<OrderItem>(e =>
        {
            e.Property(x => x.BasePrice).HasPrecision(18, 4);
            e.Property(x => x.FinalPrice).HasPrecision(18, 4);
            e.Property(x => x.VatRateSnapshot).HasPrecision(5, 2);
        });

        // Inspirations
        builder.Entity<Inspiration>(e =>
        {
            e.ToTable("Inspirations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(400).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(400).IsRequired();
            e.Property(x => x.Lede).HasMaxLength(1000);
            e.Property(x => x.HeroLabel).HasMaxLength(200);
            e.Property(x => x.PublishedAt).HasMaxLength(20);
            e.Property(x => x.Authors).HasMaxLength(500);
            e.Property(x => x.Tags).HasMaxLength(500);
            e.Property(x => x.LinkedProductSlugs).HasMaxLength(1000);
            e.Property(x => x.Content).HasColumnType("longtext");
            e.Property(x => x.ContentType).HasConversion<int>();
            e.Property(x => x.CoverImage).HasMaxLength(500);
            e.Property(x => x.GalleryJson).HasColumnType("longtext");
            e.Property(x => x.MetaTitle).HasMaxLength(300);
            e.Property(x => x.MetaDescription).HasMaxLength(500);
            e.Property(x => x.OgImage).HasMaxLength(500);
            e.HasIndex(x => new { x.BrandId, x.Slug }).IsUnique();
            e.HasOne(x => x.Brand)
                .WithMany()
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Social
        builder.Entity<SocialProfile>(e =>
        {
            e.ToTable("SocialProfiles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Handle).HasMaxLength(200);
            e.Property(x => x.Url).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Key).IsUnique();
        });

        builder.Entity<SocialVideo>(e =>
        {
            e.ToTable("SocialVideos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Platform).HasMaxLength(50).IsRequired();
            e.Property(x => x.Caption).HasMaxLength(300);
            e.Property(x => x.Duration).HasMaxLength(20);
            e.Property(x => x.PosterImage).HasMaxLength(500);
            e.Property(x => x.EmbedUrl).HasMaxLength(500);
            e.Property(x => x.FallbackUrl).HasMaxLength(500);
            e.HasIndex(x => x.SortOrder);
        });
    }
}
