using System.Text.RegularExpressions;

namespace n2_laboratorio_api.Helpers;

public static class Validacoes
{
    public const string ErroEmail = "E-mail em formato inválido.";
    public const string ErroTelefone = "Telefone deve estar no formato (47) 98888-7777.";

    public static bool Email(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static bool Telefone(string telefone) =>
        Regex.IsMatch(telefone, @"^\(\d{2}\)\s\d{5}-\d{4}$");

    public static string ApenasNumeros(string str) =>
        Regex.Replace(str, @"\D", "");

    public static bool Cpf(string cpf)
    {
        cpf = ApenasNumeros(cpf);
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;

        int Digito(int tamanho)
        {
            int soma = 0;
            for (int i = 0; i < tamanho; i++)
                soma += (cpf[i] - '0') * (tamanho + 1 - i);
            int resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        return Digito(9) == cpf[9] - '0' && Digito(10) == cpf[10] - '0';
    }
}
