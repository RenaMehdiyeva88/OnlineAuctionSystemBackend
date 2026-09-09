# Auction House System - Architecture & Schema

## 📋 Executive Summary

Premium auction platform focused on **collectible market** segments:
- 🪙 Ancient Coins (numismatic collectibles)
- ⌚ Luxury Watches (horological heritage)
- 👟 Hype Sneakers (contemporary collectibles)
- 🎮 Retro Gaming (vintage electronics)
- 📷 Electronics/Cameras (collectible optics)

---

## 1. Strategic Shift: From Mass-Market to Premium

### Why We Removed Initial Categories

| Category | Removed | Reason |
|----------|---------|--------|
| **Apple AirPods** | ✗ | Low AOV (~$150-250), high competition, no collectibility |
| **Ray-Ban Sunglasses** | ✗ | Mass-market positioning, marginal markup, seasonal trend |
| **Baseball Gloves** | ✗ | Sport equipment, not investment-grade, poor repeat buyers |

### Why Premium Categories Win

```
Metric                  Old Categories    Premium Categories
─────────────────────────────────────────────────────────────
Average Lot Value       $150-400          $800-65,000
Seller Expertise        Low               High (collectors/dealers)
Repeat Seller Rate      ~20%              ~75% (professional dealers)
Commission Potential    Low ($20-60)      High ($200-10,000+)
Community Loyalty       Transactional     Passionate (fan bases)
Authentication Need     None              Critical (enables trust)
```

---

## 2. Database Schema

### 2.1 Core Tables

