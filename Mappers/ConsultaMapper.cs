using n2_laboratorio_api.DTOs;
using n2_laboratorio_api.Models;

namespace n2_laboratorio_api.Mappers;

public static class ConsultaMapper
{
    public static ConsultaReadDTO ToReadDTO(Consulta consulta)
    {
        return new ConsultaReadDTO
        {
            Id = consulta.Id,
            PacienteId = consulta.PacienteId,
            NomePaciente = consulta.Paciente?.Nome ?? string.Empty,
            MedicoId = consulta.MedicoId,
            NomeMedico = consulta.Medico?.Nome ?? string.Empty,
            DataHora = consulta.DataHora
        };
    }

    public static Consulta ToEntity(ConsultaCreateDTO dto)
    {
        return new Consulta
        {
            PacienteId = dto.PacienteId,
            MedicoId = dto.MedicoId,
            DataHora = dto.DataHora
        };
    }
}