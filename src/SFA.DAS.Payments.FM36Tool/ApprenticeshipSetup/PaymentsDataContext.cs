using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.Payments.FM36Tool.ApprenticeshipSetup
{
    public class PaymentsDataContext : DbContext
    {
        private readonly string _connectionString;

        public virtual DbSet<LevyAccountModel> LevyAccount { get; set; }
        public virtual DbSet<ApprenticeshipModel> Apprenticeship { get; set; }
        public virtual DbSet<ApprenticeshipPriceEpisodeModel> ApprenticeshipPriceEpisode { get; set; }

        public PaymentsDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public PaymentsDataContext(DbContextOptions<PaymentsDataContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("Payments2");

            modelBuilder.ApplyConfiguration(new LevyAccountModelConfiguration());
            modelBuilder.ApplyConfiguration(new ApprenticeshipModelConfiguration());
            modelBuilder.ApplyConfiguration(new ApprenticeshipPriceEpisodeModelConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_connectionString != null)
                optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}