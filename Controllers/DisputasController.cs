using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisputasController : ControllerBase
    {
        //Construtores e métodos aqui.
        private readonly DataContext _context;

        public DisputasController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("Arma")]
        public async Task<IActionResult> AtaqueComArmaAsync(Disputa d)
        {     
            //Programação do método
            try
            {
                 Personagem atacante = await _context.Personagens.Include(p => p.Arma)
                    .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);

                 Personagem oponente = await _context.Personagens.FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                int dano = atacante.Arma.Dano + (new Random().Next(atacante.Forca));
                dano = dano - new Random().Next(oponente.Defesa);
                if(dano > 0){
                    oponente.PontosVida = oponente.PontosVida - dano;
                }
                if(oponente.PontosVida <= 0){   
                    d.Narracao = $"{oponente.Nome} foi derrotado!";
                }
                _context.Personagens.Update(oponente);
                await _context.SaveChangesAsync();

                StringBuilder dados = new StringBuilder();
                dados.AppendFormat(" Atacante: {0}. ", atacante.Nome);
                dados.AppendFormat(" Oponente: {0}. ", oponente.Nome);
                dados.AppendFormat(" Pontos de Vida do atacante: {0}. ", atacante.PontosVida);
                dados.AppendFormat(" Pontos de Vida do oponente: {0}. ", oponente.PontosVida);
                dados.AppendFormat(" Dano: {0}. ", dano);

                d.Narracao += dados.ToString();
                d.DataDisputa = DateTime.Now;

                _context.Disputas.Add(d);
                _context.SaveChanges();

                return Ok(d);
            }
            catch (System.Exception ex) 
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("Habilidade")]
        public async Task<IActionResult> AtaqueComHabilidadeAsync(Disputa d){
            try
            {
                Personagem atacante = await _context.Personagens
                    .Include(p => p.PersonagemHabilidades)
                    .ThenInclude(ph => ph.Habilidade)
                    .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);
                 
                Personagem oponente = await _context.Personagens
                    .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                PersonagemHabilidade ph = await _context.PersonagemHabilidades
                    .Include(p => p.Habilidade)
                    .FirstOrDefaultAsync(phBusca => phBusca.HabilidadeId == d.HabilidadeId);
                    

                    if(ph == null){
                        d.Narracao = $"{atacante.Nome} não possui está habilidade";
                    }
                    else{
                            int dano = ph.Habilidade.Dano + (new Random().Next(atacante.Inteligencia));
                            dano = dano - new Random().Next(oponente.Defesa);

                        if(dano > 0){
                            //oponente.PontosVida = oponente.PontosVida - dano;
                            oponente.PontosVida -= dano;
                        }
                        if(oponente.PontosVida <= 0){

                            d.Narracao += $"{oponente.Nome} foi derrotado !";
                        }

                        _context.Personagens.Update(oponente);
                        await _context.SaveChangesAsync();

                            StringBuilder dados = new StringBuilder();
                            dados.AppendFormat(" Atacante: {0}. ", atacante.Nome);
                            dados.AppendFormat(" Oponente: {0}. ", oponente.Nome);
                            dados.AppendFormat(" Pontos de Vida do atacante: {0}. ", atacante.PontosVida);
                            dados.AppendFormat(" Pontos de Vida do oponente: {0}. ", oponente.PontosVida);
                            dados.AppendFormat(" Habilidade utilizada: {0}.", ph.Habilidade.Nome);
                            dados.AppendFormat(" Dano: {0}. ", dano);

                            d.Narracao += dados.ToString();
                            d.DataDisputa = DateTime.Now;

                            _context.Disputas.Add(d);
                            _context.SaveChanges();
                    }

                 return Ok(d);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Random
        [HttpGet("PersonagemRandom")]
        public async Task<IActionResult> Sorteio()
        {
            List<Personagem> personagens =
                 await _context.Personagens.ToListAsync();
            //Sorteio com numero da quantidade de personagens - 1
            int sorteio = new Random().Next(personagens.Count);
            //busca na lista pelo indice sorteado (Não é o ID)
            Personagem p = personagens[sorteio];
            string msg = 
                string.Format("Nº Sorteado {0}. Personagem: {1}", sorteio, p.Nome);
            return Ok(msg);
        }

    }
}