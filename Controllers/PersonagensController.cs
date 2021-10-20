using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RpgApi.Data;
using RpgApi.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RpgApi.Controllers
{
    [Authorize(Roles ="Jogador, Admin")]
    [ApiController]
    [Route("[controller]")]
    public class PersonagensController : ControllerBase
    {
        private readonly DataContext _context;//Declaração
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PersonagensController(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context; //inicializaçao do atributo
            _httpContextAccessor = httpContextAccessor; //inicializar atributo

        }

        /*[HttpGet("{id}")] //Busca por id

        public async Task<IActionResult> GetSingle(int id)
        {
            try
            {
              Personagem p = await _context.Personagens
                
                                .FirstOrDefaultAsync(pBusca => pBusca.Id == id);
                return Ok(p);                                            //se executar direito vai nesse
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);                                              //se nao esse executar direito vai nesse
            }
        }*/

        [HttpGet("GetAll")]

        public async Task<IActionResult> Get()
        {
            try
            {
                 List<Personagem> lista = await _context.Personagens.ToListAsync();
                 return Ok(lista);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);  
              
            }
        }

        [HttpPost]

        public async Task<IActionResult> Add(Personagem novoPersonagem)
        {
            try
            {
                if(novoPersonagem.PontosVida > 100)
                {
                    throw new System.Exception("Pontos de vida não pode ser maior que 100");
                }
                
                //Mudança Desafio de hj 18/10
                // int usuarioId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                // novoPersonagem.Usuario = _context.Usuarios.FirstOrDefault(uBusca => uBusca.Id == usuarioId);

                //Mudança
                novoPersonagem.Usuario = _context.Usuarios.FirstOrDefault(uBusca => uBusca.Id == ObterUsuarioId());

                 await _context.Personagens.AddAsync(novoPersonagem);
                 await _context.SaveChangesAsync();

                 return Ok(novoPersonagem.Id);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }

        [HttpPut]

        public async Task<IActionResult> Update(Personagem novoPersonagem)
        {
            try
            {
                 if(novoPersonagem.PontosVida > 100)
                {
                    throw new System.Exception("Pontos de vida não pode ser maior que 100");
                }

                //Mudança Desafio de hj 18/10
                // int usuarioId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                // novoPersonagem.Usuario = _context.Usuarios.FirstOrDefault(uBusca => uBusca.Id == usuarioId);

                //Mudança
                novoPersonagem.Usuario = _context.Usuarios.FirstOrDefault(uBusca => uBusca.Id == ObterUsuarioId());

                _context.Personagens.Update(novoPersonagem);
                int linhaAfetadas = await  _context.SaveChangesAsync();

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
                 Personagem pRemover = await _context.Personagens
                        .FirstOrDefaultAsync(p => p.Id == id);

                _context.Personagens.Remove(pRemover);
                int linhaAfetadas = await _context.SaveChangesAsync();

                return Ok(linhaAfetadas);
            }
            catch (System.Exception ex)
            {
                 return BadRequest(ex.Message);  
            }
        }


        [HttpGet("{id}")]//buscar pelo id informado

         public async Task<IActionResult> GetSingle(int id)
         {
             try
             {
                  Personagem p = await _context.Personagens
                  .Include(u => u.Usuario)
                  .Include(ar => ar.Arma)
                  .Include(ph => ph.PersonagemHabilidades)
                  .ThenInclude(h => h.Habilidade)
                  .FirstOrDefaultAsync(pbusca => pbusca.Id == id);
                  return Ok(p);
             }
             catch (System.Exception ex)
             {
                  return BadRequest(ex.Message); 
             
             }
         }


         [HttpGet("GetByUser")]
         public async Task<IActionResult> GetByUserAsync()
         {
             try
             {
                int id = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

                List<Personagem> lista = await _context.Personagens
                    .Where(u => u.Usuario.Id == id).ToListAsync();

                return Ok(lista);

             }
             catch (System.Exception ex)
             {
                 return BadRequest(ex.Message);
             }
         }

        private int ObterUsuarioId(){
            return int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        
        //Metodo
        private string ObterPerfilUsuario()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
        }        

        [HttpGet("GetByPerfil")]

        public async Task<IActionResult> GetByPerfilAsync()
        {
            try
            {
                 List<Personagem> lista = new List<Personagem>();

                 if(ObterPerfilUsuario() == "Admin")
                    lista = await _context.Personagens.ToListAsync();
                else
                {
                    lista = await _context.Personagens
                                .Where(p => p.Usuario.Id == ObterUsuarioId()).ToListAsync();
                }
                return Ok(lista);

            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }


    }
}