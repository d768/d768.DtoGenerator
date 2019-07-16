using System;
using System.Linq;
using System.Threading.Tasks;
using d768.DtoGenerator.Core.Emission;
using d768.DtoGenerator.Core.Emission.TypeEmitter;
using LanguageExt;
using MoreLinq;

namespace d768.DtoGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var typesEmitter =
                new DllControllerTypeEmitter(@"C:\Projects\mobileapi\MobileApi\bin\MobileApi.dll");
        
            var either = await new DtoEmitter(
                    new DtoEmitterOptions(){AssumeAllObjectsAreStrings = true}, typesEmitter)
                .EmitAsync()
                .MapAsync(x => x.Select(y => new SourcePage(y, true)))
                .MapAsync(x => x.Select(y => y.ToString()));
            
            either
                .Match(
                    sources => sources.ForEach(x => Console.WriteLine(x)),
                    error => Console.WriteLine(error.ErrorMessage));
        
            Console.ReadLine();
        }
    }
}