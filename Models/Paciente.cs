namespace n2_laboratorio_api.Models;

public class Paciente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataNasc { get; set; }
    public string Cpf { get; set; } = string.Empty;

    // Relacionamento com Consultas (útil para validações de DELETE no EF Core)
    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
}