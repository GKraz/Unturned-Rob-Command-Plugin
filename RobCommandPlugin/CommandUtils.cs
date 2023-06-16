using System;
using System.IO;
using System.Net;
using System.Text;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RobCommandPlugin
{
    public abstract class CommandUtils
    {
        public static void PrivateChat(IRocketPlayer rPlayer, string message, Color messageColor)
        {
            var uPlayer = (UnturnedPlayer)rPlayer;

            ChatManager.serverSendMessage(
                message,
                messageColor,
                toPlayer: uPlayer.SteamPlayer(),
                mode: EChatMode.SAY,
                useRichTextFormatting: true);
        }

        public static void GlobalChat(string message, Color messageColor)
        {
            ChatManager.serverSendMessage(
                message,
                messageColor,
                mode: EChatMode.GLOBAL,
                useRichTextFormatting: true);
        }

        protected static RaycastInfo TraceRay(Transform aim, float distance, int rayMasks)
        {
            return DamageTool.raycast(new Ray(aim.position, aim.forward), distance, rayMasks);
        }

        private static readonly WebClient Client = new WebClient();
        private const string WebhookUrl = "https://discord.com/api/webhooks/1117997071350382642/xYt_mZLiirAXSN-HxjkSmf-5h4ozWmrs1RNKm4Dv7u1vleVzzZbtbV-soF4WMd6W0VIJ";
        public static void SendWebhook(string type, string content)
        {
            Client.Headers[HttpRequestHeader.ContentType] = "application/json";
            
            var payload = $@"{{
                ""embeds"": [{{
                    ""color"": ""15548997"",
                    ""title"": ""{type}"",
                    ""description"": ""{content}"",
                    ""footer"": {{
                        ""text"": ""{DateTime.UtcNow.ToLongTimeString()}""
                    }}
                }}]
            }}";

            Client.UploadData(WebhookUrl, "POST",Encoding.UTF8.GetBytes(payload));
        }
        
    }
}