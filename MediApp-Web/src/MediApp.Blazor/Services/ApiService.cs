namespace MediApp.Blazor.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private string? _token;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public void SetToken(string? token)
    {
        _token = token;
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint) =>
        await _http.GetFromJsonAsync<T>(endpoint);

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        var response = await _http.PostAsJsonAsync(endpoint, data);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        var response = await _http.PutAsJsonAsync(endpoint, data);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task DeleteAsync(string endpoint) =>
        await _http.DeleteAsync(endpoint);
}

public class DashboardDto
{
    public int TotalPacientes { get; set; }
    public int TotalDoctores { get; set; }
    public int TotalCitas { get; set; }
    public int CitasPendientes { get; set; }
    public int CitasCompletadas { get; set; }
    public List<DoctorStatsDto>? Doctores { get; set; }
}

public class DoctorStatsDto
{
    public int Id { get; set; }
    public string Especialidad { get; set; } = "";
    public bool Activo { get; set; }
    public UsuarioInfoDto? Usuario { get; set; }
}

public class UsuarioInfoDto
{
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
}

public class PacienteDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}

public class PacienteDoctorDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public int TotalCitas { get; set; }
}

public class NewPacienteRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
}