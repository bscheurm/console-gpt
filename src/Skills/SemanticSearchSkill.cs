using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Azure;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleGPT;
using Microsoft.Extensions.Options;

namespace console_gpt.Skills
{
    public class RctIndexItem
    {
        public string id { get; set; }
        public string content { get; set; }
        public string category { get; set; }
        public string sourcepage { get; set; }
        public string sourcefile { get; set; }
    }

    public class SemanticSearchSkill
    {
        string serviceName = "rct-cogsearch-poc";
        string indexName = "rct-knowledge-base";
        string version = "2021-04-30-Preview";

        private readonly SearchClient _searchClient;

        public SemanticSearchSkill(IOptions<OpenAiServiceOptions> openAIOptions) 
        {
            Uri serviceEndpoint = new Uri($"https://{serviceName}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(openAIOptions.Value.SearchApiKey);

            _searchClient = new SearchClient(serviceEndpoint, indexName, credential);
        }

        [SKFunction("Optimizes user text for searching a semantic search index.")]
        [SKFunctionName("RunSearch")]
        [SKFunctionInput(Description = "The text entered by the user that needs to optimized for search.")]
        public async Task<string> RunSearch(string userText, SKContext skContext)
        {
            SearchOptions options = new SearchOptions();
            options.Size = 10;
            
            var response = _searchClient.Search<RctIndexItem>(userText, options);

            var items = new List<RctIndexItem>();

            foreach (var item in response.Value.GetResults())
            {
                //Console.WriteLine($"{item.Document.id} [Score={item.Score}]");
                items.Add(item.Document);
            }

            string json = JsonSerializer.Serialize(items);

            skContext.Variables.Set("SearchResultsJson", json);

            return json;
        }
    }
}
