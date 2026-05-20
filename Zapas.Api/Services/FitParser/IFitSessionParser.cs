using Zapas.Api.Models;

namespace Zapas.Api.Services.FitParser;

public interface IFitSessionParser
{
    Session Parse(Stream fileStream, string? fallbackName);
}
