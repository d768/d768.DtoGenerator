using System.Collections.Generic;
using System.Linq;
using d768.DtoGenerator.Core.Infrastructure;
using LanguageExt;
using Mono.Cecil;

namespace d768.DtoGenerator.Core.Emission.TypeEmitter
{
    public class DllControllerTypeEmitter: IControllerTypesEmitter
    {
        private string _dllPath;

        public DllControllerTypeEmitter(string dllPath)
        {
            _dllPath = dllPath;
        }

        public Either<EmissionError, IEnumerable<TypeDefinition>> GetControllerTypes()
            => ReadModule(_dllPath)
                .Map(x => x.Types.Where(y => y.Name.Contains("Controller")));
        
        private Either<EmissionError, ModuleDefinition> ReadModule(string path)
            => new Try<ModuleDefinition>(() => ModuleDefinition.ReadModule(path))
                .ToEither()
                .MapLeft(ex => new EmissionError(ex));
    }
}