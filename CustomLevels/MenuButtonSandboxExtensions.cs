using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace CustomLevels
{
    internal static class MenuButtonSandboxExtensions
    {
        public static void ToggleMappingSet(MenuButtonSandbox instance)
        {
            var toggleMappingField = typeof(MenuButtonSandbox).GetField("toggleMapping", BindingFlags.NonPublic | BindingFlags.Instance);
            var editorIconField = typeof(MenuButtonSandbox).GetField("editorIcon", BindingFlags.NonPublic | BindingFlags.Instance);
            var gameIconField = typeof(MenuButtonSandbox).GetField("gameIcon", BindingFlags.NonPublic | BindingFlags.Instance);

            var editorIcon = (Sprite)editorIconField.GetValue(instance);
            var gameIcon = (Sprite)gameIconField.GetValue(instance);

            var mapping = new Dictionary<GameMode, (GameMode, Sprite)>
            {
                { GameMode.Game, (GameMode.SandboxEdit, editorIcon) },
                { GameMode.SandboxEdit, (GameMode.Game, gameIcon) },
                { GameMode.SandboxPlay, (GameMode.Game, gameIcon) },
                { (GameMode)3, (GameMode.Game, gameIcon) }
            };

            toggleMappingField.SetValue(instance, mapping);
        }
    }
}
