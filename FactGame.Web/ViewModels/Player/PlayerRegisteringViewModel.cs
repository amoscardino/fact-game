using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.ViewModels
{
    public class PlayerRegisteringViewModel
    {
        [Required]
        public string GameID { get; set; }

        public string PlayerID { get; set; }

        [Required, Display(Name = "Name")]
        public string Name { get; set; }

        [Required, Display(Name = "Symbol")]
        public string Symbol { get; set; }

        [Required, Display(Name = "Color Code")]
        [RegularExpression("^(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "The Color Code field must be a valid hexadecimal color code (with 3 or 6 digits).")]
        public string ColorCode { get; set; }

        [Required, Display(Name = "Fact"), MaxLength(140)]
        public string Fact { get; set; }

        public string GameName { get; set; }

        public PlayerRegisteringViewModel()
        {
            var random = new Random();

            ColorCode = string.Format("{0:X6}", random.Next(0x1000000));
            Symbol = "fas fa-flag";
        }
    }
}
