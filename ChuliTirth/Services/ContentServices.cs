using ChuliTirth.Data;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Services;

public class JainCalendarService : IJainCalendarService
{
    private readonly ApplicationDbContext _db;
    public JainCalendarService(ApplicationDbContext db) => _db = db;

    public Task<JainTithi?> GetTithiForDateAsync(DateOnly date) =>
        _db.JainTithis.AsNoTracking().FirstOrDefaultAsync(t => t.GregorianDate == date);

    public Task<List<JainTithi>> GetUpcomingAsync(DateOnly fromDate, int days) =>
        _db.JainTithis.AsNoTracking()
            .Where(t => t.GregorianDate >= fromDate && t.GregorianDate < fromDate.AddDays(days))
            .OrderBy(t => t.GregorianDate)
            .ToListAsync();
}

public class JainQuoteService : IJainQuoteService
{
    private readonly ApplicationDbContext _db;
    public JainQuoteService(ApplicationDbContext db) => _db = db;

    public Task<List<JainQuote>> GetActiveQuotesAsync() =>
        _db.JainQuotes.AsNoTracking().Where(q => q.IsActive).OrderBy(q => q.DisplayOrder).ToListAsync();

    public Task<List<JainQuote>> GetAllAsync() =>
        _db.JainQuotes.OrderBy(q => q.DisplayOrder).ToListAsync();

    public async Task<JainQuote> CreateAsync(JainQuote quote)
    {
        _db.JainQuotes.Add(quote);
        await _db.SaveChangesAsync();
        return quote;
    }

    public async Task UpdateAsync(JainQuote quote)
    {
        _db.JainQuotes.Update(quote);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var quote = await _db.JainQuotes.FindAsync(id);
        if (quote is null) return;
        _db.JainQuotes.Remove(quote);
        await _db.SaveChangesAsync();
    }
}

public class GalleryService : IGalleryService
{
    private readonly ApplicationDbContext _db;
    public GalleryService(ApplicationDbContext db) => _db = db;

    public Task<List<GalleryCategory>> GetCategoriesWithImagesAsync() =>
        _db.GalleryCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .Include(c => c.Images.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

    public async Task<GalleryImage> AddImageAsync(GalleryImage image)
    {
        _db.GalleryImages.Add(image);
        await _db.SaveChangesAsync();
        return image;
    }

    public async Task DeleteImageAsync(int id)
    {
        var image = await _db.GalleryImages.FindAsync(id);
        if (image is null) return;
        _db.GalleryImages.Remove(image);
        await _db.SaveChangesAsync();
    }
}

public class FacilityService : IFacilityService
{
    private readonly ApplicationDbContext _db;
    public FacilityService(ApplicationDbContext db) => _db = db;

    public Task<List<Facility>> GetActiveFacilitiesAsync() =>
        _db.Facilities.AsNoTracking().Where(f => f.IsActive).OrderBy(f => f.DisplayOrder).ToListAsync();

    public Task<List<Facility>> GetAllAsync() =>
        _db.Facilities.OrderBy(f => f.DisplayOrder).ToListAsync();

    public async Task<Facility> CreateAsync(Facility facility)
    {
        _db.Facilities.Add(facility);
        await _db.SaveChangesAsync();
        return facility;
    }

    public async Task UpdateAsync(Facility facility)
    {
        _db.Facilities.Update(facility);
        await _db.SaveChangesAsync();
    }
}

public class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _db;
    public AnnouncementService(ApplicationDbContext db) => _db = db;

    public Task<List<Announcement>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;
        return _db.Announcements.AsNoTracking()
            .Where(a => a.IsActive
                && (a.StartAtUtc == null || a.StartAtUtc <= now)
                && (a.EndAtUtc == null || a.EndAtUtc >= now))
            .OrderByDescending(a => a.Priority)
            .ThenByDescending(a => a.CreatedAtUtc)
            .ToListAsync();
    }

    public Task<List<Announcement>> GetAllAsync() =>
        _db.Announcements.OrderByDescending(a => a.CreatedAtUtc).ToListAsync();

    public async Task<Announcement> CreateAsync(Announcement announcement)
    {
        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync();
        return announcement;
    }

    public async Task UpdateAsync(Announcement announcement)
    {
        _db.Announcements.Update(announcement);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var a = await _db.Announcements.FindAsync(id);
        if (a is null) return;
        _db.Announcements.Remove(a);
        await _db.SaveChangesAsync();
    }
}

public class SiteSettingsService : ISiteSettingsService
{
    private readonly ApplicationDbContext _db;
    public SiteSettingsService(ApplicationDbContext db) => _db = db;

    public async Task<string?> GetAsync(string key)
    {
        var setting = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public Task<Dictionary<string, string?>> GetAllAsync() =>
        _db.SiteSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);

    public async Task SetAsync(string key, string? value, string category = "General")
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value, Category = category, UpdatedAtUtc = DateTime.UtcNow });
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAtUtc = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }
}
