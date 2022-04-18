using Microsoft.EntityFrameworkCore;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Data
{
    public partial class PhrasesRepository
    {
        public async Task<(int, List<Models.Phrase>)> GetPhrases(
                   long? styleId,
                   long? bandId,
                   long? songId,
                   PhraseTypeEnum? type,
                   string? contains,
                   int? numberOfNotes,
                   long? durationInTicks,
                   int? range,
                   bool? isMonotone,
                   int? step,
                   int pageNo,
                   int pageSize)
        {
            try
            {
                var source = _dbContext.Phrases
                    .Where(p =>
                    (styleId == null || p.Styles.Where(x => x.Id == styleId).Any()) &&
                    (bandId == null || p.Bands.Where(x => x.Id == bandId).Any()) &&
                    (songId == null || p.Songs.Where(x => x.Id == songId).Any()) &&
                    (type == null || p.PhraseType == type) &&
                    (numberOfNotes == null || p.NumberOfNotes == numberOfNotes) &&
                    (contains == null || p.AsString.Contains(contains)) &&
                    (durationInTicks == null || p.DurationInTicks == durationInTicks) &&
                    (range == null || p.Range == range) &&
                    (isMonotone == null || p.IsMonotone == isMonotone) &&
                    (step == null || p.Step == step));

                var total = await source.CountAsync();

                var pages = await source
                    .OrderBy(x => x.AsString)
                    .Skip((pageNo) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (total, pages);
            }
            catch(Exception fdsfsa)
            {
                throw fdsfsa;
            }
        }
    }
}
