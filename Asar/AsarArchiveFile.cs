using Newtonsoft.Json;
using System.Collections.Generic;

namespace Patcher.Asar
{
  public class AsarArchiveFile
  {
    [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
    public long? Size { get; set; }

    [JsonProperty("offset", NullValueHandling = NullValueHandling.Ignore)]
    public string Offset { get; set; }

    [JsonProperty("unpacked", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool? Unpacked { get; set; }

    [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Dictionary<string, AsarArchiveFile> Files { get; internal set; }
  }
}
