using Microsoft.EntityFrameworkCore;
using PersonalPlanner.Data;
using PersonalPlanner.Data.Models;

namespace PersonalPlanner.Services;

public class PlannerService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<Category>> GetUserCategoriesAsync(Guid userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Categories
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.IsPreset)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<PlannerEvent>> GetEventsForWindowAsync(Guid userId, DateOnly startDate, DateOnly endDate)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Events
            .Include(e => e.Category)
            .Where(e => e.UserId == userId && e.EventDate >= startDate && e.EventDate <= endDate)
            .OrderBy(e => e.EventDate)
            .ThenBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<PlannerEvent> AddEventAsync(PlannerEvent newEvent)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Events.Add(newEvent);
        await db.SaveChangesAsync();
        return newEvent;
    }

    public async Task DeleteEventAsync(Guid eventId, Guid userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var item = await db.Events.FirstOrDefaultAsync(e => e.Id == eventId && e.UserId == userId);
        if (item is not null)
        {
            db.Events.Remove(item);
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateCategoryColorAsync(Guid categoryId, Guid userId, string newColorHex)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);
        if (category is not null)
        {
            category.ColorHex = newColorHex;
            await db.SaveChangesAsync();
        }
    }

    public async Task<Category> AddCategoryAsync(Guid userId, string name, string colorHex)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var category = new Category
        {
            UserId = userId,
            Name = name.Trim(),
            ColorHex = colorHex,
            IsPreset = false
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }
}
