using System;
using System.Collections.Generic;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RobCommandPlugin
{
    public class Plugin : RocketPlugin<Configuration>
    {
        public static Plugin Instance { get; private set; }
        
        public static float RobberyCooldown { get; private set; }
        public static float VictimCooldown { get; private set; }
        public static float RobberyLength { get; private set; }
        public static Color RobMessageColor { get; private set; }
        public static Color OtherMessageColor { get; private set; }

        // Robber, Latest rob time
        public static Dictionary<Player, DateTime> RobberCooldowns = new Dictionary<Player, DateTime>();

        // Victim, Latest rob time
        public static Dictionary<Player, DateTime> VictimCooldowns = new Dictionary<Player, DateTime>();

        // Robber, (Start time, Victim, Participants, Location)
        public static Dictionary<Player, (DateTime, Player, List<Player>, string)> RobberyInfos =
            new Dictionary<Player, (DateTime, Player, List<Player>, string)>();
        

        protected override void Load()
        {
            Instance = this;
            RobberyCooldown = Configuration.Instance.RobberyCooldown;
            VictimCooldown = Configuration.Instance.VictimCooldown;
            RobberyLength = Configuration.Instance.RobberyLength;
            RobMessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.RobMessageColor, Color.red);
            OtherMessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.OtherMessageColor, Color.green);

            PlayerLife.onPlayerDied += Events.PlayerDeath;

            Logger.Log("RobCommand loaded");
        }

        protected void Update()
        {
            Events.RobberyTimeCheck();
        }

        protected override void Unload()
        {
            RobberCooldowns.Clear();
            VictimCooldowns.Clear();
            RobberyInfos.Clear();

            Instance = null;

            Logger.Log("RobCommand unloaded");
        }
        
        public override TranslationList DefaultTranslations { get; } = new TranslationList
        {
            { "Rob:Help1", "/rob help: Gives you information on how to use the rob command."},
            { "Rob:Help2", "/rob or /rob start: Begin a robbery on the player you are looking at."},
            { "Rob:Help3", "/rob assist: Assist in a robbery with someone that is in your steam group." },
            { "Rob:Help4", "/rob end: Ends a current robbery. Can be done by original robber or assistants." },
            
            { "Rob:WrongUsage", "Correct command usage: /rob <start|assist|end>" },
            { "Rob:AlreadyRobber", "You are already robbing someone!" },
            
            { "RobStart:NoPlayer", "You need to be looking at a player to use /rob!" },
            { "RobStart:RobberCooldown", "You have to wait {0} seconds to use /rob!" },
            { "RobStart:VictimCooldown", "This person is still on a robbery cooldown!" },
            
            { "RobAssist:NoTeam", "No players to assist or the robber is not in your group!"},
            
            { "RobEnd:NoRobbery", "You are not currently robbing anyone!" },
        };
    }
}