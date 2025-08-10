using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using ECARTemplate.Data;
using ECARTemplate.Models;

namespace ECARTemplate.Extensions
{
    public static class IdentityExtensions
    {
        [DebuggerStepThrough]
        private static bool HasRole(this ClaimsPrincipal principal, params string[] roles)
        {
            /*if (principal == null)
                return default;

            //var claims = principal.FindAll(ClaimTypes.GroupSid).Select(x => x.Value).ToSafeList();
            //var user = (WindowsIdentity)principal.Identity;
            //var claims= user.Groups.Select(x => x.Translate(typeof(NTAccount)).ToString()).ToSafeList();

            //return claims?.Any() == true && claims.Intersect(roles ?? new string[] { }).Any();
            bool check = false;
            var user = (WindowsIdentity)principal.Identity;
            if (user.Groups != null)
            {
                foreach (var role in roles)
                {
                    foreach (var group in user.Groups)
                    {
                        check = group.Translate(typeof(NTAccount)).ToString().Contains(role);
                        if (check)
                            break;
                    }
                }
            }
            return check;
            */
            return true;
        }

        [DebuggerStepThrough]
        /*public static IEnumerable<ListItem> AuthorizeFor(this IEnumerable<ListItem> source, ClaimsPrincipal identity)
            => source.Where(x => x.Roles.IsNullOrEmpty() || (x.Roles.HasItems() && identity.HasRole(x.Roles))).ToSafeList();
        */
        public static IEnumerable<ListItem> AuthorizeFor(this IEnumerable<ListItem> source, ClaimsPrincipal identity)
        {
            return source.ToSafeList();
        }

        [DebuggerStepThrough]
        public static string FullName(this ClaimsPrincipal principal)
        {
            /*
            string fullName = null;
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(context, principal.Identity.Name))
                {
                    if (user != null)
                    {
                        fullName = user.DisplayName;
                    }
                }
            }

            return fullName;
            */
            return "Usuario Prueba";
        }

        [DebuggerStepThrough]
        public static string Email(this ClaimsPrincipal principal)
        {
            /*
            string fullName = null;
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(context, principal.Identity.Name))
                {
                    if (user != null)
                    {
                        fullName = user.EmailAddress;
                    }
                }
            }

            return fullName;
            */

            return "prueba@ejemplo.com";
        }

        [DebuggerStepThrough]
        public static HtmlString AsRaw(this string value) => new HtmlString(value);

        [DebuggerStepThrough]
        public static string ToPage(this string href) => System.IO.Path.GetFileNameWithoutExtension(href)?.ToLower();

        [DebuggerStepThrough]
        public static bool IsVoid(this string href) => href?.ToLower() == NavigationModel.Void;

        [DebuggerStepThrough]
        public static bool IsRelatedTo(this ListItem item, string pageName) => item?.Type == ItemType.Parent && item?.Href?.ToPage() == pageName?.ToLower();

        [DebuggerStepThrough]
        public static async Task<IdentityResult> UpdateAsync<T>(this ApplicationDbContext context, T model, string id) where T : class
        {
            var entity = await context.FindAsync<T>(id);

            if (entity == null)
            {
                return IdentityResult.Failed();
            }

            context.Entry((object)entity).CurrentValues.SetValues(model);

            await context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        [DebuggerStepThrough]
        public static async Task<IdentityResult> DeleteAsync<T>(this ApplicationDbContext context, string id) where T : class
        {
            var entity = await context.FindAsync<T>(id);

            if (entity == null)
            {
                return IdentityResult.Failed();
            }

            context.Remove((object)entity);

            await context.SaveChangesAsync();

            return IdentityResult.Success;
        }
    }
}
