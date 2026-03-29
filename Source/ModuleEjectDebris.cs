using UnityEngine;

namespace MoarFX
{
    public class ModuleEjectDebris : PartModule
    {
        [KSPField]
        public int minPieces = 5;

        [KSPField]
        public int maxPieces = 10;

        [KSPField]
        public float minSpeed = 2.0f;

        [KSPField]
        public float maxSpeed = 6.0f;

        [KSPField]
        public float lifeMin = 1.5f;

        [KSPField]
        public float lifeMax = 3.0f;

        private bool hasFired = false;

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            if (hasFired) return;

            if (part.parent == null)
            {
                hasFired = true;
                SpawnDebrisBurst();
            }
        }

        private void SpawnDebrisBurst()
        {
            MoarFXSettings s = MoarFXSettings.GetSettings();
            if (s != null && !s.enableStagingDebris) return;

            int count = Random.Range(minPieces, maxPieces + 1);

            Vector3 basePos = part.transform.position;

            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("VO_StagingDebris");
                go.transform.position = basePos;

                ParticleSystem ps = go.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule main = ps.main;
                main.loop = false;
                main.startLifetime = Random.Range(lifeMin, lifeMax);
                main.startSpeed = Random.Range(minSpeed, maxSpeed);
                main.startSize = Random.Range(0.02f, 0.08f);
                main.maxParticles = 1;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.startRotation = new ParticleSystem.MinMaxCurve(0.0f, Mathf.PI * 2.0f);

                ParticleSystem.EmissionModule emission = ps.emission;
                emission.rateOverTime = 0.0f;
                emission.SetBursts(new ParticleSystem.Burst[]
                {
                    new ParticleSystem.Burst(0.0f, 1)
                });

                ParticleSystem.ShapeModule shape = ps.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 25f;
                shape.radius = 0.05f;

                ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
                r.renderMode = ParticleSystemRenderMode.Billboard;

                Texture2D tex = GameDatabase.Instance.GetTexture("MoarFX/Textures/MicroDebris", false);
                if (tex != null)
                {
                    Material mat = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
                    mat.mainTexture = tex;
                    mat.color = Color.white;
                    r.material = mat;
                }

                ps.Play();
                GameObject.Destroy(go, main.startLifetime.constant + 0.5f);
            }
        }
    }
}
