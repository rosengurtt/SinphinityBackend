﻿using Microsoft.EntityFrameworkCore;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Data
{
    public partial class PhrasesRepository
    {
        public async Task<(int, List<Sinphinity.Models.Phrase>)> GetPhrasesAsync(
                   long? styleId,
                   long? bandId,
                   long? songId,
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
                    (songId == null || p.Songs.Where(x => x.Id == songId).Any())); //&&
                    //(numberOfNotes == null || p.NumberOfNotes == numberOfNotes) &&
                    //(contains == null || p.MetricsAsString.Contains(contains) || p.PitchesAsString.Contains(contains)) &&
                    //(durationInTicks == null || p.DurationInTicks == durationInTicks) &&
                    //(range == null || p.Range == range) &&
                    //(isMonotone == null || p.IsMonotone == isMonotone) &&
                    //(step == null || p.Step == step));

                var total = await source.CountAsync();

                var pages = await source
                    .OrderBy(x => x.MetricsAsString)
                    .ThenBy(y=>y.PitchesAsString)
                    .Select(x=>x.AsSynphinityModelsPhrase())
                    .Skip((pageNo) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (total, pages);
            }
            catch (Exception fdsfsa)
            {
                throw fdsfsa;
            }
        }
        public async Task<(int, List<PhraseOccurrence>)> GetOccurrencesOfPhraseAsync(long phraseId, long songId = 0, int pageNo = 0, int pageSize = 20)
        {
            var source = _dbContext.PhrasesOccurrences.Where(po => po.PhraseId == phraseId && (songId == 0 || po.SongId == songId));

            var total = await source.CountAsync();
            var sacamela = await source
                .OrderBy(x => x.StartTick)
                .Skip((pageNo) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pages = await source
                .OrderBy(x => x.StartTick)
                .Select(y => y.AsPhraseOccurrence())
                .Skip((pageNo) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (total, pages);
        }


        public async Task<(int, List<Sinphinity.Models.Phrase>)> GetPhrasesOfSongAndVoiceAsync(long songId, byte voiceId, int pageNo, int pageSize)
        {
            try
            {
                var source = _dbContext.Phrases.Join(
                    _dbContext.PhrasesOccurrences,
                    p => p.Id,
                    po => po.PhraseId,
                    (p, po) => new { phrase = p, occurrence = po })
                    .Where(x => x.occurrence.SongId==songId && x.occurrence.Voice==voiceId);

                var total = await source.CountAsync();

                var pages = await source
                    .OrderBy(x => x.occurrence.StartTick)
                    .Select(x => x.phrase.AsSynphinityModelsPhrase())
                    .Skip((pageNo) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (total, pages);
            }
            catch (Exception fdsfsa)
            {
                throw fdsfsa;
            }
        }
    }
}
