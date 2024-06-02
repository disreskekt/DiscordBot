using System.IO;
using System.Threading.Tasks;

namespace Mp3Player.Services.Interfaces;

public interface IFileSystemService
{
    public int GetPageAmount();
    public string[] GetPage(int page, int pageAmount);
    public bool SongNameExists(string songName);
    public string[] GetAll();
    public Task AddFile(Stream mp3Stream, string songName);
}