using System.Threading.Tasks;
using Gamecure.Core.Plastic;

namespace Gamecure.GUI.Models;

public interface IPlasticService
{
    Task<PlasticContext> Run();
    Task<PlasticContext> Run(string? workspace);
}