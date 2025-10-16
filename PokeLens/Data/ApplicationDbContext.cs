using Microsoft.EntityFrameworkCore;

namespace PokeLens.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Adicione suas DbSets aqui conforme for criando as models
        // Exemplo:
        // public DbSet<Pokemon> Pokemons { get; set; }
        // public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurações das entidades vão aqui
        }
    }
}