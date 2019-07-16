using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d768.DtoGenerator.Core.Emission.TypeEmitter;
using d768.DtoGenerator.Core.Infrastructure;
using LanguageExt;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace d768.DtoGenerator.Core.Emission
{
    public class DtoEmitterOptions
    {
        public bool AssumeAllObjectsAreStrings { get; set; }
    }
    
    public class DtoEmitter : IEmitter
    {
        private readonly DtoEmitterOptions _options;
        private IControllerTypesEmitter _typesEmitter;

        public DtoEmitter(DtoEmitterOptions options, IControllerTypesEmitter typesEmitter)
        {
            _options = options;
            _typesEmitter = typesEmitter;
        }

        private Either<EmissionError, IEnumerable<EmittedType>> EmitInternal(
            IEnumerable<TypeDefinition> controllers)
        {
            var emittedTypesList = new List<EmittedType>();

            foreach (var controller in controllers)
            {
                var dynamicMethods = controller.Methods.Where(x =>
                        x.IsPublic && x.ReturnType.Name == "IHttpActionResult" && x.HasBody)
                    .ToArray();

                foreach (var methodDefinition in dynamicMethods)
                {
                    var anonymousTypes = methodDefinition
                        .Body
                        .Instructions
                        .Where(x => x.OpCode == OpCodes.Newobj && x.Operand is MethodReference)
                        .Select(x => x.Operand as MethodReference)
                        .Where(x => x.DeclaringType.Name.Contains("AnonymousType")
                                    && x.DeclaringType is GenericInstanceType)
                        .Select(x => x.DeclaringType)
                        .Cast<GenericInstanceType>()
                        .ToArray();

                    foreach (var anonymousType in anonymousTypes)
                    {
                        var emitionResult =
                            Emit(anonymousType, methodDefinition.Name, controller.Name).Try();

                        if (emitionResult.IsFaulted)
                            return new EmissionError(
                                emitionResult
                                    .Match(
                                        _ => (Exception) null,
                                        ex => ex));

                        emitionResult.IfSucc(e => emittedTypesList.Add(e));
                    }
                }
            }

            return emittedTypesList;
        }

        private Try<EmittedType> Emit(GenericInstanceType declaringType, string methodName,
            string className)
            => new Try<EmittedType>(() =>
            {
                var emittedClassName = methodName + "Dto";
                var typeDefinition = declaringType.Resolve();
                var builder = new StringBuilder();
                var properties = typeDefinition.Properties.ToArray();

                builder.AppendLine("using System;");
                builder.AppendLine();
                builder.AppendLine($"public class {emittedClassName}");
                builder.AppendLine("{");

                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    
                    var typeName = HandleTypeNameSpecialCases(HandleTypes(declaringType
                        .GenericArguments[i]
                        .Name));
                    
                    var name = property
                                   .Name
                                   .Substring(0,1)
                                   .ToUpperInvariant()
                               + (property.Name.Length > 1
                        ? property.Name.Substring(1)
                        : string.Empty);

                    builder
                        .AppendLine($"    public {typeName} {name} {{ get; set; }}");
                }

                builder.AppendLine("}");
                return new EmittedType(
                    emittedClassName,
                    builder.ToString(),
                    className,
                    typeDefinition);
            });

        private string HandleTypes(string typeName)
        {
            switch (typeName)
            {
                case "Object" when _options.AssumeAllObjectsAreStrings: return "String";
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

        public Task<Either<EmissionError, IEnumerable<EmittedType>>> EmitAsync()
            => Task.FromResult(
                _typesEmitter.GetControllerTypes()
                    .Bind(EmitInternal));
    }
}