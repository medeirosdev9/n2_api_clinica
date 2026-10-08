using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n2_laboratorio_api.Data;
using n2_laboratorio_api.DTOs;
using n2_laboratorio_api.Helpers;
using n2_laboratorio_api.Mappers;

namespace n2_laboratorio_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PacientesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PacienteReadDTO>>> GetPacientes()
    {
        var pacientes = await _context.Pacientes.ToListAsync();
        return Ok(pacientes.Select(PacienteMapper.ToReadDTO));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PacienteReadDTO>> GetPaciente(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound("Paciente não encontrado.");

        return Ok(PacienteMapper.ToReadDTO(paciente));
    }

    [HttpPost]
    public async Task<ActionResult<PacienteReadDTO>> PostPaciente(PacienteCreateDTO dto)
    {
        if (!Validacoes.Email(dto.Email)) return BadRequest(Validacoes.ErroEmail);
        if (!Validacoes.Telefone(dto.Telefone)) return BadRequest(Validacoes.ErroTelefone);
        if (dto.DataNasc > DateTime.Now) return BadRequest("Data de nascimento não pode ser no futuro.");
        if (!Validacoes.Cpf(dto.Cpf)) return BadRequest("CPF inválido.");

        var paciente = PacienteMapper.ToEntity(dto);
        paciente.Cpf = Validacoes.ApenasNumeros(dto.Cpf);

        if (await _context.Pacientes.AnyAsync(p => p.Cpf == paciente.Cpf))
            return BadRequest("Já existe um paciente cadastrado com este CPF.");

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPaciente), new { id = paciente.Id }, PacienteMapper.ToReadDTO(paciente));
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<PacienteReadDTO>> PatchPaciente(int id, PacienteUpdateDTO dto)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound("Paciente não encontrado.");

        if (dto.Cpf != null) return BadRequest("O CPF não pode ser alterado.");
        if (dto.Nome != null && string.IsNullOrWhiteSpace(dto.Nome)) return BadRequest("Nome não pode ser vazio.");
        if (dto.Email != null && !Validacoes.Email(dto.Email)) return BadRequest(Validacoes.ErroEmail);
        if (dto.Telefone != null && !Validacoes.Telefone(dto.Telefone)) return BadRequest(Validacoes.ErroTelefone);
        if (dto.DataNasc > DateTime.Now) return BadRequest("Data de nascimento não pode ser no futuro.");

        paciente.Nome = dto.Nome ?? paciente.Nome;
        paciente.Email = dto.Email ?? paciente.Email;
        paciente.Telefone = dto.Telefone ?? paciente.Telefone;
        paciente.DataNasc = dto.DataNasc ?? paciente.DataNasc;

        await _context.SaveChangesAsync();
        return Ok(PacienteMapper.ToReadDTO(paciente));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePaciente(int id)
    {
        var paciente = await _context.Pacientes
            .Include(p => p.Consultas)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paciente == null) return NotFound("Paciente não encontrado.");

        if (paciente.Consultas.Any(c => c.DataHora > DateTime.Now))
            return BadRequest("Não é possível remover paciente com consultas futuras agendadas.");

        // Remove o histórico de consultas passadas junto com o paciente
        _context.Consultas.RemoveRange(paciente.Consultas);
        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
