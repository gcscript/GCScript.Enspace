﻿using GCScript.Enspace.Enums;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GCScript.Enspace;
public class Enspace(EnMode mode, string username, string password) {
	private string _baseUrl = mode switch {
		EnMode.Production => "https://api.enspace.io",
		EnMode.Development => "https://api.stage.enspace.io",
		_ => throw new Exception("Invalid EnMode")
	};

	public async Task<string> GetToken() {
		var enAuthRequest = new EnAuthRequest { identifier = username, password = password };
		var result = await EnPostAsJsonAsync<EnAuthRequest, EnAuthResponse>("/auth/local", enAuthRequest, "");
		if (result is null || string.IsNullOrWhiteSpace(result.jwt)) { throw new Exception("Failed to authenticate with Enspace"); }
		return result.jwt;
	}

	public async Task<TResponse?> EnGetAsync<TResponse>(string endpoint, string token) {
		using var client = new HttpClient();
		client.BaseAddress = new Uri(_baseUrl);
		if (!string.IsNullOrWhiteSpace(token)) { client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); }
		client.DefaultRequestHeaders.Add("Enl-Token", "enspace4c4c");

		endpoint = endpoint.TrimStart('/');
		var response = await client.GetAsync($"/{endpoint}");

		if (response.IsSuccessStatusCode) {
			return await response.Content.ReadFromJsonAsync<TResponse>();
		}
		else {
			return default;
		}
	}

	public async Task<TResponse?> EnPostAsJsonAsync<TRequest, TResponse>(string endpoint, TRequest model, string token) {
		using var client = new HttpClient();
		client.BaseAddress = new Uri(_baseUrl);
		if (!string.IsNullOrWhiteSpace(token)) { client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); }
		client.DefaultRequestHeaders.Add("Enl-Token", "enspace4c4c");

		endpoint = endpoint.TrimStart('/');
		var response = await client.PostAsJsonAsync($"/{endpoint}", model);

		return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>() : default;
	}

	public async Task<TResponse?> EnFileUpload<TResponse>(string endpoint, List<string> filePathList, string path, TimeSpan timeout, string token) {
		if (filePathList.Count == 0) { throw new Exception("No files to upload"); }

		using var client = new HttpClient();
		client.BaseAddress = new Uri(_baseUrl);
		client.Timeout = timeout;
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		using var multipartContent = new MultipartFormDataContent {
			{ new StringContent(path), "path" }
		};

		foreach (var filePath in filePathList) {
			if (!File.Exists(filePath)) { throw new Exception($"File not found: {filePath}"); }

			var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
			fileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.MimeUtility.GetMimeMapping(filePath));
			multipartContent.Add(content: fileContent, name: "files", fileName: Path.GetFileName(filePath));
		}

		endpoint = endpoint.TrimStart('/');
		var response = await client.PostAsync($"/{endpoint}", multipartContent);
		var responseString = await response.Content.ReadAsStringAsync();

		return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>() : default;

	}
}
file class EnAuthResponse { public string? jwt { get; set; } }
file class EnAuthRequest {
	public string identifier { get; set; } = "";
	public string password { get; set; } = "";
}
