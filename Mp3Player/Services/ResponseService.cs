using System.Text;
using Mp3Player.Services.Interfaces;

namespace Mp3Player.Services;

public class ResponseService : IResponseService
{
    public string BuildSongListMessageWithPages(string[] songNames, int page, int pageAmount)
    {
        StringBuilder sb = new StringBuilder()
            .Append(page)
            .Append('/')
            .Append(pageAmount)
            .Append(':')
            .AppendLine();
        
        for (int i = 1; i < songNames.Length + 1; i++)
        {
            sb.Append(i)
                .Append('.')
                .Append(' ')
                .Append(songNames[i - 1]);
            
            if (i != songNames.Length)
            {
                sb.AppendLine();
            }
        }
        
        return sb.ToString();
    }

    public string BuildSongListMessage(string[] songNames)
    {
        StringBuilder sb = new StringBuilder();
        
        for (int i = 1; i < songNames.Length + 1; i++)
        {
            sb.Append(i)
                .Append('.')
                .Append(' ')
                .Append(songNames[i - 1]);
            
            if (i != songNames.Length)
            {
                sb.AppendLine();
            }
        }
        
        return sb.ToString();
    }
}