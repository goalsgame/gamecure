using System.Threading.Tasks;
using Gamecure.Core.Configuration;

namespace Gamecure.GUI.Models;

public interface IConfigurationService
{
    Task<AppConfig> GetAppConfig();
    Task<UserConfig?> GetUserConfig();
    Task SaveUserConfig(UserConfig config);
}