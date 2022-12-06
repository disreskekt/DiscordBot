using System;
using Discord;

namespace DiscordBot.Helpers.Extensions;

public static class TypeExtensions
{
    public static ApplicationCommandOptionType GetDiscordType(this Type type)
    {
        return type switch
        {
            not null when type == typeof(string) => ApplicationCommandOptionType.String,
            not null when type == typeof(int) => ApplicationCommandOptionType.Integer,
            not null when type == typeof(int?) => ApplicationCommandOptionType.Integer,
            not null when type == typeof(IAttachment) => ApplicationCommandOptionType.Attachment,
            _ => throw new Exception("Нет дискордного типа для параметра команды")
        };
    }
}