using SpotLights.Shared;
using SpotLights.Shared.Enums;

namespace SpotLights.Infrastructure.Interfaces.Newsletters
{
    public interface IEmailManager
    {
        Task<MailSettingDto?> GetSettingsAsync();
        Task PutSettingsAsync(MailSettingDto input);
        Task<SendNewsletterState> SendNewsletter(int postId);
    }
}
