using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Ontology> Ontologies => Set<Ontology>();
    public DbSet<OntologyAuthorPortfolio> OntologyAuthorPortfolios => Set<OntologyAuthorPortfolio>();
    public DbSet<OntologyBaseDocument> OntologyBaseDocuments => Set<OntologyBaseDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.Username).HasColumnName("username").IsRequired();
            entity.HasIndex(user => user.Username).IsUnique();
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(user => user.Role).HasColumnName("role").IsRequired();
            entity.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();
            entity.Property(user => user.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        });

        modelBuilder.Entity<Ontology>(entity =>
        {
            entity.ToTable("ontologies");
            entity.HasKey(ontology => ontology.Id);
            entity.Property(ontology => ontology.Id).HasColumnName("id");
            entity.Property(ontology => ontology.Name).HasColumnName("name").HasMaxLength(250).IsRequired();
            entity.Property(ontology => ontology.Iri).HasColumnName("iri").HasMaxLength(1000).IsRequired();
            entity.Property(ontology => ontology.Description).HasColumnName("description").IsRequired();
            entity.Property(ontology => ontology.Documentation).HasColumnName("documentation").IsRequired();
            entity.Property(ontology => ontology.Terms).HasColumnName("terms").IsRequired();
            entity.Property(ontology => ontology.SourceDocument).HasColumnName("source_document").HasMaxLength(500).IsRequired();
            entity.Property(ontology => ontology.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
            entity.HasIndex(ontology => ontology.Iri).IsUnique();
        });

        modelBuilder.Entity<OntologyAuthorPortfolio>(entity =>
        {
            entity.ToTable("ontology_author_portfolios");
            entity.HasKey(authorImage => authorImage.Id);
            entity.Property(authorImage => authorImage.Id).HasColumnName("id");
            entity.Property(authorImage => authorImage.OntologyId).HasColumnName("ontology_id");
            entity.Property(authorImage => authorImage.AuthorName).HasColumnName("author_name").HasMaxLength(250).IsRequired();
            entity.Property(author => author.PortfolioUrl).HasColumnName("portfolio_url").HasMaxLength(2000).IsRequired();
            entity.HasIndex(author => new { author.OntologyId, author.AuthorName }).IsUnique();
            entity.HasOne(author => author.Ontology)
                .WithMany(ontology => ontology.AuthorPortfolios)
                .HasForeignKey(author => author.OntologyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OntologyBaseDocument>(entity =>
        {
            entity.ToTable("ontology_base_documents");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.Id).HasColumnName("id");
            entity.Property(document => document.OntologyId).HasColumnName("ontology_id");
            entity.Property(document => document.Link).HasColumnName("link").HasMaxLength(2000).IsRequired();
            entity.Property(document => document.Description).HasColumnName("description").IsRequired();
            entity.HasIndex(document => new { document.OntologyId, document.Link }).IsUnique();
            entity.HasOne(document => document.Ontology)
                .WithMany(ontology => ontology.BaseDocuments)
                .HasForeignKey(document => document.OntologyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
