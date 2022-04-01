#nullable enable
using System;

namespace Traducir.Core.TransifexV3
{
#pragma warning disable SA1313 // This shouldn't apply to records - a future release of the StyleCop analyzers will fix this
    public sealed record TransifexClientError(string Status, string Code, string Detail, string Title);
}
