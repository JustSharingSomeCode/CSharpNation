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
        private float sizeMultiplier = 1;

        public Particles(int Widht, int Height, Config config)
        {
            maxWidth = Widht;
            maxHeight = Height;
            _config = config;
        }

        public struct Particle
        {
            public Vector2 initialPosition;
            public Vector2 position;

            public float actualRadius;
            public float initialRadius;
            public float finalRadius;            

            public int texture;
            public int opacity;
                       
            public float xIncrement;
            public float yIncrement;            

            public Particle(Vector2 _position, float _initialRadius, float _finalRadius, int Texture, int Opacity, float X_Increment, float Y_Increment)
            {
                initialPosition = _position;
                position = _position;                
                initialRadius = _initialRadius;
                finalRadius = _initialRadius * (1 + _finalRadius);
                actualRadius = initialRadius;
                texture = Texture;
                opacity = Opacity;               
                xIncrement = X_Increment;
                yIncrement = Y_Increment;
            }

            public Particle(Vector2 _position, Vector2 _initialPosition, float _initialRadius, float _finalRadius, int Texture, int Opacity, float X_Increment, float Y_Increment)
            {
                initialPosition = _initialPosition;
                position = _position;
                initialRadius = _initialRadius;
                finalRadius = _finalRadius;
                actualRadius = finalRadius - (position.X * (finalRadius - initialRadius) / initialPosition.X);
                texture = Texture;
                opacity = Opacity;
                xIncrement = X_Increment;
                yIncrement = Y_Increment;
            }

            public Particle(Vector2 _position, Vector2 _initialPosition, float _initialRadius, float _actualRadius, float _finalRadius, int Texture, int Opacity, float X_Increment, float Y_Increment)
            {
                initialPosition = _initialPosition;
                position = _position;            
                initialRadius = _initialRadius;
                finalRadius = _finalRadius;
                actualRadius = _actualRadius;
                texture = Texture;
                opacity = Opacity;
                xIncrement = X_Increment;
                yIncrement = Y_Increment;
            }
        }

        private List<Particle> particlesList = new List<Particle>();
        private List<Particle> particlesListMirror = new List<Particle>();

        public void UpdateParticles(int texture, float aditionalVelocity)
        {
            DeleteParticles();
            CreateNewParticles(texture);
            UpdateParticlesPosition(aditionalVelocity);
        }

        private void DeleteParticles()
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

        private void CreateNewParticles(int texture)
        {
            while (particlesList.Count < _config.N_Particles)
            {
                Particle P = new Particle(new Vector2(random.Next((maxWidth / 2) - (maxHeight / 4), (maxWidth / 2) + (maxHeight / 4)), random.Next((maxHeight / 2) - (maxHeight / 4), (maxHeight / 2) + (maxHeight / 4)))
                                            ,random.Next(5, 16) * sizeMultiplier, (float)random.NextDouble() * 4f, texture, random.Next(50, 255), (float)(-1 * random.NextDouble()), (float)(random.Next(-1, 2) * random.NextDouble()));
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

                Particle P2 = new Particle(new Vector2((maxWidth / 2) + ((maxWidth / 2) - P.position.X), P.position.Y), P.initialRadius, P.finalRadius, P.texture, P.opacity, P.xIncrement * -1, P.yIncrement);

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
        
        private void UpdateParticlesPosition(float aditionalVelocity)
        {
            aditionalVelocity = Clamp(aditionalVelocity, 8, 0.25f);                        

            for (int i = 0; i < particlesList.Count; i++)
            {                                
                Vector2 newVector = new Vector2(particlesList[i].position.X + (particlesList[i].xIncrement * aditionalVelocity) * (particlesList[i].actualRadius * 1.25f / 18f),
                                                particlesList[i].position.Y + (particlesList[i].yIncrement * aditionalVelocity) * (particlesList[i].actualRadius * 1.25f / 18f));

                particlesList[i] = new Particle(newVector, particlesList[i].initialPosition, particlesList[i].initialRadius, particlesList[i].finalRadius, particlesList[i].texture, particlesList[i].opacity,
                    particlesList[i].xIncrement, particlesList[i].yIncrement);

                particlesListMirror[i] = new Particle(new Vector2(maxWidth - particlesList[i].position.X, newVector.Y), particlesListMirror[i].initialPosition, particlesListMirror[i].initialRadius, particlesList[i].actualRadius, particlesListMirror[i].finalRadius, particlesListMirror[i].texture, particlesListMirror[i].opacity,
                    particlesListMirror[i].xIncrement, particlesListMirror[i].yIncrement);
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

            sizeMultiplier = maxWidth / 950f;

            particlesList.Clear();
            particlesListMirror.Clear();
        }
    }
}
