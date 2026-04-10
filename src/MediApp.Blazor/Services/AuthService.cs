namespace MediApp.Blazor.Services;

public class AuthService
{
    private readonly ApiService _api;

    public AuthService(ApiService api) => _api = api;

    public event Action? OnAuthStateChanged;

    private string? _token;
    private UsuarioDto? _currentUser;

    public async Task<LoginResult?> LoginAsync(string email, string password)
    {
        var http = new HttpClient { BaseAddress = new Uri("http://localhost:5004") };
        var tempApi = new ApiService(http);
        var result = await tempApi.PostAsync<LoginResult>("/api/auth/login", new { email, password });
        
        if (result?.Token != null)
        {
            _token = result.Token;
            _currentUser = result.Usuario;
            _api.SetToken(_token);
            OnAuthStateChanged?.Invoke();
        }
        return result;
    }

    public async Task RegisterAsync(string email, string password, string nombre, string apellido, string? telefono)
    {
        var http = new HttpClient { BaseAddress = new Uri("http://localhost:5004") };
        var tempApi = new ApiService(http);
        await tempApi.PostAsync<object>("/api/auth/register", new { email, password, nombre, apellido, telefono });
    }

    public async Task LogoutAsync()
    {
        _token = null;
        _currentUser = null;
        _api.SetToken(null);
        OnAuthStateChanged?.Invoke();
    }

    public async Task<UsuarioDto?> GetCurrentUserAsync()
    {
        if (_currentUser != null) return _currentUser;
        
        if (_token != null)
        {
            _api.SetToken(_token);
            _currentUser = await _api.GetAsync<UsuarioDto>("/api/auth/me");
        }
        return _currentUser;
    }

    public async Task<bool> IsLoggedInAsync()
    {
        return !string.IsNullOrEmpty(_token);
    }
}

public class LoginResult
{
    public string Token { get; set; } = "";
    public UsuarioDto Usuario { get; set; } = new();
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string? Telefono { get; set; }
    public int Rol { get; set; }
}