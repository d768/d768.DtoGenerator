using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace d768.DtoGenerator.Core.Emission
{
    public class EmittedTypeDefinition
    {
        public bool IsCollection => CheckIfTypeIsCollection(Type);
        public bool IsPrimitive => Type.IsPrimitive;
        public bool IsAnonymous => Type.Name.Contains("Anonymous");
        
        public IReadOnlyCollection<EmittedTypeDefinition> Properties { get; }
        
        public string TypeName { get; }
        public string Name { get; }
        public TypeReference Type { get; }

        public EmittedTypeDefinition(string name, TypeReference type, string typeName = null)
        {
            Name = name;
            Type = type;
            TypeName = typeName ?? string.Empty;
            if (IsCollection)
            {
                Properties = new EmittedTypeDefinition[0];
            }
            else if(IsPrimitive)
            {
                Properties = new EmittedTypeDefinition[0];
            }
            else if(IsAnonymous && type is GenericInstanceType genericInstanceType)
            {
                var typeDef = type.Resolve();
                Properties = typeDef
                    .Properties
                    .Zip(genericInstanceType.GenericArguments)
                    .Select(x =>
                    {
                        return new EmittedTypeDefinition(x.Item1.Name, x.Item2, Name);
                    })
                    .ToArray();
            }
            else
            {
                Properties = type
                    .Resolve()
                    .Properties
                    .Select(x =>
                    {
                        return new EmittedTypeDefinition(x.Name, x.PropertyType, Name);
                    })
                    .ToArray();
            }
        }

        private bool CheckIfTypeIsCollection(TypeReference x)
            => x.IsArray
               || x.Name.Contains("List")
               || x.Name.Contains("Collection")
               || x.Name.Contains("Enumerable");
    }
}