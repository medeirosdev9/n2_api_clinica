# ApiClinica — Trabalho N2 (API REST + ORM + DTO)

API REST em C# / .NET 10 com Entity Framework Core e SQLite para gerenciar pacientes, médicos e consultas.

## Como executar

```bash
dotnet run
```

A API sobe em `http://localhost:5150`. O banco `clinica.db` é criado automaticamente na primeira execução (migrations aplicadas no startup).

Exemplos de requisições estão em `n2_laboratorio_api.http` (podem ser usados como base no Postman).

## Estrutura

| Pasta | Conteúdo |
|---|---|
| `Models/` | `Paciente`, `Medico`, `Consulta` |
| `Data/` | `AppDbContext` (EF Core + SQLite) |
| `DTOs/` | `Create`, `Read` e `Update` DTO de cada entidade |
| `Mappers/` | Conversão manual entre entidades e DTOs |
| `Controllers/` | `PacientesController`, `MedicosController`, `ConsultasController` |
| `Helpers/` | Validações compartilhadas (e-mail, telefone, CPF) |
| `Migrations/` | Migrations do EF Core |

## Endpoints

Cada recurso possui `GET`, `GET /{id}`, `POST`, `PATCH /{id}` e `DELETE /{id}`:

- `/api/pacientes`
- `/api/medicos`
- `/api/consultas`

## Regras de negócio

**Pacientes**
- E-mail em formato válido e telefone no formato `(47) 98888-7777`.
- Data de nascimento não pode ser no futuro.
- CPF validado pelos dígitos verificadores e único no cadastro; não pode ser alterado no PATCH.

**Médicos**
- E-mail em formato válido e telefone no formato `(47) 98888-7777`.
- CRM único.

**Consultas**
- Paciente e médico precisam existir.
- Não é possível agendar no passado.
- Cada consulta dura 30 minutos; não pode haver sobreposição de horário para o mesmo médico nem para o mesmo paciente (ex.: 10:00 e 10:15 conflitam; 10:00 e 10:30 não).

**Gerais**
- No PATCH, campos enviados como `null` (ou omitidos) não são alterados.
- Não é possível remover paciente ou médico com consultas futuras. Ao remover, o histórico de consultas passadas é removido junto.
