using System;

namespace d768.DtoGenerator.Core.Infrastructure
{
    public static class Extensions
    {
        public static T SelectRecursive<T>(this T selectionBase, Func<T, T> selectRecursive,
            int maxDepth = 10)
            where T : class
        {
            var currentDepthLevel = 0;
            var currentSelectionBase = selectionBase;
            while (currentDepthLevel < maxDepth 
                   && selectRecursive(currentSelectionBase) is T
                                                    nextSelectionBase
                   && nextSelectionBase != null)
            {
                currentSelectionBase = nextSelectionBase;
            }

            return currentSelectionBase;
        }
        
        public static T SelectRecursive<T>(
            this T selectionBase, 
            Func<T, T> selectRecursive,
            Func<T, bool> selectCondition,
            int maxDepth = 10)
            where T : class
        {
            var currentDepthLevel = 0;
            var currentSelectionBase = selectionBase;
            while (currentDepthLevel < maxDepth
                   && !selectCondition(currentSelectionBase)
                   && selectRecursive(currentSelectionBase) is T
                       nextSelectionBase
                   && nextSelectionBase != null)
            {
                currentSelectionBase = nextSelectionBase;
            }

            return currentSelectionBase;
        }
    }
}