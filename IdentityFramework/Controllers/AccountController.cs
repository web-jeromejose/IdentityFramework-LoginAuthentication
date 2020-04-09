using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using IdentityFramework.Model;
using IdentityFramework.Data;

namespace IdentityFramework.Controllers
{
   // [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        #region simple Way Token
        //public IActionResult GetSampleToken()
        //{
        //    /* FYI
        //     * nuget using System.IdentityModel.Tokens.Jwt;
        //     * no other thing JUST TOKEN
        //     */
        //    var jwt = new JwtSecurityToken();
        //    var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        //    return Ok(token);

        //}

        #endregion

        #region Login Using Identity Model 


        readonly UserManager<IdentityUser> userManager;
        readonly SignInManager<IdentityUser> signInManager;
        private UserIdentityDbContext _context;


        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, UserIdentityDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this._context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Register([FromBody] User credentials)
        {
            var user = new IdentityUser { UserName = credentials.Email, Email = credentials.Email };


            try {

                var result = await userManager.CreateAsync(user, credentials.Password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);


                await signInManager.SignInAsync(user, isPersistent: false);
                  _context.User.Add(credentials);
                await _context.SaveChangesAsync();
                
                return Ok(CreateToken(user));

            } catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
         
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User credentials)
        {
            var result = await signInManager.PasswordSignInAsync(credentials.Email, credentials.Password, false, false);

            if (!result.Succeeded)
                return BadRequest();


            var user = await userManager.FindByEmailAsync(credentials.Email);

            return Ok(CreateToken(user));
        }


        string CreateToken(IdentityUser user)
        {
            var claims = new Claim[]
           {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id)
           };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is the secret phrase"));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(signingCredentials: signingCredentials, claims: claims);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }


        #endregion

    }
  
}