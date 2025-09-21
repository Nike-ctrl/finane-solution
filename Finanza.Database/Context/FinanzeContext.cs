using Finanza.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Finanza.Database.Context;

public partial class FinanzeContext : DbContext
{
    public FinanzeContext()
    {
    }

    public FinanzeContext(DbContextOptions<FinanzeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<categoria> categoria { get; set; }

    public virtual DbSet<conto> conto { get; set; }

    public virtual DbSet<nota> nota { get; set; }

    public virtual DbSet<raggruppamento_conto> raggruppamento_conto { get; set; }

    public virtual DbSet<tipo_transazione> tipo_transazione { get; set; }

    public virtual DbSet<transazione> transazione { get; set; }

    public virtual DbSet<trapasso> trapasso { get; set; }

    public virtual DbSet<view_somma_categoria_anno_mese_conto> view_somma_categoria_anno_mese_conto { get; set; }

    public virtual DbSet<view_somma_conto_anno_mese> view_somma_conto_anno_mese { get; set; }

    public virtual DbSet<view_somma_raggruppamenti_mese_anno> view_somma_raggruppamenti_mese_anno { get; set; }

    public virtual DbSet<view_sommario_transazioni> view_sommario_transazioni { get; set; }

    public virtual DbSet<view_transazione_completa> view_transazione_completa { get; set; }

    public virtual DbSet<view_valore_cumulativo_mensile> view_valore_cumulativo_mensile { get; set; }

    public virtual DbSet<vincolo_gruppo> vincolo_gruppo { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
        string? conn = configuration.GetConnectionString("DefaultConnectionFinanze");
        if (conn == null)
        {
            throw new Exception("Not found connection string DefaultConnectionFinanze inside appsettings.json");
        }
        optionsBuilder.UseNpgsql(conn);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<categoria>(entity =>
        {
            entity.HasKey(e => e.categoria_id).HasName("categoria_pk");

            entity.HasIndex(e => e.categoria_nome, "categoria_unique").IsUnique();
        });

        modelBuilder.Entity<conto>(entity =>
        {
            entity.HasKey(e => e.conto_id).HasName("conto_pk");

            entity.HasIndex(e => e.conto_nome, "conto_unique").IsUnique();
        });

        modelBuilder.Entity<nota>(entity =>
        {
            entity.HasKey(e => e.nota_id).HasName("note_pk");

            entity.ToTable(tb => tb.HasComment("tabella dove vengono salvate le note"));

            entity.HasIndex(e => new { e.anno, e.mese }, "nota_anno_idx");

            entity.HasIndex(e => e.creation, "nota_creation_idx");

            entity.Property(e => e.nota_id).HasComment("pk");
            entity.Property(e => e.anno).HasComment("anno della nota");
            entity.Property(e => e.creation)
                .HasComment("indica la data di creazione della nota")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.mese).HasComment("mese della nota");
        });

        modelBuilder.Entity<raggruppamento_conto>(entity =>
        {
            entity.HasKey(e => e.raggruppamento_conto_id).HasName("raggruppamento_conto_pk");

            entity.HasIndex(e => e.raggruppamento_conto_nome, "raggruppamento_conto_unique").IsUnique();
        });

        modelBuilder.Entity<tipo_transazione>(entity =>
        {
            entity.HasKey(e => e.tipo_transazione_id).HasName("tipo_transazione_pk");

            entity.HasIndex(e => e.tipo_transazione_nome, "tipo_transazione_unique").IsUnique();

            entity.Property(e => e.tipo_transazione_id).HasDefaultValueSql("nextval('tipo_transazione_tipo_transazione_int_seq'::regclass)");
        });

        modelBuilder.Entity<transazione>(entity =>
        {
            entity.HasKey(e => e.transazione_id).HasName("transazione_pk");

            entity.HasIndex(e => e.conto_id, "idx_transazione_conto");

            entity.HasIndex(e => new { e.conto_id, e.data }, "idx_valori_data");

            entity.HasIndex(e => new { e.anno, e.mese }, "transazione_anno_idx");

            entity.HasIndex(e => e.data, "transazione_data_idx");

            entity.Property(e => e.creazione)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.data).HasColumnType("timestamp without time zone");
            entity.Property(e => e.valore).HasComment("indica il valore positivo di inserimento");
            entity.Property(e => e.valore_reale).HasComment("questo campo viene modifica dal trigger before insert, si occupa di capire se deve essere positivo o negati dal tipo di transazione");

            entity.HasOne(d => d.categoria).WithMany(p => p.transazione)
                .HasForeignKey(d => d.categoria_id)
                .HasConstraintName("transazione_categoria_fk");

            entity.HasOne(d => d.conto).WithMany(p => p.transazione)
                .HasForeignKey(d => d.conto_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transazione_conto_fk");

            entity.HasOne(d => d.tipo_transazione).WithMany(p => p.transazione)
                .HasForeignKey(d => d.tipo_transazione_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transazione_tipo_transazione_fk");

            entity.HasOne(d => d.trapasso).WithMany(p => p.transazione)
                .HasForeignKey(d => d.trapasso_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("transazione_trapasso_fk");
        });

        modelBuilder.Entity<trapasso>(entity =>
        {
            entity.HasKey(e => e.trapasso_id).HasName("trapasso_pk");

            entity.Property(e => e.data_transazione).HasColumnType("timestamp without time zone");
            entity.Property(e => e.transazione_destinazione_id).HasComment("id transazione di destinazione");
            entity.Property(e => e.transazione_sorgente_id).HasComment("id transazione sorgente");

            entity.HasOne(d => d.categoria).WithMany(p => p.trapasso)
                .HasForeignKey(d => d.categoria_id)
                .HasConstraintName("trapasso_categoria_fk");

            entity.HasOne(d => d.transazione_destinazione).WithMany(p => p.trapassotransazione_destinazione)
                .HasForeignKey(d => d.transazione_destinazione_id)
                .HasConstraintName("trapasso_dest_fk");

            entity.HasOne(d => d.transazione_sorgente).WithMany(p => p.trapassotransazione_sorgente)
                .HasForeignKey(d => d.transazione_sorgente_id)
                .HasConstraintName("trapasso_src_fk");
        });

        modelBuilder.Entity<view_somma_categoria_anno_mese_conto>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("view_somma_categoria_anno_mese_conto");
        });

        modelBuilder.Entity<view_somma_conto_anno_mese>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("view_somma_conto_anno_mese");
        });

        modelBuilder.Entity<view_somma_raggruppamenti_mese_anno>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("view_somma_raggruppamenti_mese_anno");

            entity.Property(e => e.data).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<view_sommario_transazioni>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("view_sommario_transazioni");

            entity.Property(e => e.data).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<view_transazione_completa>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("view_transazione_completa");

            entity.Property(e => e.creazione).HasColumnType("timestamp without time zone");
            entity.Property(e => e.data).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<view_valore_cumulativo_mensile>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("view_valore_cumulativo_mensile");

            entity.Property(e => e.data).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<vincolo_gruppo>(entity =>
        {
            entity.HasKey(e => e.vincolo_gruppo_id).HasName("vincolo_gruppo_pk");

            entity.HasIndex(e => e.conto_id, "idx_vincolo_conto");

            entity.HasOne(d => d.conto).WithMany(p => p.vincolo_gruppo)
                .HasForeignKey(d => d.conto_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vincolo_gruppo_conto_fk");

            entity.HasOne(d => d.raggruppamento_conto).WithMany(p => p.vincolo_gruppo)
                .HasForeignKey(d => d.raggruppamento_conto_id)
                .HasConstraintName("vincolo_gruppo_raggruppamento_conto_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
