    using Microsoft.AspNetCore.Identity;

    namespace MultiAccountBankAPI.Models
    {
        public class ApplicationUser : IdentityUser
        {
            public string name { get; set; } // Campo extra para armazenar o nome do usuário
        }
    }
