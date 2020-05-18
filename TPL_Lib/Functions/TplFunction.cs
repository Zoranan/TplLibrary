using System.Collections.Generic;
using System.Linq;

namespace TplLib.Functions
{
    public abstract class TplFunction
    {
        public TplFunction NextFunction { get; protected set; } = null;

        /// <summary>
        /// Adds the next function to the very end of this pipeline
        /// </summary>
        /// <param name="next">The function to attach to the end of the pipeline</param>
        public void AddToPipeline(TplFunction next)
        {
            
            if (NextFunction == null)
            {
                NextFunction = next;
            }
            else
            {
                NextFunction.AddToPipeline(next);
            }
        }

        public List<TplResult> Process()
        {
            return Process(new List<TplResult>());
        }

        public List<TplResult> Process(IEnumerable<string> input)
        {
            return Process(input.Select(l => new TplResult(l)));
        }

        /// <summary>
        /// Runs the list of TplResults through all functions in the pipeline, and returns the output
        /// </summary>
        /// <param name="input">The list of rsults to process</param>
        /// <returns>A list of processed results</returns>
        public List<TplResult> Process(IEnumerable<TplResult> input)
        {
            var results = InnerProcess(input.ToList()); // Add to list so original collection is not modified

            if (NextFunction != null)
            {
                results = NextFunction.Process(results);
            }

            return results;
        }

        protected abstract List<TplResult> InnerProcess(List<TplResult> input);
    }
}
