using Microsoft.AspNetCore.Identity;

namespace Eshopping.Models
{
	public class AppUserModel: IdentityUser
	{
		public string Occupation {  get; set; }
		public string RoleId {  get; set; }
		public string Token {  get; set; }
	}
}
