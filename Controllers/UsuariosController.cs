using Microsoft.AspNetCore.Mvc;
using RpgApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RpgApi.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace RpgApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]

    public class UsuariosController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public UsuariosController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private void CriarPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public async Task<bool> UsuarioExistente(string username)
        {
            if(await _context.Usuarios.AnyAsync(x => x.Username.ToLower() == username.ToLower()))
            {
                return true;
            }


            return false;
        }

        [AllowAnonymous]
        [HttpPost("Registrar")]

        public async Task<IActionResult> RegistrarUsuario(Usuario user)
        {
            try
            {
               if(await UsuarioExistente(user.Username))  
               throw new System.Exception("Nome de usuário já existe");

               CriarPasswordHash(user.PasswordString, out byte[] hash, out byte[]  salt);
               user.PasswordString = string.Empty;
               user.PasswordHash = hash;
               user.PasswordSalt =salt;

               await _context.Usuarios.AddAsync(user);
               await _context.SaveChangesAsync();

               return Ok(user.Id);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool VerificarPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [AllowAnonymous]
        [HttpPost("Autenticar")]
        
        public async Task<IActionResult> AutenticarUsuario(Usuario credenciais)
        {
            try
            {
                 Usuario usuario = await _context.Usuarios
                    .FirstAsync(x => x.Username.ToLower().Equals(credenciais.Username.ToLower()));

                if(usuario == null)
                {
                    throw new System.Exception("Usuários não encontrado");
                }
                else if(!VerificarPasswordHash(credenciais.PasswordString, usuario.PasswordHash, usuario.PasswordSalt))
                {
                    throw new System.Exception("Senha incorreta");
                }
                else
                {
                    usuario.DataAcesso = System.DateTime.Now;
                    _context.Usuarios.Update(usuario);
                    await _context.SaveChangesAsync(); //Confirmar a alteração no banco
                    return Ok(CriarToken(usuario));
                }

            }
            catch (System.Exception ex)
            {
                 return BadRequest(ex.Message);
            }
        }
        [HttpPut("AlterarSenha")]

        public async Task<IActionResult> AlterarSenhaUsuario(Usuario credenciais)
        {
            try
            {
                 Usuario usuario = await _context.Usuarios //Busca o usuario no banco de dados atraves do loguin
                       .FirstOrDefaultAsync(x => x.Username.ToLower().Equals(credenciais.Username.ToLower()));

                if(usuario == null) //Se não achar nenhum usuario pelo logim, retorna mensagem
                    throw new System.Exception("Usuario não encontrado");


                CriarPasswordHash(credenciais.PasswordString, out byte[] hash, out byte[] salt);
                usuario.PasswordHash = hash; //Se o usuario existir, executa a criptografia(linha)
                usuario.PasswordSalt = salt; // guardando o hash e o salt nas propriedades do usuario

                _context.Usuarios.Update(usuario);
                int linhasAfetadas = await _context.SaveChangesAsync(); //Confirma a alteração no banco
                return Ok(linhasAfetadas); //Retorna as linhas afetadas (geralmente sempre 1 linha msm)
            }
            catch (System.Exception ex) 
            {
                
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAll")]

        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                 List<Usuario> lista = await _context.Usuarios.ToListAsync();
                 return Ok(lista);
            }
            catch (System.Exception ex)
            {
                
               return BadRequest(ex.Message);
            }
        }

        //continuidade

    private string CriarToken(Usuario usuario)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Username),
            new Claim(ClaimTypes.Role, usuario.Perfil)
        };

        //Gerar nosso Token !
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8
        .GetBytes(_configuration.GetSection("ConfiguracaoToken:Chave").Value));
        //Criptografia !
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            //Detalhes do token !
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creds
        };
        //Escreva o token no retorno
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }





    }
}