using Microsoft.EntityFrameworkCore;
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
        public DbSet<Song> Songs { get; set; }
        public DbSet<SongData> SongData { get; set; }
        public DbSet<MidiStatsEntity> MidiStats { get; set; }
        public DbSet<SongSimplification> SongsSimplifications { get; set; }
        public DbSet<Phrase> Phrases { get; set; }
        public DbSet<SegmentEntity> Segments { get; set; }
        public DbSet<PhraseOccurrenceEntity> PhrasesOccurrences { get; set; }
        public DbSet<Sinphinity.Models.PhrasesLink> PhrasesLinks{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Style>().ToTable("Styles");
            modelBuilder.Entity<Band>().ToTable("Bands");
            modelBuilder.Entity<Song>().ToTable("Songs");
            modelBuilder.Entity<SongData>().ToTable("SongData");
            modelBuilder.Entity<MidiStatsEntity>().ToTable("MidiStats");
            modelBuilder.Entity<Phrase>().ToTable("Phrases");
            modelBuilder.Entity<SegmentEntity>().ToTable("Segments");
            modelBuilder.Entity<PhraseOccurrenceEntity>().ToTable("PhrasesOccurrences");
            modelBuilder.Entity<Sinphinity.Models.PhrasesLink>().ToTable("PhrasesLinks");
            modelBuilder.Entity<SongSimplification>().ToTable("SongsSimplifications");
        }
    }
}
