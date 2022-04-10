using Microsoft.EntityFrameworkCore;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Data
{
    public class BandsRepository
    {
        private readonly SinphinityDbContext _dbContext;
        public BandsRepository(SinphinityDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task<(int, List<Band>)> GetBandsAsync(
            int pageNo = 0,
            int pageSize = 10,
            string? contains = null,
            long? styleId = null)
        {
            var source = _dbContext.Bands.AsQueryable()
                    .Include(z => z.Style)
                    .Where(x =>
                        (styleId == null || x.Style.Id == styleId) && (contains == null || x.Name.Contains(contains))
                        );
            var total = await source.CountAsync();
            var pageBands = await source.OrderBy(x => x.Name).Skip((pageNo) * pageSize).Take(pageSize).Select(b=>b.AsBand()).ToListAsync();
            return (total, pageBands);
        }


        public async Task<Band?> GetBandByIdAsync(long bandId)
        {
            return (await _dbContext.Bands.FindAsync(bandId)).AsBand();
        }
        public async Task<Band?> GetBandByNameAsync(string name)
        {
            return await _dbContext.Bands.Where(s => s.Name.ToLower() == name.ToLower()).Select(b=>b.AsBand()).FirstOrDefaultAsync();
        }
        public async Task<Band> AddBandAsync(Band band)
        {
            _dbContext.Bands.Add(new BandEntity(band));
            await _dbContext.SaveChangesAsync();
            return band;
        }

        public async Task<Band> UpdateBandAsync(Band band)
        {
            var bands = await _dbContext.Bands.FindAsync(band.Id);
            if (bands == null)
                throw new ApplicationException($"No band with id {band.Id}");

            _dbContext.Entry(await _dbContext.Bands.FirstOrDefaultAsync(x => x.Id == band.Id))
                    .CurrentValues.SetValues(band);
            await _dbContext.SaveChangesAsync();
            return band;
        }

        public async Task DeleteBandAsync(long bandId)
        {
            var band = await _dbContext.Bands.FindAsync(bandId);
            if (band == null)
                throw new ApplicationException($"No band with id {bandId}");

            _dbContext.Bands.Remove(band);
            await _dbContext.SaveChangesAsync();
        }
    }
}
