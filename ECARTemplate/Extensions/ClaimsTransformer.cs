using Microsoft.AspNetCore.Authentication;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq; // Agrega este using

namespace ECARTemplate.Extensions
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is WindowsIdentity wi && wi.IsAuthenticated)
            {
                if (wi.Groups != null)
                {
                    foreach (var group in wi.Groups) //-- Getting all the AD groups that user belongs to---
                    {
                        try
                        {
                            var claim = new Claim(wi.RoleClaimType, group.Value);
                            ((ClaimsIdentity)principal.Identity).AddClaim(claim); // Cast to ClaimsIdentity to add claims
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            else if (principal.Identity is ClaimsIdentity ci && ci.IsAuthenticated)
            {
                // Aquí puedes agregar lógica para extraer claims de tu ClaimsIdentity personalizada
                // Por ejemplo, si agregaste el RolUsuario como una claim durante el login:
                var rolUsuarioClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                if (rolUsuarioClaim != null)
                {
                    ci.AddClaim(new Claim(ClaimTypes.Role, rolUsuarioClaim.Value));
                }

                // Puedes agregar otras claims que necesites de tu identidad personalizada aquí
                var codigoUsuarioClaim = principal.Claims.FirstOrDefault(c => c.Type == "CodigoUsuario");
                if (codigoUsuarioClaim != null)
                {
                    ci.AddClaim(new Claim("CodigoUsuarioLogueado", codigoUsuarioClaim.Value));
                }
            }

            return Task.FromResult(principal);
        }
    }
}