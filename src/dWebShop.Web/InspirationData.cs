using dWebShop.Domain.Entities.Inspirations;

namespace dWebShop.Web;

public static class InspirationData
{
    public record ContentTypeInfo(string Label, string Plural, string Desc);

    public static readonly IReadOnlyDictionary<InspirationContentType, ContentTypeInfo> Types =
        new Dictionary<InspirationContentType, ContentTypeInfo>
        {
            [InspirationContentType.Story]      = new("Priča",     "Priče",     "Edukativni i inspiracijski članci"),
            [InspirationContentType.Reference]  = new("Referenca", "Reference", "Realizirani projekti sa fotografijama"),
            [InspirationContentType.Collection] = new("Kolekcija", "Kolekcije", "Sistemska rješenja i dizajn linije"),
        };
}
