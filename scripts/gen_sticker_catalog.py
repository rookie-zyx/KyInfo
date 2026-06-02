import json
import pathlib

manifest = pathlib.Path(__file__).resolve().parents[1] / "KyInfo.Blazor/wwwroot/stickers/manifest.json"
items = json.loads(manifest.read_text(encoding="utf-8"))
lines = [
    "namespace KyInfo.Application.Discussions;",
    "",
    "using KyInfo.Contracts.Discussions;",
    "",
    "public static class StickerCatalog",
    "{",
    "    private static readonly StickerDto[] Items =",
    "    [",
]
for it in items:
    label = it["label"].replace("\\", "\\\\").replace('"', '\\"')
    lines.append(
        f'        new() {{ Code = "{it["code"]}", Label = "{label}", Url = "/stickers/{it["file"]}" }},'
    )
lines += [
    "    ];",
    "",
    "    private static readonly HashSet<string> ValidCodes = new(",
    "        Items.Select(x => x.Code),",
    "        StringComparer.OrdinalIgnoreCase);",
    "",
    "    public static IReadOnlyList<StickerDto> GetAll() => Items;",
    "",
    "    public static bool IsValidCode(string code) => ValidCodes.Contains(code);",
    "}",
]
out = pathlib.Path(__file__).resolve().parents[1] / "src/KyInfo.Application/Discussions/StickerCatalog.cs"
out.write_text("\n".join(lines) + "\n", encoding="utf-8")
print(f"written {len(items)} stickers -> {out}")
