using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MoviePro.Enums;
using MoviePro.Models.Settings;
using MoviePro.Models.TMDB;
using MoviePro.Services.Interfaces;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Serialization.Json;
using System.Security.Policy;
using System.Threading.Tasks;

namespace MoviePro.Services
{
    public class TMDBMovieService : IRemoteMovieService
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClient;

        public TMDBMovieService(IOptions<AppSettings> appSettings, 
            IHttpClientFactory httpClient)
        {
            _appSettings = appSettings.Value;
            _httpClient = httpClient;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<ActorDetail> ActorDetailAsync(int id)
        {
            //-step 1: setup default instance of movie search
            ActorDetail actorDetail = new();
            //step 2: assemble the full request uri string
            var query = $"{_appSettings.TMDBSettings.BaseUrl}/person/{id}";
            var queryParams = new Dictionary<string, string>()
            {
                {"api_key", _appSettings.MovieProSettings.TmDbApiKey },
                {"language", _appSettings.TMDBSettings.QueryOptions.Language },
            };
            var requestUri = QueryHelpers.AddQueryString(query, queryParams);

            var client = _httpClient.CreateClient();
            //note  on system.text.json namespace and the GetFromJsonAsync method in it
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.json?view=net-6.0
            var response = await client.GetFromJsonAsync<ActorDetail>(requestUri); //SendAsync(request);

            return response;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<MovieDetail> MovieDetailAsync(int id)
        {
            //step 1: setup default instance of movie search
            MovieDetail movieDetail = new();
            //step 2: assemble the full request uri string
            var query = $"{_appSettings.TMDBSettings.BaseUrl}movie/{id}";
            var queryParams = new Dictionary<string, string>()
            {
                {"api_key", _appSettings.MovieProSettings.TmDbApiKey },
                {"language", _appSettings.TMDBSettings.QueryOptions.Language },
                {"append_to_response", _appSettings.TMDBSettings.QueryOptions.AppendToResponse }
            };

            var requestUri = QueryHelpers.AddQueryString(query, queryParams);

            var client = _httpClient.CreateClient();
            var response = await client.GetFromJsonAsync<MovieDetail>(requestUri);
            
            return response;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------
        public async Task<MovieSearch> SearchMovie(MovieCategory category, int count)
        {
            //step 1: setup default instance of movie search
            MovieSearch movieSearch = new();
            //step 2: assemble the full request uri string
            var query = $"{_appSettings.TMDBSettings.BaseUrl}movie/{category}";
            var queryParams = new Dictionary<string, string>()
            {
                {"api_key", _appSettings.MovieProSettings.TmDbApiKey },
                {"language", _appSettings.TMDBSettings.QueryOptions.Language },
                {"page", _appSettings.TMDBSettings.QueryOptions.Page }
            };

            var requestUri = QueryHelpers.AddQueryString(query, queryParams);
            
            var client = _httpClient.CreateClient();
            var response = await client.GetFromJsonAsync<MovieSearch>(requestUri);

            response.results = response.results.Take(count).ToArray();
            response.results.ToList().ForEach(r => r.poster_path = 
                                                        $"{_appSettings.TMDBSettings.BaseImagePath}/" +
                                                        $"{_appSettings.MovieProSettings.DefaultPosterSize}/{r.poster_path}");
            return response;

        }
    }
}
