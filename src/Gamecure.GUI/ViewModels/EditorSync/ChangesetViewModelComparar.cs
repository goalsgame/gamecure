using System.Collections.Generic;

namespace Gamecure.GUI.ViewModels.EditorSync;

public class ChangesetViewModelComparar : IComparer<ChangesetViewModel>
{
    // This will sort a ChangesetViewModel based on the changeset ID in descending order
    public int Compare(ChangesetViewModel? x, ChangesetViewModel? y)
    {
        if (x != null && y != null)
        {
            return y.Changeset.Id.CompareTo(x.Changeset.Id);
        }
        if (x == null)
        {
            return 1;
        }

        if (y == null)
        {
            return -1;
        }
        return 0;
    }
}