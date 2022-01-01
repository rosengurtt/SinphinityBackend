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
        public DbSet<Pattern> Patterns { get; set; }
        public DbSet<PatternSong> PatternsSongs { get; set; }
        public DbSet<BasicPattern> BasicPatterns { get; set; }
        public DbSet<BasicPatternPattern> BasicPatternsPatterns { get; set; }
        public DbSet<PatternOccurrence> PatternOccurrences { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Style>().ToTable("Styles");
            modelBuilder.Entity<Band>().ToTable("Bands");
            modelBuilder.Entity<SongEntity>().ToTable("Songs");
            modelBuilder.Entity<SongData>().ToTable("SongsData");
            modelBuilder.Entity<MidiStatsEntity>().ToTable("MidiStats");
            modelBuilder.Entity<Pattern>().ToTable("Patterns");
            modelBuilder.Entity<PatternSong>().ToTable("PatternsSongs");
            modelBuilder.Entity<BasicPattern>().ToTable("BasicPatterns");
            modelBuilder.Entity<BasicPatternPattern>().ToTable("BasicPatternsPatterns");
            modelBuilder.Entity<PatternOccurrence>().ToTable("PatternOccurrences");
            modelBuilder.Entity<SongSimplificationEntity>().ToTable("SongsSimplifications");
        }
    }
}
