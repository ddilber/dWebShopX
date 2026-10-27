# MANORTEQ: product images and technical data sheets

These files are referenced by `src/dWebShop.Infrastructure/Persistence/ManorteqSeedData.cs`.
Save each one under `SharedUploadsPath` using the file name exactly as listed (dev: `F:/PROJECTS/Blazor/dWebShopX/uploads`, prod: `/srv/dotnet/uploads_test`).

- Images go in `product_images/manorteq/`.
- PDFs go in `product_docs/manorteq/`.

manorteq.com's WAF blocks scripted downloads with HTTP 466. On 27 Oct 2026 all files below were saved through a real browser into the local `uploads/` folder (55 images, 25 PDFs, logo). They are not in git. On deploy, copy `uploads/product_images/manorteq/`, `uploads/product_docs/manorteq/` and `uploads/brand_images/manorteq-logo.png` to the server's SharedUploadsPath. Higher-resolution originals can be requested from Manorteq. The source text was scraped from manorteq.com on 27 Oct 2026.

Images base URL: `https://manorteq.com/wp-content/uploads/`

## Images → `product_images/manorteq/`

| Product | Files (path under the base URL) |
|---|---|
| HydroBar Original | 2022/03/waterbar-original@2x.png, 2022/03/install_original01@2x.webp, 2022/03/install_original02@2x.webp, 2022/03/install_original_03@2x.webp, 2022/03/install_original04@2x.webp |
| HydroBar Evo | 2022/03/waterbar-evo@2x.png, 2022/03/install_evo01@2x.webp, 2022/03/install_evo02@2x.webp, 2022/03/install_evo03@2x.webp, 2022/03/install-evo04@2x.webp, 2022/03/install_expanded@2x.webp |
| WaterBar Rigid KJ | 2022/03/rigid_KJ@2x.png, 2022/03/install_KJ01@2x.webp … install_KJ04@2x.webp |
| WaterBar Rigid CJ | 2022/03/rigid_CJ@2x.png, 2022/03/install_CJ01@2x.webp … install_CJ04@2x.webp |
| WaterBar Rigid CI | 2022/03/rigidCI@2x.png, 2022/03/install_CI01@2x.webp … install_CI04@2x.webp |
| FormPlug | 2022/03/plug@2x.png, 2022/03/install_plug01@2x.webp, 2022/03/install_plug02@2x.webp |
| FormRing | 2022/03/ring@2x.png, 2022/03/install_ring01@2x.webp, 2022/03/install_ring02@2x.webp |
| SynMastic | 2022/03/SynMastic@2x.png |
| Fixing Glue | 2022/03/glue@2x.png |
| Fixing Wire | 2022/03/fixing_Wire@2x.png, 2022/03/install_wire01@2x.webp, 2022/03/install_wire02@2x.webp |
| AquaRend Thix Regular | 2026/02/AquaRendThixRegular@2x.webp |
| AquaRend Flow Regular | 2026/02/AquaRendFlowRegular@2x.webp |
| AquaRend Thix Fine | 2026/02/AquaRendThixFine@2x.webp |
| AquaRend S-Fine | 2026/02/AquaRendS-Fine@2x.webp |
| AquaRend Fix-S | 2023/02/motive_AquaRendFixS@2x.webp, 2023/02/install_FixS01@2x.webp |
| AquaRend Fix-B | 2023/02/motive_AquaRendFixB@2x.webp, 2023/02/install_FixB01@2x.webp |
| NonShrink Flow Regular | 2026/02/NonShinkFlowRegular@2x.webp |
| NonShrink Flow Fine | 2026/03/NonShinkFlowFine@2x.webp |
| NonShrink Thix Regular | 2026/03/NonShinkThixRegular@2x.webp |
| NonShrink Thix Fine | 2026/03/NonShinkThixFine@2x.webp |
| ReadyProof Thix Regular | 2026/02/ReadyProofThixRegular@2x.webp |
| Curing Agent | 2025/05/common_curing-agent@2x.webp |
| Freezteq Original | 2025/04/common_freezteq-original@2x.webp |
| Freezteq BR-Base | 2025/05/common-freezteq-br-base@2x.webp |
| Freezteq BR-Finish | 2025/05/common-freezteq-br-finish@2x.webp |
| Freezteq BR-Bond | 2025/05/common-freezteq-br-bond@2x.webp |

Logo (optional, goes in `brand_images/`): `2022/03/manorteq-logo.png`

## TDS PDFs → `product_docs/manorteq/`

Base URL: `https://manorteq.com/downloads/`

MANORTEQ-TDS-HydroBarOriginal.pdf · MANORTEQ-TDS-HydroBarEvo.pdf · MANORTEQ-TDS-WaterBar-KJ.pdf ·
MANORTEQ-TDS-WaterBar-CJ.pdf · MANORTEQ-TDS-WaterBar-CI.pdf · MANORTEQ-TDS-FormPlug.pdf ·
MANORTEQ-TDS-FormRing.pdf · MANORTEQ-TDS-SynMastic.pdf · MANORTEQ-TDS-FixingGlue.pdf ·
MANORTEQ-TDS-FixingWire.pdf · MANORTEQ-TDS-AquaRend-Thix-Regular.pdf · MANORTEQ-TDS-AquaRend-Flow-Regular.pdf ·
MANORTEQ-TDS-AquaRend-Thix-Fine.pdf · MANORTEQ-TDS-AquaRend-S-Fine.pdf · MANORTEQ-TDS-AquaRend-Fix-S.pdf ·
MANORTEQ-TDS-AquaRend-Fix-B.pdf · MANORTEQ-TDS-NonShrink-Flow.pdf · MANORTEQ-TDS-NonShrink-Thix.pdf ·
MANORTEQ-TDS-ReadyProof-Thix-Regular.pdf · MANORTEQ-TDS-Curing-Agent.pdf · tds-freezteq-original.pdf ·
tds-freezteq-br-base.pdf · tds-freezteq-br-finish.pdf · tds-freezteq-br-bond.pdf · freezteq-installation-guide.pdf

## Still open
- **Packaging/SKU:** most products are seeded with "Prema upitu". Fix the real pack sizes in the Admin once the client sends them.
- **Founding year:** `Since` in `BrandDetail.razor` is "—" because manorteq.com doesn't state one.
