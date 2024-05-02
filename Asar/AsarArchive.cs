using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Patcher.Asar
{
  public partial class AsarArchive
  {
    private static readonly byte[] MagicHeader = [0x04, 0x00, 0x00, 0x00];

    private readonly uint FilesOffset;
    private readonly Stream Stream;
    private readonly AsarArchiveFile Metadata;
    public readonly string[] FileList;

    public AsarArchive(Stream stream)
    {
      Stream = stream;
      var br = new BinaryReader(Stream);
      if (!MagicHeader.SequenceEqual(br.ReadBytes(4))) throw new FileFormatException("Not an ASAR archive");

      FilesOffset = br.ReadUInt32() + 0x08;
      br.ReadUInt32(); // skip 4 bytes
      uint metadataSize = br.ReadUInt32();
      string metadataString = Encoding.UTF8.GetString(br.ReadBytes((int)metadataSize));
      Metadata = JsonConvert.DeserializeObject<AsarArchiveFile>(metadataString);

      var fileList = new List<string>();

      void GetFiles(AsarArchiveFile file, string path)
      {
        if (file.Files != null && file.Files.Count > 0)
        {
          foreach (var f in file.Files)
          {
            GetFiles(f.Value, Path.Combine(path, f.Key));
          }
        }
        else
        {
          fileList.Add(path);
        }
      }

      GetFiles(Metadata, "");
      FileList = [.. fileList];
    }

    public AsarArchiveFile GetFile(string fileName)
    {
      var current = Metadata;
      var path = fileName.Split(Path.DirectorySeparatorChar);
      foreach (var part in path)
      {
        if (current.Files == null || !current.Files.ContainsKey(part)) return null;
        current = current.Files[part];
      }
      if (current.Files != null && current.Files.Count > 0) return null;
      return current;
    }

    public void WriteTo(Stream outputStream)
    {
      byte[] metadataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Metadata));
      uint metadataSize = (uint)metadataBytes.Length;
      uint filesOffset = metadataSize + (uint)((-metadataSize) & 0x03) + 0x10;

      var bw = new BinaryWriter(outputStream);
      var inputStream = Stream;

      outputStream.Position = 0;
      bw.Write(MagicHeader);
      bw.Write(filesOffset - 0x08);
      bw.Write(filesOffset - 0x0C);
      bw.Write(metadataSize);
      bw.Write(metadataBytes);

      outputStream.Position = filesOffset;
      inputStream.Position = FilesOffset;
      inputStream.CopyTo(outputStream);
      outputStream.Dispose();
      inputStream.Dispose();
    }
  }
}
