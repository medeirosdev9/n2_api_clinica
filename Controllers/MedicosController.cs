using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n2_laboratorio_api.Data;
using n2_laboratorio_api.DTOs;
using n2_laboratorio_api.Helpers;
using n2_laboratorio_api.Mappers;

namespace n2_laboratorio_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicosController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicoReadDTO>>> GetMedicos()
    {
        var medicos = await _context.Medicos.ToListAsync();
        return Ok(medicos.Select(MedicoMapper.ToReadDTO));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MedicoReadDTO>> GetMedico(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) return NotFound("Médico não encontrado.");

        return Ok(MedicoMapper.ToReadDTO(medico));
    }

    [HttpPost]
    public async Task<ActionResult<MedicoReadDTO>> PostMedico(MedicoCreateDTO dto)
    {
        if (!Validacoes.Email(dto.Email)) return BadRequest(Validacoes.ErroEmail);
        if (!Validacoes.Telefone(dto.Telefone)) return BadRequest(Validacoes.ErroTelefone);

        if (await _context.Medicos.AnyAsync(m => m.CRM == dto.CRM))
            return BadRequest("Já existe um médico cadastrado com este CRM.");

        var medico = MedicoMapper.ToEntity(dto);

        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMedico), new { id = medico.Id }, MedicoMapper.ToReadDTO(medico));
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<MedicoReadDTO>> PatchMedico(int id, MedicoUpdateDTO dto)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) return NotFound("Médico não encontrado.");

        if (dto.Nome != null && string.IsNullOrWhiteSpace(dto.Nome)) return BadRequest("Nome não pode ser vazio.");
        if (dto.CRM != null && string.IsNullOrWhiteSpace(dto.CRM)) return BadRequest("CRM não pode ser vazio.");
        if (dto.Email != null && !Validacoes.Email(dto.Email)) return BadRequest(Validacoes.ErroEmail);
        if (dto.Telefone != null && !Validacoes.Telefone(dto.Telefone)) return BadRequest(Validacoes.ErroTelefone);

        if (dto.CRM != null && await _context.Medicos.AnyAsync(m => m.CRM == dto.CRM && m.Id != id))
            return BadRequest("Já existe um médico cadastrado com este CRM.");

        medico.Nome = dto.Nome ?? medico.Nome;
        medico.Email = dto.Email ?? medico.Email;
        medico.Telefone = dto.Telefone ?? medico.Telefone;
        medico.CRM = dto.CRM ?? medico.CRM;

        await _context.SaveChangesAsync();
        return Ok(MedicoMapper.ToReadDTO(medico));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedico(int id)
    {
        var medico = await _context.Medicos
            .Include(m => m.Consultas)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medico == null) return NotFound("Médico não encontrado.");

        if (medico.Consultas.Any(c => c.DataHora > DateTime.Now))
            return BadRequest("Não é possível remover médico com consultas futuras agendadas.");

        // Remove o histórico de consultas passadas junto com o médico
        _context.Consultas.RemoveRange(medico.Consultas);
        _context.Medicos.Remove(medico);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
