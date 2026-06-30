using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Wasted
{
    [BepInPlugin("x.whiteknuckle.wastedpercent", "Wastedpercent", "1.0.4")]
public class Plugin : BaseUnityPlugin
{
    private bool _wastedActive = false;
    private float _drunkLevel = 1f;
    
    // Initialize Logger and Harmony in the constructor
    public new static ManualLogSource Logger { get; private set; }
    private Harmony harmony;

    private void Awake()
    {
        Logger = base.Logger;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Logger.LogInfo("[Wasted%] Loaded");
        
        // Initialize Harmony
        harmony = new Harmony("x.whiteknuckle.wastedpercent");
        harmony.PatchAll();
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

            // Check if player is available immediately after scene load
            ENT_Player player = ENT_Player.GetPlayer();
            if (player == null)
            {
                Logger.LogError("[Wasted%] Player not found on scene load.");
            }
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

            ENT_Player player = ENT_Player.GetPlayer();
            if (player == null)
            {
                Logger.LogError("[Wasted%] Player not found when executing WastedCommand.");
                CommandConsole.Log("Could not find the player.");
                return;
            }

            if (_wastedActive)
            {
                ApplyWineBuff(player);
                CommandConsole.Log("Wasted% effect enabled at level " + _drunkLevel.ToString() + "!");
            }
            else
            {
                RemoveWineBuff(player);
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

            ENT_Player player = ENT_Player.GetPlayer();
            if (player == null)
            {
                Logger.LogError("[Wasted%] Player not found when executing WastedLevelCommand.");
                CommandConsole.Log("Could not find the player.");
                return;
            }

            if (_wastedActive)
            {
                RemoveWineBuff(player);
                ApplyWineBuff(player);
                CommandConsole.Log("Reapplying wasted effect at new level " + _drunkLevel.ToString() + "!");
            }
        }

        private void ApplyWineBuff(ENT_Player player)
        {
            BuffContainer wine = new BuffContainer
            {
                id = "intoxication",
                loseOverTime = false,
                loseRate = 0f,
                buffTime = 1f,
                multiplier = _drunkLevel,
                buffs = new List<BuffContainer.Buff>
                {
                    new BuffContainer.Buff
                    {
                        id = "intoxication",
                        maxAmount = 5f
                    }
                }
            };
            player.Buff(wine);
            _wastedActive = true;
            Logger.LogInfo("[Wasted%] Wine buff applied at level " + _drunkLevel.ToString());
        }

        private void RemoveWineBuff(ENT_Player player)
        {
            player.RemoveBuff("intoxication");
            _wastedActive = false;
            Logger.LogInfo("[Wasted%] Wine buff removed.");
        }
    }
}
