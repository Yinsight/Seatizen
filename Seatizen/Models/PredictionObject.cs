using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Seatizen.Models
{
    public class PredictionObject
    {
        public PredictionObject()
        {
            Predictions = new List<Prediction>();
        }

        [JsonPropertyName("id")]
        public Guid Id { get; set;}

        [JsonPropertyName("project")]
        public Guid Project { get; set;}

        [JsonPropertyName("iteration")]
        public Guid Iteration { get; set;}

        [JsonPropertyName("created")]
        public DateTime Created { get; set;}

        [JsonPropertyName("predictions")]
        public List<Prediction> Predictions { get; set; }
    }

    public class Prediction
    {
        public Prediction()
        {
            BoundingBox = new BoundingBox();
        }

        [JsonPropertyName("probability")]
        public decimal Probability { get; set; }

        [JsonPropertyName("tagId")]
        public Guid TagId { get; set; }

        [JsonPropertyName("tagName")]
        public string TagName { get; set; }

        [JsonPropertyName("boundingBox")]
        public BoundingBox BoundingBox { get; set; }
    }

    public class BoundingBox
    {
        [JsonPropertyName("left")]
        public decimal Left { get; set; }

        [JsonPropertyName("top")]
        public decimal Top { get; set; }

        [JsonPropertyName("width")]
        public decimal Width { get; set; }

        [JsonPropertyName("height")]
        public decimal Height { get; set; }
    }
}
