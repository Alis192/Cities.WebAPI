using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.ServiceContracts
{
    /// <summary>
    ///If an application user object is supplied based on the existing user details it has to automatically generate the JWT token and return the same as a part of this authentication response object which includes the person name, email, expiration date along with the token 
    /// </summary>
    public interface IJwtService
    {
        AuthenticationResponse CreateJwtToken(ApplicationUser user);

        ClaimsPrincipal? GetPrincipalFromJwtToken(string? token); 
    }
}
