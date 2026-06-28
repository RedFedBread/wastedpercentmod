using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Wasted
{
    [BepInPlugin("yourname.whiteknuckle.wasted", "Wasted%", "1.0.0")]
    [BepInProcess("White Knuckle.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private Coroutine? _wineCoroutine;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Logger.LogInfo("[Wasted%] Loaded");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (CommandConsole.instance != null)
            {
                CommandConsole.hasCheated = true;
                CL_GameManager.gamemode.allowAchievements = false;

                Logger.LogInfo("[Wasted%] Scene loaded, applying wine buff");

                if (_wineCoroutine != null)
                    StopCoroutine(_wineCoroutine);

                _wineCoroutine = StartCoroutine(WineRoutine());
            }
        }

        private IEnumerator WineRoutine()
        {
            yield return new WaitForSeconds(2f);

            ENT_Player player = ENT_Player.GetPlayer();

            if (player != null)
            {
                BuffContainer wine = new BuffContainer();
                wine.id = "intoxication";
                wine.loseOverTime = false;
                wine.loseRate = 0f;
                wine.buffTime = 1f;
                wine.multiplier = 20f;
                wine.buffs = new List<BuffContainer.Buff>
                {
                    new BuffContainer.Buff
                    {
                        id = "intoxication",
                        maxAmount = 5f
                    }
                };

                player.Buff(wine);

if (CL_UIManager.instance != null)
{
    var ui = CL_UIManager.instance;

    ui.SetVignette("boost", 1f);
    ui.SetVignetteTarget("boost", 1f);
    ui.SetVignetteColor("boost", Color.yellow);
}

                Logger.LogInfo("[Wasted%] Wine buff applied successfully");
            }
            else
            {
                Logger.LogError("[Wasted%] Could not find player!");
            }
        }
    }
}
