using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using d768.DtoGenerator.Core.Emission;
using d768.DtoGenerator.Core.Emission.TypeEmitter;
using LanguageExt;
using Mono.Cecil;
using MoreLinq;

namespace d768.DtoGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var typesEmitter =
                new DllControllerTypeEmitter(@"C:\Projects\mobileapi\MobileApi\bin\MobileApi.dll");

            var newAssemblyBuilder
                = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName("TempAssembly"),
                    AssemblyBuilderAccess.RunAndCollect);

            var newModule = newAssemblyBuilder.DefineDynamicModule("TempModule");

            (await new DtoDefinitionEmitter(
                        new DtoEmitterOptions() {AssumeAllObjectsAreStrings = true}, typesEmitter)
                    .EmitAsync()
                    .MapAsync(x => x
                        .Select(y => new EmittedType(y, newModule)))
                    .BindAsync(x => x.Select(y => y.CreateType()).Sequence()))
                .Match(
                    sources => sources.ForEach(
                        x => { Console.WriteLine(x); }),
                    error => Console.WriteLine(error.ErrorMessage));

            Console.ReadLine();
        }
    }
}