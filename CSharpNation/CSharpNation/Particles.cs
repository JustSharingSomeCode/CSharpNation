using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace CSharpNation
{
    class Particles
    {
        private Config _config;
        private Random random = new Random();

        private int maxWidth;
        private int maxHeight;

        public Particles(int Widht, int Height, Config config)
        {
            maxWidth = Widht;
            maxHeight = Height;
            _config = config;
        }

        public struct Particle
        {
            public Vector2 position;
            public float radius;
            public int texture;
            public int opacity;

            public float maxRadius;
            public float radiusIncrement;
            public float xIncrement;
            public float yIncrement;

            public Particle(Vector2 Position, float Radius, int Texture, int Opacity, float RadiusIncrement, float X_Increment, float Y_Increment, float MaxRadius)
            {
                position = Position;
                radius = Radius;
                texture = Texture;
                opacity = Opacity;

                radiusIncrement = RadiusIncrement;
                xIncrement = X_Increment;
                yIncrement = Y_Increment;
                maxRadius = MaxRadius;
            }
        }

        private List<Particle> particlesList = new List<Particle>();
        private List<Particle> particlesListMirror = new List<Particle>();

        public void updateParticles(int texture, float aditionalVelocity)
        {
            deleteParticles();
            createNewParticles(texture);
            updateParticlesPosition(aditionalVelocity);
        }

        private void deleteParticles()
        {
            for (int i = 0; i < particlesList.Count; i++)
            {
                if (particlesList[i].position.X < 0 || particlesList[i].position.Y < 0 || particlesList[i].position.Y > maxHeight)
                {
                    particlesList.RemoveAt(i);
                    particlesListMirror.RemoveAt(i);
                }
            }
        }

        private void createNewParticles(int texture)
        {
            while (particlesList.Count < _config.N_Particles)
            {
                Particle P = new Particle(new Vector2(random.Next((maxWidth / 2) - (maxHeight / 4), (maxWidth / 2) + (maxHeight / 4)), random.Next((maxHeight / 2) - (maxHeight / 4), (maxHeight / 2) + (maxHeight / 4)))
                                            , random.Next(5, 20), texture, random.Next(50, 255), (float)(random.NextDouble() * 0.5), (float)(-1 * random.NextDouble()), (float)(random.Next(-1, 2) * random.NextDouble()), 50);
                if (P.xIncrement > -0.5)
                {
                    P.xIncrement = -0.5f;
                }
                if (P.yIncrement > -0.5 && P.yIncrement < 0.5)
                {
                    double Direction = random.NextDouble();
                    if (Direction > 0.5)
                    {
                        P.yIncrement = (float)(1f * random.NextDouble());
                    }
                    else
                    {
                        P.yIncrement = (float)(-1f * random.NextDouble());
                    }
                }

                particlesList.Add(P);

                Particle P2 = new Particle(new Vector2((maxWidth / 2) + ((maxWidth / 2) - P.position.X), P.position.Y), P.radius, P.texture, P.opacity, P.radiusIncrement, P.xIncrement * -1, P.yIncrement, P.maxRadius);

                particlesListMirror.Add(P2);
            }
        }
        
        private float Clamp(float value, float Max, float Min)
        {
            if (value > Max)
            {
                return Max;
            }

            if (value < Min)
            {
                return Min;
            }

            return value;
        }
        
        private void updateParticlesPosition(float aditionalVelocity)
        {
            aditionalVelocity = Clamp(aditionalVelocity, 6, 0.5f);

            for (int i = 0; i < particlesList.Count; i++)
            {
                Vector2 newVector = new Vector2(particlesList[i].position.X + particlesList[i].xIncrement * aditionalVelocity,
                                                particlesList[i].position.Y + particlesList[i].yIncrement * aditionalVelocity);
                float newRadius = particlesList[i].radius /*+ (particlesList[i].radius * particlesList[i].radiusIncrement * aditionalVelocity)*/;

                particlesList[i] = new Particle(newVector, Clamp(newRadius, particlesList[i].maxRadius, 2), particlesList[i].texture, particlesList[i].opacity,
                    particlesList[i].radiusIncrement, particlesList[i].xIncrement, particlesList[i].yIncrement, particlesList[i].maxRadius);
            }

            for (int i = 0; i < particlesListMirror.Count; i++)
            {
                Vector2 newVector = new Vector2(particlesListMirror[i].position.X + particlesListMirror[i].xIncrement * aditionalVelocity,
                                                particlesListMirror[i].position.Y + particlesListMirror[i].yIncrement * aditionalVelocity);
                float newRadius = particlesListMirror[i].radius /*+ (particlesListMirror[i].radius * particlesListMirror[i].radiusIncrement * aditionalVelocity)*/;

                particlesListMirror[i] = new Particle(newVector, Clamp(newRadius, particlesListMirror[i].maxRadius, 2), particlesListMirror[i].texture, particlesListMirror[i].opacity,
                    particlesListMirror[i].radiusIncrement, particlesListMirror[i].xIncrement, particlesListMirror[i].yIncrement, particlesListMirror[i].maxRadius);
            }
        }

        public List<Particle> GetParticlesList()
        {
            List<Particle> returnList = new List<Particle>();

            for (int i = 0; i < particlesList.Count; i++)
            {
                returnList.Add(particlesList[i]);
                returnList.Add(particlesListMirror[i]);
            }

            return returnList;
        }

        public void UpdateWindowSize(int Widht, int Height)
        {
            maxWidth = Widht;
            maxHeight = Height;

            particlesList.Clear();
            particlesListMirror.Clear();
        }
    }
}
