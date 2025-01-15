using System.Text.Json.Serialization;

namespace GCScript.Enspace.Models;

internal class WorkspacesRequest {
	[JsonPropertyName("workspace")]
	public int Workspace { get; set; }
}