#### Categories Table
```sql
CREATE TABLE Categories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Slug NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(MAX),
    Icon NVARCHAR(100), -- 'coin', 'watch', 'sneaker', 'gamepad', 'camera'
    AttributeSchema NVARCHAR(MAX), -- JSON Schema for validation
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

#### Auctions Table (Core with JSONB-like approach)
```sql
CREATE TABLE Auctions (
    -- Identity
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    
    -- Category & Attributes
    CategoryId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Categories(Id),
    Attributes NVARCHAR(MAX), -- JSON: category-specific properties
    
    -- Media
    PrimaryImageUrl NVARCHAR(500), -- First image from gallery
    ImageUrls NVARCHAR(MAX), -- JSON array: ["/images/lots/..."]
    
    -- Pricing
    StartingPrice DECIMAL(18,2) NOT NULL,
    CurrentHighestBid DECIMAL(18,2),
    ReservePrice DECIMAL(18,2), -- Optional minimum
    FinalPrice DECIMAL(18,2), -- Set when ended
    
    -- Status & Timeline
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active|Ended|Unsold|Archived
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    
    -- Relationships
    SellerId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE RESTRICT,
    WinnerId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE RESTRICT,
    CategoryId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Categories(Id),
    
    -- Metadata
    ViewCount INT DEFAULT 0,
    BidCount INT DEFAULT 0,
    IsFeatured BIT DEFAULT 0,
    
    CONSTRAINT CK_AuctionStatus CHECK (Status IN ('Active', 'Ended', 'Unsold', 'Archived')),
    CONSTRAINT CK_EndTimeAfterStart CHECK (EndTime > StartTime),
    CONSTRAINT CK_ReservePositive CHECK (ReservePrice IS NULL OR ReservePrice > 0),
    INDEX IX_CategoryId (CategoryId),
    INDEX IX_Status (Status),
    INDEX IX_EndTime (EndTime)
);
```

### 2.2 Category-Specific Attribute Schemas

#### Ancient Coins
```json
{
  "origin": "Ancient Rome|Byzantine|Ptolemaic|Ancient Greece|Imperial China|...",
  "era": "string (e.g., '80 AD', '400 BC')",
  "material": "Gold|Silver|Bronze|Copper",
  "gradeSystem": "NGC|PCGS|string (e.g., 'VF-30', 'MS-70')",
  "weight": "number (grams)",
  "diameter": "number (mm)",
  "rarity": "Common|Uncommon|Rare|Very Rare",
  "provenance": "Museum|Private Collection|Excavation|Estate|Auction House",
  "certification": "string (NGC, PCGS, etc.)",
  "description": "Obverse/Reverse descriptions"
}
```

#### Luxury Watches
```json
{
  "brand": "Omega|Rolex|Seiko|Patek Philippe|Tissot|Longines",
  "model": "string (e.g., 'Speedmaster Professional 145.022')",
  "yearOfManufacture": "number (YYYY)",
  "movement": "string (e.g., 'Calibre 861 Manual Winding')",
  "caseSize": "number (mm)",
  "caseMaterial": "Stainless Steel|Gold|Titanium|Platinum",
  "waterResistance": "string (e.g., '50m', '300m', 'Diver')",
  "condition": "New|Mint|Excellent|Good|Fair",
  "originalBox": "boolean",
  "originalPapers": "boolean",
  "certification": "string (Omega Certificate, Rolex, Service History)",
  "serviceHistory": "string"
}
```

#### Hype Sneakers
```json
{
  "brand": "Nike|Jordan|Adidas|Yeezy|New Balance|Puma",
  "model": "string (e.g., 'Air Jordan 1 Retro High OG')",
  "colorway": "string (e.g., 'Black/Red/White (Chicago)')",
  "releaseYear": "number (YYYY)",
  "size": "string (US 10.5, EU 44.5)",
  "condition": "DS (Deadstock)|BNIB|VNDS|Good|Fair",
  "authenticity": "OG Box|OG Laces|All Tags|Stock X Verified",
  "hasStockX": "boolean",
  "rarity": "General Release|Limited|Collaboration|Exclusive",
  "marketPrice": "number (USD)"
}
```

#### Retro Gaming
```json
{
  "console": "NES|SNES|Genesis|Atari|Game Boy|PlayStation",
  "generation": "8-bit|16-bit|32-bit|Handheld",
  "variant": "Original|Revised|Clone|Limited Edition",
  "condition": "Sealed|Excellent|Good|Fair|Parts Only",
  "region": "NTSC-U/C|PAL|NTSC-J",
  "accessories": ["string array: Controllers, Cables, Games"],
  "workingCondition": "Full Working|Minor Issues|Untested",
  "rarity": "Common|Uncommon|Rare|Very Rare",
  "marketValue": "number (USD)"
}
```

#### Electronics (Cameras)
```json
{
  "manufacturer": "Canon|Nikon|Leica|Pentax|Hasselblad",
  "model": "string (e.g., 'AE-1 Program')",
  "type": "Film SLR|Digital DSLR|Rangefinder|Medium Format|Instant",
  "year": "number (YYYY)",
  "shutterCount": "number or 'Unknown'",
  "condition": "Mint|Excellent|Good|Fair",
  "opticsCondition": "Crystal Clear|Excellent|Good|Hazy",
  "includedLenses": ["string array of lens models"],
  "includedAccessories": ["string array"],
  "functionality": "Fully Functional|Minor Issues|Untested|Parts Only",
  "certification": "CLA Certificate|Service Record|Tested"
}
```

---

## 3. Image Architecture

### File Structure
```
public/images/lots/
├── ancient-coins/
│   ├── 1.jpg (40 KB)
│   ├── 2.jpg (127 KB)
│   ├── 3.jpg (76 KB)
│   ├── 4.jpg (67 KB)
│   └── 5.jpg (98 KB)
├── canon-camera/
│   ├── 1.jpg (47 KB)
│   ├── 2.jpg (53 KB)
│   ├── 3.jpg (60 KB)
│   ├── 4.jpg (116 KB)
│   └── 5.jpg (54 KB)
├── nes-console/
│   ├── 1.jpg (69 KB)
│   ├── 2.jpg (73 KB)
│   ├── 3.jpg (81 KB)
│   ├── 4.jpg (94 KB)
│   └── 5.jpg (65 KB)
├── nike-shoes/
│   ├── 1.jpg (35 KB)
│   ├── 2.jpg (36 KB)
│   ├── 3.jpg (61 KB)
│   ├── 4.jpg (52 KB)
│   └── 5.jpg (28 KB)
└── omega-watch/
    ├── 1.jpg (73 KB)
    ├── 2.jpg (69 KB)
    ├── 3.jpg (91 KB)
    ├── 4.jpg (85 KB)
    └── 5.jpg (85 KB)
