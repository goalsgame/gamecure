using Gamecure.Core.Editor;
using Gamecure.Core.Plastic.Parser;

namespace Gamecure.GUI.ViewModels.EditorSync;

public class ChangesetViewModel : ViewModelBase
{
    public PlasticChangeset Changeset { get; }
    public EditorVersion? Version { get; }
    public bool IsCurrentEditor { get; set; }
    public bool IsCurrentChangeset { get; set; }
    public bool HasEditor => Version != null;
    public bool CanDownload => !IsCurrentEditor && HasEditor;
    public ChangesetViewModel(PlasticChangeset changeset, EditorVersion? version)
    {
        Version = version;
        Changeset = changeset;
    }
}