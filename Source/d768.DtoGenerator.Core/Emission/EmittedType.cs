using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace d768.DtoGenerator.Core.Emission
{
    public class EmittedTypeProperty
    {
        public string Name { get; }
        public TypeDefinition Type { get; }

        public EmittedTypeProperty(string name, TypeDefinition type)
        {
            Name = name;
            Type = type;
        }
    }
    
    public class EmittedType
    {
        public string Name { get; }
        public string ParentType { get; }
        public IReadOnlyCollection<EmittedTypeProperty> Properties { get; }

        public EmittedType(string name,
        string parentType,
        GenericInstanceType type)
        {
            Name = name;
            ParentType = parentType;

            var typeDef = type.Resolve();
            Properties = typeDef
                .Properties
                .Zip(type.GenericArguments)
                .Select(x => new EmittedTypeProperty(x.Item1.Name, x.Item2.Resolve()))
                .ToArray();
        }
    }
}