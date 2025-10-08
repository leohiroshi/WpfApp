using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Models
{
    public class Pessoa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório")]
        public  string Cpf { get; set; } = string.Empty;

        public string? Endereco { get; set; }
    }
}