```

**Total: 25 images, ~1.8 MB**

### URL Format (Vite)
```
http://localhost:5174/images/lots/{category}/{1-5}.jpg
```

### Storage Strategy
- **Frontend**: Local `/public/images/lots/` (served by Vite)
- **Database**: Store relative paths `/images/lots/category/n.jpg`
- **Fallback**: Generate from category + title in frontend (imageFallback.ts)

---

## 4. Seed Data Sample

### Initial Setup SQL

```sql
-- Insert Categories
DECLARE @AncientCoinsCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @WatchesCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @SneakersCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @GamingCatId UNIQUEIDENTIFIER = NEWID();
DECLARE @CamerasCatId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Categories (Id, Name, Slug, Description, Icon, AttributeSchema) VALUES
(@AncientCoinsCatId, 'Ancient Coins', 'ancient-coins', 'Rare coins from ancient civilizations', 'coin', '{}'),
(@WatchesCatId, 'Luxury Watches', 'luxury-watches', 'Collectible timepieces and chronographs', 'watch', '{}'),
(@SneakersCatId, 'Hype Sneakers', 'hype-sneakers', 'Limited edition and rare sneakers', 'sneaker', '{}'),
(@GamingCatId, 'Retro Gaming', 'retro-gaming', 'Vintage gaming consoles and systems', 'gamepad', '{}'),
(@CamerasCatId, 'Electronics', 'electronics', 'Collectible cameras and vintage electronics', 'camera', '{}');

-- Seller
DECLARE @SellerId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Role = 'Seller');

-- Ancient Coins (5 auctions)
INSERT INTO Auctions (Id, Title, CategoryId, Description, Attributes, PrimaryImageUrl, ImageUrls, 
                      StartingPrice, ReservePrice, Status, StartTime, EndTime, SellerId, BidCount) VALUES

(NEWID(), 'Roman Denarius - Emperor Titus (79-81 AD)', @AncientCoinsCatId,
 'Authentic silver denarius from the reign of Titus. Portrait on obverse, Victory on reverse. Museum quality strike.',
 '{"origin":"Ancient Rome","year":"80 AD","material":"Silver","gradeSystem":"VF-30","weight":3.2,"diameter":18,"rarity":"Rare","provenance":"Private Collection, Zurich","certification":"NGC"}',
 '/images/lots/ancient-coins/1.jpg',
 '["/images/lots/ancient-coins/1.jpg"]',
 1500.00, 1200.00, 'Active', DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(DAY, 7, GETUTCDATE()), @SellerId, 0),

(NEWID(), 'Byzantine Gold Solidus - Justinian I', @AncientCoinsCatId,
 'Gold solidus with exceptional provenance. Portraits of Justinian and Theodora.',
 '{"origin":"Byzantine","year":"540 AD","material":"Gold","gradeSystem":"AU-58","weight":4.5,"diameter":21,"rarity":"Very Rare","provenance":"Excavation, Istanbul","certification":"PCGS"}',
 '/images/lots/ancient-coins/2.jpg',
 '["/images/lots/ancient-coins/2.jpg"]',
 3200.00, 2800.00, 'Active', DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(DAY, 10, GETUTCDATE()), @SellerId, 3),

(NEWID(), 'Ptolemaic Silver Tetradrachm - Cleopatra VII', @AncientCoinsCatId,
 'Rare tetradrachm featuring Cleopatra VII. High relief, excellent centering. Museum documentation.',
 '{"origin":"Ptolemaic Egypt","year":"50 BC","material":"Silver","gradeSystem":"XF-45","weight":14.0,"diameter":25,"rarity":"Rare","provenance":"Museum De-accession, Berlin","certification":"NGC"}',
 '/images/lots/ancient-coins/3.jpg',
 '["/images/lots/ancient-coins/3.jpg"]',
 2100.00, 1800.00, 'Active', DATEADD(HOUR, -3, GETUTCDATE()), DATEADD(DAY, 8, GETUTCDATE()), @SellerId, 1),

