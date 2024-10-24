using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Services.PropertyAccessCache;
using X39.Solutions.PdfTemplate.Services.ResourceResolver;
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
    /// <remarks>
    /// This method adds the following services:
    /// <list type="bullet">
    ///     <item><see cref="SkPaintCache"/></item>
    ///     <item><see cref="ControlExpressionCache"/></item>
    ///     <item><see cref="ITextService"/></item>
    ///     <item><see cref="IPropertyAccessCache"/></item>
    ///     <item><see cref="ITemplateData"/></item>
    ///     <item><see cref="IResourceResolver"/></item>
    /// </list>
    ///
    /// If you want to use your own implementation of <see cref="IResourceResolver"/>, you can add it after this method.
    /// See <a href="https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-registration-methods">MSDN Page</a>
    /// for more information about how to work with dependency injection in .NET.
    /// </remarks>
    public static void AddPdfTemplateServices(this IServiceCollection services)
    {
        services.AddSingleton<SkPaintCache>();
        services.AddSingleton<ControlExpressionCache>();
        services.AddSingleton<ITextService, TextService>();
        services.AddSingleton<IPropertyAccessCache, PropertyAccessCache>();
        services.AddScoped<ITemplateData, TemplateData>();
        services.AddScoped<IResourceResolver, DefaultResourceResolver>();
    }
}