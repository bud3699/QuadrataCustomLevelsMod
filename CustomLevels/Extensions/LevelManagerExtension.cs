using System.Collections;
using System.Linq;
using UnityEngine;

namespace QuadrataPatcher
{
    public static class LevelManagerExtension
    {
        public static IEnumerator HandleGameModeLevelLoad()
        {
            if (Director.gameMode.ToString() == "3")
            {
                LevelAnimation.isLevelLoading = true;

                Character leftCharacter = Finder.allCharacters.FirstOrDefault(c => c.transform.position.x < 0f);
                Character rightCharacter = Finder.allCharacters.FirstOrDefault(c => c.transform.position.x > 0f);

                yield return new WaitWhile(() => leftCharacter.moving || rightCharacter.moving);
                yield return new WaitForSecondsRealtime(0.1f);
                yield return new WaitWhile(() => leftCharacter.moving || rightCharacter.moving);
                yield return new WaitForSecondsRealtime(0.2f);

                Debug.Log("Load Finished screen here ??? Load something here to say you've completed it..");
            }
        }
    }
}
