using Microsoft.EntityFrameworkCore;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Data
{
    public class SinphinityDbContext : DbContext
    {
        public SinphinityDbContext(DbContextOptions<SinphinityDbContext> options)
            : base(options)
        { }
        public DbSet<Style> Styles { get; set; }
        public DbSet<Band> Bands { get; set; }
        public DbSet<SongEntity> Songs { get; set; }
        public DbSet<SongData> SongsData { get; set; }
        public DbSet<MidiStatsEntity> MidiStats { get; set; }
        public DbSet<SongSimplificationEntity> SongsSimplifications { get; set; }
        public DbSet<BasicMetricsEntity> BasicMetrics { get; set; }
        public DbSet<PhraseMetricsEntity> PhrasesMetrics { get; set; }
        public DbSet<EmbellishedPhraseMetricsEntity> EmbelishedPhrasesMetrics { get; set; }
        public DbSet<PhrasePitchesEntity> PhrasesPitches { get; set; }
        public DbSet<EmbellishedPhrasePitchesEntity> EmbellishedPhrasesPitches { get; set; }
        public DbSet<Phrase> Phrases { get; set; }
        public DbSet<EmbellishedPhrase> EmbelishedPhrases { get; set; }
        public DbSet<PhraseOccurrence> PhrasesOccurrences { get; set; }
        public DbSet<PhraseSong> PhrasesSongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Style>().ToTable("Styles");
            modelBuilder.Entity<Band>().ToTable("Bands");
            modelBuilder.Entity<SongEntity>().ToTable("Songs");
            modelBuilder.Entity<SongData>().ToTable("SongsData");
            modelBuilder.Entity<MidiStatsEntity>().ToTable("MidiStats");
            modelBuilder.Entity<BasicMetricsEntity>().ToTable("BasicMetrics");
            modelBuilder.Entity<PhraseMetricsEntity>().ToTable("PhrasesMetrics");
            modelBuilder.Entity<EmbellishedPhraseMetricsEntity>().ToTable("EmbellishedPhrasesMetrics");
            modelBuilder.Entity<PhrasePitchesEntity>().ToTable("PhrasesPitches");
            modelBuilder.Entity<EmbellishedPhrasePitchesEntity>().ToTable("EmbellishedPhrasesPitches");
            modelBuilder.Entity<Phrase>().ToTable("Phrases");
            modelBuilder.Entity<EmbellishedPhrase>().ToTable("EmbellishedPhrases");
            modelBuilder.Entity<PhraseOccurrence>().ToTable("PhrasesOccurrences");
            modelBuilder.Entity<PhraseSong>().ToTable("PhrasesSongs");
            modelBuilder.Entity<SongSimplificationEntity>().ToTable("SongsSimplifications");
        }
    }
}
