namespace n2_laboratorio_api.Models;

public class Medico
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string CRM { get; set; } = string.Empty;

    // Relacionamento com Consultas
    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
}