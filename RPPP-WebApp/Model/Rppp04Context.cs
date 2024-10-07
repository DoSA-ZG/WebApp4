using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.ModelsPartial;

namespace RPPP_WebApp.Model;

public partial class Rppp04Context : DbContext
{
    public Rppp04Context()
    {
    }

    public Rppp04Context(DbContextOptions<Rppp04Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Dokument> Dokument { get; set; }

    public virtual DbSet<EvidencijaRada> EvidencijaRada { get; set; }

    public virtual DbSet<KarticaProjekta> KarticaProjekta { get; set; }

    public virtual DbSet<KorisnickiRacun> KorisnickiRacun { get; set; }

    public virtual DbSet<Narucitelj> Narucitelj { get; set; }

    public virtual DbSet<PrioritetZadatka> PrioritetZadatka { get; set; }

    public virtual DbSet<PrioritetZahtjeva> PrioritetZahtjeva { get; set; }

    public virtual DbSet<Projekt> Projekt { get; set; }

    public virtual DbSet<ProjektniZahtjev> ProjektniZahtjev { get; set; }

    public virtual DbSet<StatusDokumenta> StatusDokumenta { get; set; }

    public virtual DbSet<StatusZadatka> StatusZadatka { get; set; }

    public virtual DbSet<Suradnik> Suradnik { get; set; }

    public virtual DbSet<SuradnikUloga> SuradnikUloga { get; set; }

    public virtual DbSet<Transakcija> Transakcija { get; set; }

    public virtual DbSet<VrstaDokumenta> VrstaDokumenta { get; set; }

    public virtual DbSet<VrstaProjekta> VrstaProjekta { get; set; }

    public virtual DbSet<VrstaRada> VrstaRada { get; set; }

    public virtual DbSet<VrstaSuradnika> VrstaSuradnika { get; set; }

    public virtual DbSet<VrstaTransakcije> VrstaTransakcije { get; set; }

    public virtual DbSet<VrstaUloge> VrstaUloge { get; set; }

    public virtual DbSet<VrstaZahtjeva> VrstaZahtjeva { get; set; }

    public virtual DbSet<Zadatak> Zadatak { get; set; }

    public virtual DbSet<ViewZadatak> vw_Zadatak { get; set; }
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseSqlServer("Name=ConnectionStrings:RPPP04");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Dokument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Dokument__3214EC07101390D3");

            entity.Property(e => e.DatumZadIzmj).HasColumnType("date");
            entity.Property(e => e.EkstenzijaDokumenta).HasMaxLength(255);
            entity.Property(e => e.NazivDok).HasMaxLength(255);
            entity.Property(e => e.VrPrijenos).HasColumnType("datetime");

            entity.HasOne(d => d.IdProjektNavigation).WithMany(p => p.Dokument)
                .HasForeignKey(d => d.ProjektId)
                .HasConstraintName("FK__Dokument__Projek__408F9238");

            entity.HasOne(d => d.IdStatusDokNavigation).WithMany(p => p.Dokument)
                .HasForeignKey(d => d.StatusDokumentaId)
                .HasConstraintName("FK__Dokument__Status__3EA749C6");

            entity.HasOne(d => d.IdVrstaDokNavigation).WithMany(p => p.Dokument)
                .HasForeignKey(d => d.VrstaDokumentaId)
                .HasConstraintName("FK__Dokument__VrstaD__3F9B6DFF");
        });

        modelBuilder.Entity<EvidencijaRada>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Evidenci__3214EC079B3C91C3");

            entity.Property(e => e.OpisRada).HasMaxLength(255);

            entity.HasOne(d => d.Suradnik).WithMany(p => p.EvidencijaRada)
                .HasForeignKey(d => d.SuradnikId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evidencij__Surad__4183B671");

            entity.HasOne(d => d.VrstaRada).WithMany(p => p.EvidencijaRada)
                .HasForeignKey(d => d.VrstaRadaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evidencij__Vrsta__4277DAAA");

            entity.HasOne(d => d.Zadatak).WithMany(p => p.EvidencijaRada)
                .HasForeignKey(d => d.ZadatakId)
                .HasConstraintName("FK__Evidencij__Zadat__436BFEE3");
        });

        modelBuilder.Entity<KarticaProjekta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KarticaP__3214EC0777217FB0");

            entity.HasOne(d => d.Projekt).WithMany(p => p.KarticaProjekta)
                .HasForeignKey(d => d.ProjektId)
                .HasConstraintName("FK__KarticaPr__Proje__4460231C");
        });

        modelBuilder.Entity<KorisnickiRacun>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Korisnic__3214EC07C79D00B7");
        });

        modelBuilder.Entity<Narucitelj>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Narucite__3214EC07673B74AF");

            entity.HasIndex(e => e.Oib, "UQ__Narucite__CB394B3E0B62F7D3").IsUnique();

            entity.Property(e => e.Adresa).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Iban)
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnName("IBAN");
            entity.Property(e => e.NazivNarucitelj).HasMaxLength(255);
            entity.Property(e => e.Oib)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnName("OIB");
        });

        modelBuilder.Entity<PrioritetZadatka>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Priorite__3214EC07842D6762");

            entity.Property(e => e.NazivPrioriteta)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<PrioritetZahtjeva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Priorite__3214EC074DB589A1");

            entity.Property(e => e.NazivPrioritetaZahtjeva)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Projekt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Projekt__3214EC07B3017229");

            entity.Property(e => e.DatumIsporukaPr).HasColumnType("date");
            entity.Property(e => e.KraticaProjekt).HasMaxLength(255);
            entity.Property(e => e.NazivProjekt)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Narucitelj).WithMany(p => p.Projekt)
                .HasForeignKey(d => d.NaruciteljId)
                .HasConstraintName("FK__Projekt__Narucit__45544755");

            entity.HasOne(d => d.VrstaProjekta).WithMany(p => p.Projekt)
                .HasForeignKey(d => d.VrstaProjektaId)
                .HasConstraintName("FK__Projekt__VrstaPr__46486B8E");
        });

        modelBuilder.Entity<ProjektniZahtjev>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Projektn__3214EC07D4AD8206");

            entity.Property(e => e.NazivZahtjeva)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.OpisZahtjeva)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.PrioritetZahtjeva).WithMany(p => p.ProjektniZahtjev)
                .HasForeignKey(d => d.PrioritetZahtjevaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Projektni__Prior__473C8FC7");

            entity.HasOne(d => d.Projekt).WithMany(p => p.ProjektniZahtjev)
                .HasForeignKey(d => d.ProjektId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Projektni__Proje__4830B400");

            entity.HasOne(d => d.VrstaZahtjeva).WithMany(p => p.ProjektniZahtjev)
                .HasForeignKey(d => d.VrstaZahtjevaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Projektni__Vrsta__4924D839");
        });

        modelBuilder.Entity<StatusDokumenta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StatusDo__3214EC0778CA0878");

            entity.Property(e => e.StatusDok).HasMaxLength(255);
        });

        modelBuilder.Entity<StatusZadatka>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StatusZa__3214EC0716C41CFC");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Suradnik>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Suradnik__3214EC07887BF72D");

            entity.HasIndex(e => e.BrojTelefona, "UQ__Suradnik__A06087F6C3C36316").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Suradnik__A9D10534EAEE18C6").IsUnique();

            entity.Property(e => e.BrojTelefona)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Ime)
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.Organizacija)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Prezime)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(d => d.KorisnickiRacun).WithMany(p => p.Suradnik)
                .HasForeignKey(d => d.KorisnickiRacunId)
                .HasConstraintName("FK__Suradnik__Korisn__4A18FC72");

            entity.HasOne(d => d.VrstaSuradnika).WithMany(p => p.Suradnik)
                .HasForeignKey(d => d.VrstaSuradnikaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Suradnik__VrstaS__4B0D20AB");
        });

        modelBuilder.Entity<SuradnikUloga>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Suradnik__3214EC076A868311");

            entity.Property(e => e.DatumKraj).HasColumnType("date");
            entity.Property(e => e.DatumPocetak).HasColumnType("date");

            entity.HasOne(d => d.Projekt).WithMany(p => p.SuradnikUloga)
                .HasForeignKey(d => d.ProjektId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SuradnikU__Proje__4C0144E4");

            entity.HasOne(d => d.Suradnik).WithMany(p => p.SuradnikUloga)
                .HasForeignKey(d => d.SuradnikId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SuradnikU__Surad__4CF5691D");

            entity.HasOne(d => d.VrstaUloge).WithMany(p => p.SuradnikUloga)
                .HasForeignKey(d => d.VrstaUlogeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SuradnikU__Vrsta__4DE98D56");
        });

        modelBuilder.Entity<Transakcija>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transakc__3214EC072FF830F8");

            entity.Property(e => e.DatumVrijeme).HasColumnType("datetime");
            entity.Property(e => e.Iban)
                .HasMaxLength(30)
                .HasColumnName("IBAN");

            entity.HasOne(d => d.KarticaProjekta).WithMany(p => p.TransakcijaKarticaProjekta)
                .HasForeignKey(d => d.KarticaProjektaId)
                .HasConstraintName("kartica_projekta_fk");

            entity.HasOne(d => d.KarticaProjektaId1Navigation).WithMany(p => p.TransakcijaKarticaProjektaId1Navigation)
                .HasForeignKey(d => d.KarticaProjektaId1)
                .HasConstraintName("kartica_projekta_fk1");

            entity.HasOne(d => d.VrstaTransakcije).WithMany(p => p.Transakcija)
                .HasForeignKey(d => d.VrstaTransakcijeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vrsta_transakcije_fk");
        });

        modelBuilder.Entity<VrstaDokumenta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VrstaDok__3214EC07CE7A898F");

            entity.Property(e => e.VrstaDok).HasMaxLength(255);
        });

        modelBuilder.Entity<VrstaProjekta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VrstaPro__3214EC07AFDD79D0");

            entity.Property(e => e.Vrsta)
                .IsRequired()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<VrstaRada>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VrstaRad__3214EC07BB142D77");

            entity.Property(e => e.VrstaRada1)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("VrstaRada");
        });

        modelBuilder.Entity<VrstaSuradnika>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VrstaSur__3214EC07C8528F2C");

            entity.HasIndex(e => e.Vrsta, "UQ__VrstaSur__54C57C81BA01FF1A").IsUnique();

            entity.Property(e => e.Vrsta)
                .IsRequired()
                .HasMaxLength(30);
        });

        modelBuilder.Entity<VrstaTransakcije>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VrstaTra__3214EC077B790E4F");

            entity.Property(e => e.Vrsta)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<VrstaUloge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VrstaUlo__3214EC079D9B69FA");

            entity.HasIndex(e => e.Vrsta, "UQ__VrstaUlo__54C57C816F817F2B").IsUnique();

            entity.Property(e => e.Vrsta)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<VrstaZahtjeva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VrstaZah__3214EC075E2C1870");

            entity.Property(e => e.NazivVrsteZahtjeva)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Zadatak>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Zadatak__3214EC075D39D8E8");

            entity.Property(e => e.OpisZadatak)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.PlanKraj).HasColumnType("date");
            entity.Property(e => e.PlanPocetak).HasColumnType("date");
            entity.Property(e => e.StvarniKraj).HasColumnType("date");
            entity.Property(e => e.StvarniPocetak).HasColumnType("date");

            entity.HasOne(d => d.PrioritetZadatka).WithMany(p => p.Zadatak)
                .HasForeignKey(d => d.PrioritetZadatkaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Zadatak__Priorit__51BA1E3A");

            entity.HasOne(d => d.ProjektniZahtjev).WithMany(p => p.Zadatak)
                .HasForeignKey(d => d.ProjektniZahtjevId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Zadatak__Projekt__52AE4273");

            entity.HasOne(d => d.StatusZadatka).WithMany(p => p.Zadatak)
                .HasForeignKey(d => d.StatusZadatkaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Zadatak__StatusZ__53A266AC");

            entity.HasOne(d => d.Suradnik).WithMany(p => p.Zadatak)
                .HasForeignKey(d => d.SuradnikId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Zadatak__Suradni__54968AE5");
        });
        modelBuilder.Entity<ViewZadatak>(entity => {
            entity.HasNoKey();
            //entity.ToView("vw_Zadatak");
            //u slučaju da se DbSet svojstvo zove drugačije
        });
        OnModelCreatingPartial(modelBuilder);


    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