(NEWID(), 'Greek Silver Drachma - Athens, Owl Type', @AncientCoinsCatId,
 'Classical period Athenian drachma with iconic owl. Superior strike quality.',
 '{"origin":"Ancient Greece","year":"400 BC","material":"Silver","gradeSystem":"EF-40","weight":4.2,"diameter":17,"rarity":"Common","provenance":"Estate Collection, London","certification":"PCGS"}',
 '/images/lots/ancient-coins/4.jpg',
 '["/images/lots/ancient-coins/4.jpg"]',
 890.00, 700.00, 'Active', DATEADD(HOUR, -4, GETUTCDATE()), DATEADD(DAY, 6, GETUTCDATE()), @SellerId, 5),

(NEWID(), 'Han Dynasty Bronze Spade Coin (206 BC)', @AncientCoinsCatId,
 'Rare bronze spade-shaped coin. Archaeological provenance verified. Academic documentation included.',
 '{"origin":"Imperial China","year":"100 BC","material":"Bronze","gradeSystem":"Good","weight":8.5,"diameter":45,"rarity":"Very Rare","provenance":"Research Collection","certification":"Verified"}',
 '/images/lots/ancient-coins/5.jpg',
 '["/images/lots/ancient-coins/5.jpg"]',
 1650.00, 1400.00, 'Active', DATEADD(HOUR, -5, GETUTCDATE()), DATEADD(DAY, 9, GETUTCDATE()), @SellerId, 2),

-- Luxury Watches (5 auctions)
(NEWID(), 'Omega Speedmaster Professional - 1970s (Moon Watch)', @WatchesCatId,
 'Legendary chronograph worn on Apollo 13. 42mm stainless steel. Full set with box & papers.',
 '{"brand":"Omega","model":"Speedmaster Professional 145.022","yearOfManufacture":1973,"movement":"Calibre 861","caseSize":42,"material":"Stainless Steel","waterResistance":"50m","condition":"Excellent","originalBox":true,"originalPapers":true,"certification":"Omega Heritage Certificate"}',
 '/images/lots/omega-watch/1.jpg',
 '["/images/lots/omega-watch/1.jpg","/images/lots/omega-watch/2.jpg","/images/lots/omega-watch/3.jpg"]',
 12500.00, 11000.00, 'Active', DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(DAY, 14, GETUTCDATE()), @SellerId, 8),

(NEWID(), 'Rolex Daytona Paul Newman - Reference 6239', @WatchesCatId,
 'Vintage sports chronograph 1965. Tropical dial. Original papers. Investment-grade.',
 '{"brand":"Rolex","model":"Daytona 6239","yearOfManufacture":1965,"movement":"Valjoux 72","caseSize":37,"material":"Yellow Gold","waterResistance":"100m","condition":"Good","originalBox":false,"originalPapers":true,"certification":"Rolex Authentication"}',
 '/images/lots/omega-watch/4.jpg',
 '["/images/lots/omega-watch/4.jpg","/images/lots/omega-watch/5.jpg"]',
 28000.00, 25000.00, 'Active', DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(DAY, 21, GETUTCDATE()), @SellerId, 12),

(NEWID(), 'Seiko SKX007 Diver - Cult Classic', @WatchesCatId,
 'Automatic diver. Recently serviced. Excellent condition.',
 '{"brand":"Seiko","model":"SKX007J2","yearOfManufacture":2005,"movement":"Seiko 7S26","caseSize":42,"material":"Stainless Steel","waterResistance":"200m","condition":"Excellent","originalBox":true,"originalPapers":true,"certification":"Watchmaker CLA"}',
 '/images/lots/omega-watch/1.jpg',
 '["/images/lots/omega-watch/1.jpg"]',
 450.00, 350.00, 'Active', DATEADD(HOUR, -3, GETUTCDATE()), DATEADD(DAY, 7, GETUTCDATE()), @SellerId, 4),

(NEWID(), 'Patek Philippe Nautilus 5711/1A - Discontinued', @WatchesCatId,
 'Holy Grail of modern watches. Full set. Mint condition.',
 '{"brand":"Patek Philippe","model":"Nautilus 5711/1A","yearOfManufacture":2015,"movement":"Calibre 26-330 S C","caseSize":40,"material":"Stainless Steel","waterResistance":"120m","condition":"Mint","originalBox":true,"originalPapers":true,"certification":"Patek Philippe Certificate"}',
 '/images/lots/omega-watch/2.jpg',
 '["/images/lots/omega-watch/2.jpg","/images/lots/omega-watch/3.jpg"]',
 65000.00, 60000.00, 'Active', DATEADD(HOUR, -4, GETUTCDATE()), DATEADD(DAY, 30, GETUTCDATE()), @SellerId, 15),

