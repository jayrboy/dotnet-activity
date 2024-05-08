using System;
using System.Collections.Generic;
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

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<FileXproject> FileXprojects { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=BUMBIM\\SQLEXPRESS;Initial Catalog=Activity;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_CI_AS");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_User");

            entity.ToTable("Account");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.IsDelete).HasColumnName("isDelete");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

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

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(50);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<FileXproject>(entity =>
        {
            entity.ToTable("FileXProject");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.FileId).HasColumnName("FileID");
            entity.Property(e => e.IsDelete).HasColumnName("isDelete");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.File).WithMany(p => p.FileXprojects)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("FK_FileXProject_File");

            entity.HasOne(d => d.Project).WithMany(p => p.FileXprojects)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_FileXProject_Project");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
