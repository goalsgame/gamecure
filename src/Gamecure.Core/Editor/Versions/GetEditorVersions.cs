using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.Versions;

public class GetEditorVersions : IMiddleware<EditorVersionsContext>
{
    private static readonly HttpClient _client = new();
    public async Task<EditorVersionsContext> OnInvoke(EditorVersionsContext context, ContextDelegate<EditorVersionsContext> next)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, context.IndexUrl+"&maxResults=2000")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken) }
        };

        var result = await _client.SendAsync(requestMessage);
        if (!result.IsSuccessStatusCode)
        {
            var error = $"Failed to get editor indices with status code {result.StatusCode} and reason {result.ReasonPhrase}";
            Logger.Error(error);
            return context with { Failed = true, Reason = error };
        }

        var stream = await result.Content.ReadAsStreamAsync();
        var storageList = await Json.DeserializeAsync<StorageList>(stream);

        if (storageList == null)
        {
            return context with { Failed = true, Reason = "Failed to deserialize indices in the storage list." };
        }

        if (storageList.Items == null)
        {
            return context with { Failed = true, Reason = "Items was null, is the url correct?" };
        }

        var versionIndexFiles = storageList
            .Items
            .Where(v => v.Name.EndsWith(".lvi"))
            .ToArray();

        Logger.Trace($"Found {versionIndexFiles.Length} versions.");
        List<EditorVersion> versions = new();
        foreach (var item in versionIndexFiles)
        {
            if (TryParse(item.Name, item.Created, context.Prefix!, out var editor))
            {
                versions.Add(editor);
            }
            else
            {
                Logger.Warning($"Failed to parse editor version from item with name: {item.Name}");
            }
        }

        return await next(context with
        {
            Versions = versions.ToArray()
        });

        static bool TryParse(string filename, DateTime? created, string prefix, out EditorVersion editorVersion)
        {
            Unsafe.SkipInit(out editorVersion);
            var values = filename.Split("_");
            if (values.Length != 3)
            {
                //Logger.Warning($"Expected length 3 got {values.Length}");
                return false;
            }

            if (!int.TryParse(values[1], out var changeset))
            {
                //Logger.Warning($"Expected changeset at position 1, got '{values[1]}'");
                return false;
            }

            editorVersion = new EditorVersion(values[0].Replace(prefix, string.Empty), filename, changeset, created);

            return true;
        }
    }

    private record StorageList(StorageListItems[]? Items);
    private record StorageListItems(string Name)
    {
        [JsonPropertyName("timeCreated")]
        public DateTime? Created { get; init; }
    }
}