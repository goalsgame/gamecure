using System.Threading.Tasks;
using Gamecure.Core.Google;

namespace Gamecure.GUI.Models;

public interface IGoogleAuthService
{
    Task<GoogleAuthContext> Run(string clientSecret, bool reset = false);
}