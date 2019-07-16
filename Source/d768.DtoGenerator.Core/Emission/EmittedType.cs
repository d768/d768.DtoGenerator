using Mono.Cecil;

namespace d768.DtoGenerator.Core.Emission
{
    public class EmittedType
    {
        public string Name { get; }
        public string SourceCode { get; }

        public string ParentType { get; }

        public TypeDefinition TypeDefinition { get; }

        public EmittedType(string name, 
        string sourceCode, 
        string parentType,
            TypeDefinition typeDefinition)
        {
            Name = name;
            SourceCode = sourceCode;
            ParentType = parentType;
            TypeDefinition = typeDefinition;
        }
    }
}