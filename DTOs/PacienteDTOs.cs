namespace n2_laboratorio_api.DTOs;

public class PacienteCreateDTO
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataNasc { get; set; }
    public string Cpf { get; set; } = string.Empty;
}

public class PacienteReadDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public DateTime DataNasc { get; set; }
    public string Cpf { get; set; } = string.Empty;
}

public class PacienteUpdateDTO
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNasc { get; set; }
}