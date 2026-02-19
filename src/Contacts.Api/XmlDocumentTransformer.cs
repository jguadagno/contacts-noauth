using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Contacts.Api;

/// <summary>
/// Transforms OpenAPI document to include XML documentation comments.
/// </summary>
public sealed class XmlDocumentTransformer() : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Set document metadata
        document.Info = new OpenApiInfo()
        {
            Title = "Coding with JoeG Contact API",
            Version = "v1",
            Description = "The API for the Contacts Application on Coding with JoeG",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new OpenApiContact
            {
                Name = "Joseph Guadagno",
                Email = "jguadagno@hotmail.com",
                Url = new Uri("https://www.josephguadagno.net"),
            }
        };

        return Task.CompletedTask;
    }
}
