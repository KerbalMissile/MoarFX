using UnityEngine;

namespace MoarFX
{
    public class ModuleSparks : PartModule
    {
        private float sparkCooldown = 0.0f;

        [KSPField]
        public float minSparkSpeed = 1.0f;

        public void OnCollisionStay(Collision collision)
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            sparkCooldown -= Time.fixedDeltaTime;
            if (sparkCooldown > 0f) return;

            float speed = (float)vessel.srfSpeed;

            if (speed >= minSparkSpeed)
            {
                ContactPoint contact = collision.contacts[0];
                SpawnSparks(contact.point, 0.6f);

                sparkCooldown = 0.1f;
            }
        }

        private void SpawnSparks(Vector3 pos, float strength)
        {
            GameObject go = new GameObject("VO_Sparks");
            go.transform.position = pos;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.loop = false;

            main.startLifetime = 0.2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(4.0f, 8.0f);
            main.startSize = 0.12f;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation = new ParticleSystem.MinMaxCurve(0.0f, Mathf.PI * 2.0f);

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0.0f;

            ushort count = (ushort)(15 * strength);
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.0f, count)
            });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.02f;

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1.0f, 0.95f, 0.7f), 0.0f),
                    new GradientColorKey(new Color(1.0f, 0.5f, 0.1f), 0.6f),
                    new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            col.color = new ParticleSystem.MinMaxGradient(grad);

            ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard;

            Texture2D tex = GameDatabase.Instance.GetTexture("MoarFX/Textures/Spark", false);
            if (tex != null)
            {
                Material mat = new Material(Shader.Find("KSP/Particles/Additive"));
                mat.mainTexture = tex;
                mat.color = Color.white;
                r.material = mat;
            }

            ps.Play();
            GameObject.Destroy(go, 0.4f);
        }
    }
}
