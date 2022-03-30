using System.Collections.Immutable;
using System.Threading.Tasks;
using Traducir.Core.Models;
using Traducir.Core.Models.Services;

namespace Traducir.Core.Services
{
    public interface ITransifexService
    {
        Task<ImmutableArray<TransifexString>> GetStringsFromTransifexAsync();

        Task<bool> PushStringsToTransifexAsync(ImmutableArray<SOString> strings);
    }
}