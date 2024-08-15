using Steamworks;

namespace com.github.zehsteam.SellMyScrap;

internal class SteamUtils
{
    public static bool IsLocalPlayerInsym()
    {
        return IsLocalPlayer("Insym", 76561198008109303);
    }

    public static bool IsLocalPlayerThorlar()
    {
        return IsLocalPlayer("Thorlar", 76561197964616102);
    }

    public static bool IsLocalPlayerTakerst()
    {
        return IsLocalPlayer("Takerst", 76561197980238122);
    }

    public static bool IsLocalPlayerPsycho()
    {
        return IsLocalPlayer("PsychoHypnotic", 76561197970440803);
    }

    public static bool IsLocalPlayer(string username, ulong steamId)
    {
        if (!SteamClient.IsValid) return false;
        if (!SteamClient.IsLoggedOn) return false;

        if (SteamClient.Name == username) return true;
        if (SteamClient.SteamId == steamId) return true;

        return false;
    }
}
