using n2_laboratorio_api.DTOs;
using n2_laboratorio_api.Models;

namespace n2_laboratorio_api.Mappers;

public static class PacienteMapper
{
    public static PacienteReadDTO ToReadDTO(Paciente paciente)
    {
        return new PacienteReadDTO
        {
            Id = paciente.Id,
            Nome = paciente.Nome,
            Email = paciente.Email,
            Telefone = paciente.Telefone,
            DataNasc = paciente.DataNasc,
            Cpf = paciente.Cpf
        };
    }

    public static Paciente ToEntity(PacienteCreateDTO dto)
    {
        return new Paciente
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            DataNasc = dto.DataNasc,
            Cpf = dto.Cpf
        };
    }
}