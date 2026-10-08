using System.ComponentModel.DataAnnotations;

namespace n2_laboratorio_api.DTOs;

public class PacienteCreateDTO
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Telefone { get; set; } = string.Empty;
    [Required] public DateTime? DataNasc { get; set; }
    [Required] public string Cpf { get; set; } = string.Empty;
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
    public string? Cpf { get; set; } // Só existe para rejeitar a alteração de CPF
}
