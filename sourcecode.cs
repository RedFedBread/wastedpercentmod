using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Wasted
{
    [BepInPlugin("x.whiteknuckle.wastedpercent", "Wastedpercent", "1.0.1")]
    [BepInProcess("White Knuckle.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private Coroutine? _wineCoroutine;
        private bool _wastedActive = false;
        private float _drunkLevel = 1f;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Logger.LogInfo("[Wasted%] Loaded");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (CommandConsole.instance == null) return;

            CommandConsole.BuildCommand("wasted", new System.Action<string[]>(WastedCommand))
                .Description("(true/false): Toggle the wasted effect")
                .AutocompleteSingleBool()
                .NotCheat();

            CommandConsole.BuildCommand("wastedlevel", new System.Action<string[]>(WastedLevelCommand))
                .Description("[value]: Set the drunk level, reapplies if wasted is active")
                .NotCheat();

            Logger.LogInfo("[Wasted%] Scene loaded");
        }

        private void WastedCommand(string[] args)
        {
            if (args.Length == 0)
            {
                _wastedActive = !_wastedActive;
            }
            else if (!bool.TryParse(args[0], out _wastedActive))
            {
                CommandConsole.Log("Unable to parse " + args[0] + " arg needs to be a boolean (true/false).");
                return;
            }

            if (_wastedActive)
            {
                if (_wineCoroutine != null)
                    StopCoroutine(_wineCoroutine);
                _wineCoroutine = StartCoroutine(WineRoutine());
                CommandConsole.Log("Wasted% effect enabled at level " + _drunkLevel.ToString() + "!");
            }
            else
            {
                ENT_Player player = ENT_Player.GetPlayer();
                if (player != null)
                    player.RemoveBuff("intoxication");

                CommandConsole.Log("Wasted% effect disabled!");
            }
        }

        private void WastedLevelCommand(string[] args)
        {
            if (args.Length == 0)
            {
                CommandConsole.Log("Incorrect Syntax: wastedlevel [value]");
                return;
            }

            float level;
            if (!float.TryParse(args[0], out level))
            {
                CommandConsole.Log("Unable to parse " + args[0] + " arg needs to be a number.");
                return;
            }

            _drunkLevel = level;
            CommandConsole.Log("Drunk level set to " + _drunkLevel.ToString());

            if (_wastedActive)
            {
                ENT_Player player = ENT_Player.GetPlayer();
                if (player != null)
                    player.RemoveBuff("intoxication");

                if (_wineCoroutine != null)
                    StopCoroutine(_wineCoroutine);

                _wineCoroutine = StartCoroutine(WineRoutine());
                CommandConsole.Log("Reapplying wasted effect at new level " + _drunkLevel.ToString() + "!");
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
                wine.multiplier = _drunkLevel;
                wine.buffs = new List<BuffContainer.Buff>
                {
                    new BuffContainer.Buff
                    {
                        id = "intoxication",
                        maxAmount = 5f
                    }
                };

                player.Buff(wine);
                _wastedActive = true;
                Logger.LogInfo("[Wasted%] Wine buff applied at level " + _drunkLevel.ToString());
            }
            else
            {
                Logger.LogError("[Wasted%] Could not find player!");
            }
        }
    }
}
