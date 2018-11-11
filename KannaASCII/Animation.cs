using Newtonsoft.Json;
using System.Collections.Generic;

namespace KannaASCII
{
    class Animation
    {
        [JsonProperty("framerate")]
        public int Framerate { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("images")]
        public List<string> Images { get; set; }

        [JsonProperty("frames")]
        public List<List<int>> Frames { get; set; }

        [JsonIgnore]
        public List<ImageSource> ImageSources { get; set; }
    }
}
 