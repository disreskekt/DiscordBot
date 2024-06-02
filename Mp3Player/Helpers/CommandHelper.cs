using Discord;

namespace Mp3Player.Helpers;

public static class CommandHelper
{
    public static MessageComponent BuildButtons(int page, int pageAmount)
    {
        // int indexOfSlash = message.IndexOf('/');
        // string pageNumberString = message.Substring(0, indexOfSlash);
        // int pageNumber = Convert.ToInt32(pageNumberString);
        //
        // int indexOfColon = message.IndexOf(':');
        // string pageAmountString = message.Substring(indexOfSlash + 1, indexOfColon - indexOfSlash - 1);
        // int pageAmout = Convert.ToInt32(pageAmountString);
        
        ComponentBuilder componentBuilder = new ComponentBuilder();
        if (page > 1)
        {
            componentBuilder.WithButton(customId: "left_arrow_page", emote: new Emoji("⬅"));
        }
        if (page < pageAmount)
        {
            componentBuilder.WithButton(customId: "right_arrow_page", emote: new Emoji("➡"));
        }
        
        return componentBuilder.Build();
    }
}