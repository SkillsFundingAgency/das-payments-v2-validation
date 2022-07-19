using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SFA.DAS.Payments.FM36Tool.ApprenticeshipSetup
{
    public class ApprenticeshipPriceEpisodeModelConfiguration : IEntityTypeConfiguration<ApprenticeshipPriceEpisodeModel>
    {
        public void Configure(EntityTypeBuilder<ApprenticeshipPriceEpisodeModel> builder)
        {
            builder.ToTable("ApprenticeshipPriceEpisode", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName(@"Id").IsRequired();
            builder.Property(x => x.ApprenticeshipId).HasColumnName(@"ApprenticeshipId").IsRequired();
            builder.Property(x => x.StartDate).HasColumnName(@"StartDate").IsRequired();
            builder.Property(x => x.EndDate).HasColumnName(@"EndDate");
            builder.Property(x => x.Cost).HasColumnName(@"Cost").IsRequired();
            builder.Property(x => x.Removed).HasColumnName(@"Removed").IsRequired();


        }
    }
}