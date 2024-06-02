using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Mp3Player.Configs;
using Mp3Player.Services.Interfaces;

namespace Mp3Player.Services;

public class FileSystemService : IFileSystemService
{
    private readonly FileSystemConfig _fileSystemConfig;
    
    public FileSystemService(IOptions<FileSystemConfig> fileSystemConfig)
    {
        _fileSystemConfig = fileSystemConfig.Value;
    }
    
    public int GetPageAmount()
    {
        if (!Directory.Exists(_fileSystemConfig.MusicFolderPath))
        {
            Directory.CreateDirectory(_fileSystemConfig.MusicFolderPath);
            return 1;
        }
        
        string[] strings = Directory.GetFiles(_fileSystemConfig.MusicFolderPath);
        
        int count = strings.Length;
        
        int countDividedByTen = count / 10;
        int countPercentOfTen = count % 10;
        
        int pageAmount;
        if (countPercentOfTen == 0)
        {
            pageAmount = countDividedByTen;
        }
        else
        {
            pageAmount = countDividedByTen + 1;
        }
        
        return pageAmount;
    }
    
    public string[] GetPage(int page, int pageAmount)
    {
        if (!Directory.Exists(_fileSystemConfig.MusicFolderPath))
        {
            Directory.CreateDirectory(_fileSystemConfig.MusicFolderPath);
        }
        
        return Directory.GetFiles(_fileSystemConfig.MusicFolderPath)
            .Skip(page * 10 - 10)
            .Take(10)
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray()!;
    }
    
    public string[] GetAll()
    {
        if (!Directory.Exists(_fileSystemConfig.MusicFolderPath))
        {
            Directory.CreateDirectory(_fileSystemConfig.MusicFolderPath);
        }
        
        return Directory.GetFiles(_fileSystemConfig.MusicFolderPath)
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray()!;
    }
    
    public bool SongNameExists(string songName)
    {
        return File.Exists(_fileSystemConfig.MusicFolderPath + '\\' + songName + ".mp3");
    }
    
    public async Task AddFile(Stream mp3Stream, string songName)
    {
        await using FileStream fileStream = File.Create(_fileSystemConfig.MusicFolderPath + '\\' + songName + ".mp3");
        await mp3Stream.CopyToAsync(fileStream);
    }
}