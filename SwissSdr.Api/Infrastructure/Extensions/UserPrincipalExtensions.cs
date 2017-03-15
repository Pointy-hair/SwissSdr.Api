using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public static class UserPrincipalExtensions
    {
		public static string GetSubject(this ClaimsPrincipal principal)
		{
			var subClaim = principal.FindFirst("sub");
			if (subClaim == null)
			{
				throw new InvalidOperationException("Can not find sub claim on principal");
			}
			return subClaim.Value;
		}
    }
}
