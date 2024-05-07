using Microsoft.EntityFrameworkCore;
using activityCore.Models;
using File = activityCore.Models.File;

namespace activityCore.Data;

public partial class ActivityContext : DbContext
{
    public ActivityContext()
    {
    }

    public ActivityContext(DbContextOptions<ActivityContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=BUMBIM\\SQLEXPRESS;Initial Catalog=Activity;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_CI_AS");

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("Activity");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActivityHeaderId).HasColumnName("ActivityHeaderID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.IsDelete).HasColumnName("isDelete");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.ActivityHeader).WithMany(p => p.InverseActivityHeader)
                .HasForeignKey(d => d.ActivityHeaderId)
                .HasConstraintName("FK_Activity_ActivityHeaderID");

            entity.HasOne(d => d.Project).WithMany(p => p.Activities)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Project_ProjectID");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.ToTable("File");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(50);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Project");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.IsDelete).HasColumnName("isDelete");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Token).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
