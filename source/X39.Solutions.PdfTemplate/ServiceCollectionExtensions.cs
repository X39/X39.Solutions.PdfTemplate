using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Services.PropertyAccessCache;
using X39.Solutions.PdfTemplate.Services.TextService;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Contains extension methods for the service collection.
/// </summary>
[PublicAPI]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required for the generator to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    public static void AddPdfTemplateServices(this IServiceCollection services)
    {
        services.AddSingleton<SkPaintCache>();
        services.AddSingleton<ControlExpressionCache>();
        services.AddSingleton<ITextService, TextService>();
        services.AddSingleton<IPropertyAccessCache, PropertyAccessCache>();
        services.AddScoped<ITemplateData, TemplateData>();
    }
}