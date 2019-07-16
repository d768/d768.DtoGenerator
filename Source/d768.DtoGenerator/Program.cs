using System;
using System.Threading.Tasks;
using d768.DtoGenerator.Core.Emission;
using d768.DtoGenerator.Core.Emission.TypeEmitter;

namespace d768.DtoGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var typesEmitter =
                new DllControllerTypeEmitter(@"C:\Projects\mobileapi\MobileApi\bin\MobileApi.dll");
        
            var result = await new DtoEmitter(
                    new DtoEmitterOptions(){AssumeAllObjectsAreStrings = true}, typesEmitter)
                .EmitAsync();
        
            Console.WriteLine("Hello World!");
        }
    }
}