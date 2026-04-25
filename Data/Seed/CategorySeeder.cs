using Microsoft.EntityFrameworkCore;
using ThirteenthAvenue.Data;
using ThirteenthAvenue.Models;

namespace ThirteenthAvenue.Data.Seed
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync())
                return; // already seeded

            var categories = new List<Category>
            {
                new Category { Name = "Music" },
                new Category { Name = "Arts & Culture" },
                new Category { Name = "Food & Drinks" },
                new Category { Name = "Sports & wellness" },
                new Category { Name = "Concert" },
                new Category { Name = "Adventures" },
                new Category { Name = "Cinema" },
                new Category { Name = "Performance" },
                new Category { Name = "Conferences" },
                new Category { Name = "Technology" },
                new Category { Name = "Fundraising" },
                new Category { Name = "Family" },
                new Category { Name = "Spirituality" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}