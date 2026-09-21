using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecamNewBackend.Models;

namespace RecamNewBackend.Data;

public class RecamDbContext : IdentityDbContext<User>
{
    public RecamDbContext(DbContextOptions<RecamDbContext> options)
        : base(options)
    {
    }

    public DbSet<PhotographyCompany> PhotographyCompanies { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<ListingCase> ListingCases { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<CaseContact> CaseContacts { get; set; }
    public DbSet<SelectedMedia> SelectedMedias { get; set; }
    public DbSet<StatusHistory> StatusHistories { get; set; }
    public DbSet<MediaSelectionHistory> MediaSelectionHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SelectedMedia>()
            .HasOne(sm => sm.MediaAsset)
            .WithMany(ma => ma.SelectedMedias)
            .HasForeignKey(sm => sm.MediaAssetId)
            .OnDelete(DeleteBehavior.NoAction);
            
        modelBuilder.Entity<Agent>()
        .HasOne(a => a.PhotographyCompany)
        .WithMany(p => p.Agents)
        .HasForeignKey(a => a.PhotographyCompanyId)
        .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SelectedMedia>()
            .HasOne(sm => sm.Agent)
            .WithMany()
            .HasForeignKey(sm => sm.AgentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<StatusHistory>()
            .HasOne(sh => sh.ChangedByUser)
            .WithMany()
            .HasForeignKey(sh => sh.ChangedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MediaSelectionHistory>()
            .HasOne(h => h.MediaAsset)
            .WithMany()
            .HasForeignKey(h => h.MediaAssetId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MediaSelectionHistory>()
            .HasOne(h => h.Agent)
            .WithMany()
            .HasForeignKey(h => h.AgentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}