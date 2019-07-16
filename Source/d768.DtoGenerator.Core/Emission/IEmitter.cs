using System.Collections.Generic;
using System.Threading.Tasks;
using d768.DtoGenerator.Core.Infrastructure;
using LanguageExt;

namespace d768.DtoGenerator.Core.Emission
{
    public interface IEmitter
    {
        Task<Either<EmissionError, IEnumerable<EmittedTypeDefinition>>> EmitAsync();
    }
}