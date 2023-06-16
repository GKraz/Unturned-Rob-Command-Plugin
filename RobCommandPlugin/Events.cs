using System;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RobCommandPlugin
{
    public static class Events
    {
        private static Plugin Plugin => Plugin.Instance;
        private static float RobberyLength => Plugin.RobberyLength;
        private static Color RobMessageColor => Plugin.RobMessageColor;
        
        public static void RobberyTimeCheck()
        {
            var expiredRobberies = Plugin.RobberyInfos.Where(pair =>
                DateTime.UtcNow > pair.Value.Item1.ToUniversalTime().AddMinutes(RobberyLength));
            
            foreach (var valuePair in expiredRobberies)
            {
                Plugin.RobberyInfos.Remove(valuePair.Key);
                
                CommandUtils.SendWebhook("Rob End", $"**{valuePair.Key.name}**'s robbery has expired.");
                CommandUtils.GlobalChat($"The robbery at {valuePair.Value.Item4} has concluded.", RobMessageColor);
                CommandUtils.PrivateChat(UnturnedPlayer.FromPlayer(valuePair.Key), "You're robbery has expired.", RobMessageColor);
            }
        }

        public static void PlayerDeath(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
        {
            var valuePair = Plugin.RobberyInfos.FirstOrDefault(pair => pair.Value.Item3.Contains(sender.player));

            if (valuePair.Key == null) return;

            if (!valuePair.Value.Item3.Contains(sender.player)) return;

            valuePair.Value.Item3.Remove(sender.player);
            
            CommandUtils.SendWebhook("Rob Death", $"**{sender.player.name}** died during a robbery.");

            if (!valuePair.Value.Item3.IsEmpty()) return;
            
            CommandUtils.SendWebhook("Rob End", $"**{valuePair.Key.name}**'s robbery has ended.");
            CommandUtils.GlobalChat($"The robbery at {valuePair.Value.Item4} has concluded.", RobMessageColor);
        }
    }
}