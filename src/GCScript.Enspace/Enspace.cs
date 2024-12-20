using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace GCScript.Enspace;
public class Enspace {
	private readonly string _username;
	private readonly string _password;
	private readonly string _baseUrl;

	private string _token = string.Empty;
	private DateTime _tokenExpiryTime = DateTime.MinValue;

	private HttpClient _httpClient;

	public Enspace(string username, string password, string baseUrl = "https://api.enspace.io") {
		if (string.IsNullOrWhiteSpace(username)) {
			throw new ArgumentException("Username cannot be null or empty.", nameof(username));
		}

		if (string.IsNullOrWhiteSpace(password)) {
			throw new ArgumentException("Password cannot be null or empty.", nameof(password));
		}

		if (string.IsNullOrWhiteSpace(baseUrl)) {
			throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
		}

		_username = username;
		_password = password;
		_baseUrl = baseUrl.TrimEnd('/');
		_httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl), };
		_httpClient.DefaultRequestHeaders.Add("Enl-Token", "enspace4c4c");
	}

	private async Task<string> GetTokenAsync() {
		if (string.IsNullOrWhiteSpace(_token) || DateTime.UtcNow >= _tokenExpiryTime) {
			await RefreshTokenAsync().ConfigureAwait(false);
		}
		return _token;
	}

	private async Task RefreshTokenAsync() {
		var enAuthRequest = new EnAuthRequest { identifier = _username, password = _password };
		var response = await _httpClient.PostAsync("/auth/local", new StringContent(JsonSerializer.Serialize(enAuthRequest), Encoding.UTF8, "application/json"));
		response.EnsureSuccessStatusCode();
		var result = await response.Content.ReadFromJsonAsync<EnAuthResponse>().ConfigureAwait(false);
		if (result is null || string.IsNullOrWhiteSpace(result.jwt)) { throw new Exception("Failed to authenticate with Enspace"); }

		_token = result.jwt;
		_tokenExpiryTime = DateTime.UtcNow.AddHours(1);
		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
	}

	public async Task<TResponse?> GetAsync<TResponse>(string endpoint, bool throwException = true) {
		endpoint = endpoint.TrimStart('/');
		await GetTokenAsync();
		var response = await _httpClient.GetAsync($"/{endpoint}");
		if (throwException) { response.EnsureSuccessStatusCode(); }
		return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>() : default;
	}

	public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest model, bool throwException = true) {
		endpoint = endpoint.TrimStart('/');
		await GetTokenAsync();
		var response = await _httpClient.PostAsync($"/{endpoint}", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
		if (throwException) { response.EnsureSuccessStatusCode(); }
		return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>() : default;
	}

	public async Task<string?> PostAsync<TRequest>(string endpoint, TRequest model, bool throwException = true) {
		endpoint = endpoint.TrimStart('/');
		await GetTokenAsync();
		var response = await _httpClient.PostAsync($"/{endpoint}", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
		if (throwException) { response.EnsureSuccessStatusCode(); }
		return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : default;
	}

	public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest model, bool throwException = true) {
		endpoint = endpoint.TrimStart('/');
		await GetTokenAsync();
		var response = await _httpClient.PutAsync($"/{endpoint}", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
		if (throwException) { response.EnsureSuccessStatusCode(); }
		return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>() : default;
	}

	public async Task<string?> PutAsync<TRequest>(string endpoint, TRequest model, bool throwException = true) {
		endpoint = endpoint.TrimStart('/');
		await GetTokenAsync();
		var response = await _httpClient.PutAsync($"/{endpoint}", new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json"));
		if (throwException) { response.EnsureSuccessStatusCode(); }
		return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : default;
	}

	public async Task<TResponse?> FileUploadAsync<TResponse>(string endpoint, List<string> filePathList, string path, TimeSpan timeout, bool throwException = true) {
		if (filePathList.Count == 0) { throw new Exception("No files to upload"); }

		using var multipartContent = new MultipartFormDataContent {
			{ new StringContent(path), "path" }
		};

		foreach (var filePath in filePathList) {
			if (!File.Exists(filePath)) { throw new Exception($"File not found: {filePath}"); }

			var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
			fileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.MimeUtility.GetMimeMapping(filePath));
			multipartContent.Add(content: fileContent, name: "files", fileName: Path.GetFileName(filePath));
		}

		_httpClient.Timeout = timeout;
		endpoint = endpoint.TrimStart('/');
		await GetTokenAsync();
		var response = await _httpClient.PostAsync($"/{endpoint}", multipartContent);
		if (throwException) { response.EnsureSuccessStatusCode(); }
		var responseString = await response.Content.ReadAsStringAsync();

		return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>() : default;
	}
}
file class EnAuthResponse { public string? jwt { get; set; } }
file class EnAuthRequest {
	public string identifier { get; set; } = "";
	public string password { get; set; } = "";
}
