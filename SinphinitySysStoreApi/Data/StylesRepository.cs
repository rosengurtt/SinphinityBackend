using Microsoft.EntityFrameworkCore;
using SinphinitySysStore.Models;
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

        public async Task<(int, List<Sinphinity.Models.Style>)> GetStylesAsync(
            int pageNo = 0,
            int pageSize = 10,
            string? contains = null)
        {
            var source = _dbContext.Styles.AsQueryable().Where(x => contains == null || x.Name.Contains(contains));
            var total = await source.CountAsync();
            var pagedStyles = await source.OrderBy(x => x.Name).Skip((pageNo) * pageSize).Take(pageSize).Select(y=>y.AsStyle()).ToListAsync();
            return (total, pagedStyles);
        }


 
        public async Task<Sinphinity.Models.Style> GetStyleByIdAsync(long styleId)
        {
            return (await _dbContext.Styles.FindAsync(styleId)).AsStyle();
        }
        public async Task<Sinphinity.Models.Style> GetStyleByNameAsync(string name)
        {
            return await _dbContext.Styles.Where(s => s.Name.ToLower() == name.ToLower()).Select(x=>x.AsStyle()).FirstOrDefaultAsync();
        }
        public async Task<Sinphinity.Models.Style> AddStyleAsync(Sinphinity.Models.Style style)
        {
            _dbContext.Styles.Add(new Style(style));
            await _dbContext.SaveChangesAsync();
            return style;
        }

        public async Task<Sinphinity.Models.Style> UpdateStyleAsync(Sinphinity.Models.Style style)
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
