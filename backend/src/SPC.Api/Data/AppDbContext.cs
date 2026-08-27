using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SPC.Api.Data.Entities;
using SPC.Core.Models;

namespace SPC.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();

    public DbSet<RecipeEntity> Recipes => Set<RecipeEntity>();

    public DbSet<IngredientEntity> Ingredients => Set<IngredientEntity>();

    public DbSet<ProfileEntity> Profiles => Set<ProfileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NormalizedUsername).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(128);
            entity.Property(e => e.NormalizedUsername).HasMaxLength(128);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<RecipeEntity>(entity =>
        {
            entity.ToTable("recipes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.FamilyId });
            entity.HasIndex(e => new { e.AccountId, e.UpdatedAt });
            entity.Property(e => e.Name).HasMaxLength(512);
            entity.Property(e => e.VariantLabel).HasMaxLength(256);
            entity.Property(e => e.ActualDishWeightG).HasPrecision(12, 4);
            ConfigureJsonb(entity.Property(e => e.Ingredients));
            ConfigureJsonb(entity.Property(e => e.Spices));
            ConfigureJsonb(entity.Property(e => e.Instructions));
            ConfigureJsonb(entity.Property(e => e.Notes));
        });

        modelBuilder.Entity<IngredientEntity>(entity =>
        {
            entity.ToTable("ingredients");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.NormalizedName }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(512);
            entity.Property(e => e.NormalizedName).HasMaxLength(512);
            entity.Property(e => e.CaloriesPer100g).HasPrecision(12, 4);
        });

        modelBuilder.Entity<ProfileEntity>(entity =>
        {
            entity.ToTable("profiles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountId);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.WeightKg).HasPrecision(12, 4);
            entity.Property(e => e.HeightCm).HasPrecision(12, 4);
            entity.Property(e => e.CustomPal).HasPrecision(8, 4);
            ConfigureJsonb(entity.Property(e => e.MealSplit));
        });
    }

    private static void ConfigureJsonb<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<T> property)
        where T : class, new()
    {
        var converter = new ValueConverter<T, string>(
            value => JsonSerializer.Serialize(value, JsonOptions),
            json => JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T());

        var comparer = new ValueComparer<T>(
            (left, right) => JsonSerializer.Serialize(left, JsonOptions) == JsonSerializer.Serialize(right, JsonOptions),
            value => JsonSerializer.Serialize(value, JsonOptions).GetHashCode(StringComparison.Ordinal),
            value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions) ?? new T());

        property.HasConversion(converter).HasColumnType("jsonb").Metadata.SetValueComparer(comparer);
    }
}
