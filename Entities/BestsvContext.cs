using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace Api.Entities
{
    public partial class BestsvContext : DbContext
    {
        public BestsvContext()
        {
        }

        public BestsvContext(DbContextOptions<BestsvContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountRole> AccountRoles { get; set; }
        public virtual DbSet<Avatar> Avatars { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Certificate> Certificates { get; set; }
        public virtual DbSet<College> Colleges { get; set; }
        public virtual DbSet<Good> Goods { get; set; }
        public virtual DbSet<GoodsTag> GoodsTags { get; set; }
        public virtual DbSet<Identification> Identifications { get; set; }
        public virtual DbSet<LinkPage> LinkPages { get; set; }
        public virtual DbSet<Negotiation> Negotiations { get; set; }
        public virtual DbSet<NegotiationDetail> NegotiationDetails { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<OrderDetailEditHistory> OrderDetailEditHistories { get; set; }
        public virtual DbSet<OrderEvaluation> OrderEvaluations { get; set; }
        public virtual DbSet<OrderPayment> OrderPayments { get; set; }
        public virtual DbSet<OrderTag> OrderTags { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<ProcessStatus> ProcessStatuses { get; set; }
        public virtual DbSet<Purchase> Purchases { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<ZipCode> ZipCodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                optionsBuilder.UseSqlServer(configuration.GetConnectionString("BestsvDatabase"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("databasebestsv")
                .HasAnnotation("Relational:Collation", "Vietnamese_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.AccountId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.CreatedAt).HasColumnType("date");

                entity.Property(e => e.DateOfBirth).HasColumnType("date");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(7)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .IsFixedLength(true)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength(true)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.SecrectKey)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength(true)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Sex).HasComment("0: nữ 1: nam null: khác");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(20)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.Avatar)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.AvatarId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Account_Avatar");

                entity.HasOne(d => d.ZipCode)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.ZipCodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Account_ZipCode1");
            });

            modelBuilder.Entity<AccountRole>(entity =>
            {
                entity.HasKey(e => new { e.AccountId, e.RoleId });

                entity.ToTable("Account_Role");

                entity.Property(e => e.AccountId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountRoles)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK_Account_Role_Account");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AccountRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Account_Role_Role");
            });

            modelBuilder.Entity<Avatar>(entity =>
            {
                entity.ToTable("Avatar");

                entity.Property(e => e.AvatarId).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Uri)
                    .IsRequired()
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.CategoryId).ValueGeneratedOnAdd();

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Description)
                    .HasMaxLength(200)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.ParentCategory)
                    .WithMany(p => p.InverseParentCategory)
                    .HasForeignKey(d => d.ParentCategoryId)
                    .HasConstraintName("FK_Category_Category");
            });

            modelBuilder.Entity<Certificate>(entity =>
            {
                entity.ToTable("Certificate");

                entity.Property(e => e.CertificateDate).HasColumnType("date");

                entity.Property(e => e.CertificateName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasMaxLength(100)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Certificates)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("FK_Certificate_Supplier");
            });

            modelBuilder.Entity<College>(entity =>
            {
                entity.ToTable("College");

                entity.Property(e => e.CollegeName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<Good>(entity =>
            {
                entity.HasKey(e => e.GoodsId);

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Note)
                    .HasMaxLength(200)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Price).HasColumnType("money");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.UploadDate).HasColumnType("date");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Goods)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Goods_Category");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Goods)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Goods_Supplier");
            });

            modelBuilder.Entity<GoodsTag>(entity =>
            {
                entity.HasKey(e => new { e.GoodsId, e.TagId });

                entity.ToTable("Goods_Tag");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.TagId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.Goods)
                    .WithMany(p => p.GoodsTags)
                    .HasForeignKey(d => d.GoodsId)
                    .HasConstraintName("FK_Goods_Tag_Goods");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.GoodsTags)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Goods_Tag_Tag");
            });

            modelBuilder.Entity<Identification>(entity =>
            {
                entity.ToTable("Identification");

                entity.Property(e => e.IdentificationId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Expired).HasColumnType("datetime");

                entity.Property(e => e.Pin)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .HasColumnName("PIN")
                    .IsFixedLength(true)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<LinkPage>(entity =>
            {
                entity.ToTable("LinkPage");

                entity.Property(e => e.PageName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.LinkPages)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("FK_LinkPage_Supplier");
            });

            modelBuilder.Entity<Negotiation>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.SupplierId })
                    .HasName("PK_Negotiate");

                entity.ToTable("Negotiation");

                entity.Property(e => e.SupplierId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Expired).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Negotiations)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Negotiate_Order");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Negotiations)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Negotiate_Supplier");
            });

            modelBuilder.Entity<NegotiationDetail>(entity =>
            {
                entity.HasKey(e => new { e.OrderDetailId, e.SupplierId })
                    .HasName("PK_NegotiateDetail");

                entity.ToTable("NegotiationDetail");

                entity.Property(e => e.SupplierId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Expired).HasColumnType("datetime");

                entity.Property(e => e.OrderDetailUnitPrice).HasColumnType("money");

                entity.HasOne(d => d.OrderDetail)
                    .WithMany(p => p.NegotiationDetails)
                    .HasForeignKey(d => d.OrderDetailId)
                    .HasConstraintName("FK_NegotiateDetail_OrderDetail");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.NegotiationDetails)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NegotiateDetail_Supplier");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.AccountId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("Người mua hàng")
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.CommissionPercent).HasColumnType("numeric(3, 1)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeliveryAt).HasColumnType("datetime");

                entity.Property(e => e.DescriptionFileUri)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.DescriptionText)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasDefaultValueSql("(N'rỗng')")
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.DoWorkAt).HasColumnType("datetime");

                entity.Property(e => e.Expired).HasColumnType("datetime");

                entity.Property(e => e.SupplierId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasComment("Người bán")
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Tip).HasColumnType("money");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_Account");

                entity.HasOne(d => d.ProcessStatus)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.ProcessStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_ProcessStatus");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("FK_Order_Supplier");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.FileUri)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Note)
                    .HasMaxLength(200)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.UnitPrice).HasColumnType("money");

                entity.Property(e => e.UploadedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_TradeDetail_Category");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_OrderDetail_Order");
            });

            modelBuilder.Entity<OrderDetailEditHistory>(entity =>
            {
                entity.ToTable("OrderDetailEditHistory");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DecriptionFileUri)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Requirement)
                    .IsRequired()
                    .HasMaxLength(200)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.OrderDetail)
                    .WithMany(p => p.OrderDetailEditHistories)
                    .HasForeignKey(d => d.OrderDetailId)
                    .HasConstraintName("FK_OrderDetailEditHistory_OrderDetail");
            });

            modelBuilder.Entity<OrderEvaluation>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.Type })
                    .HasName("PK_OrderEvaluation_1");

                entity.ToTable("OrderEvaluation");

                entity.Property(e => e.Type).HasComment("(0: A-S ; 1: S-A)");

                entity.Property(e => e.Comment)
                    .HasMaxLength(200)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.CreatedAt).HasColumnType("smalldatetime");

                entity.Property(e => e.Rate).HasComment("Thang đo 50 ngang với 5 sao");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderEvaluations)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderEvaluation_Order");
            });

            modelBuilder.Entity<OrderPayment>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.PaymentId })
                    .HasName("PK_Trade_Payment");

                entity.ToTable("Order_Payment");

                entity.Property(e => e.Amount).HasColumnType("money");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderPayments)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Order_Payment_Order");

                entity.HasOne(d => d.Payment)
                    .WithMany(p => p.OrderPayments)
                    .HasForeignKey(d => d.PaymentId)
                    .HasConstraintName("FK_Trade_Payment_Payment");
            });

            modelBuilder.Entity<OrderTag>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.TagId })
                    .HasName("PK_Trade_Tag");

                entity.ToTable("Order_Tag");

                entity.Property(e => e.TagId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderTags)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Order_Tag_Order");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.OrderTags)
                    .HasForeignKey(d => d.TagId)
                    .HasConstraintName("FK_Trade_Tag_Tag");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payment");

                entity.Property(e => e.PaymentId).ValueGeneratedOnAdd();

                entity.Property(e => e.PaymentName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<ProcessStatus>(entity =>
            {
                entity.ToTable("ProcessStatus");

                entity.Property(e => e.ProcessStatusId).ValueGeneratedOnAdd();

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => new { e.GoodsId, e.AccountId });

                entity.ToTable("Purchase");

                entity.Property(e => e.GoodsId)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.AccountId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.BoughtAt).HasColumnType("datetime");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Purchases)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Purchase_Account");

                entity.HasOne(d => d.Goods)
                    .WithMany(p => p.Purchases)
                    .HasForeignKey(d => d.GoodsId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Purchase_Goods");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshToken");

                entity.Property(e => e.RefreshTokenId).ValueGeneratedNever();

                entity.Property(e => e.AccountId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Expired).HasColumnType("date");

                entity.Property(e => e.Ipaddress)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("IPAddress")
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.RoleId).ValueGeneratedOnAdd();

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Supplier");

                entity.Property(e => e.SupplierId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(100)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.Career)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.Property(e => e.FolderUri)
                    .IsRequired()
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");

                entity.HasOne(d => d.College)
                    .WithMany(p => p.Suppliers)
                    .HasForeignKey(d => d.CollegeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Supplier_College");

                entity.HasOne(d => d.SupplierNavigation)
                    .WithOne(p => p.Supplier)
                    .HasForeignKey<Supplier>(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Supplier_Account");
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tag");

                entity.Property(e => e.TagId)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            modelBuilder.Entity<ZipCode>(entity =>
            {
                entity.ToTable("ZipCode");

                entity.Property(e => e.ZipCodeId).ValueGeneratedNever();

                entity.Property(e => e.Position)
                    .IsRequired()
                    .HasMaxLength(50)
                    .UseCollation("SQL_Latin1_General_CP1_CI_AS");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
