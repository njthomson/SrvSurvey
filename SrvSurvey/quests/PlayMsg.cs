using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace SrvSurvey.quests;

/// <summary> A runtime/delivered message. Content fields will be null unless they are overriding statically declared values </summary>
public class PlayMsg
{
    [JsonIgnore, AllowNull] public PlayQuest parent;

    public string id;
    public DateTimeOffset received;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? from;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? subject;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? body;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? chapter;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[]? actions;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool read;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? replied;

    public override string ToString()
    {
        return $"{id}:{subject ?? body} ({received:z})";
    }

    public static PlayMsg send(DefMsg? msg = null, string? from = null, string? subject = null, string? body = null, string? chapter = null)
    {
        if (msg?.actions != null && chapter == null) throw new Exception($"Chapter must be set when using messages with actions. Id: {msg.id}");

        var newMsg = new PlayMsg()
        {
            id = msg?.id ?? DateTimeOffset.UtcNow.ToString("yyyyMMddhhmmss"),
            received = DateTimeOffset.UtcNow,
            chapter = chapter,
            actions = msg?.actions?.Keys.ToArray(),
        };

        // store nothing if the following values match the template Msg

        newMsg.from = from == null || from == msg?.from ? null : from ?? msg!.from;
        newMsg.subject = subject == null || subject == msg?.subject ? null : subject ?? msg!.subject;
        newMsg.body = body == null || body == msg?.body ? null : body ?? msg!.body;

        return newMsg;
    }
}
