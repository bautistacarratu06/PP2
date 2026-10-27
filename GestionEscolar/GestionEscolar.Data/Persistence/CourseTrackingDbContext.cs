using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GestionEscolar.Data.Persistence;

public sealed class CourseTrackingDbContext(DbContextOptions<CourseTrackingDbContext> options) : DbContext(options)
{
    public DbSet<SectionEntity> Sections => Set<SectionEntity>();

    public DbSet<StudentEntity> Students => Set<StudentEntity>();

    public DbSet<ClassSessionEntity> ClassSessions => Set<ClassSessionEntity>();

    public DbSet<AssignmentEntity> Assignments => Set<AssignmentEntity>();

    public DbSet<AttendanceRecordEntity> AttendanceRecords => Set<AttendanceRecordEntity>();

    public DbSet<SubmissionEntity> Submissions => Set<SubmissionEntity>();

    public DbSet<InterventionEntity> Interventions => Set<InterventionEntity>();

    public DbSet<SettingEntity> Settings => Set<SettingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SectionEntity>(entity =>
        {
            entity.ToTable("section", "dbo");
            entity.HasKey(e => e.Id).HasName("pk_section");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn(1, 1);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("uq_section_name");
        });

        modelBuilder.Entity<StudentEntity>(entity =>
        {
            entity.ToTable("student", "dbo");
            entity.HasKey(e => e.Dni).HasName("pk_student");
            entity.Property(e => e.Dni).HasColumnName("dni").HasColumnType("varchar(20)").HasMaxLength(20);
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn(1, 1).ValueGeneratedOnAdd();
            entity.HasAlternateKey(e => e.Id).HasName("uq_student_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.StudentNumber).HasColumnName("student_number").HasMaxLength(20).IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(80).IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(80).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(30);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(120);
            entity.Property(e => e.EnrolledOn)
                .HasColumnName("enrolled_on")
                .HasColumnType("date")
                .HasDefaultValueSql("CAST(SYSDATETIME() AS date)")
                .HasAnnotation("Relational:DefaultConstraintName", "df_student_enrolled_on")
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("SYSDATETIME()")
                .HasAnnotation("Relational:DefaultConstraintName", "df_student_created_at")
                .ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.SectionId, e.StudentNumber })
                .IsUnique()
                .HasDatabaseName("uq_student_section_student_number");
            entity.HasIndex(e => e.SectionId).HasDatabaseName("ix_student_section_id");
            entity.HasOne(e => e.Section)
                .WithMany(s => s.Students)
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_student_section");
        });

        modelBuilder.Entity<ClassSessionEntity>(entity =>
        {
            entity.ToTable("class_session", "dbo");
            entity.HasKey(e => e.Id).HasName("pk_class_session");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn(1, 1);
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.SessionDate).HasColumnName("session_date").HasColumnType("date");
            entity.HasIndex(e => new { e.SectionId, e.SessionDate })
                .IsUnique()
                .HasDatabaseName("uq_class_session_section_date");
            entity.HasOne(e => e.Section)
                .WithMany(s => s.ClassSessions)
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_class_session_section");
        });

        modelBuilder.Entity<AssignmentEntity>(entity =>
        {
            entity.ToTable("assignment", "dbo");
            entity.HasKey(e => e.Id).HasName("pk_assignment");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn(1, 1);
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(e => e.DueDate).HasColumnName("due_date").HasColumnType("date");
            entity.HasOne(e => e.Section)
                .WithMany(s => s.Assignments)
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_assignment_section");
        });

        modelBuilder.Entity<AttendanceRecordEntity>(entity =>
        {
            entity.ToTable("attendance_record", "dbo", table =>
                table.HasCheckConstraint(
                    "ck_attendance_record_status",
                    "[status] IN (N'presente', N'ausente')"));
            entity.HasKey(e => e.Id).HasName("pk_attendance_record");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn(1, 1);
            entity.Property(e => e.ClassSessionId).HasColumnName("class_session_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(10).IsRequired();
            entity.Property(e => e.CorrectedAt).HasColumnName("corrected_at").HasColumnType("datetime2(0)");
            entity.HasIndex(e => new { e.ClassSessionId, e.StudentId })
                .IsUnique()
                .HasDatabaseName("uq_attendance_record_session_student");
            entity.HasOne(e => e.ClassSession)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(e => e.ClassSessionId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_attendance_record_class_session");
            entity.HasOne(e => e.Student)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(e => e.StudentId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_attendance_record_student");
        });

        modelBuilder.Entity<SubmissionEntity>(entity =>
        {
            entity.ToTable("submission", "dbo", table =>
                table.HasCheckConstraint(
                    "ck_submission_status",
                    "[status] IN (N'entregó', N'no entregó')"));
            entity.HasKey(e => e.Id).HasName("pk_submission");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn(1, 1);
            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(12).IsRequired();
            entity.HasIndex(e => new { e.AssignmentId, e.StudentId })
                .IsUnique()
                .HasDatabaseName("uq_submission_assignment_student");
            entity.HasOne(e => e.Assignment)
                .WithMany(s => s.Submissions)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_submission_assignment");
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Submissions)
                .HasForeignKey(e => e.StudentId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_submission_student");
        });

        modelBuilder.Entity<InterventionEntity>(entity =>
        {
            entity.ToTable("intervention", "dbo", table =>
            {
                table.HasCheckConstraint(
                    "ck_intervention_channel",
                    "[channel] IN (N'Llamada', N'WhatsApp', N'Correo', N'Presencial')");
                table.HasCheckConstraint(
                    "ck_intervention_outcome",
                    "[outcome] IN (N'Contactado - se comprometió a volver', N'Contactado - sin novedades', N'No contestó', N'Abandonó confirmado')");
            });
            entity.HasKey(e => e.Id).HasName("pk_intervention");
            entity.Property(e => e.Id).HasColumnName("id").UseIdentityColumn(1, 1);
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.InterventionDate)
                .HasColumnName("intervention_date")
                .HasColumnType("date")
                .HasDefaultValueSql("CAST(SYSDATETIME() AS date)")
                .HasAnnotation("Relational:DefaultConstraintName", "df_intervention_date")
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
            entity.Property(e => e.Channel).HasColumnName("channel").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Outcome).HasColumnName("outcome").HasMaxLength(60).IsRequired();
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(500);
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Interventions)
                .HasForeignKey(e => e.StudentId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("fk_intervention_student");
        });

        modelBuilder.Entity<SettingEntity>(entity =>
        {
            entity.ToTable("setting", "dbo", table =>
                table.HasCheckConstraint("ck_setting_value", "[setting_value] BETWEEN 0 AND 100"));
            entity.HasKey(e => e.Key).HasName("pk_setting");
            entity.Property(e => e.Key).HasColumnName("setting_key").HasMaxLength(40);
            entity.Property(e => e.Value).HasColumnName("setting_value");
            entity.HasData(
                new SettingEntity { Key = "high_threshold", Value = 50 },
                new SettingEntity { Key = "medium_threshold", Value = 25 });
        });
    }
}
