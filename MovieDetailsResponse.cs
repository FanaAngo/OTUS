using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProjectAfishaBot
{
    class MovieDetailsResponse
    {
        [JsonPropertyName("results")]
        public List<Movies> Movie { get; set; }

        public class Movies
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("release_date")]
            public string ReleaseDate { get; set; }

            [JsonPropertyName("id")]
            public int ID { get; set; }

            [JsonPropertyName("vote_average")]
            public decimal voteAverage { get; set; }
        }
    }
}