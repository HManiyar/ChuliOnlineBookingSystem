using ChuliTirth.Models.Entities;

namespace ChuliTirth.Interfaces;

public interface IJainCalendarService
{
    Task<JainTithi?> GetTithiForDateAsync(DateOnly date);
    Task<List<JainTithi>> GetUpcomingAsync(DateOnly fromDate, int days);
}

public interface IJainQuoteService
{
    Task<List<JainQuote>> GetActiveQuotesAsync();
    Task<List<JainQuote>> GetAllAsync();
    Task<JainQuote> CreateAsync(JainQuote quote);
    Task UpdateAsync(JainQuote quote);
    Task DeleteAsync(int id);
}

public interface IGalleryService
{
    Task<List<GalleryCategory>> GetCategoriesWithImagesAsync();
    Task<GalleryImage> AddImageAsync(GalleryImage image);
    Task DeleteImageAsync(int id);
}

public interface IFacilityService
{
    Task<List<Facility>> GetActiveFacilitiesAsync();
    Task<List<Facility>> GetAllAsync();
    Task<Facility> CreateAsync(Facility facility);
    Task UpdateAsync(Facility facility);
}

public interface IAnnouncementService
{
    Task<List<Announcement>> GetActiveAsync();
    Task<List<Announcement>> GetAllAsync();
    Task<Announcement> CreateAsync(Announcement announcement);
    Task UpdateAsync(Announcement announcement);
    Task DeleteAsync(int id);
}

public interface ISiteSettingsService
{
    Task<string?> GetAsync(string key);
    Task<Dictionary<string, string?>> GetAllAsync();
    Task SetAsync(string key, string? value, string category = "General");
}
