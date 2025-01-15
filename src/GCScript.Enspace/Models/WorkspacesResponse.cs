using System.Text.Json.Serialization;

namespace GCScript.Enspace.Models;
public class WorkspacesResponse {
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("reference")]
	public string? Reference { get; set; }

	[JsonPropertyName("status")]
	public string? Status { get; set; }

	[JsonPropertyName("max_users")]
	public int MaxUsers { get; set; }

	[JsonPropertyName("owner")]
	public int Owner { get; set; }

	[JsonPropertyName("created_at")]
	public DateTime CreatedAt { get; set; }

	[JsonPropertyName("updated_at")]
	public DateTime UpdatedAt { get; set; }

	[JsonPropertyName("mailbox_status")]
	public string? MailboxStatus { get; set; }

	[JsonPropertyName("license")]
	public int License { get; set; }

	[JsonPropertyName("initial_screen_path")]
	public string? InitialScreenPath { get; set; }

	[JsonPropertyName("has_topic_on_novu")]
	public bool HasTopicOnNovu { get; set; }

	[JsonPropertyName("logs")]
	public bool Logs { get; set; }

	[JsonPropertyName("days_to_keep_logs")]
	public int DaysToKeepLogs { get; set; }
}
