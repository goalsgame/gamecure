using System.Threading.Tasks;

namespace Gamecure.GUI.Models;

public interface IGamecureVersion
{
    Task<bool> HasNewVersion();

}