    using Microsoft.AspNetCore.Identity;

    namespace MultiAccountBankAPI.Models
    {
        public class ApplicationUser : IdentityUser
        {
            public string Name { get; set; } // Campo extra para armazenar o nome do usuário
        }
    }
