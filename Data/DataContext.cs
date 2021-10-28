using System;
using Microsoft.EntityFrameworkCore;
using RpgApi.Models;
using RpgApi.Models.Enums;


namespace RpgApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){
        
        }

        public DbSet<Personagem> Personagens {get; set; }
        public DbSet<Arma> Armas {get; set; }

        public DbSet<Usuario> Usuarios {get; set;}
         
        public DbSet<Habilidade> Habilidades {get; set;}
        public DbSet<PersonagemHabilidade> PersonagemHabilidades {get; set;}
        public DbSet<Disputa> Disputas { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.Entity<Personagem>().HasData
            (
               
            new Personagem() { Id = 1 }, //Frodo Cavaleiro             
            new Personagem() { Id = 2, Nome = "Sam", PontosVida=100, Forca=15, Defesa=25, Inteligencia=30, Classe=ClasseEnum.Cavaleiro},     
            new Personagem() { Id = 3, Nome = "Galadriel", PontosVida=100, Forca=18, Defesa=21, Inteligencia=35, Classe=ClasseEnum.Clerigo },
            new Personagem() { Id = 4, Nome = "Gandalf", PontosVida=100, Forca=18, Defesa=18, Inteligencia=37, Classe=ClasseEnum.Mago },
            new Personagem() { Id = 5, Nome = "Hobbit", PontosVida=100, Forca=20, Defesa=17, Inteligencia=31, Classe=ClasseEnum.Cavaleiro },
            new Personagem() { Id = 6, Nome = "Celeborn", PontosVida=100, Forca=21, Defesa=13, Inteligencia=34, Classe=ClasseEnum.Clerigo },
            new Personagem() { Id = 7, Nome = "Radagast", PontosVida=100, Forca=25, Defesa=11, Inteligencia=35, Classe=ClasseEnum.Mago } 
            );

             modelBuilder.Entity<Arma>().HasData
            (
               
            new Arma() { Id = 1, Nome="Arma A", Dano=35, PersonagemId=1},             
            new Arma() { Id = 2, Nome ="Arma B", Dano=35, PersonagemId=2},     
            new Arma() { Id = 3, Nome ="Arma C", Dano=31, PersonagemId=3}
            
            );
             modelBuilder.Entity<PersonagemHabilidade>()
                .HasKey(ph => new NewClass(ph.PersonagemId, ph.HabilidadeId));
            
            modelBuilder.Entity<Habilidade>().HasData
            (
            new Habilidade() { Id = 1, Nome="Adormecer", Dano=39, },             
            new Habilidade() { Id = 2, Nome ="Congelar", Dano=41, },     
            new Habilidade() { Id = 3, Nome ="Hipnotizar", Dano=37,}
            
            );
        
            modelBuilder.Entity<Usuario>().Property(u => u.Perfil).HasDefaultValue("Jogador");

        }

    }

    internal class NewClass
    {
        public int PersonagemId { get; }
        public object HabilidadeId { get; }

        public NewClass(int personagemId, object habilidadeId)
        {
            PersonagemId = personagemId;
            HabilidadeId = habilidadeId;
        }

        public override bool Equals(object obj)
        {
            return obj is NewClass other &&
                   PersonagemId == other.PersonagemId &&
                   System.Collections.Generic.EqualityComparer<object>.Default.Equals(HabilidadeId, other.HabilidadeId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PersonagemId, HabilidadeId);
        }
    }
}