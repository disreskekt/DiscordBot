using System;
using System.Collections.Generic;
using System.Linq;
using Mp3Player.Services.Interfaces;

namespace Mp3Player.Services;

public class SearchService : ISearchService
{
    private readonly IFileSystemService _fileSystemService;
    
    public SearchService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }
    
    public string[] Search(string songName)
    {
        string[] partsOfSongName = songName.ToLowerInvariant()
            .Split(new[]{' ', '-', '_', '=', '+', '(', ')', '&', '%', ';', ':', '/', '\\'},
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        
        Dictionary<string,int> dictionary = new();
        foreach (string fsName in _fileSystemService.GetAll())
        {
            string toLower = fsName.ToLowerInvariant();
            
            int containCount = partsOfSongName.Count(partOfSongName => toLower.Contains(partOfSongName));
            
            dictionary.Add(toLower, containCount);
        }
        
        return dictionary.Where(kvp => kvp.Value > 0)
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .Select(kvp => kvp.Key)
            .ToArray();
    }
}