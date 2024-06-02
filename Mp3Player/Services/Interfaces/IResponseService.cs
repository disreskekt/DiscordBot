namespace Mp3Player.Services.Interfaces;

public interface IResponseService
{
    public string BuildSongListMessageWithPages(string[] songNames, int page, int pageAmount);
    public string BuildSongListMessage(string[] songNames);
}