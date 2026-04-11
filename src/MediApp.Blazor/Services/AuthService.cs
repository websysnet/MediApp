using Microsoft.JSInterop;

namespace MediApp.Blazor.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ApiService _api;
    private readonly IJSRuntime _js;

    public AuthService(HttpClient http, ApiService api, IJSRuntime js)
    {
        _http = http;
        _api = api;
        _js = js;
    }

    public event Action? OnAuthStateChanged;

    private string? _token;
    private UsuarioDto? _currentUser;

    public string? CurrentToken => _token;

    public async Task<LoginResult?> LoginAsync(string email, string password)
    {
        var result = await _api.PostAsync<LoginResult>("/api/auth/login", new { email, password });
        
        if (result?.Token != null)
        {
            _token = result.Token;
            _currentUser = result.Usuario;
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            _api.SetToken(_token);
            await _js.InvokeVoidAsync("localStorage.setItem", "token", _token);
            OnAuthStateChanged?.Invoke();
        }
        return result;
    }

    public async Task RegisterAsync(string email, string password, string nombre, string apellido, string? telefono)
    {
        await _api.PostAsync<object>("/api/auth/register", new { email, password, nombre, apellido, telefono });
    }

    public async Task LogoutAsync()
    {
        _token = null;
        _currentUser = null;
        _api.SetToken(null);
        await _js.InvokeVoidAsync("localStorage.removeItem", "token");
        OnAuthStateChanged?.Invoke();
    }

    public async Task RestoreAsync()
    {
        try
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", "token");
            if (!string.IsNullOrEmpty(token))
            {
                _token = token;
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _api.SetToken(token);
            }
        }
        catch
        {
            // Ignorar errores de JS interop durante prerender
        }
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

    public async Task EnsureInitializedAsync()
    {
        if (_token == null)
        {
            try
            {
                var token = await _js.InvokeAsync<string?>("localStorage.getItem", "token");
                if (!string.IsNullOrEmpty(token))
                {
                    _token = token;
                    _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    _api.SetToken(token);
                }
            }
            catch
            {
                // Ignorar durante prerender
            }
        }
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