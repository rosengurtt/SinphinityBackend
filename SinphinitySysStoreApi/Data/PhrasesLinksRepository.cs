using Microsoft.EntityFrameworkCore;
using Sinphinity.Models;

namespace SinphinitySysStore.Data
{
    public partial class PhrasesRepository
    {

        public async Task GeneratePhrasesLinksForSong(long songId)
        {
            var ocs = await _dbContext.PhrasesOccurrences.Where(x => x.SongId == songId).ToListAsync();
            var songsSimplifications = await _dbContext.SongsSimplifications
                .Where(x => x.SongData.SongId == songId && x.Version == 0).ToListAsync();

            foreach (var oc in ocs)
            {
                var links = _dbContext.PhrasesOccurrences.Where(x => x.SongId == songId && x.StartTick >= oc.StartTick && oc.EndTick + 96 > x.StartTick && x.Id != oc.Id)
                    .ToList();
                if (links.Any())
                {
                    foreach (var link in links)
                    {
                        var pl = new PhrasesLink
                        {
                            PhraseId1 = oc.PhraseId,
                            PhraseId2 = link.PhraseId,
                            ShiftInTicks = link.StartTick - oc.StartTick,
                            PitchShift = link.StartingPitch - oc.StartingPitch,
                            SongId = songId,
                            TicksFromStart= Math.Min(link.StartTick, oc.StartTick),
                            Instrument1 = oc.Instrument,
                            Instrument2 = link.Instrument
                        };
                        var isAlreadyThere = _dbContext.PhrasesLinks
                            .Where(x => x.PhraseId1 == pl.PhraseId1 && x.PhraseId2 == pl.PhraseId2 && x.SongId == pl.SongId && x.PhraseType==pl.PhraseType && 
                            x.ShiftInTicks==pl.ShiftInTicks&& x.PitchShift==pl.PitchShift)
                            .Any();
                        if (!isAlreadyThere)
                        {
                            try
                            {
                                _dbContext.PhrasesLinks.Add(pl);
                                await _dbContext.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
            }
        }
    }
}
