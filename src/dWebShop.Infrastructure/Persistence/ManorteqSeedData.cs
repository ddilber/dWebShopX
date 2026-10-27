using static dWebShop.Infrastructure.Persistence.CatalogSeedData;

namespace dWebShop.Infrastructure.Persistence;

// MANORTEQ catalog — scraped from manorteq.com (Oct 2026) and translated to
// Croatian. Kept apart from CatalogSeedData because it is seeded per brand
// (SeedBrandCatalogAsync), so it also lands on databases that already hold
// the STO/XYPEX/CORTEC catalog.
//
// Image and TDS paths are relative to SharedUploadsPath; the files themselves
// are listed in tools/manorteq/assets.md with their source URLs.
internal static class ManorteqSeedData
{
    public const string BrandSlug = "manorteq";

    private const string Img = "/product_images/manorteq/";
    private const string Doc = "/product_docs/manorteq/";

    public static readonly CategorySeed[] Categories =
    [
        new("brtvene-trake", "Brtvene trake i pribor", "Bubreće i krute brtvene trake za radne spojeve te pribor za ugradnju."),
        new("bubrece-trake", "Bubreće brtvene trake", "Hidrofilne trake koje u dodiru s vodom bubre i brtve radni spoj.", "brtvene-trake"),
        new("krute-trake", "Krute brtvene trake", "Sustav trajnih fizičkih barijera s bitumenskim ili bentonitnim slojem.", "brtvene-trake"),
        new("distanceri-oplate", "Brtvljenje distancera oplate", "FormPlug i FormRing za vodonepropusno brtvljenje cijevi distancera.", "brtvene-trake"),
        new("pribor", "Pribor za ugradnju", "Mastiks, ljepilo i žica za pričvršćivanje brtvenih traka.", "brtvene-trake"),

        new("mortovi", "Mortovi", "Hidroizolacijski mortovi, zalivne mase i gotovi beton s Xypex tehnologijom."),
        new("aquarend", "Sustav žbuke i estriha AquaRend", "Mortovi za sanaciju, hidroizolaciju i izravnavanje betona i zida.", "mortovi"),
        new("armatura", "Armatura za AquaRend", "Mreže za mehaničko sidrenje AquaRend mortova na podlogu.", "mortovi"),
        new("zalivne-mase", "Bezskupljajuće zalivne mase", "NonShrink mase za sidrenje i vodonepropusno zalijevanje šupljina.", "mortovi"),
        new("gotovi-beton", "Gotova betonska mješavina", "Vodonepropusni beton u vrećama za manje radove.", "mortovi"),

        new("dodaci-za-beton", "Dodaci za beton", "Sredstva za njegu svježeg betona i mortova."),

        new("freezteq", "Freezteq", "Sustav za sanaciju kapilarne vlage i paropropusne žbuke."),
        new("kapilarna-vlaga", "Sanacija kapilarne vlage", "Smrznuti štapići za kemijsku barijeru protiv kapilarne vlage.", "freezteq"),
        new("paropropusna-zbuka", "Paropropusni sustav žbuke", "Sanacijske žbuke za zidove nakon tretmana kapilarne vlage.", "freezteq"),
    ];

    // One SKU per product: price stays "Na upit" on the storefront, so the
    // seeded price is 0 and the single "Pakiranje" value doubles as SKU name.
    private static OptionSeed[] Pack(string value) => [new("Pakiranje", [value])];
    private static SkuSeed[] Sku(string value, string uom) => [new([value], 0m, "order", uom)];

    private static DocSeed Tds(string product, string file, string size) =>
        new($"tds:{product}", size, Doc + file);

