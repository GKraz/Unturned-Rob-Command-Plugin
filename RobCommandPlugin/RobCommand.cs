using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace RobCommandPlugin
{
    public class RobCommand : CommandUtils, IRocketCommand
    {
        private static Plugin Plugin => Plugin.Instance;
        private static float RobberyCooldown => Plugin.RobberyCooldown;
        private static float VictimCooldown => Plugin.VictimCooldown;
        private static Color RobMessageColor => Plugin.RobMessageColor;
        private static Color OtherMessageColor => Plugin.OtherMessageColor;

        public void Execute(IRocketPlayer caller, string[] args)
        {
            if (args.Length > 1)
            {
                PrivateChat(caller, Plugin.Translate("Rob:WrongUsage"), OtherMessageColor);
                return;
            }

            if (args.Length == 0)
            {
                StartRob(caller);
                return;
            }

            switch (args[0])
            {
                case "start":
                case "s":    
                    StartRob(caller);
                    break;
                
                case "assist":
                case "a":
                case "as":
                case "ass":
                    AssistRob(caller);
                    break;
                
                case "end":
                case "e":    
                    EndRob(caller);
                    break;
                
                case "help":
                case "h":    
                    PrivateChat(caller, Plugin.Translate("Rob:Help1"), OtherMessageColor);
                    PrivateChat(caller, Plugin.Translate("Rob:Help2"), OtherMessageColor);
                    PrivateChat(caller, Plugin.Translate("Rob:Help3"), OtherMessageColor);
                    PrivateChat(caller, Plugin.Translate("Rob:Help4"), OtherMessageColor);
                    break;

                default:
                    PrivateChat(caller, Plugin.Translate("Rob:WrongUsage"), OtherMessageColor);
                    break;
            }
        }

        
        private static void StartRob(IRocketPlayer caller)
        {
            var uPlayer = (UnturnedPlayer) caller;
            
            if (Plugin.RobberyInfos.ContainsKey(uPlayer.Player))
            {
                PrivateChat(caller, Plugin.Translate("Rob:AlreadyRobber"), OtherMessageColor);
                return;
            }
            
            if (Plugin.RobberCooldowns.TryGetValue(uPlayer.Player, out var robTime))
            {
                if (DateTime.UtcNow < robTime.ToUniversalTime().AddMinutes(RobberyCooldown))
                {
                    PrivateChat(
                        caller, 
                        Plugin.Translate(
                            "RobStart:RobberCooldown", 
                            Math.Round((robTime.ToUniversalTime().AddMinutes(RobberyCooldown) - 
                                        DateTime.UtcNow).TotalSeconds)), 
                        OtherMessageColor);
                    return;
                }
            }
            
            var targetInfo = TraceRay(uPlayer.Player.look.aim, 10f, RayMasks.PLAYER | RayMasks.PLAYER_INTERACT);
            
            if (targetInfo.player == null || targetInfo.player.name == uPlayer.Player.name)
            {
                PrivateChat(caller, Plugin.Translate("RobStart:NoPlayer"), OtherMessageColor);
                return;
            }
            
            if (Plugin.VictimCooldowns.TryGetValue(targetInfo.player, out var victimTime))
            {
                if (DateTime.UtcNow < victimTime.ToUniversalTime().AddMinutes(VictimCooldown))
                {
                    PrivateChat(caller, Plugin.Translate("RobStart:VictimCooldown"), OtherMessageColor);
                    return;
                }
            }
            
            var allNodes = LocationDevkitNodeSystem.Get().GetAllNodes();
            var node = allNodes.OrderBy(n => Vector3.Distance(n.transform.position, uPlayer.Position))
                .FirstOrDefault();
            var location = node != null ? node.locationName : "an unknown location";
            
            Plugin.RobberyInfos.Add(uPlayer.Player, (DateTime.UtcNow, targetInfo.player, new List<Player> {uPlayer.Player}, location));

            SendWebhook("Rob Start", $"**{uPlayer.Player.name}** is robbing **{targetInfo.player.name}** at **{location}**.");
            GlobalChat($"Someone is being robbed at {location}!", RobMessageColor);

            Plugin.RobberCooldowns[uPlayer.Player] = DateTime.UtcNow;
            Plugin.VictimCooldowns[targetInfo.player] = DateTime.UtcNow;
        }
        
        
        private static void AssistRob(IRocketPlayer caller)
        {
            var uPlayer = (UnturnedPlayer) caller;

            if (Plugin.RobberyInfos.ContainsKey(uPlayer.Player))
            {
                PrivateChat(caller, Plugin.Translate("Rob:AlreadyRobber"), OtherMessageColor);
                return;
            }

            var otherSPlayer = Provider.clients.FirstOrDefault(p =>
                UnturnedPlayer.FromSteamPlayer(p).Player.quests.isMemberOfSameGroupAs(uPlayer.Player)
                && Plugin.RobberyInfos.ContainsKey(p.player));

            if (otherSPlayer == null)
            {
                PrivateChat(caller, Plugin.Translate("RobAssist:NoTeam"), OtherMessageColor);
                return;
            }
            
            var otherUPlayer = UnturnedPlayer.FromSteamPlayer(otherSPlayer);

            Plugin.RobberyInfos[otherUPlayer.Player].Item3.Add(otherUPlayer.Player);
            
            SendWebhook("Rob Assist", $"**{uPlayer.Player.name}** is assisting **{otherUPlayer.Player.name}** in a robbery.");
            GlobalChat("A player is assisting in the robbery!", RobMessageColor);
        }
        
        
        private static void EndRob(IRocketPlayer caller)
        {
            var uPlayer = (UnturnedPlayer) caller;
            
            var valuePair = Plugin.RobberyInfos.FirstOrDefault(pair => pair.Value.Item3.Contains(uPlayer.Player));

            if (valuePair.Key is null)
            {
                PrivateChat(caller, Plugin.Translate("RobEnd:NoRobbery"), OtherMessageColor);
                return;
            }

            Plugin.RobberyInfos.Remove(valuePair.Key);
            
            SendWebhook("Rob End", $"**{valuePair.Key.name}**'s robbery has concluded.");
            GlobalChat($"The robbery at {valuePair.Value.Item4} has concluded.", RobMessageColor);
        }

        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "rob";
        public string Help => "Start, assist, or end a robbery.";
        public string Syntax => "/rob <start|end|assist>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "rob" };
    }
}