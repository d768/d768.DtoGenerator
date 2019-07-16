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

    public class DtoDefinitionEmitter : IEmitter
    {
        private readonly DtoEmitterOptions _options;
        private IControllerTypesEmitter _typesEmitter;

        public DtoDefinitionEmitter(DtoEmitterOptions options, IControllerTypesEmitter typesEmitter)
        {
            _options = options;
            _typesEmitter = typesEmitter;
        }

        private Either<EmissionError, IEnumerable<EmittedTypeDefinition>> EmitInternal(
            IEnumerable<TypeDefinition> controllers)
        {
            var emittedTypesList = new List<EmittedTypeDefinition>();

            foreach (var controller in controllers)
            {
                var dynamicMethods = controller.Methods.Where(x =>
                        x.IsPublic && x.ReturnType.Name == "IHttpActionResult" && x.HasBody)
                    .ToArray();

                foreach (var methodDefinition in dynamicMethods)
                {
                    var returnType = GetReturnType(methodDefinition);

                    if (returnType.IsSome)
                    {
                        var emissionResult =
                            Emit(
                                (GenericInstanceType) returnType, 
                                controller.Name+methodDefinition.Name)
                                .Try();

                        if (emissionResult.IsFaulted)
                            return new EmissionError(
                                emissionResult
                                    .Match(
                                        _ => (Exception) null,
                                        ex => ex));

                        emissionResult.IfSucc(e => emittedTypesList.Add(e));
                    }
                }
            }

            return emittedTypesList;
        }

        private Option<GenericInstanceType> GetReturnType(MethodDefinition methodDefinition)
            => new Try<GenericInstanceType>(() => methodDefinition
                    .Body
                    .Instructions
                    .Where(x => x.OpCode == OpCodes.Callvirt
                                && x.Operand is GenericInstanceMethod genericInstanceMethod
                                && genericInstanceMethod.FullName.Contains(
                                    "OkNegotiatedContentResult"))
                    .Select(x => x.Operand as GenericInstanceMethod)
                    .First()
                    .GenericArguments
                    .First() as GenericInstanceType)
                .ToOption();

        private Try<EmittedTypeDefinition> Emit(GenericInstanceType declaringType,
            string typeName)
            => () => new EmittedTypeDefinition(
                typeName + "Dto",
                declaringType);

        public Task<Either<EmissionError, IEnumerable<EmittedTypeDefinition>>> EmitAsync()
            => Task.FromResult(
                _typesEmitter.GetControllerTypes()
                    .Bind(EmitInternal));
    }
}