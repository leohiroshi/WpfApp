using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Services.Validators
{
    public static class CpfValidator
    {
        public static bool IsValid(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;
            var digits = new string(cpf.Where(char.IsDigit).ToArray());
            if (digits.Length != 11) return false;
            if (new string(digits[0], 11) == digits) return false;

            int Calc(ReadOnlySpan<char> s, int len)
            {
                int sum = 0, weight = len + 1;
                for (int i = 0; i < len; i++) sum += (s[i] - '0') * (weight - i);
                var mod = sum % 11;
                return mod < 2 ? 0 : 11 - mod;
            }

            var d1 = Calc(digits.AsSpan(), 9);
            var d2 = Calc(digits.AsSpan(), 10);
            return d1 == (digits[9] - '0') && d2 == (digits[10] - '0');
        }
    }
}
