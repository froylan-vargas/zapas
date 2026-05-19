using Zapas.Api.Models;

namespace Zapas.Api.Services;

public interface IFitSessionParser
{
    Session Parse(Stream fileStream, string? fallbackName);
}
