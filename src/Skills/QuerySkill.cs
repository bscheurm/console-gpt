using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ConsoleGPT.Skills
{
    /// <summary>
    /// A Sematic Kernel skill that interacts with ChatGPT
    /// </summary>
    public class QuerySkill
    {
        private readonly IKernel _kernel;
        
        public QuerySkill(IKernel semanticKernel,
                         IOptions<OpenAiServiceOptions> openAIOptions)
        {
            _kernel = semanticKernel;

            // Configure the semantic kernel
            _kernel.Config.AddAzureTextCompletionService(
                "TnRDev-AOAI", 
                openAIOptions.Value.CompletionModelDeploymentId, 
                openAIOptions.Value.AzureEndpoint, 
                openAIOptions.Value.AzureKey);
        }

        /// <summary>
        /// Send a prompt to the LLM.
        /// </summary>
        [SKFunction("Optimize user text for search.")]
        [SKFunctionName("Optimize")]
        public async Task<string> Optimize(string prompt)
        {
            var reply = string.Empty;
            try
            {
                var skillsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Skills");
                var skill = _kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "QuerySkill");
                
                var context = new ContextVariables(); 
                context.Set("INPUT", prompt); 

                var result = await _kernel.RunAsync(context, skill["OptimizeQuery"]);

                Console.WriteLine($"Input: {prompt}, Optimized: {result}");

                reply = result.Result;
            }
            catch (AIException aiex)
            {
                // Reply with the error message if there is one
                reply = $"OpenAI returned an error ({aiex.Message}). Please try again.";
            }

            return reply;
        }
    }
}