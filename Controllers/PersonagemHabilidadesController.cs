using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;
using System.Linq;


namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonagemHabilidadesController : ControllerBase
    {
        private readonly DataContext _context;
        public PersonagemHabilidadesController(DataContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddPersonagemHabilidadeAsync(PersonagemHabilidade novoPersonagemHabilidade)
        {
            try
            {
                Personagem personagem = await _context.Personagens
                    .Include(p => p.Arma) // inclui a propriedade arma no objeto p
                    .Include(p => p.PersonagemHabilidades) // inclui lista de PersonagemHabilidade no objeto p
                    .ThenInclude(ps => ps.Habilidade)
                    .FirstOrDefaultAsync(p => p.Id == novoPersonagemHabilidade.PersonagemId);
                if (personagem == null)
                    throw new System.Exception("Personagem não encontrado para o Id Informado.");
                Habilidade habilidade = await _context.Habilidades
                                    .FirstOrDefaultAsync(h => h.Id == novoPersonagemHabilidade.HabilidadeId);
                if (habilidade == null)
                    throw new System.Exception("Habilidade não encontrada.");
                PersonagemHabilidade ph = new PersonagemHabilidade();
                ph.Personagem = personagem;
                ph.Habilidade = habilidade;
                await _context.PersonagemHabilidades.AddAsync(ph);
                int linhasAfetadas = await _context.SaveChangesAsync();
                return Ok(linhasAfetadas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Getall()
        {
            try
            {
                 List<PersonagemHabilidade> ph = await _context.PersonagemHabilidades.ToListAsync();
            return Ok(ph);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Buscar Personagem pelo {id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPersonagem(int id) //retorna lista de personagem com id
        {
            try
            {
                List<PersonagemHabilidade> phLista = new List<PersonagemHabilidade>(); 
                
                phLista = await _context.PersonagemHabilidades
                .Include(p => p.Personagem)
                .Include(p => p.Habilidade)
                 .Where(p => p.PersonagemId == id).ToListAsync();

                //Retorno da Lista de Personagens !
                return Ok(phLista); 
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Trazer Lista de Habilidades !
        [HttpGet("GetHabilidade")] 
        public async Task<IActionResult> GetHabilidade()
        {
            try
            {
                List<Habilidade> H = new List<Habilidade>();
                H = await _context.Habilidades.ToListAsync();
                
                return Ok(H);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("DeletePersonagemHabilidade")]
         public async Task<IActionResult> DeleteAsync(PersonagemHabilidade ph) //remover os dados da tabela 
        {
            try
            {
              PersonagemHabilidade phRemover = await _context.PersonagemHabilidades
                .FirstOrDefaultAsync(phBusca => phBusca.PersonagemId == ph.PersonagemId
                && phBusca.HabilidadeId == ph.HabilidadeId);
                
                if(phRemover == null)
                {
                    throw new System.Exception("Personagem o habilidade não encontrada!");
                }
                _context.PersonagemHabilidades.Remove(phRemover);
                int linhasAfetadas = await _context.SaveChangesAsync();
                return Ok(linhasAfetadas);
             }
            catch (System.Exception ex)
            { 
              return BadRequest(ex.Message);
            }
     }
        



    



    }
}