(NEWID(), 'Tissot PRX Titanium 35mm - Modern Classic', @WatchesCatId,
 'Contemporary dress watch. Titanium case. Factory sealed.',
 '{"brand":"Tissot","model":"T137.407.11.351.00","yearOfManufacture":2023,"movement":"Quartz ETA F06.111","caseSize":35,"material":"Titanium","waterResistance":"100m","condition":"New","originalBox":true,"originalPapers":true,"certification":"Official"}',
 '/images/lots/omega-watch/3.jpg',
 '["/images/lots/omega-watch/3.jpg"]',
 650.00, 500.00, 'Active', DATEADD(HOUR, -5, GETUTCDATE()), DATEADD(DAY, 5, GETUTCDATE()), @SellerId, 2),

-- Hype Sneakers (5 auctions)
(NEWID(), 'Nike Air Jordan 1 Retro High OG "Chicago" - 1985', @SneakersCatId,
 'Deadstock OG 1985 release. Original box with tissue. Flight logo tag intact.',
 '{"brand":"Jordan","model":"Air Jordan 1 Retro High OG","colorway":"Black/Red/White","releaseYear":1985,"size":"US 10.5","condition":"DS","authenticity":"OG Box, OG Laces, All Tags","hasStockX":true,"rarity":"Very Rare"}',
 '/images/lots/nike-shoes/1.jpg',
 '["/images/lots/nike-shoes/1.jpg"]',
 8500.00, 7500.00, 'Active', DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(DAY, 10, GETUTCDATE()), @SellerId, 20),

(NEWID(), 'Nike Dunk Low "Panda" 2021 - BNIB', @SneakersCatId,
 'Brand new in box. Perfect for collectors. Never worn.',
 '{"brand":"Nike","model":"Dunk Low","colorway":"White/Black","releaseYear":2021,"size":"US 9","condition":"BNIB","authenticity":"OG Box, OG Laces","hasStockX":true,"rarity":"General Release"}',
 '/images/lots/nike-shoes/2.jpg',
 '["/images/lots/nike-shoes/2.jpg"]',
 250.00, 150.00, 'Active', DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(DAY, 4, GETUTCDATE()), @SellerId, 6),

(NEWID(), 'Travis Scott x Nike SB Dunk Low "Cactus Jack"', @SneakersCatId,
 'VNDS. Worn once for photography. StockX verified.',
 '{"brand":"Nike","model":"SB Dunk Low x Travis Scott","colorway":"Reverse Mocha","releaseYear":2021,"size":"US 11","condition":"VNDS","authenticity":"OG Box, Receipts, StockX Verified","hasStockX":true,"rarity":"Collaboration"}',
 '/images/lots/nike-shoes/3.jpg',
 '["/images/lots/nike-shoes/3.jpg"]',
 1200.00, 900.00, 'Active', DATEADD(HOUR, -3, GETUTCDATE()), DATEADD(DAY, 12, GETUTCDATE()), @SellerId, 9),

(NEWID(), 'Yeezy 350 V2 "Zebra" 2017 - Pre-owned', @SneakersCatId,
 'Well-maintained. Authentic Adidas Yeezy. Minor creasing.',
 '{"brand":"Adidas Yeezy","model":"350 V2","colorway":"Zebra","releaseYear":2017,"size":"US 10","condition":"Good","authenticity":"OG Box","hasStockX":true,"rarity":"Limited Release"}',
 '/images/lots/nike-shoes/4.jpg',
 '["/images/lots/nike-shoes/4.jpg"]',
 650.00, 500.00, 'Active', DATEADD(HOUR, -4, GETUTCDATE()), DATEADD(DAY, 8, GETUTCDATE()), @SellerId, 7),

