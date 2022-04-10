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
        public DbSet<StyleEntity> Styles { get; set; }
        public DbSet<BandEntity> Bands { get; set; }
        public DbSet<SongEntity> Songs { get; set; }
        public DbSet<SongData> SongsData { get; set; }
        public DbSet<MidiStatsEntity> MidiStats { get; set; }
        public DbSet<SongSimplificationEntity> SongsSimplifications { get; set; }
        public DbSet<PhraseEntity> Phrases { get; set; }
        public DbSet<PhraseOccurrence> PhrasesOccurrences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StyleEntity>().ToTable("Styles");
            modelBuilder.Entity<BandEntity>().ToTable("Bands");
            modelBuilder.Entity<SongEntity>().ToTable("Songs");
            modelBuilder.Entity<SongData>().ToTable("SongsData");
            modelBuilder.Entity<MidiStatsEntity>().ToTable("MidiStats");
            modelBuilder.Entity<PhraseEntity>().ToTable("Phrases");
            modelBuilder.Entity<PhraseOccurrence>().ToTable("PhrasesOccurrences");
            modelBuilder.Entity<SongSimplificationEntity>().ToTable("SongsSimplifications");
        }
    }
}
