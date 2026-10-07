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
        // 1. Validar se o Paciente existe
        if (!await _context.Pacientes.AnyAsync(p => p.Id == dto.PacienteId))
            return BadRequest("Paciente não encontrado.");

        // 2. Validar se o Médico existe
        if (!await _context.Medicos.AnyAsync(m => m.Id == dto.MedicoId))
            return BadRequest("Médico não encontrado.");

        // 3. Validar se a data/hora é no futuro
        if (dto.DataHora < DateTime.Now)
            return BadRequest("Não é possível agendar consultas no passado.");

        // 4. Validar sobreposição para o Médico
        if (await TemSobreposicaoMedicoAsync(dto.MedicoId, dto.DataHora))
            return BadRequest("O médico já possui uma consulta agendada que conflita com este horário.");

        // 5. Validar sobreposição para o Paciente
        if (await TemSobreposicaoPacienteAsync(dto.PacienteId, dto.DataHora))
            return BadRequest("O paciente já possui uma consulta agendada que conflita com este horário.");

        var consulta = ConsultaMapper.ToEntity(dto);

        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();

        // Recarrega as navegações para incluir no DTO de resposta
        await _context.Entry(consulta).Reference(c => c.Paciente).LoadAsync();
        await _context.Entry(consulta).Reference(c => c.Medico).LoadAsync();

        return CreatedAtAction(nameof(GetConsulta), new { id = consulta.Id }, ConsultaMapper.ToReadDTO(consulta));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchConsulta(int id, ConsultaUpdateDTO dto)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null) return NotFound("Consulta não encontrada.");

        int novoPacienteId = dto.PacienteId ?? consulta.PacienteId;
        int novoMedicoId = dto.MedicoId ?? consulta.MedicoId;
        DateTime novaDataHora = dto.DataHora ?? consulta.DataHora;

        // Validar se o Paciente existe (se foi alterado)
        if (dto.PacienteId.HasValue && !await _context.Pacientes.AnyAsync(p => p.Id == novoPacienteId))
            return BadRequest("Paciente não encontrado.");

        // Validar se o Médico existe (se foi alterado)
        if (dto.MedicoId.HasValue && !await _context.Medicos.AnyAsync(m => m.Id == novoMedicoId))
            return BadRequest("Médico não encontrado.");

        // Validar se a nova data/hora é no futuro
        if (novaDataHora < DateTime.Now)
            return BadRequest("Não é possível agendar consultas no passado.");

        // Validar sobreposição para o Médico (ignorando a própria consulta atual)
        if (await TemSobreposicaoMedicoAsync(novoMedicoId, novaDataHora, consulta.Id))
            return BadRequest("O médico já possui uma consulta agendada que conflita com este horário.");

        // Validar sobreposição para o Paciente (ignorando a própria consulta atual)
        if (await TemSobreposicaoPacienteAsync(novoPacienteId, novaDataHora, consulta.Id))
            return BadRequest("O paciente já possui uma consulta agendada que conflita com este horário.");

        // Atualizar os campos
        consulta.PacienteId = novoPacienteId;
        consulta.MedicoId = novoMedicoId;
        consulta.DataHora = novaDataHora;

        await _context.SaveChangesAsync();

        // Recarrega as navegações para o DTO de retorno
        await _context.Entry(consulta).Reference(c => c.Paciente).LoadAsync();
        await _context.Entry(consulta).Reference(c => c.Medico).LoadAsync();

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

    // --- Métodos Auxiliares para Validação de Sobreposição (Duração de 30 minutos) ---

    private async Task<bool> TemSobreposicaoMedicoAsync(int medicoId, DateTime inicioNova, int? consultaIdIgnorar = null)
    {
        DateTime fimNova = inicioNova.AddMinutes(30);

        return await _context.Consultas.AnyAsync(c =>
            (consultaIdIgnorar == null || c.Id != consultaIdIgnorar) &&
            c.MedicoId == medicoId &&
            c.DataHora < fimNova &&
            c.DataHora.AddMinutes(30) > inicioNova);
    }

    private async Task<bool> TemSobreposicaoPacienteAsync(int pacienteId, DateTime inicioNova, int? consultaIdIgnorar = null)
    {
        DateTime fimNova = inicioNova.AddMinutes(30);

        return await _context.Consultas.AnyAsync(c =>
            (consultaIdIgnorar == null || c.Id != consultaIdIgnorar) &&
            c.PacienteId == pacienteId &&
            c.DataHora < fimNova &&
            c.DataHora.AddMinutes(30) > inicioNova);
    }
}