(NEWID(), 'Nike Air Max 90 "Reverse Infrared" OG 2010', @SneakersCatId,
 'Vintage 2010 release. Excellent condition. Display piece.',
 '{"brand":"Nike","model":"Air Max 90","colorway":"Reverse Infrared","releaseYear":2010,"size":"US 8.5","condition":"Excellent","authenticity":"OG Box (Degraded)","hasStockX":false,"rarity":"Uncommon"}',
 '/images/lots/nike-shoes/5.jpg',
 '["/images/lots/nike-shoes/5.jpg"]',
 380.00, 250.00, 'Active', DATEADD(HOUR, -5, GETUTCDATE()), DATEADD(DAY, 6, GETUTCDATE()), @SellerId, 3),

-- Retro Gaming (5 auctions)
(NEWID(), 'Nintendo Entertainment System (NES) Original 1983', @GamingCatId,
 'Black model with original controllers and RF cable. Tested and fully working. NO YELLOWING.',
 '{"console":"NES","generation":"8-bit","variant":"Original 1983","condition":"Excellent","region":"NTSC-U/C","accessories":["Original Controllers (2x)","RF Cable","Power Cable"],"workingCondition":"Full Working","rarity":"Common"}',
 '/images/lots/nes-console/1.jpg',
 '["/images/lots/nes-console/1.jpg","/images/lots/nes-console/2.jpg"]',
 450.00, 300.00, 'Active', DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(DAY, 7, GETUTCDATE()), @SellerId, 4),

(NEWID(), 'Sega Genesis Model 1 - Complete in Box (CIB)', @GamingCatId,
 'Boxed original with Sonic cartridge and all accessories. Original manual included.',
 '{"console":"Sega Genesis","generation":"16-bit","variant":"Model 1","condition":"Excellent","region":"NTSC-U/C","accessories":["Sonic","Controller","Adapter","AV Cable","Box","Manual","Styrofoam"],"workingCondition":"Full Working","rarity":"Uncommon"}',
 '/images/lots/nes-console/2.jpg',
 '["/images/lots/nes-console/2.jpg","/images/lots/nes-console/3.jpg"]',
 650.00, 500.00, 'Active', DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(DAY, 9, GETUTCDATE()), @SellerId, 6),

(NEWID(), 'Super Nintendo SNES - Mint Condition Boxed', @GamingCatId,
 'Boxed original. Never played. All connections verified.',
 '{"console":"SNES","generation":"16-bit","variant":"Original PAL","condition":"Mint","region":"PAL","accessories":["Controllers (2x)","Power Cable","AV Cable","RF Adapter","Box","Manual"],"workingCondition":"Full Working - Never Used","rarity":"Rare"}',
 '/images/lots/nes-console/3.jpg',
 '["/images/lots/nes-console/3.jpg","/images/lots/nes-console/4.jpg"]',
 1200.00, 1000.00, 'Active', DATEADD(HOUR, -3, GETUTCDATE()), DATEADD(DAY, 14, GETUTCDATE()), @SellerId, 8),

(NEWID(), 'Atari 2600 - Wood Grain Model (Rare)', @GamingCatId,
 'Very rare wood grain finish. Working with 10 classic games.',
 '{"console":"Atari 2600","generation":"8-bit","variant":"Wood Grain","condition":"Good","region":"NTSC-U/C","accessories":["Joysticks (2x)","Power Adapter","RF Cable","10 Cartridges"],"workingCondition":"Fully Functional","rarity":"Very Rare"}',
 '/images/lots/nes-console/4.jpg',
 '["/images/lots/nes-console/4.jpg"]',
 800.00, 600.00, 'Active', DATEADD(HOUR, -4, GETUTCDATE()), DATEADD(DAY, 11, GETUTCDATE()), @SellerId, 5),

(NEWID(), 'Game Boy Pocket - Silver Japan (Backlit Mod)', @GamingCatId,
 'Handheld classic. Backlit modded. Tetris and carry case included.',
 '{"console":"Game Boy Pocket","generation":"Handheld","variant":"Japan Silver","condition":"Good","region":"NTSC-J","accessories":["Tetris","Carry Case","USB-C Battery","Screen Protector"],"workingCondition":"Full Working with Backlight","rarity":"Common"}',
 '/images/lots/nes-console/5.jpg',
 '["/images/lots/nes-console/5.jpg"]',
 320.00, 200.00, 'Active', DATEADD(HOUR, -5, GETUTCDATE()), DATEADD(DAY, 5, GETUTCDATE()), @SellerId, 2),

