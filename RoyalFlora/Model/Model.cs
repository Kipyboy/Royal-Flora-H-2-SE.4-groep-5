using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

public class MyDbContext : DbContext
{
    public DbSet<Rol> Rollen { get; set; }
    public DbSet<Bedrijf> Bedrijven { get; set; }
    public DbSet<Gebruiker> Gebruikers { get; set; }
    public DbSet<Veiling> Veilingen { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Foto> Fotos { get; set; }

    public string DbPath { get; }

    public MyDbContext()
    {
        //locatie is in de bin map maar het moet zo voor nu
        var projectDir = AppContext.BaseDirectory;
        var databaseDir = System.IO.Path.Combine(projectDir, "database");
        System.IO.Directory.CreateDirectory(databaseDir);
        DbPath = System.IO.Path.Combine(databaseDir, "mydb.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // alle keys
        modelBuilder.Entity<Rol>().HasKey(r => r.IdRollen);
        modelBuilder.Entity<Bedrijf>().HasKey(b => b.IdBedrijf);
        modelBuilder.Entity<Gebruiker>().HasKey(g => g.IdGebruiker);
        modelBuilder.Entity<Veiling>().HasKey(v => v.IdVeilingen);
        modelBuilder.Entity<Product>().HasKey(p => p.IdProduct);
        modelBuilder.Entity<Foto>().HasKey(f => new { f.IdFoto, f.IdProduct });

        // Bedrijf-Gebruiker
        modelBuilder.Entity<Gebruiker>()
            .HasOne(g => g.BedrijfNavigation)
            .WithMany(b => b.Gebruikers)
            .HasForeignKey(g => g.Bedrijf)
            .OnDelete(DeleteBehavior.NoAction);

        // Gebruiker - VeilingenKoper
        modelBuilder.Entity<Veiling>()
            .HasOne(v => v.KoperNavigation)
            .WithMany(g => g.VeilingenKoper)
            .HasForeignKey(v => v.Koper)
            .OnDelete(DeleteBehavior.NoAction);

        // Gebruiker - VeilingenMeester
        modelBuilder.Entity<Veiling>()
            .HasOne(v => v.VeilingMeesterNavigation)
            .WithMany(g => g.VeilingenMeester)
            .HasForeignKey(v => v.VeilingMeester)
            .OnDelete(DeleteBehavior.NoAction);

        // Gebruiker - Rol
        modelBuilder.Entity<Gebruiker>()
            .HasOne(g => g.RolNavigation)
            .WithMany(r => r.Gebruikers)
            .HasForeignKey(g => g.Rol)
            .OnDelete(DeleteBehavior.NoAction);

        // Bedrijf - OprichterGebruiker
        modelBuilder.Entity<Bedrijf>()
            .HasOne(b => b.OprichterGebruiker)
            .WithMany(g => g.BedrijvenOprichter)
            .HasForeignKey(b => b.Oprichter)
            .OnDelete(DeleteBehavior.NoAction);

        // Product - Veiling
        modelBuilder.Entity<Product>()
            .HasOne(p => p.VeilingNavigation)
            .WithMany(v => v.Products)
            .HasForeignKey(p => p.Veiling)
            .IsRequired(false)  // Make it optional
            .OnDelete(DeleteBehavior.NoAction);

        // Product - Leverancier
        modelBuilder.Entity<Product>()
            .HasOne(p => p.LeverancierNavigation)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.Leverancier)
            .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<Rol>()
            .Navigation(r => r.Gebruikers).AutoInclude()
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<Bedrijf>()
            .Navigation(b => b.Gebruikers).AutoInclude()
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<Bedrijf>()
            .Navigation(b => b.Products).AutoInclude()
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<Gebruiker>()
            .Navigation(g => g.VeilingenKoper).AutoInclude()
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<Gebruiker>()
            .Navigation(g => g.VeilingenMeester).AutoInclude()
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<Gebruiker>()
            .Navigation(g => g.BedrijvenOprichter).AutoInclude()
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<Veiling>()
            .Navigation(v => v.Products).AutoInclude()
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<Product>()
            .Navigation(p => p.Fotos).AutoInclude()
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}

public class Rol
{
    public int IdRollen { get; set; }
    public string RolNaam { get; set; }

    public List<Gebruiker> Gebruikers { get; set; } = new List<Gebruiker>();
}

public class Bedrijf
{
    public int IdBedrijf { get; set; }
    public string BedrijfNaam { get; set; }
    public string KVK { get; set; }
    public int? Oprichter { get; set; }
    public string Adress { get; set; }

    public Gebruiker OprichterGebruiker { get; set; }
    public List<Gebruiker> Gebruikers { get; set; } = new List<Gebruiker>();
    public List<Product> Products { get; set; } = new List<Product>();
}

public class Gebruiker
{
    public int IdGebruiker { get; set; }
    public string VoorNaam { get; set; }
    public string AchterNaam { get; set; }
    public string E_mail { get; set; }
    public int? Role { get; set; }
    public string Postcode { get; set; }
    public string Adress { get; set; }
    public string Wachtwoord { get; set; }
    public int Rol { get; set; }
    public string Telefoonnummer { get; set; }
    public int? Bedrijf { get; set; }

    public Rol RolNavigation { get; set; }
    public Bedrijf BedrijfNavigation { get; set; }
    public List<Veiling> VeilingenKoper { get; set; } = new List<Veiling>();
    public List<Veiling> VeilingenMeester { get; set; } = new List<Veiling>();
    public List<Bedrijf> BedrijvenOprichter { get; set; } = new List<Bedrijf>();
}

public class Veiling
{
    public int IdVeilingen { get; set; }
    public DateTime? StartDatum { get; set; }
    public DateTime? EindDatum { get; set; }
    public int Koper { get; set; }
    public int? BeginPrijs { get; set; }
    public string MinimumPrijs { get; set; }
    public int VeilingMeester { get; set; }

    public Gebruiker KoperNavigation { get; set; }
    public Gebruiker VeilingMeesterNavigation { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int IdProduct { get; set; }
    public string ProductNaam { get; set; }
    public string productBeschrijving { get; set; }
    public string MinimumPrijs { get; set; }
    public int? Veiling { get; set; } // Changed to nullable
    public int? Leverancier { get; set; }

    public Veiling VeilingNavigation { get; set; }
    public Bedrijf LeverancierNavigation { get; set; }
    public List<Foto> Fotos { get; set; } = new List<Foto>();
}

public class Foto
{
    public int IdFoto { get; set; }
    public string FotoPath { get; set; }
    public int IdProduct { get; set; }

    public Product Product { get; set; }
}