namespace KyInfo.Application.Discussions;

using KyInfo.Contracts.Discussions;

public static class StickerCatalog
{
    private static readonly StickerDto[] Items =
    [
        new() { Code = "neutral", Label = "面无表情", Url = "/stickers/neutral.png" },
        new() { Code = "grin_big", Label = "大笑", Url = "/stickers/grin_big.png" },
        new() { Code = "grin", Label = "露齿笑", Url = "/stickers/grin.png" },
        new() { Code = "grin_squint", Label = "眯眼笑", Url = "/stickers/grin_squint.png" },
        new() { Code = "beam", Label = "笑眯眯", Url = "/stickers/beam.png" },
        new() { Code = "grin_open", Label = "张嘴笑", Url = "/stickers/grin_open.png" },
        new() { Code = "smile", Label = "微笑", Url = "/stickers/smile.png" },
        new() { Code = "upside_down", Label = "倒脸", Url = "/stickers/upside_down.png" },
        new() { Code = "frown_slight", Label = "微皱眉", Url = "/stickers/frown_slight.png" },
        new() { Code = "frown", Label = "皱眉", Url = "/stickers/frown.png" },
        new() { Code = "pout_angry", Label = "生气", Url = "/stickers/pout_angry.png" },
        new() { Code = "neutral_line", Label = "平脸", Url = "/stickers/neutral_line.png" },
        new() { Code = "expressionless", Label = "无表情", Url = "/stickers/expressionless.png" },
        new() { Code = "open_mouth", Label = "张嘴", Url = "/stickers/open_mouth.png" },
        new() { Code = "hushed", Label = "嘘", Url = "/stickers/hushed.png" },
        new() { Code = "astonished", Label = "惊讶", Url = "/stickers/astonished.png" },
        new() { Code = "flushed", Label = "脸红", Url = "/stickers/flushed.png" },
        new() { Code = "frown_open", Label = "皱眉张嘴", Url = "/stickers/frown_open.png" },
        new() { Code = "anguished", Label = "痛苦", Url = "/stickers/anguished.png" },
        new() { Code = "angry", Label = "愤怒", Url = "/stickers/angry.png" },
        new() { Code = "angry_red", Label = "红脸怒", Url = "/stickers/angry_red.png" },
        new() { Code = "relieved", Label = "放松", Url = "/stickers/relieved.png" },
        new() { Code = "unamused", Label = "无语", Url = "/stickers/unamused.png" },
        new() { Code = "smirk", Label = "坏笑", Url = "/stickers/smirk.png" },
        new() { Code = "wink", Label = "眨眼", Url = "/stickers/wink.png" },
        new() { Code = "kiss", Label = "飞吻", Url = "/stickers/kiss.png" },
        new() { Code = "heart_eyes", Label = "爱心眼", Url = "/stickers/heart_eyes.png" },
        new() { Code = "kissing", Label = "亲亲", Url = "/stickers/kissing.png" },
        new() { Code = "kiss_closed", Label = "闭眼亲", Url = "/stickers/kiss_closed.png" },
        new() { Code = "kiss_smile", Label = "微笑亲", Url = "/stickers/kiss_smile.png" },
        new() { Code = "pensive", Label = "沉思", Url = "/stickers/pensive.png" },
        new() { Code = "persevere", Label = "坚持", Url = "/stickers/persevere.png" },
        new() { Code = "blush", Label = "害羞", Url = "/stickers/blush.png" },
        new() { Code = "smile_eyes", Label = "微笑眼", Url = "/stickers/smile_eyes.png" },
        new() { Code = "hug", Label = "拥抱", Url = "/stickers/hug.png" },
        new() { Code = "dizzy", Label = "晕", Url = "/stickers/dizzy.png" },
        new() { Code = "surprised", Label = "吃惊", Url = "/stickers/surprised.png" },
        new() { Code = "weary", Label = "疲惫", Url = "/stickers/weary.png" },
        new() { Code = "squint_tongue", Label = "眯眼吐舌", Url = "/stickers/squint_tongue.png" },
        new() { Code = "tongue", Label = "吐舌", Url = "/stickers/tongue.png" },
        new() { Code = "wink_tongue", Label = "眨眼吐舌", Url = "/stickers/wink_tongue.png" },
        new() { Code = "yum", Label = "美味", Url = "/stickers/yum.png" },
        new() { Code = "roll_eyes", Label = "翻白眼", Url = "/stickers/roll_eyes.png" },
        new() { Code = "wide_eyes", Label = "瞪眼", Url = "/stickers/wide_eyes.png" },
        new() { Code = "grimace", Label = "龇牙", Url = "/stickers/grimace.png" },
        new() { Code = "innocent", Label = "天使", Url = "/stickers/innocent.png" },
        new() { Code = "teeth", Label = "露齿", Url = "/stickers/teeth.png" },
        new() { Code = "sweat_smile", Label = "汗笑", Url = "/stickers/sweat_smile.png" },
        new() { Code = "sad_sweat", Label = "汗丧", Url = "/stickers/sad_sweat.png" },
        new() { Code = "cry", Label = "流泪", Url = "/stickers/cry.png" },
        new() { Code = "disappointed", Label = "失望", Url = "/stickers/disappointed.png" },
        new() { Code = "cold_sweat", Label = "冷汗", Url = "/stickers/cold_sweat.png" },
        new() { Code = "worried", Label = "担心", Url = "/stickers/worried.png" },
        new() { Code = "fearful", Label = "害怕", Url = "/stickers/fearful.png" },
        new() { Code = "joy_tears", Label = "笑哭", Url = "/stickers/joy_tears.png" },
        new() { Code = "sleepy", Label = "困", Url = "/stickers/sleepy.png" },
        new() { Code = "cool", Label = "酷", Url = "/stickers/cool.png" },
        new() { Code = "mask", Label = "口罩", Url = "/stickers/mask.png" },
        new() { Code = "sleeping", Label = "睡觉", Url = "/stickers/sleeping.png" },
        new() { Code = "sob", Label = "大哭", Url = "/stickers/sob.png" },
        new() { Code = "bandage", Label = "受伤", Url = "/stickers/bandage.png" },
        new() { Code = "money", Label = "财迷", Url = "/stickers/money.png" },
        new() { Code = "think", Label = "思考", Url = "/stickers/think.png" },
        new() { Code = "thermometer", Label = "发烧", Url = "/stickers/thermometer.png" },
        new() { Code = "zipper", Label = "闭嘴", Url = "/stickers/zipper.png" },
        new() { Code = "nerd", Label = "书呆子", Url = "/stickers/nerd.png" },
        new() { Code = "triumph", Label = "得意", Url = "/stickers/triumph.png" },
        new() { Code = "poop", Label = "便便", Url = "/stickers/poop.png" },
    ];

    private static readonly HashSet<string> ValidCodes = new(
        Items.Select(x => x.Code),
        StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<StickerDto> GetAll() => Items;

    public static bool IsValidCode(string code) => ValidCodes.Contains(code);
}
