extern alias SteamworksNetUnstripped;

using MelonLoader;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QuadrataPatcher
{
    public static class SteamworksManager
    {
        public static bool IsAvailable { get; private set; } = false;
        public static string SteamUsername { get; private set; } = "UncoolUser";
        public static ulong SteamID { get; private set; } = 0;
        public static bool IsCool { get; set; } = false;


        public static void Initialize()
        {
            var (coolEnough, fakeCoolnessID) = AmICoolEnoughForTheParty();

            if (!coolEnough)
            {
                if (fakeCoolnessID)
                {
                    string[] bouncerLines = new[]
                    {
                        "The bouncer squints at your ID, then shakes his head. 'Nice try, imposter.'",
                        "'This ID's faker than a three-dollar bill,' the bouncer growls. You're not getting in.",
                        "You flash your coolness ID. The bouncer laughs. 'That's adorable. Now scram.'",
                        "The velvet rope stays put. 'Fake ID? Not tonight, buddy.'",
                        "'We’ve seen that fake before,' the bouncer mutters. 'Try again when you’re legit.'"
                    };
                    var random = new Random();
                    MelonLogger.Error(bouncerLines[random.Next(bouncerLines.Length)]);
                }
                else
                {
                    MelonLogger.Error("You are not cool enough for the party.");
                }
                return;
            }
            try
            {
                MelonLogger.Msg("You ARE cool enough for the party");

                var steamAPIType = Type.GetType("Steamworks.SteamAPI, Steamworks.NET");
                var steamFriendsType = Type.GetType("Steamworks.SteamFriends, Steamworks.NET");
                var steamUserType = Type.GetType("Steamworks.SteamUser, Steamworks.NET");

                if (steamAPIType == null || steamFriendsType == null || steamUserType == null)
                    throw new FileNotFoundException("Steamworks.NET types not found");

                var initMethod = steamAPIType.GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
                var getPersonaNameMethod = steamFriendsType.GetMethod("GetPersonaName", BindingFlags.Public | BindingFlags.Static);
                var getSteamIDMethod = steamUserType.GetMethod("GetSteamID", BindingFlags.Public | BindingFlags.Static);

                if (initMethod == null || getPersonaNameMethod == null || getSteamIDMethod == null)
                    throw new MissingMethodException("One or more Steamworks.NET methods not found");

                bool success = (bool)initMethod.Invoke(null, null);
                if (success)
                {
                    string username = getPersonaNameMethod.Invoke(null, null) as string;
                    object steamIDStruct = getSteamIDMethod.Invoke(null, null);

                    if (steamIDStruct != null)
                    {
                        var accountIDField = steamIDStruct.GetType().GetField("m_AccountID", BindingFlags.Public | BindingFlags.Instance);
                        if (accountIDField != null)
                        {
                            object rawID = accountIDField.GetValue(steamIDStruct);
                            if (rawID is uint accountID)
                            {
                                SteamID = accountID;
                            }
                            else
                            {
                                MelonLogger.Warning("m_AccountID field exists but could not be cast to uint.");
                            }
                        }
                        else
                        {
                            string steamIDString = steamIDStruct.ToString();
                            if (ulong.TryParse(steamIDString, out ulong parsedID))
                            {
                                SteamID = parsedID;
                            }
                            else
                            {
                                MelonLogger.Warning($"Failed to parse SteamID from string: '{steamIDString}'");
                            }
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("GetSteamID() returned null.");
                    }

                    if (!string.IsNullOrEmpty(username))
                    {
                        SteamUsername = username;
                        IsAvailable = true;

                        if (SteamID != 0)
                        {
                            MelonLogger.Msg($"Unstripped Steamworks initialized successfully as '{SteamUsername}' with SteamID: {SteamID}");
                            if (SteamID == 76561198380362844) { MelonLogger.Msg("You really are cool, VIP CHISENOA"); }
                            IsCool = true;
                        }
                        else
                        {
                            MelonLogger.Msg($"Unstripped Steamworks initialized successfully as '{SteamUsername}', but SteamID could not be retrieved.");
                        }
                    }
                    else
                    {
                        MelonLogger.Warning("Steamworks initialized, but failed to retrieve username.");
                    }
                }
                else
                {
                    MelonLogger.Warning("Steamworks failed to initialize. Custom online features may not work.");
                }
            }
            catch (FileNotFoundException)
            {
                MelonLogger.Warning("Some Online features will be disabled. For full functionality, please use **unstripped Steamworks.NET libraries**.");
                MelonLogger.Msg("You can find setup instructions and the correct libraries on the mod's GitHub page.");
            }
            catch (MissingMethodException ex)
            {
                MelonLogger.Warning($"Steamworks method missing: {ex.Message}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Steamworks initialization failed: {ex.Message}");
            }
        }

        private static (bool coolEnough, bool fakeCoolnessID) AmICoolEnoughForTheParty()
        {
            bool coolEnough = false;
            bool FakeCoolnessID = true;
            try
            {
                int[] asciiCodes = { 115, 116, 101, 97, 109, 95, 97, 112, 105, 54, 52, 46, 100, 108, 108 };
                string secretSauce = new string(asciiCodes.Select(code => (char)code).ToArray());

                string[] flavorTrail = new[]{new string(new[] { (char)81, (char)117, (char)97, (char)100, (char)114, (char)97, (char)116, (char)97, (char)95, (char)68, (char)97, (char)116, (char)97 }),new string(new[] { (char)80, (char)108, (char)117, (char)103, (char)105, (char)110, (char)115 }),new string(new[] { (char)120, (char)56, (char)54, (char)95, (char)54, (char)52 })};

                string hideyHole = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,flavorTrail[0],flavorTrail[1],flavorTrail[2],secretSauce);

                byte[] partySnacks = File.ReadAllBytes(hideyHole);
                long perfectBiteSize = 298856;
                string legendaryFlavor = "A44E5537939AE4EEBC69000589AA9B2437A667813A1657CC779198BAE9B815A9";

                if (partySnacks.LongLength == perfectBiteSize)
                {
                    using (var magicMixer = System.Security.Cryptography.SHA256.Create())
                    {
                        byte[] sprinkleDust = magicMixer.ComputeHash(partySnacks);
                        string finalJudgment = BitConverter.ToString(sprinkleDust).Replace("-", "");

                        if (string.Equals(finalJudgment, legendaryFlavor, StringComparison.OrdinalIgnoreCase))
                        {
                            FakeCoolnessID = !(coolEnough = true);
                        }
                    }
                }
            }
            catch
            {
                string[] coolnessMessages = new[]
                {
                    "No ID, no entry. Coolness credentials not presented.",//
                    "Denied at the door: Coolness ID missing.",//
                    "You tried to roll in without a coolness pass. Not happening.",//
                    "Access denied: Coolness ID not found. Come back when you're certified.",//
                    "Hold up—where's your coolness ID? Can't let you in without it."
                };
                var random = new Random();
                int index = random.Next(coolnessMessages.Length);
                MelonLogger.Error(coolnessMessages[index]);
            }

            return (coolEnough, FakeCoolnessID);
        }




        public static string GetSteamUsername() => SteamUsername;

        public static ulong GetSteamID() => SteamID;

        public static void Update()
        {
            if (IsAvailable)
            {
                try
                {
                    SteamworksNetUnstripped::Steamworks.SteamAPI.RunCallbacks();
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"Steamworks callback error: {ex.Message}");
                }
            }
        }

        public static void Shutdown()
        {
            if (IsAvailable)
            {
                try
                {
                    SteamworksNetUnstripped::Steamworks.SteamAPI.Shutdown();
                    MelonLogger.Msg("Steamworks shut down successfully.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"Steamworks shutdown error: {ex.Message}");
                }
                finally
                {
                    IsAvailable = false;
                }
            }
        }
    }
}
