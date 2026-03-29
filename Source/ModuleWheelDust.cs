using UnityEngine;

namespace MoarFX
{
    public class ModuleWheelDust : PartModule
    {
        [KSPField]
        public float minSpeed = 1.0f;

        [KSPField]
        public float smokeHeight = 0.03f;

        [KSPField]
        public float distanceBetweenPuffs = 0.3f;

        [KSPField]
        public float dustSize = 0.4f;
        [KSPField]
        public float dustLifetime = 1.0f;
        [KSPField]
        public float dustUpSpeed = 0.7f;

        [KSPField]
        public bool avoidKerbalKonstructs = true;

        private ModuleWheelBase wheel;
        private Transform wheelTransform;

        private Vector3 lastDustPos;
        private bool hasLastPos = false;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (!HighLogic.LoadedSceneIsFlight)
                return;

            wheel = part.FindModuleImplementing<ModuleWheelBase>();

            if (wheel != null && !string.IsNullOrEmpty(wheel.wheelColliderTransformName))
                wheelTransform = part.FindModelTransform(wheel.wheelColliderTransformName);

            if (wheelTransform == null)
                wheelTransform = part.transform;
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (vessel == null || wheel == null)
                return;

            MoarFXSettings s = null;
            if (HighLogic.CurrentGame != null)
                s = MoarFXSettings.GetSettings();

            if (s != null && !s.enableWheelDust)
                return;

            if (!wheel.isGrounded)
            {
                hasLastPos = false;
                return;
            }

            if (vessel.Splashed)
            {
                hasLastPos = false;
                return;
            }

            if (vessel.srfSpeed < minSpeed)
                return;

            if (IsOnRunway())
                return;

            if (avoidKerbalKonstructs && IsOnKerbalKonstructs())
                return;

            Vector3 currentPos = wheelTransform.position;

            if (!hasLastPos)
            {
                lastDustPos = currentPos;
                hasLastPos = true;
                SpawnDust(currentPos);
                return;
            }

            float distance = Vector3.Distance(currentPos, lastDustPos);
            if (distance >= distanceBetweenPuffs)
            {
                SpawnDust(currentPos);
                lastDustPos = currentPos;
            }
        }

        private bool IsOnRunway()
        {
            if (vessel == null || vessel.mainBody == null)
                return false;

            if (vessel.mainBody.bodyName != "Kerbin")
                return false;

            if (string.IsNullOrEmpty(vessel.landedAt))
                return false;

            string place = vessel.landedAt.ToLower();
            return place.Contains("runway");
        }

        private bool IsOnKerbalKonstructs()
        {
            if (vessel == null) return false;
            if (!string.IsNullOrEmpty(vessel.landedAt))
            {
                string la = vessel.landedAt.ToLower();
                if (la.Contains("kerbalkonstructs") || la.Contains("kerbal konstructs") || la.Contains("konstructs") || la.Contains("kk_") || la.Contains("kk-"))
                    return true;
            }
            return false;
        }

