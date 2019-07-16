using System;

namespace d768.DtoGenerator.Core.Infrastructure
{
    public class EmissionError
    {
        public string ErrorMessage { get; }

        public EmissionError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public EmissionError(Exception ex)
        {
            ErrorMessage = $"{ex.GetType().Name}:{ex.Message}";
        }

        public static EmissionError None => new EmissionError("None");
    }
}