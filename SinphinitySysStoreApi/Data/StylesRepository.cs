using Microsoft.EntityFrameworkCore;
using Sinphinity.Models;
using System.Linq.Dynamic.Core;

namespace SinphinitySysStore.Data
{
    public class StylesRepository
    {
        private readonly SinphinityDbContext _dbContext;
        public StylesRepository(SinphinityDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task<(int, List<Style>)> GetStylesAsync(
            int pageNo = 0,
            int pageSize = 10,
            string? contains = null)
        {
            var source = _dbContext.Styles.AsQueryable().Where(x => contains == null || x.Name.Contains(contains));
            var total = await source.CountAsync();
            var pagedStyles = await source.OrderBy(x => x.Name).Skip((pageNo) * pageSize).Take(pageSize).ToListAsync();
            return (total, pagedStyles);
        }


 
        public async Task<Style> GetStyleByIdAsync(long styleId)
        {
            return await _dbContext.Styles.FindAsync(styleId);
        }
        public async Task<Style> GetStyleByNameAsync(string name)
        {
            return await _dbContext.Styles.Where(s => s.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
        }
        public async Task<Style> AddStyleAsync(Style style)
        {
            _dbContext.Styles.Add(style);
            await _dbContext.SaveChangesAsync();
            return style;
        }

        public async Task<Style> UpdateStyleAsync(Style style)
        {
            var styles = await _dbContext.Styles.FindAsync(style.Id);
            if (styles == null)
                throw new ApplicationException($"No style with id {style.Id}");

            _dbContext.Entry(await _dbContext.Styles.FirstOrDefaultAsync(x => x.Id == style.Id))
                    .CurrentValues.SetValues(style);
            await _dbContext.SaveChangesAsync();
            return style;
        }

        public async Task DeleteStyleAsync(long styleId)
        {
            var style = await _dbContext.Styles.FindAsync(styleId);
            if (style == null)
                throw new ApplicationException($"No style with id {styleId}");

            _dbContext.Styles.Remove(style);
            await _dbContext.SaveChangesAsync();
        }
    }
}
