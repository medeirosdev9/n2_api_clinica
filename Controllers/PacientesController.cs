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
        var erro = ValidarCampos(dto.Email, dto.Telefone, dto.DataNasc, dto.Cpf);
        if (erro != null) return BadRequest(erro);

        var cpfLimpo = ApenasNumeros(dto.Cpf);
        if (await _context.Pacientes.AnyAsync(p => p.Cpf == cpfLimpo))
            return BadRequest("Já existe um paciente cadastrado com este CPF.");

        var paciente = PacienteMapper.ToEntity(dto);
        paciente.Cpf = cpfLimpo;

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPaciente), new { id = paciente.Id }, PacienteMapper.ToReadDTO(paciente));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchPaciente(int id, PacienteUpdateDTO dto)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null) return NotFound("Paciente não encontrado.");

        if (dto.Email != null && !ValidarEmail(dto.Email))
            return BadRequest("E-mail em formato inválido.");

        if (dto.Telefone != null && !ValidarTelefone(dto.Telefone))
            return BadRequest("Telefone deve estar no formato (47) 98888-7777.");

        if (dto.DataNasc.HasValue && dto.DataNasc.Value > DateTime.Now)
            return BadRequest("Data de nascimento não pode ser no futuro.");

        if (dto.Nome != null) paciente.Nome = dto.Nome;
        if (dto.Email != null) paciente.Email = dto.Email;
        if (dto.Telefone != null) paciente.Telefone = dto.Telefone;
        if (dto.DataNasc.HasValue) paciente.DataNasc = dto.DataNasc.Value;

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

        var temConsultaFutura = paciente.Consultas.Any(c => c.DataHora > DateTime.Now);
        if (temConsultaFutura)
            return BadRequest("Não é possível remover paciente com consultas futuras agendadas.");

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // --- Métodos Auxiliares de Validação ---

    private static string? ValidarCampos(string email, string telefone, DateTime dataNasc, string cpf)
    {
        if (!ValidarEmail(email)) return "E-mail em formato inválido.";
        if (!ValidarTelefone(telefone)) return "Telefone deve estar no formato (47) 98888-7777.";
        if (dataNasc > DateTime.Now) return "Data de nascimento não pode ser no futuro.";
        if (!ValidarCPF(cpf)) return "CPF numérico inválido.";
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

    private static string ApenasNumeros(string str)
    {
        return Regex.Replace(str, @"[^\d]", "");
    }

    private static bool ValidarCPF(string cpf)
    {
        cpf = ApenasNumeros(cpf);
        if (cpf.Length != 11 || new string(cpf[0], 11) == cpf) return false;

        int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        string digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito += resto.ToString();

        return cpf.EndsWith(digito);
    }
}