    public static readonly ProductSeed[] Products =
    [
        // ===== BUBREĆE BRTVENE TRAKE =====
        new(
            Slug: "manorteq-hydrobar-original", BrandSlug: BrandSlug, CategorySlug: "bubrece-trake",
            Name: "HydroBar Original",
            Short: "Bentonitna bubreća brtvena traka za radne spojeve — provjereno rješenje uz Xypex sustav.",
            Desc: """
                <p>Brtvene trake od natrijeva bentonita jednostavno su i provjereno rješenje za brtvljenje radnih spojeva. Beton se može hidroizolirati u masi dodacima poput Xypex Admixa, no na radnom spoju, gdje dolazi do skupljanja betona, hidrofilna bubreća traka zatvara preostali procjep i postaje sastavni dio hidroizolacijskog rješenja.</p>
                <p>HydroBar Original u dodiru s vodom bubri do 350 %, ovisno o nečistoćama u vodi. Time u očvrslom betonu stvara unutarnji tlak koji brtvi protiv prodora vode iz bilo kojeg smjera. Ne utječe na Xypex kristalnu strukturu, kao što ni kristalna struktura ne utječe na bentonit. Zahvaljujući udjelu butilne gume brtvi i nakon ponovljenih ciklusa vlaženja i sušenja, a ima povećanu otpornost na ispiranje u odnosu na uobičajene bentonitne trake.</p>
                <p>Namijenjena je za uobičajene uvjete. Za slanu vodu koristite HydroBar Evo; za onečišćena okruženja (gnojivo, gnojnica) obratite nam se za savjet.</p>
                <h4>Ugradnja</h4>
                <ul>
                  <li>Postaviti na glatku podlogu (beton ili metal) bez prašine i cementnog mlijeka; ne ugrađivati za jake kiše.</li>
                  <li>Traka mora biti najmanje 80 mm udaljena od bilo koje vanjske površine betona.</li>
                  <li>Pričvršćivanje: Manorteq Fixing Wire s čavlima i podloškama (razmak do 250 mm), Fixing Glue, SynMastic ili čavli s podloškama (razmak do 200 mm).</li>
                  <li>Na nastavcima trake preklopiti najmanje 100 mm, jednu uz drugu — ne jednu na drugu.</li>
                  <li>Oštećene ili već nabubrene trake ne ugrađivati.</li>
                </ul>
                """,
            Info: [new("Materijal", "Natrijev bentonit s butilnom gumom"), new("Bubrenje", "do 350 %"),
                   new("Sila brtvljenja", "1,25 t/m"), new("Ciklusi vlaženja/sušenja", "> 3"),
                   new("Slana voda", "Ne — koristiti HydroBar Evo"), new("Dimenzije", "20 × 25 mm, duljina 5 m")],
            Docs: [Tds("HydroBar Original", "MANORTEQ-TDS-HydroBarOriginal.pdf", "1,6 MB")],
            Tags: ["Hidroizolacija", "Radni spojevi", "Xypex kompatibilno"],
            Options: Pack("Kolut 5 m"), Skus: Sku("Kolut 5 m", "kolut"),
            Images: [Img + "waterbar-original@2x.png", Img + "install_original01@2x.webp", Img + "install_original02@2x.webp", Img + "install_original_03@2x.webp", Img + "install_original04@2x.webp"]),

        new(
            Slug: "manorteq-hydrobar-evo", BrandSlug: BrandSlug, CategorySlug: "bubrece-trake",
            Name: "HydroBar Evo",
            Short: "Sintetička bubreća brtvena traka — bubrenje do 1000 %, otporna na slanu vodu i cikluse vlaženja.",
            Desc: """
                <p>HydroBar Evo je nova generacija bubrećih brtvenih traka s većim kapacitetom ekspanzije od tradicionalnih bentonitnih sustava. Zahvaljujući sintetičkom sastavu u dodiru s vodom bubri do 1000 % izvornog volumena, a tlak koji pritom stvara na beton — iako je traka četvrtine veličine bentonitne — više je nego dvostruk.</p>
                <p>Kao nevlaknasti materijal ne može se isprati i zadržava ista svojstva bubrenja i nakon više od 35 ciklusa vlaženja i sušenja. Nakon sušenja vraća se gotovo na izvorni oblik. Znatno je lakša i savitljivija od HydroBar Originala, što olakšava ugradnju iznad glave i provlačenje između armature.</p>
                <h4>Ugradnja</h4>
                <ul>
                  <li>Postaviti na glatku podlogu (beton ili metal) bez prašine i cementnog mlijeka.</li>
                  <li>Primarno pričvršćivanje: Manorteq Fixing Glue; alternativno Fixing Wire s čavlima i podloškama.</li>
                  <li>Ne pričvršćivati samo čavlima i podloškama — traka se može potrgati pri betoniranju.</li>
                  <li>Šupljine između trake i podloge obavezno ispuniti SynMastikom.</li>
                </ul>
                """,
            Info: [new("Materijal", "Sintetički hidrofilni polimer"), new("Bubrenje", "do 1000 %"),
                   new("Sila brtvljenja", "2,6 t/m"), new("Ciklusi vlaženja/sušenja", "> 35"),
                   new("Slana voda", "Da"), new("Dimenzije", "5 × 20 mm, duljina 10 m")],
            Docs: [Tds("HydroBar Evo", "MANORTEQ-TDS-HydroBarEvo.pdf", "1,1 MB")],
            Tags: ["Hidroizolacija", "Radni spojevi", "Xypex kompatibilno"],
            Options: Pack("Kolut 10 m"), Skus: Sku("Kolut 10 m", "kolut"),
            Images: [Img + "waterbar-evo@2x.png", Img + "install_evo01@2x.webp", Img + "install_evo02@2x.webp", Img + "install_evo03@2x.webp", Img + "install-evo04@2x.webp", Img + "install_expanded@2x.webp"]),

        // ===== KRUTE BRTVENE TRAKE =====
        new(
            Slug: "manorteq-waterbar-rigid-kj", BrandSlug: BrandSlug, CategorySlug: "krute-trake",
            Name: "WaterBar Rigid KJ",
            Short: "Kruta brtvena traka s bitumenskim slojem za spoj zida i ploče.",
            Desc: """
                <p>WaterBar Rigid KJ razvoj je tradicionalne fizičke barijere na spoju ploče i zida, dopunjen bitumenskim slojem. Veže se na armaturu i ubetonira u ploču, a može ostati izložena koliko je potrebno (dok je zaštitna traka na mjestu) prije betoniranja zida.</p>
                <p>Krute trake čine sustav — većina projekata koristi barem dva proizvoda iz linije. KJ se mehanički i bez prekida spaja s WaterBar Rigid CJ i CI. Elementi postaju trajni dio konstrukcije, ne ovise o ciklusima vlaženja i sušenja te pružaju trajnu, statičnu zaštitu. Kombinacija fizičke barijere i sekundarne zaštite provjereno je rješenje u srednjoj Europi.</p>
                <h4>Ugradnja</h4>
                <ul>
                  <li>Ukloniti donju zaštitnu foliju (rub koji se pričvršćuje na armaturu).</li>
                  <li>Postaviti traku na armaturu na predviđeno mjesto spoja i pričvrstiti.</li>
                  <li>Preporučuje se obuka na gradilištu.</li>
                </ul>
                """,
            Info: [new("Sekundarna zaštita", "Bitumen"), new("Slana voda", "Da"),
                   new("Ciklusi vlaženja/sušenja", "Neograničeno"), new("Primjena", "Spoj zida i ploče, sa ili bez kickera")],
            Docs: [Tds("WaterBar Rigid KJ", "MANORTEQ-TDS-WaterBar-KJ.pdf", "")],
            Tags: ["Hidroizolacija", "Radni spojevi", "Xypex kompatibilno"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "kom"),
            Images: [Img + "rigid_KJ@2x.png", Img + "install_KJ01@2x.webp", Img + "install_KJ02@2x.webp", Img + "install_KJ03@2x.webp", Img + "install_KJ04@2x.webp"]),

        new(
            Slug: "manorteq-waterbar-rigid-cj", BrandSlug: BrandSlug, CategorySlug: "krute-trake",
            Name: "WaterBar Rigid CJ",
            Short: "Kruta brtvena traka za radne spojeve u zidovima i pločama — ujedno služi kao oplata između betoniranja.",
            Desc: """
                <p>WaterBar Rigid CJ razvoj je tradicionalne fizičke barijere s dodatnim slojem bitumena ili bentonita. Ima dvostruku namjenu: zaustavlja prolaz vode kroz radni spoj i istodobno služi kao oplata između dviju faza betoniranja.</p>
                <p>Ugrađena paralelno s licem betona stvara složen, vijugav put za vodu. Ako se beton skupi, voda i dalje ne prolazi — zahvaljujući bitumenskom sloju koji se veže s betonom (CJ-B) ili bentonitnom sloju koji bubri (CJ-C). Mehanički se spaja s WaterBar Rigid KJ u cjelovito, trajno rješenje.</p>
                <p>Krute trake skuplje su po metru od bubrećih, ali uklanjaju potrebu za oplatom i omogućuju dulje dionice betoniranja, što skraćuje rokove gradnje.</p>
                """,
            Info: [new("Sekundarna zaštita", "Bitumen (CJ-B) ili bentonit (CJ-C)"), new("Slana voda", "Da"),
                   new("Ciklusi vlaženja/sušenja", "Neograničeno"), new("Primjena", "Ploče i zidovi"),
                   new("Oplata", "Nije potrebna")],
            Docs: [Tds("WaterBar Rigid CJ", "MANORTEQ-TDS-WaterBar-CJ.pdf", "")],
            Tags: ["Hidroizolacija", "Radni spojevi", "Xypex kompatibilno"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "kom"),
            Images: [Img + "rigid_CJ@2x.png", Img + "install_CJ01@2x.webp", Img + "install_CJ02@2x.webp", Img + "install_CJ03@2x.webp", Img + "install_CJ04@2x.webp"]),

        new(
            Slug: "manorteq-waterbar-rigid-ci", BrandSlug: BrandSlug, CategorySlug: "krute-trake",
            Name: "WaterBar Rigid CI",
            Short: "Element za izazivanje kontrolirane, vodonepropusne pukotine u velikim betonskim površinama.",
            Desc: """
                <p>WaterBar Rigid CI štedi vrijeme u planu gradnje: betoniranje više nije ograničeno omjerom stranica ni veličinom dionice. Element izaziva kontroliranu, vodonepropusnu pukotinu u masivnom betonu.</p>
                <p>Kod prevelikih dionica skupljanje betona uzrokuje pukotinu na nepredvidivom mjestu. S WaterBar Rigid CI pukotina je:</p>
                <ul>
                  <li>na unaprijed poznatom mjestu,</li>
                  <li>ravna i estetski prihvatljiva,</li>
                  <li>kontrolirana,</li>
                  <li>vodonepropusna.</li>
                </ul>
                <p>Postavlja se između slojeva armature; element za izazivanje pukotine mora zahvaćati najmanje 50 % debljine betona.</p>
                """,
            Info: [new("Sekundarna zaštita", "Bitumen ili bentonit"), new("Slana voda", "Da"),
                   new("Ciklusi vlaženja/sušenja", "Neograničeno"), new("Visina elementa", "min. 50 % debljine betona")],
            Docs: [Tds("WaterBar Rigid CI", "MANORTEQ-TDS-WaterBar-CI.pdf", "")],
            Tags: ["Hidroizolacija", "Radni spojevi", "Xypex kompatibilno"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "kom"),
            Images: [Img + "rigidCI@2x.png", Img + "install_CI01@2x.webp", Img + "install_CI02@2x.webp", Img + "install_CI03@2x.webp", Img + "install_CI04@2x.webp"]),

        // ===== DISTANCERI OPLATE =====
        new(
            Slug: "manorteq-formplug", BrandSlug: BrandSlug, CategorySlug: "distanceri-oplate",
            Name: "FormPlug",
            Short: "Čep za unutarnje brtvljenje cijevi distancera oplate — vodonepropusan do 6 bara.",
            Desc: """
                <p>FormPlug je najjednostavnije rješenje za brtvljenje unutrašnjosti cijevi distancera (tie-bolt). Sastoji se od kape od istog materijala kao HydroBar Evo i čvrstog plastičnog čepa: kapa sprječava curenje vode, a čep osigurava mehaničko sidrenje. Zajedno zaustavljaju vodu pod tlakom do 6 bara.</p>
                <p>Samobubreće brtvljenje aktivirano vodom postiže se jednim udarcem čekića — brzo, jednostavno i 100 % vodonepropusno. Radi kao sustav s Manorteq FormRingom.</p>
                <h4>Ugradnja</h4>
                <ul>
                  <li>Ukloniti konus oplate i nečistoće iz otvora.</li>
                  <li>Umetnuti FormPlug u cijev i zabiti čekićem dok čvrsto ne sjedne. Kratko umakanje gumenog dijela u čistu vodu olakšava umetanje.</li>
                  <li>Završna obrada nije potrebna.</li>
                </ul>
                """,
            Info: [new("Promjer", "22 mm — za cijevi unutarnjeg promjera 22 mm"), new("Vodonepropusnost", "do 6 bara"),
                   new("Materijal kape", "Kao HydroBar Evo")],
            Docs: [Tds("FormPlug", "MANORTEQ-TDS-FormPlug.pdf", "1,1 MB")],
            Tags: ["Hidroizolacija", "Oplata", "Xypex kompatibilno"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "kom"),
            Images: [Img + "plug@2x.png", Img + "install_plug01@2x.webp", Img + "install_plug02@2x.webp"]),

        new(
            Slug: "manorteq-formring", BrandSlug: BrandSlug, CategorySlug: "distanceri-oplate",
            Name: "FormRing",
            Short: "Bubreći prsten za vanjsko brtvljenje oko cijevi distancera oplate — vodonepropusan do 6 bara.",
            Desc: """
                <p>Manorteq FormRing najjednostavnije je rješenje za brtvljenje vanjske površine cijevi distancera. Sprječava procjeđivanje vode uz cijev do kojeg dolazi zbog skupljanja betona i zaustavlja vodu pod tlakom do 6 bara.</p>
                <p>Izrađen je od istog materijala kao HydroBar Evo. Samobubreće brtvljenje aktivirano vodom brzo se ugrađuje i 100 % je vodonepropusno. Radi kao sustav s Manorteq FormPlugom.</p>
                <h4>Ugradnja</h4>
                <ul>
                  <li>Navući FormRing na sredinu cijevi distancera, najmanje 100 mm od bilo koje vanjske površine betona.</li>
                  <li>Umakanje cijevi u čistu vodu olakšava navlačenje.</li>
                  <li>Izbjegavati ugradnju za stalne kiše; prerano nabubreni prsten nakon sušenja se vraća na izvorne dimenzije.</li>
                </ul>
                """,
            Info: [new("Unutarnji promjer", "24 mm — za cijevi vanjskog promjera 24–30 mm"), new("Vodonepropusnost", "do 6 bara"),
                   new("Materijal", "Kao HydroBar Evo")],
            Docs: [Tds("FormRing", "MANORTEQ-TDS-FormRing.pdf", "2,1 MB")],
            Tags: ["Hidroizolacija", "Oplata", "Xypex kompatibilno"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "kom"),
            Images: [Img + "ring@2x.png", Img + "install_ring01@2x.webp", Img + "install_ring02@2x.webp"]),

        // ===== PRIBOR =====
        new(
            Slug: "manorteq-synmastic", BrandSlug: BrandSlug, CategorySlug: "pribor",
            Name: "SynMastic",
            Short: "Hidrofilni poliuretanski mastiks koji u dodiru s vodom bubri do 450 %.",
            Desc: """
                <p>SynMastic je visokokvalitetni jednokomponentni poliuretanski mastiks koji u dodiru s vodom povećava volumen do 450 %.</p>
                <h4>Primjena</h4>
                <ul>
                  <li>Ispunjavanje šupljina između grube podloge i trake HydroBar Original ili Evo.</li>
                  <li>Ispunjavanje procjepa iza oštrih zavoja trake.</li>
                  <li>Lijepljenje traka HydroBar na podlogu.</li>
                  <li>Vodo- i plinonepropusno brtvljenje prodora cijevi i kabelskih kanala.</li>
                </ul>
                <h4>Svojstva</h4>
                <ul>
                  <li>Vodo- i plinonepropusan do 5 bara; trajno elastičan nakon otvrdnjavanja.</li>
                  <li>Odlično prianjanje na većinu građevinskih materijala, u interijeru i eksterijeru.</li>
                  <li>Bubri na 200 % nakon 24 h, 300 % nakon 72 h; vraća se na izvorni oblik kad se voda ukloni.</li>
                  <li>Bez otapala; na čistom betonu nije potreban predpremaz.</li>
                </ul>
                """,
            Info: [new("Vrsta", "Jednokomponentni poliuretanski mastiks"), new("Bubrenje", "do 450 %"),
                   new("Vodonepropusnost", "do 5 bara"), new("Pakiranje", "Uložak 310 ml, 12 kom/kutija")],
            Docs: [Tds("SynMastic", "MANORTEQ-TDS-SynMastic.pdf", "")],
            Tags: ["Hidroizolacija", "Radni spojevi", "Pribor"],
            Options: Pack("Kutija 12 × 310 ml"), Skus: Sku("Kutija 12 × 310 ml", "kutija"),
            Images: [Img + "SynMastic@2x.png"]),

        new(
            Slug: "manorteq-fixing-glue", BrandSlug: BrandSlug, CategorySlug: "pribor",
            Name: "Fixing Glue",
            Short: "Ljepilo za brzo i čvrsto lijepljenje bubrećih traka HydroBar na beton i čelik.",
            Desc: """
                <p>Fixing Glue osigurava brzo i čvrsto prianjanje traka HydroBar Original i HydroBar Evo na betonske i čelične podloge. Brzo stvaranje kožice i otvrdnjavanje omogućuju betoniranje već nakon nekoliko sati. Nakon otvrdnjavanja potpuno je otporan na vremenske uvjete.</p>
                <p>Nanosi se standardnim pištoljem za kartuše. Za optimalno prianjanje ljepilom treba prekriti cijelu kontaktnu površinu trake, a traku zatim kratko pritisnuti.</p>
                <p>Primarno je rješenje za pričvršćivanje HydroBar Evo te HydroBar Originala na čeliku; alternativa je za HydroBar Original na betonu. Na čistoj podlozi (beton, plastika, čelik) predpremaz nije potreban.</p>
                """,
            Info: [new("Podloge", "Beton, čelik, plastika"), new("Betoniranje", "nakon nekoliko sati"),
                   new("Pakiranje", "Uložak 290 ml, 12 kom/kutija")],
            Docs: [Tds("Fixing Glue", "MANORTEQ-TDS-FixingGlue.pdf", "1,2 MB")],
            Tags: ["Radni spojevi", "Pribor"],
            Options: Pack("Kutija 12 × 290 ml"), Skus: Sku("Kutija 12 × 290 ml", "kutija"),
            Images: [Img + "glue@2x.png"]),

        new(
            Slug: "manorteq-fixing-wire", BrandSlug: BrandSlug, CategorySlug: "pribor",
            Name: "Fixing Wire",
            Short: "Profilirana čelična žica za sigurno pričvršćivanje traka HydroBar prije betoniranja.",
            Desc: """
                <p>Fixing Wire posebno je oblikovana za sigurno pričvršćivanje traka HydroBar. Žica u obliku slova U pribija se čavlima preko trake u beton i drži je na mjestu tijekom betoniranja.</p>
                <h4>Ugradnja</h4>
                <ul>
                  <li>Postaviti HydroBar Original ili Evo na glatku betonsku podlogu bez prašine i cementnog mlijeka.</li>
                  <li>Pričvrstiti Fixing Wire čavlima i podloškama na razmaku od najviše 250 mm.</li>
                </ul>
                """,
            Info: [new("Materijal", "Čelična žica, U-profil"), new("Duljina", "1 m"), new("Razmak pričvršćenja", "max. 250 mm")],
            Docs: [Tds("Fixing Wire", "MANORTEQ-TDS-FixingWire.pdf", "1,9 MB")],
            Tags: ["Radni spojevi", "Pribor"],
            Options: Pack("Komad 1 m"), Skus: Sku("Komad 1 m", "kom"),
            Images: [Img + "fixing_Wire@2x.png", Img + "install_wire01@2x.webp", Img + "install_wire02@2x.webp"]),

        // ===== AQUAREND =====
        new(
            Slug: "manorteq-aquarend-thix-regular", BrandSlug: BrandSlug, CategorySlug: "aquarend",
            Name: "AquaRend Thix Regular",
            Short: "Tiksotropni mort R3 za konstrukcijsku sanaciju i hidroizolaciju betona i zida — s Xypex tehnologijom.",
            Desc: """
                <p>Manorteq AquaRend Thix Regular jednokomponentna je suha mješavina morta za konstrukcijsku sanaciju, hidroizolaciju, krpanje i obnovu oštećenih betonskih i zidanih konstrukcija u debljem sloju. Izvrsna hidroizolacijska svojstva osigurava Xypex tehnologija u sastavu.</p>
                <p>Najsvestraniji je proizvod u liniji: koristi se kao žbuka ili estrih. Drži vlastitu težinu pa je prikladan za vertikalne površine i rad iznad glave, a na horizontalnim površinama služi kao estrih. Za tekuću izvedbu pogledajte AquaRend Flow Regular.</p>
                <p>Može se koristiti samostalno ili sa završnim slojem AquaRend Thix Fine ili S-Fine. Gdje je potrebno pojačano prianjanje ili voda prodire s negativne strane, preporučuje se armatura AquaRend Fix-S ili Fix-B. Na sanirane površine mogu se nanijeti isključivo paropropusne boje i premazi.</p>
                """,
            Info: [new("Razred", "R3, nizak modul elastičnosti (EN 1504-3)"), new("Granulacija", "do 4 mm"),
                   new("Vodonepropusnost", "do 5 bara / 50 m vodnog stupca (EN 12390-8)"), new("Primjena", "Vertikalno, iznad glave, horizontalno")],
            Docs: [Tds("AquaRend Thix Regular", "MANORTEQ-TDS-AquaRend-Thix-Regular.pdf", "")],
            Tags: ["Hidroizolacija", "Sanacija betona", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "AquaRendThixRegular@2x.webp"]),

        new(
            Slug: "manorteq-aquarend-flow-regular", BrandSlug: BrandSlug, CategorySlug: "aquarend",
            Name: "AquaRend Flow Regular",
            Short: "Tekući hidroizolacijski mort za obnovu ravnih horizontalnih betonskih površina — s Xypex tehnologijom.",
            Desc: """
                <p>Manorteq AquaRend Flow Regular jednokomponentna je suha mješavina morta za hidroizolaciju, krpanje i obnovu oštećenih betonskih i zidanih konstrukcija. Izvrsna hidroizolacijska svojstva osigurava Xypex tehnologija u sastavu.</p>
                <p>Namijenjen je isključivo ravnim horizontalnim površinama; za vertikalne, kose i površine iznad glave koristite AquaRend Thix Regular. Nanosi se u debljini 10–30 mm, samostalno ili sa završnim slojem AquaRend Thix Fine ili S-Fine. Gdje je potrebno pojačano prianjanje ili voda prodire s negativne strane, preporučuje se AquaRend Fix-S.</p>
                """,
            Info: [new("Granulacija", "do 4 mm"), new("Debljina sloja", "10–30 mm"),
                   new("Vodonepropusnost", "do 50 m vodnog stupca (EN 12390-8)"), new("Primjena", "Ravne horizontalne površine")],
            Docs: [Tds("AquaRend Flow Regular", "MANORTEQ-TDS-AquaRend-Flow-Regular.pdf", "")],
            Tags: ["Hidroizolacija", "Sanacija betona", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "AquaRendFlowRegular@2x.webp"]),

        new(
            Slug: "manorteq-aquarend-thix-fine", BrandSlug: BrandSlug, CategorySlug: "aquarend",
            Name: "AquaRend Thix Fine",
            Short: "Fini hidroizolacijski mort za tanke slojeve i glađu završnu površinu — s Xypex tehnologijom.",
            Desc: """
                <p>Manorteq AquaRend Thix Fine jednokomponentna je suha mješavina morta za hidroizolaciju i obnovu betonskih i zidanih konstrukcija, za primjene gdje je potreban tanki sloj (3–12 mm) ili glađa završna površina. Izvrsna hidroizolacijska svojstva osigurava Xypex tehnologija u sastavu.</p>
                <p>Drži vlastitu težinu pa je prikladan za vertikalne površine i rad iznad glave, a može se koristiti i na horizontalnim površinama. Nanosi se samostalno ili preko AquaRend Thix Regular ili Flow Regular. Na sanirane površine mogu se nanijeti isključivo paropropusne boje i premazi.</p>
                """,
            Info: [new("Debljina sloja", "3–12 mm"), new("Vodonepropusnost", "do 5 bara / 50 m vodnog stupca (EN 12390-8)"),
                   new("Primjena", "Vertikalno, iznad glave, horizontalno")],
            Docs: [Tds("AquaRend Thix Fine", "MANORTEQ-TDS-AquaRend-Thix-Fine.pdf", "")],
            Tags: ["Hidroizolacija", "Sanacija betona", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "AquaRendThixFine@2x.webp"]),

        new(
            Slug: "manorteq-aquarend-s-fine", BrandSlug: BrandSlug, CategorySlug: "aquarend",
            Name: "AquaRend S-Fine",
            Short: "Superfini mort za završnu obradu saniranog betona i brzo vraćanje u uporabu preko Xypex premaza.",
            Desc: """
                <p>Manorteq AquaRend S-Fine jednokomponentna je suha, sitnozrnata mješavina morta za finu završnu obradu saniranog betona i betona s manjim površinskim oštećenjima. Koristi se i kao superfini završni sloj preko AquaRend linije te kao sloj za brzo vraćanje u uporabu (Quick Return To Service) preko Xypex premaza ili sanacije.</p>
                <p>Nanosi se u sloju 2–4 mm. Zbog male debljine nije vodonepropusan prema EN 12390-8, no sadrži Xypex tehnologiju radi kompatibilnosti s AquaRend i Xypex sustavima. Kao dio sustava stvara površinski sloj nepropustan za tekućine, uključujući vodu, naftne derivate i organska otapala.</p>
                """,
            Info: [new("Debljina sloja", "2–4 mm"), new("Primjena", "Završni sloj preko AquaRend ili Xypex sustava")],
            Docs: [Tds("AquaRend S-Fine", "MANORTEQ-TDS-AquaRend-S-Fine.pdf", "")],
            Tags: ["Sanacija betona", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "AquaRendS-Fine@2x.webp"]),

        // ===== ARMATURA =====
        new(
            Slug: "manorteq-aquarend-fix-s", BrandSlug: BrandSlug, CategorySlug: "armatura",
            Name: "AquaRend Fix-S",
            Short: "Čelična zavarena mreža za mehaničko sidrenje AquaRend mortova na podlogu.",
            Desc: """
                <p>Manorteq AquaRend Fix-S služi za mehaničko vezivanje mortova AquaRend Thix Regular i Flow Regular na podlogu, osobito ondje gdje prevladava negativni tlak vode. Preporučuje se i za sanaciju mehanički opterećenih horizontalnih površina, poput garaža i skladišta.</p>
                <ul>
                  <li>Jednostavno pričvršćivanje i mala potrebna debljina pokrovnog sloja.</li>
                  <li>Prikladna za podove s ugrađenim grijanjem.</li>
                </ul>
                <h4>Ugradnja</h4>
                <p>Mreža se pričvršćuje na prethodno postavljena sidra — čelične šipke promjera 3–6 mm usidrene masom NonShrink Thix u bušotine dubine najmanje 65 mm na razmaku do 250–300 mm, ili na vijke.</p>
                """,
            Info: [new("Materijal", "Čelična žica Ø 2 mm, zavarena"), new("Okno", "40 × 40 mm"), new("Dimenzije ploče", "3000 × 1500 mm")],
            Docs: [Tds("AquaRend Fix-S", "MANORTEQ-TDS-AquaRend-Fix-S.pdf", "")],
            Tags: ["Sanacija betona", "Pribor"],
            Options: Pack("Ploča 3,0 × 1,5 m"), Skus: Sku("Ploča 3,0 × 1,5 m", "kom"),
            Images: [Img + "motive_AquaRendFixS@2x.webp", Img + "install_FixS01@2x.webp"]),

        new(
            Slug: "manorteq-aquarend-fix-b", BrandSlug: BrandSlug, CategorySlug: "armatura",
            Name: "AquaRend Fix-B",
            Short: "Bazaltna mreža za sidrenje AquaRend mortova u agresivnim sredinama.",
            Desc: """
                <p>Manorteq AquaRend Fix-B mreža je od bazaltnih vlakana za mehaničko vezivanje mortova AquaRend Thix Regular, Thix Fine i Flow Regular na podlogu u posebno agresivnim sredinama, primjerice ondje gdje prevladava negativni hidrostatski tlak ili postoji opasnost od korozivnih tvari.</p>
                <ul>
                  <li>Otporna na lužine i visoke temperature; otporna na većinu kiselih i korozivnih tvari.</li>
                  <li>Niska toplinska vodljivost; negoriva.</li>
                  <li>Jednostavno pričvršćivanje i mala potrebna debljina pokrovnog sloja.</li>
                </ul>
                <p>Napomena: AquaRend S-Fine nije dovoljno debeo kao drugi sloj preko Fix-B; ispod njega nanijeti AquaRend Thix (min. 10 mm) ili Fine (min. 5 mm). Sidrene bušotine: promjer najmanje 12 mm, dubina 75–100 mm.</p>
                """,
            Info: [new("Materijal", "Bazaltna vlakna"), new("Otpornost", "Lužine, temperatura, korozivne tvari"), new("Gorivost", "Negoriva")],
            Docs: [Tds("AquaRend Fix-B", "MANORTEQ-TDS-AquaRend-Fix-B.pdf", "")],
            Tags: ["Sanacija betona", "Pribor"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "kom"),
            Images: [Img + "motive_AquaRendFixB@2x.webp", Img + "install_FixB01@2x.webp"]),

        // ===== ZALIVNE MASE =====
        new(
            Slug: "manorteq-nonshrink-flow-regular", BrandSlug: BrandSlug, CategorySlug: "zalivne-mase",
            Name: "NonShrink Flow Regular",
            Short: "Tekuća bezskupljajuća zalivna masa visoke čvrstoće za šupljine od 20 mm — s Xypex tehnologijom.",
            Desc: """
                <p>Manorteq NonShrink Flow Regular jednokomponentna je bezskupljajuća suha mješavina visoke čvrstoće za trajno i vodonepropusno sidrenje elemenata te zatvaranje šupljina u poduprtim horizontalnim betonskim konstrukcijama. Izvrsna hidroizolacijska svojstva osigurava Xypex tehnologija u sastavu.</p>
                <p>Razvijena je za ručno zalijevanje manjih volumena, za šupljine od 20 mm naviše bez ograničenja dubine. Podnosi većinu dinamičkih opterećenja i štiti sidra. Ispunjeni otvori brtve se protiv vode i niza drugih tekućina. Koristi se i za zatvaranje spojeva u montažnim, vertikalnim i nepoduprtim horizontalnim konstrukcijama.</p>
                """,
            Info: [new("Šupljine", "od 20 mm, bez ograničenja dubine"), new("Tlačna čvrstoća 7 dana", "> 50 MPa (EN 12190)"),
                   new("Tlačna čvrstoća 28 dana", "> 65 MPa (EN 12190)"), new("Konzistencija", "Tekuća")],
            Docs: [Tds("NonShrink Flow", "MANORTEQ-TDS-NonShrink-Flow.pdf", "")],
            Tags: ["Hidroizolacija", "Sidrenje", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "NonShinkFlowRegular@2x.webp"]),

        new(
            Slug: "manorteq-nonshrink-flow-fine", BrandSlug: BrandSlug, CategorySlug: "zalivne-mase",
            Name: "NonShrink Flow Fine",
            Short: "Tekuća bezskupljajuća zalivna masa za male šupljine od 5 do 35 mm — s Xypex tehnologijom.",
            Desc: """
                <p>Manorteq NonShrink Flow Fine jednokomponentna je bezskupljajuća suha mješavina visoke čvrstoće za trajno i vodonepropusno sidrenje elemenata te zatvaranje šupljina u poduprtim horizontalnim betonskim konstrukcijama. Izvrsna hidroizolacijska svojstva osigurava Xypex tehnologija u sastavu.</p>
                <p>Razvijena je za ručno zalijevanje manjih volumena, za šupljine dubine 5–35 mm. Podnosi većinu dinamičkih opterećenja i štiti sidra. Koristi se i za zatvaranje spojeva u montažnim, vertikalnim i nepoduprtim horizontalnim konstrukcijama.</p>
                """,
            Info: [new("Šupljine", "5–35 mm"), new("Tlačna čvrstoća 7 dana", "> 45 MPa (EN 12190)"),
                   new("Tlačna čvrstoća 28 dana", "> 60 MPa (EN 12190)"), new("Konzistencija", "Tekuća")],
            Docs: [Tds("NonShrink Flow", "MANORTEQ-TDS-NonShrink-Flow.pdf", "")],
            Tags: ["Hidroizolacija", "Sidrenje", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "NonShinkFlowFine@2x.webp"]),

        new(
            Slug: "manorteq-nonshrink-thix-regular", BrandSlug: BrandSlug, CategorySlug: "zalivne-mase",
            Name: "NonShrink Thix Regular",
            Short: "Tiksotropna bezskupljajuća masa za sidrenje u vertikalnom betonu, šupljine od 20 mm — s Xypex tehnologijom.",
            Desc: """
                <p>Manorteq NonShrink Thix Regular jednokomponentna je bezskupljajuća suha mješavina visoke čvrstoće za sidrenje i stvaranje vodonepropusnih zona u vertikalnim i nepoduprtim horizontalnim betonskim konstrukcijama. Izvrsna hidroizolacijska svojstva osigurava Xypex tehnologija u sastavu.</p>
                <p>Namijenjena je ručnom zalijevanju manjih volumena, za šupljine od 20 mm naviše. Podnosi većinu dinamičkih opterećenja i pruža lužnatu zaštitu sidrima. Ispunjeni otvori brtve se protiv vode i drugih tekućina, uključujući naftne derivate.</p>
                """,
            Info: [new("Šupljine", "od 20 mm"), new("Tlačna čvrstoća 7 dana", "> 45 MPa (EN 12190)"),
                   new("Tlačna čvrstoća 28 dana", "> 55 MPa (EN 12190)"), new("Konzistencija", "Tiksotropna")],
            Docs: [Tds("NonShrink Thix", "MANORTEQ-TDS-NonShrink-Thix.pdf", "")],
            Tags: ["Hidroizolacija", "Sidrenje", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "NonShinkThixRegular@2x.webp"]),

        new(
            Slug: "manorteq-nonshrink-thix-fine", BrandSlug: BrandSlug, CategorySlug: "zalivne-mase",
            Name: "NonShrink Thix Fine",
            Short: "Tiksotropna bezskupljajuća masa za male šupljine od 5 do 35 mm — s Xypex tehnologijom.",
            Desc: """
                <p>Manorteq NonShrink Thix Fine jednokomponentna je bezskupljajuća suha mješavina visoke čvrstoće za sidrenje i stvaranje vodonepropusnih zona u vertikalnim i nepoduprtim horizontalnim betonskim konstrukcijama. Izvrsna hidroizolacijska svojstva osigurava Xypex tehnologija u sastavu.</p>
                <p>Namijenjena je ručnom zalijevanju manjih volumena, za šupljine dubine 5–35 mm. Podnosi većinu dinamičkih opterećenja i pruža lužnatu zaštitu sidrima. Ispunjeni otvori brtve se protiv vode i drugih tekućina, uključujući naftne derivate.</p>
                """,
            Info: [new("Šupljine", "5–35 mm"), new("Konzistencija", "Tiksotropna")],
            Docs: [Tds("NonShrink Thix", "MANORTEQ-TDS-NonShrink-Thix.pdf", "")],
            Tags: ["Hidroizolacija", "Sidrenje", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "NonShinkThixFine@2x.webp"]),

        // ===== GOTOVI BETON =====
        new(
            Slug: "manorteq-readyproof-thix-regular", BrandSlug: BrandSlug, CategorySlug: "gotovi-beton",
            Name: "ReadyProof Thix Regular",
            Short: "Vodonepropusni beton u vrećama s Xypex tehnologijom — za manje radove gdje mikser nije opcija.",
            Desc: """
                <p>Manorteq ReadyProof Thix Regular gotova je vodonepropusna betonska mješavina u vrećama s Xypex tehnologijom u sastavu. Koristi se za manje radove gdje dovoz cijelog miksera betona nije opravdan.</p>
                <p>Otporna je na vodu, naftne derivate i organska otapala. Preporučuje se za hidroizolaciju:</p>
                <ul>
                  <li>većih šupljina u betonu (npr. ostavljeni otvori),</li>
                  <li>općeg betona u manjim količinama,</li>
                  <li>revizijskih okana,</li>
                  <li>spremnika za vodu,</li>
                  <li>jama dizala.</li>
                </ul>
                <p>Može se koristiti i za sanaciju betonskih konstrukcija, čineći ih nepropusnima za vodu, naftne derivate i otapala.</p>
                """,
            Info: [new("Vrsta", "Jednokomponentna betonska mješavina s kristalnom tehnologijom"), new("Otpornost", "Voda, naftni derivati, organska otapala")],
            Docs: [Tds("ReadyProof Thix Regular", "MANORTEQ-TDS-ReadyProof-Thix-Regular.pdf", "")],
            Tags: ["Hidroizolacija", "Xypex tehnologija"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "ReadyProofThixRegular@2x.webp"]),

        // ===== DODACI ZA BETON =====
        new(
            Slug: "manorteq-curing-agent", BrandSlug: BrandSlug, CategorySlug: "dodaci-za-beton",
            Name: "Curing Agent",
            Short: "Sredstvo za njegu koje zadržava vlagu u svježem betonu i mortu — idealno uz Xypex sustave.",
            Desc: """
                <p>Manorteq Curing Agent sredstvo je za njegu koje smanjuje gubitak vode iz svježeg betona i mortova, osobito ondje gdje se koriste Xypex Admix, Concentrate, Patch'n Plug i Modified te Manorteq sustavi s Xypex tehnologijom (AquaRend i ReadyProof).</p>
                <p>Na svježem betonu i mortu ne samo da smanjuje gubitak slobodne vode, nego poboljšava i otvrdnjavanje površine te smanjuje njezinu upojnost. Na površinama tretiranim Xypex tehnologijom stvara uvjete pogodne za rast kristalne strukture. Gdje Xypex tehnički list propisuje svakodnevno vlaženje čistom vodom, nanošenje Curing Agenta uklanja potrebu za tim postupkom.</p>
                """,
            Info: [new("Namjena", "Njega svježeg betona, mortova i premaza"), new("Kompatibilnost", "Xypex, AquaRend, ReadyProof")],
            Docs: [Tds("Curing Agent", "MANORTEQ-TDS-Curing-Agent.pdf", "")],
            Tags: ["Njega betona", "Xypex kompatibilno"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "kom"),
            Images: [Img + "common_curing-agent@2x.webp"]),

        // ===== FREEZTEQ =====
        new(
            Slug: "manorteq-freezteq-original", BrandSlug: BrandSlug, CategorySlug: "kapilarna-vlaga",
            Name: "Freezteq Original",
            Short: "Smrznuti štapići s aktivnim tvarima za kemijsku barijeru protiv kapilarne vlage u zidovima.",
            Desc: """
                <p>Freezteq Original je prethodno pakirana otopina s aktivnim tvarima koja se prije uporabe smrzava. Smrznuti štapići umeću se izravno u bušotine u sljubnicama zida, čime se kapilarna vlaga napada izravno na glavnim putovima prodora.</p>
                <p>Učinkovitost proizlazi iz postupnog širenja aktivnih tvari kroz kapilare. Kao nositelj koristi vodu — isti medij kojim se vlaga širi — pa djeluje točno ondje gdje je problem.</p>
                <h4>Tehničke prednosti</h4>
                <ul>
                  <li>Bez tlaka — nema oštećenja kakva mogu nastati kod tlačnih sustava.</li>
                  <li>Lako je točno izračunati potrebnu količinu za tretman zida.</li>
                  <li>Zid i dalje normalno diše.</li>
                </ul>
                <h4>Praktične prednosti</h4>
                <ul>
                  <li>Jednostavna i brza ugradnja, s unutarnje ili vanjske strane.</li>
                  <li>Za različite vrste i debljine zidova; bez posebne opreme.</li>
                  <li>Bez mirisa, negoriv.</li>
                </ul>
                """,
            Info: [new("Primjena", "Kapilarna vlaga u zidanim zidovima"), new("Ugradnja", "Bušotine u sljubnicama, bez tlaka"),
                   new("Miris", "Bez mirisa"), new("Gorivost", "Negoriv")],
            Docs: [Tds("Freezteq Original", "tds-freezteq-original.pdf", ""),
                   new("doc:Upute za ugradnju — Freezteq Original", "", Doc + "freezteq-installation-guide.pdf")],
            Tags: ["Kapilarna vlaga", "Sanacija zidova"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "kom"),
            Images: [Img + "common_freezteq-original@2x.webp"]),

        new(
            Slug: "manorteq-freezteq-br-base", BrandSlug: BrandSlug, CategorySlug: "paropropusna-zbuka",
            Name: "Freezteq BR-Base",
            Short: "Visokoporozna vodoodbojna sanacijska žbuka za unutarnju obnovu vlažnih zidova (EN 998-1).",
            Desc: """
                <p>Freezteq BR-Base visokoporozan je materijal vodoodbojnih svojstava, namijenjen unutarnjoj obnovi vlažnih zidanih konstrukcija. Pomaže prirodnom isušivanju zida i zadržava soli u svojoj poroznoj strukturi, čime učinkovito štiti od oštećenja vlagom.</p>
                <ul>
                  <li>Visoka difuzijska propusnost.</li>
                  <li>Velik kapacitet upijanja soli.</li>
                  <li>Dugotrajno suzbijanje učinaka vlage.</li>
                </ul>
                <p>Jednokomponentna suha mješavina od portland cementa, vapna, standardnih i lakih agregata, posebnih dodataka (uključujući vodoodbojne) i polipropilenskih vlakana za trajnost. Za unutarnju primjenu, kao dio sustava Freezteq Breathable Render nakon tretmana Freezteq Originalom.</p>
                """,
            Info: [new("Norma", "EN 998-1"), new("Primjena", "Unutarnja"), new("Svojstva", "Porozna, vodoodbojna, akumulira soli")],
            Docs: [Tds("Freezteq BR-Base", "tds-freezteq-br-base.pdf", "")],
            Tags: ["Kapilarna vlaga", "Sanacija zidova"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "common-freezteq-br-base@2x.webp"]),

        new(
            Slug: "manorteq-freezteq-br-finish", BrandSlug: BrandSlug, CategorySlug: "paropropusna-zbuka",
            Name: "Freezteq BR-Finish",
            Short: "Fini paropropusni završni sloj sustava Freezteq Breathable Render, do 2 mm (EN 998-1).",
            Desc: """
                <p>Freezteq BR-Finish završni je sloj sustava Freezteq Breathable Render i daje finu završnu površinu. Nanosi se u debljini do 2 mm.</p>
                <p>Jednokomponentna suha mješavina na bazi anorganskih veziva, standardnih i lakih punila te posebnih dodataka koji osiguravaju povećanu poroznost i vodoodbojnost.</p>
                <ul>
                  <li>Nizak otpor difuziji — propušta vodenu paru.</li>
                  <li>Prikladna podloga za odgovarajuće boje.</li>
                  <li>Arhitektonski završni izgled površine.</li>
                </ul>
                """,
            Info: [new("Norma", "EN 998-1"), new("Debljina sloja", "do 2 mm"), new("Svojstva", "Paropropusna, vodoodbojna")],
            Docs: [Tds("Freezteq BR-Finish", "tds-freezteq-br-finish.pdf", "164 KB")],
            Tags: ["Kapilarna vlaga", "Sanacija zidova"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "common-freezteq-br-finish@2x.webp"]),

        new(
            Slug: "manorteq-freezteq-br-bond", BrandSlug: BrandSlug, CategorySlug: "paropropusna-zbuka",
            Name: "Freezteq BR-Bond",
            Short: "Paropropusni sloj za prianjanje koji sidri Freezteq BR-Base na podlogu (EN 998-1).",
            Desc: """
                <p>Freezteq BR-Bond dio je sustava Freezteq Breathable Render — učinkovit sloj za prianjanje koji osigurava dobro sidrenje žbuke Freezteq BR-Base na podlogu.</p>
                <p>Jednokomponentna suha mješavina izvrsnih adhezijskih svojstava, porozne strukture i dobre paropropusnosti, sastavljena od portland cementa i posebnih dodataka koji osiguravaju prianjanje i aeraciju mješavine.</p>
                <ul>
                  <li>Miješa se samo s vodom; nanosi se ručno.</li>
                  <li>Dobro prianjanje na podlogu.</li>
                  <li>Visoka poroznost, nizak otpor difuziji vodene pare.</li>
                </ul>
                """,
            Info: [new("Norma", "EN 998-1, dodatak ZA"), new("Nanošenje", "Ručno"), new("Svojstva", "Porozna, paropropusna")],
            Docs: [Tds("Freezteq BR-Bond", "tds-freezteq-br-bond.pdf", "161 KB")],
            Tags: ["Kapilarna vlaga", "Sanacija zidova"],
            Options: Pack("Prema upitu"), Skus: Sku("Prema upitu", "vreća"),
            Images: [Img + "common-freezteq-br-bond@2x.webp"]),
    ];
}
