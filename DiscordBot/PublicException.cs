using System;
using Discord.WebSocket;

namespace DiscordBot;

public class PublicException : Exception
{
    private static DiscordSocketClient _client;
    private const ulong OUR_GUILD = 663898503076118528;
    private const ulong BRANCH_NAME = 1021858121636904961;

    public static void SetClient(DiscordSocketClient client)
    {
        _client = client;
    }
    
    public PublicException(string message)
        : base(message)
    {
        // SocketChannel socketChannel = _client.GetChannel(CHANNEL_ID);
        //
        // IGroupChannel? channel = socketChannel as IGroupChannel;
        SocketTextChannel targetThread = _client.GetGuild(OUR_GUILD).GetTextChannel(BRANCH_NAME);
        targetThread.SendMessageAsync(message).GetAwaiter().GetResult();
    }
}