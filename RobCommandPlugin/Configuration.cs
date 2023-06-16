using Rocket.API;

namespace RobCommandPlugin
{
    public class Configuration : IRocketPluginConfiguration
    {
        public float RobberyCooldown { get; set; }
        public float VictimCooldown { get; set; }
        public float RobberyLength { get; set; }
        public string RobMessageColor { get; set; }
        public string OtherMessageColor { get; set; }

        public void LoadDefaults()
        {
            RobberyCooldown = 1;
            VictimCooldown = 2;
            RobberyLength = 1;
            RobMessageColor = "red";
            OtherMessageColor = "green";
        }
    }
}