using n2_laboratorio_api.DTOs;
using n2_laboratorio_api.Models;

namespace n2_laboratorio_api.Mappers;

public static class MedicoMapper
{
    public static MedicoReadDTO ToReadDTO(Medico medico)
    {
        return new MedicoReadDTO
        {
            Id = medico.Id,
            Nome = medico.Nome,
            Email = medico.Email,
            Telefone = medico.Telefone,
            CRM = medico.CRM
        };
    }

    public static Medico ToEntity(MedicoCreateDTO dto)
    {
        return new Medico
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            CRM = dto.CRM
        };
    }
}