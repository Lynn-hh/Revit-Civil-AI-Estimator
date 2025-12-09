using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RevitAiEstimator
{
    public class EstimationResult
    {
        public string CostCode { get; set; } = "N/A";
        public string Description { get; set; } = "AI Request Failed";
        public double UnitRate { get; set; } = 0.0;
    }

    public class AiService
    {
        // TODO: PASTE YOUR API KEY HERE
        // SECURITY WARNING: Never commit your real API key to GitHub!
        private const string ApiKey = "sk-YOUR_OPENAI_API_KEY_HERE";         private const string Endpoint = "https://api.openai.com/v1/chat/completions";

        public EstimationResult GetEstimation(ElementData input, string context)
        {
            try
            {
                // Fix for UI Freeze: Run async task on a background thread to avoid deadlocking the Revit UI thread
                return Task.Run(() => CallOpenAiAsync(input, context)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                return new EstimationResult { Description = "Error: " + ex.Message };
            }
        }

        private async Task<EstimationResult> CallOpenAiAsync(ElementData input, string context)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

                var prompt = $"You are a top Civil Engineering Quantity Surveyor. " +
                             $"Context: {context}. " +  // Added User Context
                             $"Classify this element into CESMM4 (Civil Engineering Standard Method of Measurement). " +
                             $"Element: Category={input.Category}, Family={input.FamilyName}, Type={input.TypeName}, Material={input.MaterialName}, Qty={input.MetricQuantity} {input.Unit}. " +
                             $"Return ONLY a strict JSON object with fields: 'cost_code' (string), 'description' (string), 'unit_rate' (number). Do not include markdown formatting.";

                var requestBody = new
                {
                    model = "gpt-4o", // or gpt-3.5-turbo
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful API that returns strictly valid JSON." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(Endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return new EstimationResult { Description = $"API Error: {response.StatusCode} - {error}" };
                }

                string responseString = await response.Content.ReadAsStringAsync();
                
                // Parse Response
                JObject jsonResponse = JObject.Parse(responseString);
                string aiContent = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString();

                // Clean up markdown if AI adds it (```json ... ```)
                if (aiContent.Contains("```"))
                {
                    aiContent = aiContent.Replace("```json", "").Replace("```", "").Trim();
                }

                JObject resultJson = JObject.Parse(aiContent);

                return new EstimationResult
                {
                    CostCode = resultJson["cost_code"]?.ToString(),
                    Description = resultJson["description"]?.ToString(),
                    UnitRate = resultJson["unit_rate"]?.ToObject<double>() ?? 0
                };
            }
        }
    }
}
