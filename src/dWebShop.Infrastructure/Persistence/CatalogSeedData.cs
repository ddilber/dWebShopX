namespace dWebShop.Infrastructure.Persistence;

// Static catalog seed data — moved out of the dWebShop.Web layer so the DB
// initializer (which lives in Infrastructure) can drive seeding without a
// circular reference. Shape mirrors the legacy ShopData record types one-for-one.
internal static class CatalogSeedData
{
    public record CategorySeed(string Slug, string Name, string Description, string? ParentSlug = null);
    public record InfoRow(string Key, string Value);
    public record DocSeed(string Name, string Size, string Path = "");
    public record OptionSeed(string Name, string[] Values);
    public record SkuSeed(string[] Opts, decimal Price, string Stock, string Uom);
    public record ProductSeed(
        string Slug, string BrandSlug, string CategorySlug,
        string Name, string Short, string Desc,
        InfoRow[] Info, DocSeed[] Docs, string[] Tags,
        OptionSeed[] Options, SkuSeed[] Skus,
        string[]? Images = null);

    // Brand display data — overlays the minimal brand rows created in
    // SeedBrandsAsync so the rich marketing copy from the legacy site survives.
    public record BrandSeed(string Slug, string Name, string Description);

    public static readonly BrandSeed[] Brands =
    [
        new("sto",    "STO",    "Fasadni i završni sistemi za energetsku efikasnost."),
        new("xypex",  "XYPEX",  "Kristalna hidroizolacija za beton."),
        new("cortec", "CORTEC", "Migrirajuća antikorozivna zaštita."),
        new("manorteq", "MANORTEQ", "Brtvene trake, mortovi i pribor za hidroizolaciju betona — razvijeni za rad s Xypex sustavom."),
    ];

    public static readonly Dictionary<string, CategorySeed[]> CategoriesByBrand = new()
    {
        ["sto"] =
        [
            new("fasadni-sistemi", "Fasadni sistemi (ETICS)", "Kompletni fasadni sistemi sa toplinskom izolacijom."),
            new("dekorativne-zbuke", "Dekorativne žbuke", "Završne žbuke za fasade i interijere."),
            new("boje-i-premazi", "Boje i premazi", "Fasadne i unutrašnje boje, lazure, impregnacije."),
            new("izolacija", "Toplinska izolacija", "EPS, mineralna vuna, fiksiranje."),
        ],
        ["xypex"] =
        [
            new("admix", "Admix dodaci", "Dodaci za beton — integralna hidroizolacija."),
            new("premazi", "Premazi", "Concentrate, Modified, Megamix."),
            new("sanacija", "Sanacija", "Patch 'n Plug, Quikset za pukotine i prokišnjavanja."),
        ],
        ["cortec"] =
        [
            new("vpci-folije", "VpCI folije i papir", "Antikorozivna ambalaža."),
            new("vpci-aditivi", "VpCI aditivi", "Inhibitori korozije za sisteme."),
            new("mci-beton", "MCI za beton", "Migrirajući inhibitori za armirani beton."),
            new("premazi", "Antikorozivni premazi", "Industrijski premazi za metale."),
        ],
    };

