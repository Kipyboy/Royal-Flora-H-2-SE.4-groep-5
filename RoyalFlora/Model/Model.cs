using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MyDbContext : DbContext
{
    public DbSet<Rol> Rollen { get; set; }
    public DbSet<Bedrijf> Bedrijven { get; set; }
    public DbSet<Gebruiker> Gebruikers { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Foto> Fotos { get; set; }
    public DbSet<Locatie> Locaties { get; set; }
    public DbSet<Veiling> Veilingen { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys
        modelBuilder.Entity<Rol>().HasKey(r => r.IdRollen);
        modelBuilder.Entity<Bedrijf>().HasKey(b => b.KVK);
        modelBuilder.Entity<Gebruiker>().HasKey(g => g.IdGebruiker);
        modelBuilder.Entity<Status>().HasKey(s => s.IdStatus);
        modelBuilder.Entity<Product>().HasKey(p => p.IdProduct);
        modelBuilder.Entity<Foto>().HasKey(f => new { f.IdFoto, f.IdProduct });
        modelBuilder.Entity<Locatie>().HasKey(l => l.IdLocatie);
        modelBuilder.Entity<Veiling>().HasKey(v => new { v.Locatie_idLocatie, v.Products_IdProduct });

        // Seed data - Rollen
        modelBuilder.Entity<Rol>().HasData(
            new Rol { IdRollen = 1, RolNaam = "Aanvoerder" },
            new Rol { IdRollen = 2, RolNaam = "Inkoper" },
            new Rol { IdRollen = 3, RolNaam = "Veilingmeester" }
        );

        // Seed data - Statusen
        modelBuilder.Entity<Status>().HasData(
            new Status { IdStatus = 1, Beschrijving = "Geregistreerd" },
            new Status { IdStatus = 2, Beschrijving = "Ingepland" },
            new Status { IdStatus = 3, Beschrijving = "Geveild" },
            new Status { IdStatus = 4, Beschrijving = "Verkocht" },
            new Status { IdStatus = 5, Beschrijving = "Gepauzeerd" }
        );

        // Bedrijf - Oprichter (Gebruiker)
        modelBuilder.Entity<Bedrijf>()
            .HasOne(b => b.OprichterNavigation)
            .WithMany(g => g.BedrijvenOprichter)
            .HasForeignKey(b => b.Oprichter)
            .OnDelete(DeleteBehavior.NoAction);

        // Gebruiker - Rol
        modelBuilder.Entity<Gebruiker>()
            .HasOne(g => g.RolNavigation)
            .WithMany(r => r.Gebruikers)
            .HasForeignKey(g => g.Rol)
            .OnDelete(DeleteBehavior.NoAction);

        // Gebruiker - Bedrijf (KVK)
        modelBuilder.Entity<Gebruiker>()
            .HasOne(g => g.BedrijfNavigation)
            .WithMany(b => b.Gebruikers)
            .HasForeignKey(g => g.KVK)
            .OnDelete(DeleteBehavior.NoAction);

        // Product - Leverancier (Bedrijf)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.LeverancierNavigation)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.Leverancier)
            .OnDelete(DeleteBehavior.NoAction);

        // Product - Koper (Gebruiker)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.KoperNavigation)
            .WithMany(g => g.ProductsKoper)
            .HasForeignKey(p => p.Koper)
            .OnDelete(DeleteBehavior.NoAction);

        // Product - Status
        modelBuilder.Entity<Product>()
            .HasOne(p => p.StatusNavigation)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.Status)
            .OnDelete(DeleteBehavior.NoAction);

        // Foto - Product
        modelBuilder.Entity<Foto>()
            .HasOne(f => f.ProductNavigation)
            .WithMany(p => p.Fotos)
            .HasForeignKey(f => f.IdProduct)
            .OnDelete(DeleteBehavior.NoAction);

        // Veiling - Locatie
        modelBuilder.Entity<Veiling>()
            .HasOne(v => v.LocatieNavigation)
            .WithMany(l => l.Veilingen)
            .HasForeignKey(v => v.Locatie_idLocatie)
            .OnDelete(DeleteBehavior.NoAction);

        // Veiling - Product
        modelBuilder.Entity<Veiling>()
            .HasOne(v => v.ProductNavigation)
            .WithMany(p => p.Veilingen)
            .HasForeignKey(v => v.Products_IdProduct)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

// Entity Classes
[Table("Rollen")]
public class Rol
{
    [Key]
    public int IdRollen { get; set; }
    
    [StringLength(45)]
    public string? RolNaam { get; set; }

    // Navigation properties
    public virtual ICollection<Gebruiker> Gebruikers { get; set; } = new List<Gebruiker>();
}

[Table("Status")]
public class Status
{
    [Key]
    public int IdStatus { get; set; }
    
    [StringLength(45)]
    public string? Beschrijving { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

[Table("Locatie")]
public class Locatie
{
    [Key]
    public int IdLocatie { get; set; }
    
    [StringLength(45)]
    public string? locatieNaam { get; set; }

    // Navigation properties
    public virtual ICollection<Veiling> Veilingen { get; set; } = new List<Veiling>();
}

[Table("Bedrijf")]
public class Bedrijf
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int? KVK { get; set; }
    
    [StringLength(45)]
    public string? BedrijfNaam { get; set; }
    
    [StringLength(45)]
    public string? Adress { get; set; }
    
    [StringLength(45)]
    public string? Postcode { get; set; }
    
    public int? Oprichter { get; set; }

    // Navigation properties
    [ForeignKey("Oprichter")]
    public virtual Gebruiker? OprichterNavigation { get; set; }
    
    public virtual ICollection<Gebruiker> Gebruikers { get; set; } = new List<Gebruiker>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

[Table("Gebruiker")]
public class Gebruiker
{
    [Key]
    public int IdGebruiker { get; set; }
    
    [Required]
    [StringLength(45)]
    public string VoorNaam { get; set; } = string.Empty;
    
    [Required]
    [StringLength(45)]
    public string AchterNaam { get; set; } = string.Empty;
    
    [Required]
    [StringLength(45)]
    [Column("E-mail")]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(60)]
    public string Wachtwoord { get; set; } = string.Empty;
    
    [StringLength(45)]
    public string? Postcode { get; set; }
    
    [StringLength(45)]
    public string? Adress { get; set; }
    
    [StringLength(45)]
    public string? Telefoonnummer { get; set; }
    
    public int Rol { get; set; }
    public int? KVK { get; set; }

    // Navigation properties
    [ForeignKey("Rol")]
    public virtual Rol? RolNavigation { get; set; }
    
    [ForeignKey("KVK")]
    public virtual Bedrijf? BedrijfNavigation { get; set; }
    
    public virtual ICollection<Bedrijf> BedrijvenOprichter { get; set; } = new List<Bedrijf>();
    public virtual ICollection<Product> ProductsKoper { get; set; } = new List<Product>();
}

[Table("Products")]
public class Product
{
    [Key]
    public int IdProduct { get; set; }
    
    [StringLength(45)]
    public string? ProductNaam { get; set; }

    [Column(TypeName = "text")]
    public string? ProductBeschrijving { get; set; }

    public int? Aantal { get; set; }
    
    [Column(TypeName = "decimal(10, 2)")]
    public decimal MinimumPrijs { get; set; }

    public DateTime? Datum { get; set; }

    public string? Locatie { get; set; }
    
    public int? Leverancier { get; set; }
    public int? Koper { get; set; }
    [Column(TypeName = "decimal(10, 2)")]
    public decimal verkoopPrijs { get; set; }
    public int? Status { get; set; }
    [Column(TypeName = "decimal(10, 2)")]
    public decimal? StartPrijs { get; set; }

    // Navigation properties
    [ForeignKey("Leverancier")]
    public virtual Bedrijf? LeverancierNavigation { get; set; }
    
    [ForeignKey("Koper")]
    public virtual Gebruiker? KoperNavigation { get; set; }
    
    [ForeignKey("Status")]
    public virtual Status? StatusNavigation { get; set; }
    
    public virtual ICollection<Foto> Fotos { get; set; } = new List<Foto>();
    public virtual ICollection<Veiling> Veilingen { get; set; } = new List<Veiling>();
}

[Table("Fotos")]
public class Foto
{
    [Key]
    [Column(Order = 0)]
    public int IdFoto { get; set; }
    
    [Key]
    [Column(Order = 1)]
    public int IdProduct { get; set; }
    
    [Column(TypeName = "text")]
    public string? FotoPath { get; set; }

    // Navigation properties
    [ForeignKey("IdProduct")]
    public virtual Product? ProductNavigation { get; set; }
}

[Table("Veiling")]
public class Veiling
{
    [Key]
    [Column(Order = 0)]
    public int Locatie_idLocatie { get; set; }
    
    [Key]
    [Column(Order = 1)]
    public int Products_IdProduct { get; set; }
    
    [Column(TypeName = "date")]
    public DateTime? datum { get; set; }
    
    [StringLength(45)]
    public string? ordernummer { get; set; }

    // Navigation properties
    [ForeignKey("Locatie_idLocatie")]
    public virtual Locatie? LocatieNavigation { get; set; }
    
    [ForeignKey("Products_IdProduct")]
    public virtual Product? ProductNavigation { get; set; }
}