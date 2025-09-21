using Microsoft.AspNetCore.Mvc;

namespace Finanze.Web.Models
{
    public class LoginModel
    {
        [FromForm(Name = "code")]
        public string? Code { get; set; }
    }
}