-- Electronics (5 auctions)
(NEWID(), 'Canon AE-1 Program Film SLR - 1980s', @CamerasCatId,
 'Compact 35mm film camera. Fully functional. Includes Canon FD 50mm f/1.8 lens.',
 '{"manufacturer":"Canon","model":"AE-1 Program","type":"Film SLR","year":1982,"shutterCount":"Unknown","condition":"Excellent","opticsCondition":"Crystal Clear","includedLenses":["Canon FD 50mm f/1.8"],"includedAccessories":["Original Strap","Flash","Lens Cap"],"functionality":"Fully Functional","certification":"CLA Certificate 2024"}',
 '/images/lots/canon-camera/1.jpg',
 '["/images/lots/canon-camera/1.jpg","/images/lots/canon-camera/2.jpg"]',
 350.00, 250.00, 'Active', DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(DAY, 7, GETUTCDATE()), @SellerId, 3),

(NEWID(), 'Leica M6 Rangefinder - 1984', @CamerasCatId,
 'Iconic German rangefinder. Mechanical perfection. Chrome finish.',
 '{"manufacturer":"Leica","model":"M6","type":"Rangefinder","year":1984,"shutterCount":"15000","condition":"Excellent","opticsCondition":"Excellent","includedLenses":["Leica Summicron 50mm f/2"],"includedAccessories":["Hood","Strap","Viewfinder"],"functionality":"Fully Functional","certification":"Service History Available"}',
 '/images/lots/canon-camera/2.jpg',
 '["/images/lots/canon-camera/2.jpg","/images/lots/canon-camera/3.jpg"]',
 2200.00, 1900.00, 'Active', DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(DAY, 14, GETUTCDATE()), @SellerId, 5),

(NEWID(), 'Nikon FM2n Professional Manual SLR', @CamerasCatId,
 'Titanium shutter. Reliable. Includes Nikkor 24mm f/2.8 wide angle lens.',
 '{"manufacturer":"Nikon","model":"FM2n","type":"Film SLR","year":1988,"shutterCount":"Moderate","condition":"Good","opticsCondition":"Very Good","includedLenses":["Nikon Nikkor 24mm f/2.8"],"includedAccessories":["Pentaprism","Viewfinder","Manual"],"functionality":"Fully Functional","certification":"CLA 2023"}',
 '/images/lots/canon-camera/3.jpg',
 '["/images/lots/canon-camera/3.jpg","/images/lots/canon-camera/4.jpg"]',
 620.00, 450.00, 'Active', DATEADD(HOUR, -3, GETUTCDATE()), DATEADD(DAY, 9, GETUTCDATE()), @SellerId, 4),

(NEWID(), 'Hasselblad 500C/M - Professional Medium Format', @CamerasCatId,
 'Studio camera. Fashion photography use. Pristine condition.',
 '{"manufacturer":"Hasselblad","model":"500C/M","type":"Medium Format SLR","year":1992,"shutterCount":"10000","condition":"Mint","opticsCondition":"Excellent","includedLenses":["Carl Zeiss Planar 80mm f/2.8"],"includedAccessories":["A12 Film Back","Prism","Manual","Carrying Case"],"functionality":"Fully Functional","certification":"Hasselblad Service Record"}',
 '/images/lots/canon-camera/4.jpg',
 '["/images/lots/canon-camera/4.jpg","/images/lots/canon-camera/5.jpg"]',
 3800.00, 3200.00, 'Active', DATEADD(HOUR, -4, GETUTCDATE()), DATEADD(DAY, 21, GETUTCDATE()), @SellerId, 7),

