using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Gamecure.GUI.ViewModels;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void SetProperty<T>(ref T prop, in T value, [CallerMemberName] string? propertyName = null)
    {
        var changed = !prop?.Equals(value) ?? true;
        if (changed)
        {
            prop = value;
            OnPropertyChanged(propertyName);
        }
    }
}