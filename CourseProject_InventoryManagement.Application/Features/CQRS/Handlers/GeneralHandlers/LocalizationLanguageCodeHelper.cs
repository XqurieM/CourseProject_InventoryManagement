using CourseProject_InventoryManagement.Domain.Enums;

namespace CourseProject_InventoryManagement.Application.Features.CQRS.Handlers.GeneralHandlers
{
    internal static class LocalizationLanguageCodeHelper
    {
        public static string NormalizeOrDefault(string? languageCode, LanguageType? fallbackLanguage = null)
        {
            if (!string.IsNullOrWhiteSpace(languageCode))
            {
                var normalized = languageCode.Trim().ToLowerInvariant();
                return normalized switch
                {
                    "tr" or "tr-tr" or "turkish" => "tr",
                    "en" or "en-us" or "en-gb" or "english" => "en",
                    "de" or "de-de" or "german" => "de",
                    "fr" or "fr-fr" or "french" => "fr",
                    "es" or "es-es" or "spanish" => "es",
                    "it" or "it-it" or "italian" => "it",
                    "ru" or "ru-ru" or "russian" => "ru",
                    "zh" or "zh-cn" or "zh-tw" or "chinese" => "zh",
                    _ => normalized
                };
            }

            return fallbackLanguage.HasValue
                ? MapFromEnum(fallbackLanguage.Value)
                : "en";
        }

        public static string MapFromEnum(LanguageType languageType) =>
            languageType switch
            {
                LanguageType.Turkish => "tr",
                LanguageType.English => "en",
                LanguageType.German => "de",
                LanguageType.French => "fr",
                LanguageType.Spanish => "es",
                LanguageType.Italian => "it",
                LanguageType.Russian => "ru",
                LanguageType.Chinese => "zh",
                _ => "en"
            };
    }
}
