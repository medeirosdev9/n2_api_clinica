using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n2_laboratorio_api.Data;
using n2_laboratorio_api.DTOs;
using n2_laboratorio_api.Mappers;
using n2_laboratorio_api.Models;

namespace n2_laboratorio_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultasController : ControllerBase
{
    private const int DuracaoMinutos = 30;

    private readonly AppDbContext _context;

    public ConsultasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConsultaReadDTO>>> GetConsultas()
    {
        var consultas = await _context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .OrderBy(c => c.DataHora)
            .ToListAsync();

        return Ok(consultas.Select(ConsultaMapper.ToReadDTO));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConsultaReadDTO>> GetConsulta(int id)
    {
        var consulta = await _context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta == null) return NotFound("Consulta não encontrada.");

        return Ok(ConsultaMapper.ToReadDTO(consulta));
    }

    [HttpPost]
    public async Task<ActionResult<ConsultaReadDTO>> PostConsulta(ConsultaCreateDTO dto)
    {
        var consulta = ConsultaMapper.ToEntity(dto);

        var erro = await ValidarAsync(consulta);
        if (erro != null) return BadRequest(erro);

        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();
        await CarregarNavegacoesAsync(consulta);

        return CreatedAtAction(nameof(GetConsulta), new { id = consulta.Id }, ConsultaMapper.ToReadDTO(consulta));
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ConsultaReadDTO>> PatchConsulta(int id, ConsultaUpdateDTO dto)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) return NotFound("Consulta não encontrada.");

        consulta.PacienteId = dto.PacienteId ?? consulta.PacienteId;
        consulta.MedicoId = dto.MedicoId ?? consulta.MedicoId;
        consulta.DataHora = dto.DataHora ?? consulta.DataHora;

        var erro = await ValidarAsync(consulta);
        if (erro != null) return BadRequest(erro);

        await _context.SaveChangesAsync();
        await CarregarNavegacoesAsync(consulta);

        return Ok(ConsultaMapper.ToReadDTO(consulta));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConsulta(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) return NotFound("Consulta não encontrada.");

        _context.Consultas.Remove(consulta);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // --- Métodos Auxiliares ---

    private async Task<string?> ValidarAsync(Consulta consulta)
    {
        if (!await _context.Pacientes.AnyAsync(p => p.Id == consulta.PacienteId))
            return "Paciente não encontrado.";

        if (!await _context.Medicos.AnyAsync(m => m.Id == consulta.MedicoId))
            return "Médico não encontrado.";

        if (consulta.DataHora < DateTime.Now)
            return "Não é possível agendar consultas no passado.";

        // Duas consultas de 30 min se sobrepõem se a distância entre os inícios for menor que 30 min
        var inicio = consulta.DataHora.AddMinutes(-DuracaoMinutos);
        var fim = consulta.DataHora.AddMinutes(DuracaoMinutos);
        var conflitos = _context.Consultas
            .Where(c => c.Id != consulta.Id && c.DataHora > inicio && c.DataHora < fim);

        if (await conflitos.AnyAsync(c => c.MedicoId == consulta.MedicoId))
            return "O médico já possui uma consulta agendada que conflita com este horário.";

        if (await conflitos.AnyAsync(c => c.PacienteId == consulta.PacienteId))
            return "O paciente já possui uma consulta agendada que conflita com este horário.";

        return null;
    }

    private async Task CarregarNavegacoesAsync(Consulta consulta)
    {
        await _context.Entry(consulta).Reference(c => c.Paciente).LoadAsync();
        await _context.Entry(consulta).Reference(c => c.Medico).LoadAsync();
    }
}
