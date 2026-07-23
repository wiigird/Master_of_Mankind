using Godot;

namespace Master_of_Mankind.Master_of_MankindCode.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions
{
    private static string ResourcePath(params string[] parts) =>
        $"{MainFile.ResPath}/{string.Join("/", parts.Select(part => part.Trim('/', '\\')))}";

    public static string ImagePath(this string path)
    {
        return ResourcePath("images", path);
    }

    public static string CardImagePath(this string path)
    {
        path = ResourcePath("images", "card_portraits", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find card image path: " + path);
        return ResourcePath("images", "card_portraits", "card.png");
    }

    public static string BigCardImagePath(this string path)
    {
        path = ResourcePath("images", "card_portraits", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big card image path: " + path);
        return ResourcePath("images", "card_portraits", "big", "card.png");
    }

    public static string PowerImagePath(this string path)
    {
        path = ResourcePath("images", "powers", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find power image path: " + path);
        return ResourcePath("images", "powers", "power.png");
    }

    public static string BigPowerImagePath(this string path)
    {
        path = ResourcePath("images", "powers", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big power image path: " + path);
        return ResourcePath("images", "powers", "big", "power.png");
    }

    public static string RelicImagePath(this string path)
    {
        path = ResourcePath("images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find relic image path: " + path);
        return ResourcePath("images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = ResourcePath("images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big relic image path: " + path);
        return ResourcePath("images", "relics", "big", "relic.png");
    }

    public static string CharacterUiPath(this string path)
    {
        return ResourcePath("images", "charui", path);
    }
}
