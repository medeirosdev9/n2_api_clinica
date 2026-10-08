using System.ComponentModel.DataAnnotations;

namespace n2_laboratorio_api.DTOs;

public class MedicoCreateDTO
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Telefone { get; set; } = string.Empty;
    [Required] public string CRM { get; set; } = string.Empty;
}

public class MedicoReadDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string CRM { get; set; } = string.Empty;
}

public class MedicoUpdateDTO
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? CRM { get; set; }
}