    public static readonly ProductSeed[] Products =
    [
        // ===== STO =====
        new(
            Slug: "sto-armat-classic", BrandSlug: "sto", CategorySlug: "fasadni-sistemi",
            Name: "StoArmat Classic",
            Short: "Mineralno-cementno ljepilo i armaturni mort za STO ETICS sisteme.",
            Desc: "Univerzalni mineralni mort za lijepljenje izolacijskih ploča i armiranje sa fiber-mrežom. Otporan na pukotine, paropropustan, kompatibilan sa svim STO završnim slojevima.",
            Info: [new("Vrsta","Mineralno-cementni mort"), new("Potrošnja","4–5 kg/m²"),
                   new("Otvoreno vrijeme","15 min"), new("Temperatura primjene","+5°C do +30°C"), new("Osnova","Beton, žbuka, EPS/MW")],
            Docs: [new("Tehnički list","420 KB"), new("Sigurnosno-tehnički list (MSDS)","180 KB"), new("CE deklaracija","95 KB")],
            Tags: ["Fasada","ETICS","Energetska efikasnost"],
            Options: [new("Pakovanje", ["25 kg vreća","1000 kg paleta"])],
            Skus: [new(["25 kg vreća"],   28.50m,  "in",    "vreća"),
                   new(["1000 kg paleta"], 1080.00m,"in",    "paleta")]),

        new(
            Slug: "sto-deco-color", BrandSlug: "sto", CategorySlug: "dekorativne-zbuke",
            Name: "StoSilco Color",
            Short: "Silikonska dekorativna žbuka — paropropusna, otporna na vremenske uticaje.",
            Desc: "Dekorativna fasadna žbuka na bazi silikonske emulzije. Visoka paropropusnost, vodoodbojnost i otpornost na zagađenja. Dostupna u zrnima od 1.0 do 3.0 mm i preko 800 nijansi.",
            Info: [new("Vezivo","Silikonska emulzija"), new("Veličina zrna","1.0 / 1.5 / 2.0 / 3.0 mm"),
                   new("Potrošnja","2.4–4.5 kg/m²"), new("Sušenje","24h dodir, 7 dana puno"), new("Standard","EN 15824")],
            Docs: [new("Tehnički list","510 KB"), new("Karta nijansi (StoColor System)","4.2 MB"), new("Sigurnosno-tehnički list","170 KB")],
            Tags: ["Fasada","Dekorativno","Završni sloj"],
            Options: [new("Zrno", ["1.0 mm","1.5 mm","2.0 mm","3.0 mm"]),
                      new("Boja", ["Bijela 00","Toplo siva 32114","Pješčana 31407","Grafit 36302"]),
                      new("Pakovanje", ["25 kg kanta"])],
            Skus: [new(["1.5 mm","Bijela 00","25 kg kanta"],          92.00m,  "in",    "kanta"),
                   new(["2.0 mm","Bijela 00","25 kg kanta"],          92.00m,  "in",    "kanta"),
                   new(["1.5 mm","Toplo siva 32114","25 kg kanta"],  108.50m,  "low",   "kanta"),
                   new(["2.0 mm","Toplo siva 32114","25 kg kanta"],  108.50m,  "in",    "kanta"),
                   new(["1.5 mm","Pješčana 31407","25 kg kanta"],    108.50m,  "in",    "kanta"),
                   new(["2.0 mm","Grafit 36302","25 kg kanta"],      124.00m,  "order", "kanta")]),

        new(
            Slug: "sto-color-x", BrandSlug: "sto", CategorySlug: "boje-i-premazi",
            Name: "StoColor X",
            Short: "Univerzalna fasadna boja na bazi silikatne emulzije.",
            Desc: "Fasadna boja sa visokom paropropusnošću i otpornošću na alge i gljivice. Dugotrajna boja postojana na UV zračenje.",
            Info: [new("Vezivo","Silikat-disperzija"), new("Sjaj","Mat"), new("Potrošnja","0.20–0.25 L/m²"), new("Razrijeđivač","Voda (do 5%)")],
            Docs: [new("Tehnički list","380 KB"), new("Sigurnosno-tehnički list","160 KB")],
            Tags: ["Fasada","Boja","Anti-algae"],
            Options: [new("Pakovanje", ["5 L kanta","15 L kanta"]),
                      new("Baza", ["Bijela","Toniranje (po nijansi)"])],
            Skus: [new(["5 L kanta","Bijela"],              38.90m,  "in",    "kanta"),
                   new(["15 L kanta","Bijela"],            102.00m,  "in",    "kanta"),
                   new(["15 L kanta","Toniranje (po nijansi)"], 134.00m, "order", "kanta")]),

        new(
            Slug: "sto-eps-100", BrandSlug: "sto", CategorySlug: "izolacija",
            Name: "STOTherm EPS 100",
            Short: "Bijeli ekspandirani polistiren za fasadne ETICS sisteme.",
            Desc: "Toplinska izolacijska ploča EPS klasa 100. Stabilna, lagana, otpornog ruba. Za zidove iznad zemlje u STO ETICS sistemima.",
            Info: [new("Klasa","EPS 100"), new("λ","0.036 W/mK"), new("Format ploče","100 × 50 cm"), new("Reakcija na požar","E")],
            Docs: [new("Tehnički list","290 KB"), new("CE deklaracija","95 KB")],
            Tags: ["Izolacija","EPS","Fasada"],
            Options: [new("Debljina", ["8 cm","10 cm","12 cm","15 cm","18 cm"])],
            Skus: [new(["8 cm"],  4.20m, "in",  "m²"),
                   new(["10 cm"], 5.10m, "in",  "m²"),
                   new(["12 cm"], 6.10m, "in",  "m²"),
                   new(["15 cm"], 7.55m, "in",  "m²"),
                   new(["18 cm"], 9.10m, "low", "m²")]),

        new(
            Slug: "sto-mw-035", BrandSlug: "sto", CategorySlug: "izolacija",
            Name: "STOTherm Mineral MW 035",
            Short: "Mineralna vuna za negorive ETICS sisteme.",
            Desc: "Lamelna mineralna vuna sa vlaknima okomito na površinu. Negoriva (A1), visoka paropropusnost, primjenjiva na visoke objekte.",
            Info: [new("Klasa","MW 035"), new("λ","0.035 W/mK"), new("Format","120 × 20 cm"), new("Reakcija na požar","A1")],
            Docs: [new("Tehnički list","320 KB"), new("CE deklaracija","95 KB")],
            Tags: ["Izolacija","MW","Negorivo","Fasada"],
            Options: [new("Debljina", ["10 cm","12 cm","14 cm","16 cm","18 cm","20 cm"])],
            Skus: [new(["10 cm"], 11.40m, "in",    "m²"),
                   new(["12 cm"], 13.60m, "in",    "m²"),
                   new(["14 cm"], 15.80m, "in",    "m²"),
                   new(["16 cm"], 18.10m, "in",    "m²"),
                   new(["18 cm"], 20.30m, "low",   "m²"),
                   new(["20 cm"], 22.60m, "order", "m²")]),

        // ===== XYPEX =====
        new(
            Slug: "xypex-admix-c-1000", BrandSlug: "xypex", CategorySlug: "admix",
            Name: "Xypex Admix C-1000",
            Short: "Kristalni dodatak betonu za integralnu hidroizolaciju.",
            Desc: "Dodaje se u beton tokom miješanja. Reaguje sa cementom i vlagom formirajući nerastvorljive kristale koji trajno blokiraju kapilarni transport vode. Beton ostaje paropropusan ali nepropusan za tečnu vodu.",
            Info: [new("Doziranje","1–3% mase cementa"), new("Aktivacija","Voda + cement (in-situ)"),
                   new("Certifikat","NSF/ANSI 61 (pitka voda)"), new("Pukotine","Zatvara do 0.4 mm"), new("Životni vijek","Trajno")],
            Docs: [new("Tehnički list (Admix C-1000)","480 KB"), new("Sigurnosno-tehnički list","210 KB"),
                   new("NSF/ANSI 61 certifikat","340 KB"), new("Studija slučaja — temelj rezervoara","1.8 MB")],
            Tags: ["Hidroizolacija","Beton","Pitka voda"],
            Options: [new("Pakovanje", ["8 kg kutija","20 kg kutija"]),
                      new("Brzina vezivanja", ["Normal (NF)","Brza (C-1000 NF)"])],
            Skus: [new(["8 kg kutija","Normal (NF)"],  168.00m, "in",  "kutija"),
                   new(["20 kg kutija","Normal (NF)"],  396.00m, "in",  "kutija"),
                   new(["20 kg kutija","Brza (C-1000 NF)"], 432.00m, "low", "kutija")]),

        new(
            Slug: "xypex-concentrate", BrandSlug: "xypex", CategorySlug: "premazi",
            Name: "Xypex Concentrate",
            Short: "Kristalni premaz za hidroizolaciju postojećih betonskih konstrukcija.",
            Desc: "Najjača formula iz Xypex porodice. Aplicira se kao premaz na beton — kristali rastu unutar postojeće strukture i zatvaraju kapilare. Idealno za sanaciju bazena, rezervoara, podruma.",
            Info: [new("Potrošnja","1.0–1.6 kg/m²"), new("Sloj","2 sloja po 0.65 kg/m²"),
                   new("Vrijeme njege","3 dana (raspršiti vodu)"), new("Otpornost na pritisak vode","Pozitivni i negativni")],
            Docs: [new("Tehnički list (Concentrate)","460 KB"), new("Uputstvo za primjenu","720 KB"), new("Sigurnosno-tehnički list","210 KB")],
            Tags: ["Hidroizolacija","Sanacija","Beton"],
            Options: [new("Pakovanje", ["9 kg kanta","22.5 kg kanta"])],
            Skus: [new(["9 kg kanta"],    245.00m, "in", "kanta"),
                   new(["22.5 kg kanta"], 580.00m, "in", "kanta")]),

        new(
            Slug: "xypex-patch-plug", BrandSlug: "xypex", CategorySlug: "sanacija",
            Name: "Xypex Patch 'n Plug",
            Short: "Brzovezujući mort za zaustavljanje aktivnog prokišnjavanja.",
            Desc: "Hidraulički cement koji veže za 60–90 sekundi i odmah zaustavlja prodor vode pod pritiskom. Za rupe, pukotine i spojnice u betonu.",
            Info: [new("Vrijeme vezivanja","60–90 sek"), new("Snaga 28d","> 30 MPa"), new("Primjena","Aktivni propust vode")],
            Docs: [new("Tehnički list","380 KB"), new("Sigurnosno-tehnički list","180 KB")],
            Tags: ["Sanacija","Pukotine","Hitna intervencija"],
            Options: [new("Pakovanje", ["4.5 kg kanta","13.6 kg kanta"])],
            Skus: [new(["4.5 kg kanta"],  89.00m,  "in", "kanta"),
                   new(["13.6 kg kanta"], 245.00m, "in", "kanta")]),

        new(
            Slug: "xypex-megamix-2", BrandSlug: "xypex", CategorySlug: "premazi",
            Name: "Xypex Megamix II",
            Short: "Hidroizolacijski mort za sanaciju oštećenih betonskih površina.",
            Desc: "Sanacijski mort sa kristalnom tehnologijom — istovremeno popravlja, izravnava i hidroizolira beton. Debljina sloja 6–38 mm.",
            Info: [new("Debljina","6–38 mm po sloju"), new("Adhezija","> 1.5 MPa"), new("Otpornost na klorid","Visoka")],
            Docs: [new("Tehnički list","440 KB"), new("Sigurnosno-tehnički list","195 KB")],
            Tags: ["Sanacija","Hidroizolacija"],
            Options: [new("Pakovanje", ["25 kg vreća"])],
            Skus: [new(["25 kg vreća"], 86.00m, "in", "vreća")]),

        // ===== CORTEC =====
        new(
            Slug: "cortec-vpci-126", BrandSlug: "cortec", CategorySlug: "vpci-folije",
            Name: "CORTEC VpCI-126",
            Short: "Antikorozivna stretch folija sa VpCI tehnologijom.",
            Desc: "Polietilenska folija impregnirana isparljivim inhibitorima korozije. Štiti metal u zatvorenom prostoru — bez premaza, masti ili dehidracije. Aktivna do 5 godina.",
            Info: [new("Materijal","LDPE + VpCI"), new("Boja","Plava (transparentna)"),
                   new("Trajanje zaštite","Do 5 godina (zatvoreno)"), new("Reciklaža","Da, kao standardni PE")],
            Docs: [new("Tehnički list","410 KB"), new("Sigurnosno-tehnički list","190 KB"), new("Certifikat MIL-PRF-22019", "320 KB")],
            Tags: ["Antikorozija","Skladištenje","Transport"],
            Options: [new("Širina", ["600 mm","900 mm","1200 mm","1500 mm"]),
                      new("Debljina", ["75 µm","100 µm","150 µm"])],
            Skus: [new(["600 mm","75 µm"],   48.00m,  "in",    "rola"),
                   new(["900 mm","100 µm"],  96.00m,  "in",    "rola"),
                   new(["1200 mm","100 µm"], 128.00m, "low",   "rola"),
                   new(["1500 mm","150 µm"], 195.00m, "order", "rola")]),

        new(
            Slug: "cortec-mci-2005", BrandSlug: "cortec", CategorySlug: "mci-beton",
            Name: "CORTEC MCI-2005",
            Short: "Migrirajući inhibitor korozije za armirani beton.",
            Desc: "Aditiv za beton koji difundira do armature i formira zaštitni film. Sprječava koroziju izazvanu kloridima i karbonizacijom. Za nove konstrukcije i sanaciju.",
            Info: [new("Doziranje","0.6 L/m³ betona"), new("Tip korozije","Klorida i karbonatna"), new("Životni vijek armature","Produžen 3–5×")],
            Docs: [new("Tehnički list (MCI-2005)","460 KB"), new("Sigurnosno-tehnički list","200 KB"), new("Studija — most Sava","2.4 MB")],
            Tags: ["Antikorozija","Beton","Infrastruktura"],
            Options: [new("Pakovanje", ["19 L kanta","208 L bure"])],
            Skus: [new(["19 L kanta"], 380.00m,  "in",    "kanta"),
                   new(["208 L bure"], 3950.00m, "order", "bure")]),

        new(
            Slug: "cortec-vpci-369", BrandSlug: "cortec", CategorySlug: "vpci-aditivi",
            Name: "CORTEC VpCI-369",
            Short: "Dugotrajna antikorozivna zaštita za skladištenje na otvorenom.",
            Desc: "Naftno-bazirani inhibitor za zaštitu metala tokom transporta i dugotrajnog skladištenja. Lako se uklanja prije eksploatacije.",
            Info: [new("Tip","Naftno-bazirani"), new("Sloj","~25 µm"), new("Trajanje (na otvorenom)","Do 24 mjeseca"), new("Uklanjanje","Mineralni rastvarač")],
            Docs: [new("Tehnički list","420 KB"), new("Sigurnosno-tehnički list","210 KB")],
            Tags: ["Antikorozija","Industrija","Transport"],
            Options: [new("Pakovanje", ["5 L kanister","19 L kanta","208 L bure"])],
            Skus: [new(["5 L kanister"],  92.00m,   "in",    "kanister"),
                   new(["19 L kanta"],   320.00m,   "in",    "kanta"),
                   new(["208 L bure"],  3280.00m,   "order", "bure")]),

        new(
            Slug: "cortec-ecoshield", BrandSlug: "cortec", CategorySlug: "premazi",
            Name: "CORTEC EcoShield 386",
            Short: "Vodno-bazirani antikorozivni premaz na bazi VpCI.",
            Desc: "Akrilni premaz sa migrirajućim inhibitorima korozije. Nije toksičan, primjenjiv u zatvorenim prostorima, pogodan za primarni i završni sloj.",
            Info: [new("Tip","Vodno-bazirani akrilik"), new("Sloj","40–60 µm"), new("VOC","< 50 g/L"), new("Boja","Bijela / siva / crna")],
            Docs: [new("Tehnički list","450 KB"), new("Sigurnosno-tehnički list","195 KB")],
            Tags: ["Antikorozija","Premaz","Niska VOC"],
            Options: [new("Boja", ["Bijela","Siva RAL 7035","Crna"]),
                      new("Pakovanje", ["5 L kanta","19 L kanta"])],
            Skus: [new(["Bijela","5 L kanta"],        78.00m,  "in",  "kanta"),
                   new(["Bijela","19 L kanta"],       268.00m, "in",  "kanta"),
                   new(["Siva RAL 7035","19 L kanta"],268.00m, "in",  "kanta"),
                   new(["Crna","19 L kanta"],         280.00m, "low", "kanta")]),
    ];

    // Map the legacy "in" / "low" / "order" stock buckets to integer stock
    // quantities the DB stores. Picks values that round-trip through the
    // page's stock-bucket logic (>threshold = in, >0 = low, 0 = order).
    public static int StockFromBucket(string bucket) => bucket switch
    {
        "in"  => 100,
        "low" => 3,
        _     => 0,
    };
}
