namespace n2_laboratorio_api.DTOs;

public class ConsultaCreateDTO
{
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }
    public DateTime DataHora { get; set; }
}

public class ConsultaReadDTO
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public string NomePaciente { get; set; } = string.Empty;
    public int MedicoId { get; set; }
    public string NomeMedico { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
}

public class ConsultaUpdateDTO
{
    public int? PacienteId { get; set; }
    public int? MedicoId { get; set; }
    public DateTime? DataHora { get; set; }
}