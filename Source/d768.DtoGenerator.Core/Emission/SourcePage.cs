using System.Text;

namespace d768.DtoGenerator.Core.Emission
{
    public class SourcePage
    {
        private readonly bool _assumeAllObjectsAreStrings;
        public EmittedTypeDefinition TypeDefinition { get; }

        public SourcePage(EmittedTypeDefinition typeDefinition, bool assumeAllObjectsAreStrings = true)
        {
            _assumeAllObjectsAreStrings = assumeAllObjectsAreStrings;
            TypeDefinition = typeDefinition;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("using System;");
            builder.AppendLine();
            builder.AppendLine($"public class {TypeDefinition.Name}");
            builder.AppendLine("{");

            foreach (var property in TypeDefinition.Properties)
            {
                var name = property
                               .Name
                               .Substring(0, 1)
                               .ToUpperInvariant()
                           + (property.Name.Length > 1
                               ? property.Name.Substring(1)
                               : string.Empty);

                builder
                    .AppendLine($"    public {HandleTypeNameSpecialCases(HandleTypes(property.Type.Name))} {name} {{ get; set; }}");
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        private string HandleTypes(string typeName)
        {
            switch (typeName)
            {
                case "Object" when _assumeAllObjectsAreStrings: return "String";
                default: return typeName;
            }
        }

        private string HandleTypeNameSpecialCases(string typeName)
        {
            switch (typeName)
            {
                case "Boolean": return "bool";
                case "Int32": return "int";
                case "Int64": return "long";
                case "String": return "string";
                case "Decimal": return "bool";
                case "Object": return "object";
                default: return typeName;
            }
        }
    }
}