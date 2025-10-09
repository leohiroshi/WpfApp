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

        public string Iniciais
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nome))
                    return "??";

                var palavras = Nome.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (palavras.Length >= 2)
                {
                    return $"{palavras[0][0]}{palavras[1][0]}".ToUpper();
                }
                else if (palavras.Length == 1 && palavras[0].Length >= 2)
                {
                    return palavras[0].Substring(0, 2).ToUpper();
                }
                else if (palavras.Length == 1 && palavras[0].Length == 1)
                {
                    return palavras[0].ToUpper();
                }
                return "??";
            }
        }
    }
}
