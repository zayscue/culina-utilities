using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenScraping;
using OpenScraping.Config;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RecipeScrapers.Food.Com
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var recipeId = "44061";
            Console.Write($"Enriching Recipe {recipeId}...");
            var html = await GetRecipeWebPageContent(recipeId);
            ScrapeHtml(html);
            Console.Write($" Finished{Environment.NewLine}");

            var recipeIdTwo = "137739";
            Console.Write($"Enriching Recipe {recipeIdTwo}...");
            var htmlTwo = await GetRecipeWebPageContent(recipeIdTwo);
            var results = ScrapeHtml(htmlTwo);
            Console.Write($" Finished{Environment.NewLine}");
        }

        static async Task<string> GetRecipeWebPageContent(string recipeId)
        {
            const string BaseAddress = "https://www.food.com/recipe";
            var httpClientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 100,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
            using var httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(BaseAddress)
            };
            var response = await httpClient.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri($"{BaseAddress}/{recipeId}"),
                Method = HttpMethod.Get
            });
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        static JContainer ScrapeHtml(string html)
        {
            var jsonConfig = @"
            {
                'imageUrl': './/link[@rel=\'image_src\']/@href',
                'serves': './/div[@class=\'recipe-facts__details recipe-facts__servings\']//a[@class=\'theme-color\']',
                'yields': './/div[@class=\'recipe-facts__details recipe-facts__yield\']//a[@class=\'theme-color\']',
                'ingredients': {
                    '_xpath': './/ul[@class=\'recipe-ingredients__list\']//li[@class=\'recipe-ingredients__item\']',
                    'quantity': './/div[@class=\'recipe-ingredients__ingredient-quantity\']',
                    'part': {
                        '_xpath': './/div[contains(@class, \'recipe-ingredients__ingredient-part\')]'
                    }
                },
                'servingSize': './/div[@class=\'recipe-nutrition__main-info\']//p[1]',
                'servingsPerRecipe': './/div[@class=\'recipe-nutrition__main-info\']//p[2]',
                'nutritionInfo': {
                    '_xpath': './/section[@class=\'recipe-nutrition__info\']//div[contains(@class, \'recipe-nutrition__section\')]',
                    'nutritionItem': './/p[contains(@class, \'recipe-nutrition__item\')]'
                }
            }
            ";
            var config = StructuredDataConfig.ParseJsonString(jsonConfig);
            var openScraping = new StructuredDataExtractor(config);
            var scrapingResults = openScraping.Extract(html);
            return scrapingResults;
        }
    }
}
