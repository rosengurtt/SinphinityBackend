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
        public DbSet<PhrasePitchesEntity> PhrasesPitches { get; set; }
        public DbSet<Phrase> Phrases { get; set; }
        public DbSet<BasicMetricsPhraseMetrics> BasicMetricsPhrasesMetrics { get; set; }
        public DbSet<PhrasesOccurrence> PhrasesOccurrences { get; set; }
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
            modelBuilder.Entity<PhrasePitchesEntity>().ToTable("PhrasesPitches");
            modelBuilder.Entity<Phrase>().ToTable("Phrases");
            modelBuilder.Entity<BasicMetricsPhraseMetrics>().ToTable("BasicMetricsPhrasesMetrics");
            modelBuilder.Entity<PhrasesOccurrence>().ToTable("PhrasesOccurrences");
            modelBuilder.Entity<PhraseSong>().ToTable("PhrasesSongs");
            modelBuilder.Entity<SongSimplificationEntity>().ToTable("SongsSimplifications");
        }
    }
}
