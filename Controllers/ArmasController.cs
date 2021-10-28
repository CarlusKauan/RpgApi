using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArmasController : ControllerBase
    {
        private readonly DataContext _context;

        public ArmasController(DataContext context)
        {
            _context = context; 
        }

        [HttpGet("{id}")] 
        public async Task<IActionResult> GetSingle(int id)
        {
            try
            {
                 Arma p = await _context.Armas.FirstOrDefaultAsync(aBusca => aBusca.Id == id);
                 return Ok(p);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {
            try
            {
                 List<Arma> lista = await _context.Armas.ToListAsync();
                 return Ok(lista);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
 
       [HttpPost]
        public async Task<IActionResult> Add(Arma novaArma)
        {
            try
            {
                if (novaArma.Dano == 0)
                {
                    throw new System.Exception("Dano da arma não pode ser ser 0!");
                }

                Personagem personagem = await _context.Personagens
                              .FirstOrDefaultAsync(p => p.Id == novaArma.PersonagemId);

                if(personagem == null)
                {
                    throw new System.Exception("Seu usuário não contem personagens com Id do Personagem informado");
                }

                Arma buscaArma = await _context.Armas
                                .FirstOrDefaultAsync(a => a.PersonagemId == novaArma.PersonagemId);

                if(buscaArma != null)
                    throw new System.Exception("O personagem selecionado já contém uma arma atribuída a ele.");

                await _context.Armas.AddAsync(novaArma);
                await _context.SaveChangesAsync();

                return Ok(novaArma.Id);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut]
        public async Task<IActionResult> Update(Arma novoArma)
        {
            try
            {
                if(novoArma.Dano > 2000)
                {
                    throw new System.Exception("O Dano da arma não pode ser maior que 2000");
                }

                 _context.Armas.Update(novoArma);
                 int linhaAfetadas = await _context.SaveChangesAsync();

                 return Ok(linhaAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                 Arma pRemover = await _context.Armas
                    .FirstOrDefaultAsync(a => a.Id == id);
                
                _context.Armas.Remove(pRemover);
                int linhaAfetadas = await _context.SaveChangesAsync();

                return Ok(linhaAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        


    }
}