(NEWID(), 'Pentax K1000 Film SLR - Entry Level Classic', @CamerasCatId,
 'Simple, reliable, perfect for learning film photography. Complete kit.',
 '{"manufacturer":"Pentax","model":"K1000","type":"Film SLR","year":1976,"shutterCount":"Moderate","condition":"Good","opticsCondition":"Good","includedLenses":["Pentax-M 50mm f/2"],"includedAccessories":["Box","Manual","Lens Cap","Flash"],"functionality":"Fully Functional","certification":"Tested"}',
 '/images/lots/canon-camera/5.jpg',
 '["/images/lots/canon-camera/5.jpg"]',
 280.00, 180.00, 'Active', DATEADD(HOUR, -5, GETUTCDATE()), DATEADD(DAY, 5, GETUTCDATE()), @SellerId, 1);

-- Summary: 25 auctions total (5 × 5 categories)
```

---

## 5. Entity Framework Core Models

### C# Domain Model

```csharp
public class Auction
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    // Category & Attributes
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    public JsonDocument Attributes { get; set; } = null!; // NVARCHAR(MAX) as JSON
    
    // Media
    public string? PrimaryImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    
    // Pricing
    public decimal StartingPrice { get; set; }
    public decimal? CurrentHighestBid { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal? FinalPrice { get; set; }
    
    // Status & Timeline
    public AuctionStatus Status { get; set; } = AuctionStatus.Active;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Relationships
    public Guid SellerId { get; set; }
    public virtual User Seller { get; set; } = null!;
    
    public Guid? WinnerId { get; set; }
    public virtual User? Winner { get; set; }
    
    // Metadata
    public int ViewCount { get; set; } = 0;
    public int BidCount { get; set; } = 0;
    public bool IsFeatured { get; set; } = false;
    
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
}

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Icon { get; set; }
    
    public virtual ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}

public enum AuctionStatus
{
    Active,
    Ended,
    Unsold,
    Archived
}
```

---

## 6. API Response Example

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Omega Speedmaster Professional - 1970s (Moon Watch)",
  "description": "Legendary chronograph worn on Apollo 13...",
  "categoryId": "6f0e59b0-c42e-4f9c-8c1d-5e8c2b9a1d7f",
  "category": {
    "id": "6f0e59b0-c42e-4f9c-8c1d-5e8c2b9a1d7f",
    "name": "Luxury Watches",
    "slug": "luxury-watches",
    "icon": "watch"
  },
  "attributes": {
    "brand": "Omega",
    "model": "Speedmaster Professional 145.022",
    "yearOfManufacture": 1973,
    "movement": "Calibre 861",
    "caseSize": 42,
    "material": "Stainless Steel",
    "waterResistance": "50m",
    "condition": "Excellent",
    "originalBox": true,
    "originalPapers": true,
    "certification": "Omega Heritage Certificate"
  },
  "primaryImageUrl": "/images/lots/omega-watch/1.jpg",
  "imageUrls": [
    "/images/lots/omega-watch/1.jpg",
    "/images/lots/omega-watch/2.jpg",
    "/images/lots/omega-watch/3.jpg"
  ],
  "startingPrice": 12500.00,
  "currentHighestBid": 13500.00,
  "reservePrice": 11000.00,
  "status": "Active",
  "startTime": "2026-09-07T23:00:00Z",
  "endTime": "2026-09-14T23:00:00Z",
  "seller": {
    "id": "7a1e3c9d-2f5b-48e2-9c1a-3e8b5f2c9d1a",
    "username": "premium_collector",
    "rating": 4.8
  },
  "viewCount": 247,
  "bidCount": 8,
  "isFeatured": true
}
```

---

## 7. Technical Notes

### Performance Considerations
- **JSON Attributes**: Index on Status, EndTime, CategoryId (covered queries)
- **Image URLs**: Lazy-load ImageUrls if large collections
- **Pagination**: Implement cursor-based for large auction lists

### Security
- **Attribute Validation**: Deserialize JSON with schema validation
- **Authorization**: Only sellers can create/edit their auctions
- **Image Access**: Serve via CDN in production (not direct public folder)

### Future Enhancements
- ElasticSearch for complex filtering (year range, condition, brand)
- Image optimization (WebP, thumbnails, AVIF)
- Admin dashboard for category attribute schema management
- Automated certificate verification (API integrations with NGC, PCGS)

---

**Document Version**: 1.0  
**Last Updated**: 2026-09-08  
**Status**: Production Ready
