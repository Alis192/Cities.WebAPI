using CitiesManager.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CitiesManager.Core.DTO;
using CitiesManager.Core.ServiceContracts;
using System.Security.Claims;

namespace CitiesManager.WebAPI.Controllers.v1
{
    /// <summary>
    /// 
    /// </summary>
    [AllowAnonymous]
    [ApiVersion("1.0")]
    public class AccountController : CustomControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager; //CREATE, DELETE, UPDATE, SEARCH, ADDING ROLE for user
        private readonly SignInManager<ApplicationUser> _signInManager; //Sing in or sign out a user 
        private readonly RoleManager<ApplicationRole> _roleManager; //Create and Delete roles
        private readonly IJwtService _jwtService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="roleManager"></param>
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApplicationUser>> PostRegister(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMessage);
            }

            // Create user
            ApplicationUser user = new ApplicationUser()
            {
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName
            };


            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                //sing-in
                await _signInManager.SignInAsync(user, isPersistent: false); //Auth cookie will be deleted as soon as browser is closed

                var authenticationResponse = _jwtService.CreateJwtToken(user);

                return Ok(authenticationResponse);
            }
            else
            {
                string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description)); //error1 | error2
                return Problem(errorMessage);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        //Method for remote validation
        [HttpGet]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Ok(true);
            }
            else
            {
                return Ok(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginDTO loginDTO)
        {
            //Validation
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMessage);
            }


            var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);

                if (user == null) //To satisfy compiler
                {
                    return NoContent();
                }

                //Sign-in   

                var authenticationResponse = _jwtService.CreateJwtToken(user); //Creating JWT token
                user.RefreshToken = authenticationResponse.RefreshToken; //authenticationResponse.RefreshToken represents a new refresh token that is generated as a product of the JWT service
                user.RefreshTokenExpirationDate = authenticationResponse.RefreshTokenExpirationDateTime;

                await _userManager.UpdateAsync(user); //Saving refresh token in ApplicationUser database

                return Ok(authenticationResponse);
            }
            else
            {
                return Problem("Invalid email or password");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        [HttpGet("logout")]
        public async Task<IActionResult> GetLogout(LoginDTO loginDTO)
        {
            await _signInManager.SignOutAsync();

            return NoContent();
        }


        [HttpPost("generate-new-jwt-token")]
        public async Task<IActionResult> GenerateNewAccessToken(TokenModel tokenModel)
        {
            if (tokenModel == null)
            {
                return BadRequest("Invalid client request");
            }

            // The JWT token should be read and verify the user identity 
            // Because in the payload of the JWT token already it contains the user ID

            ClaimsPrincipal? principal = _jwtService.GetPrincipalFromJwtToken(tokenModel.Token);



            if (principal == null)
            {
                return BadRequest("Invalid jwt access token");
            }

            string? email = principal.FindFirstValue(ClaimTypes.Email);

            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpirationDate <= DateTime.Now) //Checking refresh token with the one that stored in db
            {
                return BadRequest("Invalid refresh token");
            }

            //If Refresh token is not expired then new JWT token will be created
            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user); //Generates both JWT and Refresh token

            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpirationDate = authenticationResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(user); //Updating the user with the new values of refresh token

            return Ok(authenticationResponse);

        }
    }
}