using dWebShop.Domain.Entities.Blog;
using dWebShop.Domain.Entities.Inspirations;
using dWebShop.Domain.Entities.Orders;
using dWebShop.Domain.Entities.Partners;
using dWebShop.Domain.Entities.Pricing;
using dWebShop.Domain.Entities.Products;
using dWebShop.Domain.Entities.ShoppingCart;
using dWebShop.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Common.Interfaces;

public interface IAppDbContext : IAsyncDisposable
{
    DbSet<Inspiration> Inspirations { get; }

    DbSet<Post> Posts { get; }
    DbSet<PostCategory> PostCategories { get; }
    DbSet<PostTag> PostTags { get; }
    DbSet<PostMeta> PostMetas { get; }
    DbSet<PostComment> PostComments { get; }

    DbSet<SocialProfile> SocialProfiles { get; }
    DbSet<SocialVideo> SocialVideos { get; }

    DbSet<Brand> Brands { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductDetails> ProductDetails { get; }
    DbSet<ProductInfo> ProductInfos { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductDocument> ProductDocuments { get; }
    DbSet<ProductSku> ProductSkus { get; }
    DbSet<ProductOption> ProductOptions { get; }
    DbSet<ProductOptionValue> ProductOptionValues { get; }
    DbSet<SkuOptionValue> SkuOptionValues { get; }

    DbSet<Partner> Partners { get; }
    DbSet<Address> Addresses { get; }

    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    DbSet<Pricelist> Pricelists { get; }
    DbSet<PricelistItem> PricelistItems { get; }
    DbSet<ClientPricelist> ClientPricelists { get; }
    DbSet<ClientDiscount> ClientDiscounts { get; }
    DbSet<CustomerProductPrice> CustomerProductPrices { get; }
    DbSet<VatRate> VatRates { get; }
    DbSet<PaymentTerms> PaymentTerms { get; }
    DbSet<DiscountDefinition> DiscountDefinitions { get; }
    DbSet<DiscountVersion> DiscountVersions { get; }
    DbSet<DiscountRule> DiscountRules { get; }

    DbSet<ShoppingCartItem> ShoppingCartItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
