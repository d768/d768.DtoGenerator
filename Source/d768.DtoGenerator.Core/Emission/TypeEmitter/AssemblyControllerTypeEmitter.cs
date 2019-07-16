using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using d768.DtoGenerator.Core.Infrastructure;
using LanguageExt;
using Mono.Cecil;

namespace d768.DtoGenerator.Core.Emission.TypeEmitter
{
    public class AssemblyControllerTypeEmitter: IControllerTypesEmitter
    {
        private Assembly _assembly;

        public AssemblyControllerTypeEmitter(Assembly assembly)
        {
            _assembly = assembly;
        }

        public Either<EmissionError, IEnumerable<TypeDefinition>> GetControllerTypes()
            => new Try<AssemblyDefinition>(() => AssemblyDefinition
                    .ReadAssembly(_assembly.Location))
                .ToEither()
                .MapLeft(x => new EmissionError(x))
                .Map(def => def.Modules.SelectMany(x => x.Types))
                .Map(x => x.Where(y => y.Name.Contains("Controller")));
    }
}