        private void SpawnDust(Vector3 wheelPos)
        {
            Vector3 up = vessel.upAxis;
            if (up == Vector3.zero) up = Vector3.up;

            RaycastHit hit;
            Vector3 spawnPos;
            bool hitKonstructs = false;

            if (Physics.Raycast(wheelPos, -up, out hit, 5f))
            {
                spawnPos = hit.point + hit.normal * smokeHeight;

                if (avoidKerbalKonstructs && HitIsKerbalKonstructs(hit))
                    hitKonstructs = true;
            }
            else
            {
                spawnPos = wheelPos + up * smokeHeight;
            }

            if (hitKonstructs)
                return;

            CelestialBody body = vessel.mainBody;

            string biomeName = "Default";
            if (body != null && body.BiomeMap != null)
            {
                CBAttributeMapSO.MapAttribute att = body.BiomeMap.GetAtt(vessel.latitude, vessel.longitude);
                if (att != null)
                    biomeName = att.name;
            }

            Color baseColor = GetDefaultDustColor(body);
            Color dustColor = MoarFXDustColors.GetColor(body, biomeName, baseColor);

            float g = (body != null) ? (float)body.GeeASL : 1.0f;
            float mass = (vessel != null) ? (float)vessel.GetTotalMass() : 10.0f;

            g = Mathf.Clamp(g, 0.05f, 3.0f);
            mass = Mathf.Clamp(mass, 1.0f, 200.0f);

            float impactFactor = Mathf.Sqrt(g * mass) / 10.0f;
            impactFactor = Mathf.Clamp(impactFactor, 0.2f, 2.0f);

            float lowGFactor = 1.0f / g;
            lowGFactor = Mathf.Clamp(lowGFactor, 1.0f, 6.0f);

            float finalLifetime;
            float finalUpSpeed;

            if (g >= 0.7f)
            {
                finalLifetime = dustLifetime * Mathf.Lerp(1.0f, 1.6f, impactFactor - 0.2f);
                finalUpSpeed  = dustUpSpeed  * Mathf.Lerp(0.6f, 1.3f, impactFactor - 0.2f);
            }
            else
            {
                finalLifetime = dustLifetime * (1.5f + 0.2f * (lowGFactor - 1.0f));
                finalUpSpeed  = dustUpSpeed  * (0.4f + 0.1f * (lowGFactor - 1.0f));
            }

            finalLifetime = Mathf.Clamp(finalLifetime, 0.8f, 4.0f);
            finalUpSpeed  = Mathf.Clamp(finalUpSpeed, 0.2f, 2.0f);

            GameObject go = new GameObject("VO_WheelDust");
            go.transform.position = spawnPos;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 40;
            main.startColor = new ParticleSystem.MinMaxGradient(dustColor);

            main.startLifetime = new ParticleSystem.MinMaxCurve(
                0.8f * finalLifetime,
                1.2f * finalLifetime
            );

            main.startSize = new ParticleSystem.MinMaxCurve(
                0.5f * dustSize,
                1.3f * dustSize
            );

            main.startSpeed = new ParticleSystem.MinMaxCurve(
                0.3f * finalUpSpeed,
                0.9f * finalUpSpeed
            );

            var emission = ps.emission;
            emission.rateOverTime = 0.0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.0f, (short)10)
            });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.16f;
            shape.rotation = Quaternion.LookRotation(up).eulerAngles;

            var sizeOver = ps.sizeOverLifetime;
            sizeOver.enabled = true;

            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 0.7f);
            sizeCurve.AddKey(0.4f, 1.0f);
            sizeCurve.AddKey(1.0f, 1.4f);
            sizeOver.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

            var colOver = ps.colorOverLifetime;
            colOver.enabled = true;

            Gradient grad = new Gradient();
            GradientColorKey[] cKeys = new GradientColorKey[2];
            cKeys[0].color = dustColor; cKeys[0].time = 0.0f;
            cKeys[1].color = dustColor; cKeys[1].time = 1.0f;

            GradientAlphaKey[] aKeys = new GradientAlphaKey[4];
            aKeys[0].alpha = 0.0f; aKeys[0].time = 0.0f;
            aKeys[1].alpha = 1.0f; aKeys[1].time = 0.10f;
            aKeys[2].alpha = 1.0f; aKeys[2].time = 0.70f;
            aKeys[3].alpha = 0.0f; aKeys[3].time = 1.0f;

            grad.SetKeys(cKeys, aKeys);
            colOver.color = new ParticleSystem.MinMaxGradient(grad);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.2f * (1.0f + 0.3f * (lowGFactor - 1.0f));
            noise.frequency = 0.8f;
            noise.scrollSpeed = 0.2f;

            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard;

            Texture2D tex = GameDatabase.Instance.GetTexture("MoarFX/Textures/WheelSmoke", false);
            if (tex == null)
                tex = GameDatabase.Instance.GetTexture("Squad/FX/smokepuff1", false);
            if (tex == null)
                tex = Texture2D.whiteTexture;

            Material mat = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
            mat.mainTexture = tex;
            mat.color = Color.white;
            r.material = mat;

            ps.Play();
            GameObject.Destroy(go, finalLifetime + 2.0f);
        }

        private bool HitIsKerbalKonstructs(RaycastHit hit)
        {
            if (hit.collider == null) return false;

            Transform t = hit.collider.transform;
            while (t != null)
            {
                string n = t.name.ToLower();
                if (n.Contains("kerbalkonstructs") || n.Contains("kerbal konstructs") || n.Contains("konstructs") || n.Contains("kk_") || n.Contains("kk-"))
                    return true;
                t = t.parent;
            }
            return false;
        }

        private Color GetDefaultDustColor(CelestialBody body)
        {
            if (body == null)
                return new Color(0.7f, 0.7f, 0.7f, 1.0f);

            string name = body.bodyName;

            if (name == "Kerbin")
                return new Color(0.55f, 0.65f, 0.45f, 1.0f);

            if (name == "Mun")
                return new Color(0.75f, 0.75f, 0.75f, 1.0f);

            if (name == "Minmus")
                return new Color(0.80f, 0.95f, 0.90f, 1.0f);

            if (name == "Duna")
                return new Color(0.85f, 0.45f, 0.25f, 1.0f);

            if (name == "Ike")
                return new Color(0.50f, 0.45f, 0.40f, 1.0f);

            if (name == "Laythe")
                return new Color(0.65f, 0.62f, 0.58f, 1.0f);

            if (name == "Vall")
                return new Color(0.80f, 0.85f, 0.90f, 1.0f);

            if (name == "Tylo")
                return new Color(0.55f, 0.55f, 0.55f, 1.0f);

            if (name == "Bop")
                return new Color(0.55f, 0.48f, 0.40f, 1.0f);

            if (name == "Pol")
                return new Color(0.80f, 0.75f, 0.55f, 1.0f);

            if (name == "Dres")
                return new Color(0.60f, 0.60f, 0.60f, 1.0f);

            if (name == "Moho")
                return new Color(0.45f, 0.40f, 0.35f, 1.0f);

            return new Color(0.7f, 0.7f, 0.7f, 1.0f);
        }
    }
}
