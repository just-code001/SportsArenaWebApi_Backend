using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SportsArenaWebApi_Backend.Models;

public partial class SportsArenaDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public SportsArenaDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SportsArenaDbContext(DbContextOptions<SportsArenaDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<Tblblog> Tblblogs { get; set; }

    public virtual DbSet<Tblbooking> Tblbookings { get; set; }

    public virtual DbSet<Tblcafeitem> Tblcafeitems { get; set; }

    public virtual DbSet<Tblcafeorder> Tblcafeorders { get; set; }

    public virtual DbSet<Tblcafeorderdetail> Tblcafeorderdetails { get; set; }

    public virtual DbSet<Tblinquiry> Tblinquiries { get; set; }

    public virtual DbSet<Tblpayment> Tblpayments { get; set; }

    public virtual DbSet<Tblreview> Tblreviews { get; set; }

    public virtual DbSet<Tblrole> Tblroles { get; set; }

    public virtual DbSet<Tblsportcategory> Tblsportcategories { get; set; }

    public virtual DbSet<Tbluser> Tblusers { get; set; }

    public virtual DbSet<Tblvenue> Tblvenues { get; set; }

    public virtual DbSet<Tblvenueslot> Tblvenueslots { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(_configuration.GetConnectionString("projectConString"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tblblog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__tblblogs__2975AA281A49D203");

            entity.ToTable("tblblogs");

            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.PublishDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("publish_date");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.Author).WithMany(p => p.Tblblogs)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblblogs__author__0E391C95");
        });

        modelBuilder.Entity<Tblbooking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__tblbooki__5DE3A5B11F1245C9");

            entity.ToTable("tblbookings");

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.PayableAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("payable_amount");
            entity.Property(e => e.PaymentPaid).HasColumnName("payment_paid");
            entity.Property(e => e.SlotId).HasColumnName("slot_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Slot).WithMany(p => p.Tblbookings)
                .HasForeignKey(d => d.SlotId)
                .HasConstraintName("FK__tblbookin__slot___719CDDE7");

            entity.HasOne(d => d.User).WithMany(p => p.Tblbookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblbookin__user___72910220");
        });

        modelBuilder.Entity<Tblcafeitem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__tblcafei__52020FDD5AB20181");

            entity.ToTable("tblcafeitems");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.AvailableQuantity).HasColumnName("available_quantity");
            entity.Property(e => e.ItemName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("item_name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
        });

        modelBuilder.Entity<Tblcafeorder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__tblcafeo__465962298145CE46");

            entity.ToTable("tblcafeorders");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Tblcafeorders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblcafeor__user___02C769E9");
        });

        modelBuilder.Entity<Tblcafeorderdetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__tblcafeo__3C5A40804FCFDA55");

            entity.ToTable("tblcafeorderdetails");

            entity.Property(e => e.OrderDetailId).HasColumnName("order_detail_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Item).WithMany(p => p.Tblcafeorderdetails)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__tblcafeor__item___0697FACD");

            entity.HasOne(d => d.Order).WithMany(p => p.Tblcafeorderdetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__tblcafeor__order__05A3D694");
        });

        modelBuilder.Entity<Tblinquiry>(entity =>
        {
            entity.HasKey(e => e.InquiryId).HasName("PK__tblinqui__A1FB453A1573EC69");

            entity.ToTable("tblinquiry");

            entity.Property(e => e.InquiryId).HasColumnName("inquiry_id");
            entity.Property(e => e.InquiryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("inquiry_date");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Tblinquiries)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblinquir__user___0A688BB1");
        });

        modelBuilder.Entity<Tblpayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__tblpayme__ED1FC9EA00289888");

            entity.ToTable("tblpayments");

            entity.HasIndex(e => e.TransactionId, "UQ__tblpayme__85C600AE322E3234").IsUnique();

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("payment_status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("transaction_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.Tblpayments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblpaymen__booki__7849DB76");
        });

        modelBuilder.Entity<Tblreview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__tblrevie__60883D905BA62587");

            entity.ToTable("tblreviews");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Comment)
                .HasColumnType("text")
                .HasColumnName("comment");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VenueId).HasColumnName("venue_id");

            entity.HasOne(d => d.User).WithMany(p => p.Tblreviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblreview__user___7D0E9093");

            entity.HasOne(d => d.Venue).WithMany(p => p.Tblreviews)
                .HasForeignKey(d => d.VenueId)
                .HasConstraintName("FK__tblreview__venue__7C1A6C5A");
        });

        modelBuilder.Entity<Tblrole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tblroles__760965CCFFB05D19");

            entity.ToTable("tblroles");

            entity.HasIndex(e => e.Rolename, "UQ__tblroles__4685A06293D640CB").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Rolename)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Tblsportcategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__tblsport__D54EE9B4EEF07FAF");

            entity.ToTable("tblsportcategory");

            entity.HasIndex(e => e.Categoryname, "UQ__tblsport__1A0D12CEABA77D39").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Categoryname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("categoryname");
        });

        modelBuilder.Entity<Tbluser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__tblusers__B9BE370FB7EE2E37");

            entity.ToTable("tblusers");

            entity.HasIndex(e => e.Email, "UQ__tblusers__AB6E6164F3080A8A").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Contact)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("contact");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Tblusers)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__tblusers__role_i__625A9A57");
        });

        modelBuilder.Entity<Tblvenue>(entity =>
        {
            entity.HasKey(e => e.VenueId).HasName("PK__tblvenue__82A8BE8D9A68E537");

            entity.ToTable("tblvenue");

            entity.Property(e => e.VenueId).HasColumnName("venue_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("location");
            entity.Property(e => e.Priceperhour)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("priceperhour");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.VenueImage)
                .HasColumnType("text")
                .HasColumnName("venue_image");
            entity.Property(e => e.Venuename)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("venuename");

            entity.HasOne(d => d.Category).WithMany(p => p.Tblvenues)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__tblvenue__catego__69FBBC1F");

            entity.HasOne(d => d.Provider).WithMany(p => p.Tblvenues)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblvenue__provid__690797E6");
        });

        modelBuilder.Entity<Tblvenueslot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__tblvenue__971A01BB29F6AA81");

            entity.ToTable("tblvenueslots");

            entity.Property(e => e.SlotId).HasColumnName("slot_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsBooked).HasColumnName("is_booked");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.VenueId).HasColumnName("venue_id");

            entity.HasOne(d => d.Venue).WithMany(p => p.Tblvenueslots)
                .HasForeignKey(d => d.VenueId)
                .HasConstraintName("FK__tblvenues__venue__6DCC4D03");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
