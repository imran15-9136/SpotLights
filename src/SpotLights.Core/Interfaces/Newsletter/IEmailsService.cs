using SpotLights.Shared;
using SpotLights.Shared.Enums;

namespace SpotLights.Core.Interfaces.Newsletter
{
    public interface IEmailsService
    {
        Task<MailSettingDto?> GetSettingsAsync();
        Task PutSettingsAsync(MailSettingDto input);
        Task<SendNewsletterState> SendNewsletter(int postId);
    }
}
