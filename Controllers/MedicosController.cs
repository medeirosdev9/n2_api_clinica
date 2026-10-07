using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n2_laboratorio_api.Data;
using n2_laboratorio_api.DTOs;
using n2_laboratorio_api.Mappers;
using n2_laboratorio_api.Models;
using System.Text.RegularExpressions;

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
        var erro = ValidarCampos(dto.Email, dto.Telefone);
        if (erro != null) return BadRequest(erro);

        var medico = MedicoMapper.ToEntity(dto);

        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMedico), new { id = medico.Id }, MedicoMapper.ToReadDTO(medico));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchMedico(int id, MedicoUpdateDTO dto)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null) return NotFound("Médico não encontrado.");

        if (dto.Email != null && !ValidarEmail(dto.Email))
            return BadRequest("E-mail em formato inválido.");

        if (dto.Telefone != null && !ValidarTelefone(dto.Telefone))
            return BadRequest("Telefone deve estar no formato (47) 98888-7777.");

        if (dto.Nome != null) medico.Nome = dto.Nome;
        if (dto.Email != null) medico.Email = dto.Email;
        if (dto.Telefone != null) medico.Telefone = dto.Telefone;
        if (dto.CRM != null) medico.CRM = dto.CRM;

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

        var temConsultaFutura = medico.Consultas.Any(c => c.DataHora > DateTime.Now);
        if (temConsultaFutura)
            return BadRequest("Não é possível remover médico com consultas futuras agendadas.");

        _context.Medicos.Remove(medico);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // --- Métodos Auxiliares de Validação ---

    private static string? ValidarCampos(string email, string telefone)
    {
        if (!ValidarEmail(email)) return "E-mail em formato inválido.";
        if (!ValidarTelefone(telefone)) return "Telefone deve estar no formato (47) 98888-7777.";
        return null;
    }

    private static bool ValidarEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private static bool ValidarTelefone(string telefone)
    {
        return Regex.IsMatch(telefone, @"^\(\d{2}\)\s\d{5}-\d{4}$");
    }
}