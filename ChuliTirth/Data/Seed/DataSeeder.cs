using ChuliTirth.Models.Config;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ChuliTirth.Data.Seed;

// All content below is clearly-marked SAMPLE data for development/demo purposes.
// It is not verified Chuli Tirth information — replace via the Admin panel before going live.
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var seedAdmin = services.GetRequiredService<IOptions<SeedAdminSettings>>().Value;
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        if (!env.IsDevelopment() && seedAdmin.Email.Equals("admin@chulitirth.local", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "SECURITY: Running outside Development with the default SeedAdmin email " +
                "(admin@chulitirth.local). If this is the first run against a fresh database, " +
                "the seeded admin account uses the default README credentials — rotate SeedAdmin " +
                "Email/Password in configuration immediately.");
        }

        await SeedUsersAsync(db, seedAdmin, env.IsDevelopment());
        await SeedRoomTypesAndRoomsAsync(db);
        await SeedFacilitiesAsync(db);
        await SeedBhojanshalaAsync(db);
        await SeedJainTithisAsync(db);
        await SeedJainQuotesAsync(db);
        await SeedAnnouncementsAsync(db);
        await SeedGalleryAsync(db);
        await SeedBookingRulesAsync(db);
        await SeedSiteSettingsAsync(db);
    }

    private static async Task SeedUsersAsync(ApplicationDbContext db, SeedAdminSettings seedAdmin, bool isDevelopment)
    {
        if (await db.Users.AnyAsync()) return;

        var hasher = new PasswordHasher<User>();
        var admin = new User { FullName = seedAdmin.FullName, Email = seedAdmin.Email.ToLowerInvariant(), Mobile = "9999900000", Role = UserRole.Admin };
        admin.PasswordHash = hasher.HashPassword(admin, seedAdmin.Password);
        db.Users.Add(admin);

        // The Manager/Guest demo accounts (with publicly-documented passwords) only make sense
        // for local dev/demo — a production Dharamshala has no use for a fake "Test Pilgrim".
        if (isDevelopment)
        {
            var manager = new User { FullName = "Sample Manager", Email = "manager@chulitirth.local", Mobile = "9999900001", Role = UserRole.Manager };
            manager.PasswordHash = hasher.HashPassword(manager, "Manager@12345");

            var guest = new User { FullName = "Test Pilgrim", Email = "guest@chulitirth.local", Mobile = "9999900002", Role = UserRole.Guest };
            guest.PasswordHash = hasher.HashPassword(guest, "Guest@12345");

            db.Users.AddRange(manager, guest);
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedRoomTypesAndRoomsAsync(ApplicationDbContext db)
    {
        if (await db.RoomTypes.AnyAsync()) return;

        var amenities = new[]
        {
            new Amenity { Name = "Attached Bathroom", NameGujarati = "જોડાયેલ બાથરૂમ", NameHindi = "संलग्न बाथरूम", Icon = "bi-droplet" },
            new Amenity { Name = "Hot Water", NameGujarati = "ગરમ પાણી", NameHindi = "गर्म पानी", Icon = "bi-thermometer-sun" },
            new Amenity { Name = "Parking", NameGujarati = "પાર્કિંગ", NameHindi = "पार्किंग", Icon = "bi-p-square" },
            new Amenity { Name = "Wi-Fi", NameGujarati = "વાઇ-ફાઇ", NameHindi = "वाई-फाई", Icon = "bi-wifi" },
            new Amenity { Name = "Air Conditioning", NameGujarati = "એર કન્ડિશનર", NameHindi = "एयर कंडीशनर", Icon = "bi-snow" },
            new Amenity { Name = "Bed with Linen", NameGujarati = "પથારી", NameHindi = "बिस्तर", Icon = "bi-house-heart" },
            new Amenity { Name = "Drinking Water", NameGujarati = "પીવાનું પાણી", NameHindi = "पीने का पानी", Icon = "bi-cup-straw" },
        };
        db.Amenities.AddRange(amenities);
        await db.SaveChangesAsync();

        // Every physical room has AC fitted; "AC Room" vs "Non-AC Room" is the guest's choice of
        // whether AC service is included for their stay, not a separate category of room — both
        // rate tiers draw from the same shared pool of 35 rooms created below (see Room.cs).
        var roomTypes = new List<RoomType>
        {
            new()
            {
                Name = "AC Room", NameGujarati = "એસી રૂમ", NameHindi = "एसी रूम",
                Description = "Comfortable room with attached bathroom and air conditioning enabled for your stay.",
                DescriptionGujarati = "જોડાયેલ બાથરૂમ અને તમારા રોકાણ માટે સક્રિય એર-કન્ડિશનર સાથે આરામદાયક રૂમ.",
                DescriptionHindi = "संलग्न बाथरूम और आपके ठहरने के लिए सक्रिय एयर कंडीशनर के साथ आरामदायक कमरा।",
                Capacity = 3, BedCount = 2, Price = 1000, IsAC = true, HasAttachedBathroom = true, HasHotWater = true, HasWifi = true, HasParking = true, DisplayOrder = 1
            },
            new()
            {
                Name = "Non-AC Room", NameGujarati = "નોન-એસી રૂમ", NameHindi = "नॉन-एसी रूम",
                Description = "The same comfortable room with attached bathroom, booked without air conditioning service.",
                DescriptionGujarati = "જોડાયેલ બાથરૂમ સાથે એ જ આરામદાયક રૂમ, એર-કન્ડિશનર સેવા વગર બુક કરેલ.",
                DescriptionHindi = "संलग्न बाथरूम के साथ वही आरामदायक कमरा, एयर कंडीशनर सेवा के बिना बुक किया गया।",
                Capacity = 3, BedCount = 2, Price = 500, IsAC = false, HasAttachedBathroom = true, HasHotWater = true, HasWifi = true, HasParking = true, DisplayOrder = 2
            }
        };
        db.RoomTypes.AddRange(roomTypes);
        await db.SaveChangesAsync();

        var roomImageByName = new Dictionary<string, string>
        {
            ["AC Room"] = "/images/rooms/room-ac.jpg",
            ["Non-AC Room"] = "/images/rooms/room-nonac.jpg",
        };
        foreach (var rt in roomTypes)
        {
            var roomImg = roomImageByName.GetValueOrDefault(rt.Name, "/images/rooms/placeholder-room.svg");
            db.RoomImages.Add(new RoomImage { RoomTypeId = rt.Id, ImageUrl = roomImg, AltText = rt.Name, DisplayOrder = 1 });

            var amenityIds = amenities.Select(a => a.Id).ToList();
            if (!rt.IsAC) amenityIds = amenityIds.Where(id => amenities.First(a => a.Id == id).Name != "Air Conditioning").ToList();
            foreach (var amenityId in amenityIds)
            {
                db.RoomAmenities.Add(new RoomAmenity { RoomTypeId = rt.Id, AmenityId = amenityId });
            }
        }

        // Real room/floor mapping for the 35 physical rooms. One shared pool — not owned by
        // either RoomType above (see Room.cs / RoomAvailabilityService for why).
        var floorRanges = new[]
        {
            ("Ground Floor", 1, 8),
            ("First Floor", 9, 16),
            ("Second Floor", 201, 219),
        };
        foreach (var (floor, start, end) in floorRanges)
        {
            for (var n = start; n <= end; n++)
            {
                db.Rooms.Add(new Room { RoomNumber = n.ToString(), Floor = floor, Status = RoomStatus.Available });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedFacilitiesAsync(ApplicationDbContext db)
    {
        if (await db.Facilities.AnyAsync()) return;

        var facilities = new[]
        {
            ("Parking", "પાર્કિંગ", "पार्किंग", "bi-p-square"),
            ("Drinking Water", "પીવાનું પાણી", "पीने का पानी", "bi-cup-straw"),
            ("Hot Water", "ગરમ પાણી", "गर्म पानी", "bi-thermometer-sun"),
            ("Jain Bhojanshala", "જૈન ભોજનશાળા", "जैन भोजनशाला", "bi-egg-fried"),
            ("Prayer Area", "પ્રાર્થના સ્થળ", "प्रार्थना स्थल", "bi-flower1"),
            ("Temple", "મંદિર", "मंदिर", "bi-bank2"),
            ("Garden", "બગીચો", "बगीचा", "bi-tree"),
            ("Lift", "લિફ્ટ", "लिफ्ट", "bi-arrow-down-up"),
            ("Wheelchair Accessibility", "વ્હીલચેર સુવિધા", "व्हीलचेयर सुविधा", "bi-universal-access"),
            ("Security", "સુરક્ષા", "सुरक्षा", "bi-shield-check"),
            ("CCTV", "સીસીટીવી", "सीसीटीवी", "bi-camera-video"),
            ("Wi-Fi", "વાઇ-ફાઇ", "वाई-फाई", "bi-wifi"),
        };

        var order = 1;
        foreach (var (name, gu, hi, icon) in facilities)
        {
            db.Facilities.Add(new Facility { Name = name, NameGujarati = gu, NameHindi = hi, Icon = icon, DisplayOrder = order++, Description = $"Sample: {name} available for all visitors." });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedBhojanshalaAsync(ApplicationDbContext db)
    {
        if (await db.BhojanshalaTimings.AnyAsync()) return;

        var timings = new[]
        {
            ("Navkarsi", "નવકારશી", "नवकारसी", new TimeOnly(7, 0), new TimeOnly(8, 30)),
            ("Lunch", "બપોરનું ભોજન", "दोपहर का भोजन", new TimeOnly(11, 30), new TimeOnly(13, 30)),
            ("Chovihar", "ચોવિહાર", "चोविहार", new TimeOnly(17, 30), new TimeOnly(18, 30)),
            ("Dinner", "રાત્રિભોજન", "रात्रिभोज", new TimeOnly(18, 30), new TimeOnly(20, 0)),
        };

        var order = 1;
        foreach (var (name, gu, hi, start, end) in timings)
        {
            db.BhojanshalaTimings.Add(new BhojanshalaTiming
            {
                MealName = name, MealNameGujarati = gu, MealNameHindi = hi,
                StartTime = start, EndTime = end, DisplayOrder = order++,
                Note = "Sample timing — may vary by Tithi, season or management decision."
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedJainTithisAsync(ApplicationDbContext db)
    {
        if (await db.JainTithis.AnyAsync()) return;

        var tithiNames = new[]
        {
            ("Ekam", "એકમ", "एकम"), ("Beej", "બીજ", "बीज"), ("Trij", "ત્રીજ", "तीज"), ("Choth", "ચોથ", "चौथ"),
            ("Pancham", "પાંચમ", "पंचमी"), ("Chhath", "છઠ", "छठ"), ("Saatam", "સાતમ", "सप्तमी"), ("Aatham", "આઠમ", "अष्टमी"),
            ("Nom", "નોમ", "नवमी"), ("Dasham", "દશમ", "दशमी")
        };

        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5));
        var random = new Random(42);
        for (var i = 0; i < 45; i++)
        {
            var date = today.AddDays(i - 5);
            var (en, gu, hi) = tithiNames[i % tithiNames.Length];
            var paksha = (i / 15) % 2 == 0 ? ("Sud", "સુદ", "शुक्ल") : ("Vad", "વદ", "कृष्ण");

            db.JainTithis.Add(new JainTithi
            {
                GregorianDate = date,
                TithiName = en, TithiNameGujarati = gu, TithiNameHindi = hi,
                Paksha = paksha.Item1, PakshaGujarati = paksha.Item2, PakshaHindi = paksha.Item3,
                Nakshatra = "Sample Nakshatra",
                SpecialOccasion = i == 20 ? "Sample: Paryushan Mahaparva begins" : null,
                SpecialOccasionGujarati = i == 20 ? "નમૂનો: પર્યુષણ મહાપર્વ પ્રારંભ" : null,
                SpecialOccasionHindi = i == 20 ? "नमूना: पर्युषण महापर्व प्रारंभ" : null
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedJainQuotesAsync(ApplicationDbContext db)
    {
        if (await db.JainQuotes.AnyAsync()) return;

        var quotes = new[]
        {
            ("Ahimsa Parmo Dharma — Non-violence is the supreme religion.", "અહિંસા પરમો ધર્મ", "अहिंसा परमो धर्म"),
            ("Live and let live.", "જીવો અને જીવવા દો", "जियो और जीने दो"),
            ("Parasparopagraho Jivanam — All life is bound together by mutual support and interdependence.", "પરસ્પરોપગ્રહો જીવાનામ", "परस्परोपग्रहो जीवानाम्"),
            ("Truth is the essence of all conduct.", "સત્ય એ સર્વ આચરણનો સાર છે", "सत्य ही सभी आचरण का सार है"),
            ("Conquer anger with forgiveness.", "ક્ષમાથી ક્રોધને જીતો", "क्षमा से क्रोध को जीतें"),
        };

        var order = 1;
        foreach (var (en, gu, hi) in quotes)
        {
            db.JainQuotes.Add(new JainQuote { TextEnglish = en, TextGujarati = gu, TextHindi = hi, DisplayOrder = order++ });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedAnnouncementsAsync(ApplicationDbContext db)
    {
        if (await db.Announcements.AnyAsync()) return;

        db.Announcements.Add(new Announcement
        {
            Title = "Welcome to Chuli Tirth Dharamshala (Sample Announcement)",
            TitleGujarati = "ચુલી તીર્થ ધર્મશાળામાં આપનું સ્વાગત છે (નમૂનો)",
            TitleHindi = "चुली तीर्थ धर्मशाला में आपका स्वागत है (नमूना)",
            Body = "Online room booking is now available. Please review our booking rules before confirming your stay. This is sample content — replace via Admin > Announcements.",
            BodyGujarati = "ઓનલાઇન રૂમ બુકિંગ હવે ઉપલબ્ધ છે. કૃપા કરીને તમારો રોકાણ કન્ફર્મ કરતા પહેલા અમારા બુકિંગ નિયમો વાંચો.",
            BodyHindi = "ऑनलाइन रूम बुकिंग अब उपलब्ध है। कृपया अपना प्रवास कन्फर्म करने से पहले हमारे बुकिंग नियम पढ़ें।",
            Priority = AnnouncementPriority.Normal
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedGalleryAsync(ApplicationDbContext db)
    {
        if (await db.GalleryCategories.AnyAsync()) return;

        var categoryImages = new (string Name, string NameGujarati, string NameHindi, string ImageUrl)[]
        {
            ("Tirth", "તીર્થ", "तीर्थ", "/images/hero/hero-temple.jpg"),
            ("Temple", "મંદિર", "मंदिर", "/images/gallery/gallery-domes.jpg"),
            ("Dharamshala", "ધર્મશાળા", "धर्मशाला", "/images/gallery/gallery-complex.jpg"),
            ("Rooms", "રૂમ", "कमरे", "/images/rooms/room-ac.jpg"),
            ("Bhojanshala", "ભોજનશાળા", "भोजनशाला", "/images/gallery/gallery-bhojanshala.jpg"),
            ("Facilities", "સુવિધાઓ", "सुविधाएं", "/images/gallery/gallery-carving.jpg"),
            ("Events", "કાર્યક્રમો", "कार्यक्रम", "/images/gallery/gallery-hilltop.jpg"),
            ("Surroundings", "આસપાસનો વિસ્તાર", "आसपास का क्षेत्र", "/images/gallery/gallery-hilltop.jpg"),
        };
        var order = 1;
        foreach (var (name, nameGu, nameHi, imageUrl) in categoryImages)
        {
            var category = new GalleryCategory { Name = name, NameGujarati = nameGu, NameHindi = nameHi, DisplayOrder = order++ };
            db.GalleryCategories.Add(category);
            await db.SaveChangesAsync();

            db.GalleryImages.Add(new GalleryImage
            {
                GalleryCategoryId = category.Id,
                ImageUrl = imageUrl,
                Caption = name,
                DisplayOrder = 1
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedBookingRulesAsync(ApplicationDbContext db)
    {
        if (await db.BookingRules.AnyAsync()) return;

        var rules = new (string title, string desc, string category)[]
        {
            ("Check-in / Check-out", "Sample: Check-in from 12:00 PM, check-out by 10:00 AM. Configurable by admin.", "Timing"),
            ("Identification", "Sample: A valid photo ID is required at check-in for all adult guests.", "Eligibility"),
            ("Jain Dietary Premises", "Sample: Only pure vegetarian (Jain) food is permitted on the premises. Onion and garlic are not allowed in the Bhojanshala.", "Conduct"),
            ("Cancellation Policy", "Cancel 7+ days before check-in for a full refund, 48 hours to 7 days before for a 50% refund, or within 48 hours of check-in for no refund. Refunds to a paid gateway booking are issued automatically when you cancel.", "Cancellation"),
            ("Quiet Hours", "Sample: Please maintain silence between 10:00 PM and 6:00 AM out of respect for fellow pilgrims.", "Conduct"),
            ("Maximum Stay", "Sample: Stays beyond 3 nights require prior approval from the Trust office.", "Occupancy"),
        };

        var order = 1;
        foreach (var (title, desc, category) in rules)
        {
            db.BookingRules.Add(new BookingRule { Title = title, Description = desc, Category = category, DisplayOrder = order++ });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedSiteSettingsAsync(ApplicationDbContext db)
    {
        if (await db.SiteSettings.AnyAsync()) return;

        var settings = new Dictionary<string, string>
        {
            ["DharamshalaName"] = "Chuli Tirth Dharamshala",
            ["Address"] = "Sample Address — Chuli Tirth, Gujarat, India",
            ["Phone"] = "+91 99999 00000",
            ["Email"] = "info@chulitirth.local",
            ["WhatsApp"] = "+91 99999 00000",
            ["GoogleMapsUrl"] = "https://maps.google.com/?q=Chuli+Jain+Tirth",
            ["CheckInTime"] = "12:00",
            ["CheckOutTime"] = "10:00",
            ["HomepageHeroText"] = "Sample: Experience peace and devotion at Chuli Tirth",
            ["FooterText"] = "Sample: Serving pilgrims with Jain hospitality and Sadharmik Bhakti.",
        };

        foreach (var (key, value) in settings)
        {
            db.SiteSettings.Add(new SiteSetting { Key = key, Value = value, Category = "General", UpdatedAtUtc = DateTime.UtcNow });
        }
        await db.SaveChangesAsync();
    }
}
