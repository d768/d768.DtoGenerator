using System.Collections.Generic;
using d768.DtoGenerator.Core.Infrastructure;
using LanguageExt;
using Mono.Cecil;

namespace d768.DtoGenerator.Core.Emission.TypeEmitter
{
    public interface IControllerTypesEmitter
    {
        Either<EmissionError, IEnumerable<TypeDefinition>> GetControllerTypes();
    }
}