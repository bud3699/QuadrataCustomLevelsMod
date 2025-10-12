using HarmonyLib;
using Mindlabor.Utils;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QuadrataPatcher
{
    public static class LevelManagerExtension
    {
        public static IEnumerator HandleGameModeLevelLoad(
            AudioSourceSettings firstDiamond,
            AudioSourceSettings secondDiamond,
            int side,
            AudioSourceSettings success)
        {
            var levelManagerType = typeof(LevelManager);
            var traverse = Traverse.Create(levelManagerType);

            var addDiamondsMethod = levelManagerType.GetMethod("AddDiamonds", BindingFlags.Public | BindingFlags.Static);
            addDiamondsMethod?.Invoke(null, new object[] { 1 });

            int diamonds = traverse.Field("collectedDiamonds").GetValue<int>();
            Debug.Log($"Diamonds collected: {diamonds}");

            if (diamonds == 1)
            {
                AudioManager.instance?.PlaySFX(firstDiamond, side);
                yield break;
            }

            if (diamonds < 2)
            {
                yield break;
            }

            AudioManager.instance?.PlaySFX(secondDiamond, side);
            AudioManager.instance?.PlaySFX(success);

            Finder.allCharacters.ToList().ForEach(c => c.MakeUninteractable());

            if (Director.gameMode == GameMode.SandboxPlay)
            {
                Debug.Log("Finished Custom Level");
                CustomLevelCompleteUI.Show();
            }
            if (Director.gameMode == GameMode.Game || Director.gameMode.ToString() == "3")
            {
                int levelIndex = traverse.Field("levelIndex").GetValue<int>();
                LevelAnimation.isLevelLoading = true;

                if (Director.gameMode == GameMode.Game)
                {
                    FBPP.SetInt(SaveManager.currentLevel, levelIndex + 1);
                }

                Character leftCharacter = Finder.allCharacters.FirstOrDefault(c => c.transform.position.x < 0f);
                Character rightCharacter = Finder.allCharacters.FirstOrDefault(c => c.transform.position.x > 0f);

                yield return new WaitWhile(() => leftCharacter.moving || rightCharacter.moving);
                yield return new WaitForSecondsRealtime(0.1f);
                yield return new WaitWhile(() => leftCharacter.moving || rightCharacter.moving);
                yield return new WaitForSecondsRealtime(0.2f);

                if (Director.gameMode == GameMode.Game)
                {
                    CoroutineUtils.RunCoroutine(LevelManager.LoadLevel(levelIndex + 1));
                }
                else if (Director.gameMode.ToString() == "3")
                {
                    Director.instance.CloseLevel();
                    yield return new WaitWhile(() => LevelAnimation.isLevelLoading);
                    yield return new WaitForSecondsRealtime(0.1f);
                    Director.instance?.Init();
                    Debug.Log("Load Finished screen here ??? Load something here to say you've completed it.. for now just reload level..");
                }
            }

        }

    }
}
