using Microsoft.AspNetCore.Authentication;
using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Threading.Tasks;

public class ActiveDirectoryClaimsTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var windowsIdentity = principal.Identity as ClaimsIdentity;

        if (windowsIdentity != null && windowsIdentity.IsAuthenticated)
        {
            try
            {
                // Crea el contexto de AD con tu dominio "ecar"
                using (var context = new PrincipalContext(ContextType.Domain, "ecar.local"))
                {
                    // Obtiene el nombre de usuario sin el dominio (e.g., "usuario" de "ecar\usuario")
                    string samAccountName = windowsIdentity.Name.Split('\\')[1];
                    var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);

                    if (user != null)
                    {
                        // TODO: Reemplaza "nombre-del-grupo-de-administradores-AD" con el nombre real de tu grupo de AD
                        // Verifica si el usuario es miembro del grupo de administradores
                        if (user.IsMemberOf(context, IdentityType.Name, "Ecar-Credenciales-Admins"))
                        {
                            windowsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                        }

                        // TODO: Reemplaza "nombre-del-grupo-de-usuarios-estandar-AD" con el nombre real de tu grupo de AD
                        // Verifica si el usuario es miembro del grupo de usuarios estándar
                        if (user.IsMemberOf(context, IdentityType.Name, "Ecar-Credenciales-Users"))
                        {
                            windowsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Estandar"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Aquí puedes registrar el error si la conexión con el AD falla,
                // para propósitos de depuración. Por ahora, el usuario simplemente no obtendrá roles.
                // _logger.LogError("Error al consultar el Directorio Activo: {Message}", ex.Message);
            }
        }

        return Task.FromResult(principal);
    }
}