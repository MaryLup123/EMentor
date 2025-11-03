using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace eduMentor.Models
{
    public class Role : IdentityRole<int>
    {
        public string Descripcion { get; set; }

    }
}
