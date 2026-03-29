using UnityEngine;

namespace MoarFX
{
    public class MoarFXSettings : GameParameters.CustomParameterNode
    {
        public override string Title
        {
            get { return "MoarFX"; }
        }

        public override GameParameters.GameMode GameMode
        {
            get { return GameParameters.GameMode.ANY; }
        }

        public override string Section
        {
            get { return "MoarFX"; }
        }

        public override int SectionOrder
        {
            get { return 1; }
        }

        public override string DisplaySection
        {
            get { return Section; }
        }

        public override bool HasPresets
        {
            get { return false; }
        }

        [GameParameters.CustomParameterUI(
            "Enable Wheel Dust",
            toolTip = "Show dust particles from wheels when driving on terrain.",
            autoPersistance = true)]
        public bool enableWheelDust = true;

        [GameParameters.CustomParameterUI(
            "Enable Touchdown Smoke",
            toolTip = "Show smoke when wheels touch down at speed.",
            autoPersistance = true)]
        public bool enableTouchdownSmoke = true;

        [GameParameters.CustomIntParameterUI(
            "Wheel Smoke Density (Low 10 / Med 20 / High 30)",
            toolTip = "Higher density gives thicker smoke but costs more performance.",
            minValue = 10,
            maxValue = 30,
            stepSize = 10,
            autoPersistance = true)]
        public int wheelSmokeDensity = 20;

        [GameParameters.CustomParameterUI(
            "Enable Sparks",
            toolTip = "Show sparks when parts scrape the ground at high speed.",
            autoPersistance = true)]
        public bool enableSparks = true;

        [GameParameters.CustomParameterUI(
            "Enable Staging Debris",
            toolTip = "Spawn small debris when stages separate.",
            autoPersistance = true)]
        public bool enableStagingDebris = true;

        [GameParameters.CustomParameterUI(
            "Enable Micro Debris in LKO",
            toolTip = "Show tiny drifting debris specks in low orbit.",
            autoPersistance = true)]
        public bool enableMicroDebris = true;

        [GameParameters.CustomParameterUI(
            "Enable SRB Burnout Effect",
            toolTip = "Show sparks and ash when solid boosters burn out.",
            autoPersistance = true)]
        public bool enableSRBBurnout = true;

        public static MoarFXSettings GetSettings()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<MoarFXSettings>();
        }
    }
}
