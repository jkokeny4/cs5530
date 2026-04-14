using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class LMSContext : DbContext
    {
        public LMSContext()
        {
        }

        public LMSContext(DbContextOptions<LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrator> Administrators { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<AssignmentCatergory> AssignmentCatergories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Enrolled> Enrolleds { get; set; } = null!;
        public virtual DbSet<Professor> Professors { get; set; } = null!;
        public virtual DbSet<Sshkey> Sshkeys { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("name=LMS:LMSConnectionString", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.16-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID");

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.AssId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => new { e.CatId, e.Name }, "catID")
                    .IsUnique();

                entity.Property(e => e.AssId)
                    .HasColumnType("int(11)")
                    .HasColumnName("assID");

                entity.Property(e => e.CatId)
                    .HasColumnType("int(11)")
                    .HasColumnName("catID");

                entity.Property(e => e.Contents).HasMaxLength(8192);

                entity.Property(e => e.Due).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Points).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.Cat)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.CatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignments_ibfk_1");
            });

            modelBuilder.Entity<AssignmentCatergory>(entity =>
            {
                entity.HasKey(e => e.CatId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => new { e.Name, e.ClassId }, "Name")
                    .IsUnique();

                entity.HasIndex(e => e.ClassId, "classID");

                entity.Property(e => e.CatId)
                    .HasColumnType("int(11)")
                    .HasColumnName("catID");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("classID");

                entity.Property(e => e.GradeWeight)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("gradeWeight");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.AssignmentCatergories)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AssignmentCatergories_ibfk_1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => e.ProfessorId, "professorID");

                entity.HasIndex(e => new { e.CourseId, e.Year, e.Season }, "uniqueCourseSemester")
                    .IsUnique();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("classID");

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("courseID");

                entity.Property(e => e.EndTime)
                    .HasColumnType("time")
                    .HasColumnName("endTime");

                entity.Property(e => e.Location)
                    .HasMaxLength(100)
                    .HasColumnName("location");

                entity.Property(e => e.ProfessorId)
                    .HasMaxLength(8)
                    .HasColumnName("professorID")
                    .IsFixedLength();

                entity.Property(e => e.Season)
                    .HasColumnType("enum('Spring','Summer','Fall')")
                    .HasColumnName("season");

                entity.Property(e => e.StartTime)
                    .HasColumnType("time")
                    .HasColumnName("startTime");

                entity.Property(e => e.Year)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("year");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_1");

                entity.HasOne(d => d.Professor)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.ProfessorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_2");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => new { e.Number, e.DId }, "Number")
                    .IsUnique();

                entity.HasIndex(e => e.DId, "dID");

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("courseID");

                entity.Property(e => e.DId)
                    .HasColumnType("int(11)")
                    .HasColumnName("dID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Number).HasColumnType("int(11)");

                entity.HasOne(d => d.DIdNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.DId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Courses_ibfk_1");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DeptId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.SubjectAbbrev, "subjectAbbrev")
                    .IsUnique();

                entity.Property(e => e.DeptId)
                    .HasColumnType("int(11)")
                    .HasColumnName("deptID");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.SubjectAbbrev)
                    .HasMaxLength(4)
                    .HasColumnName("subjectAbbrev");
            });

            modelBuilder.Entity<Enrolled>(entity =>
            {
                entity.HasKey(e => new { e.UId, e.ClassId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("Enrolled");

                entity.HasIndex(e => e.ClassId, "classID");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("classID");

                entity.Property(e => e.Grade).HasMaxLength(2);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrolleds)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrolled_ibfk_2");

                entity.HasOne(d => d.UIdNavigation)
                    .WithMany(p => p.Enrolleds)
                    .HasForeignKey(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrolled_ibfk_1");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.DId, "dID");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.DId)
                    .HasColumnType("int(11)")
                    .HasColumnName("dID");

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.HasOne(d => d.DIdNavigation)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.DId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Professors_ibfk_1");
            });

            modelBuilder.Entity<Sshkey>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("sshkey");

                entity.Property(e => e.Sshkey1)
                    .HasColumnType("text")
                    .HasColumnName("sshkey");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.DId, "dID");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.DId)
                    .HasColumnType("int(11)")
                    .HasColumnName("dID");

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.HasOne(d => d.DIdNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.DId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Students_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => e.SubId)
                    .HasName("PRIMARY");

                entity.ToTable("Submission");

                entity.HasIndex(e => new { e.AssId, e.SId }, "assID")
                    .IsUnique();

                entity.HasIndex(e => e.SId, "sID");

                entity.Property(e => e.SubId)
                    .HasColumnType("int(11)")
                    .HasColumnName("subID");

                entity.Property(e => e.AssId)
                    .HasColumnType("int(11)")
                    .HasColumnName("assID");

                entity.Property(e => e.Contents).HasMaxLength(8192);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.SId)
                    .HasMaxLength(8)
                    .HasColumnName("sID")
                    .IsFixedLength();

                entity.Property(e => e.Score).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.Ass)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.AssId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_2");

                entity.HasOne(d => d.SIdNavigation)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.SId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
