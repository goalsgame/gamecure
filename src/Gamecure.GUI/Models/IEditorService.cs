using System.Threading.Tasks;
using Gamecure.Core.Editor;
using Gamecure.Core.Editor.DownloadAndCopy;
using Gamecure.Core.Editor.Setup;

namespace Gamecure.GUI.Models;

public interface IEditorService
{
    Task<EditorVersionResult> GetVersions();
    Task<DownloadEditorContext> DownloadEditor(EditorVersion version);
    Task<EditorSetupContext> RunSetup(string? workspace);
}