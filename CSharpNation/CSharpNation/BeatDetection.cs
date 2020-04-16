using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpNation
{
    class BeatDetection
    {
        private double previousE = 0;
        private double currentE = 0;
        public bool beatDetected = false;

        private double max = 0;
        private int maxIndex = 0;        

        float increaseFactor;
        int wavePoint = 18;
        int rep = 1;

        public bool DetectBeat(double Sensibility, ref List<double> currentWave, ref List<double> finalWave, int waveCount)
        {
            if (currentWave == null || currentWave.Count == 0)
            {
                return false;
            }

            if (beatDetected)
            {               
                CreateWaveAnimation(ref currentWave, ref finalWave);
                return false;
            }

            currentE = 0;
            for (int i = 0; i < 9; i++)
            {
                currentE += currentWave[i];
            }


            if (currentE > previousE * Sensibility && currentE > 60)
            {
                if (!beatDetected)
                {
                    //Console.WriteLine(" | {0} | ->  |  {1}  |", currentE, previousE * Sensibility);
                    previousE = currentE;                    
                    beatDetected = true;
                    DetectPeek(currentWave);
                    return true;
                }
            }

            previousE = currentE;

            return false;
        }

        public void DetectPeek(List<double> currentWave)
        {
            max = 0;
            maxIndex = 0;
            for (int i = 0; i < 9; i++)
            {
                if (currentWave[i] > max)
                {
                    max = currentWave[i];
                    maxIndex = i;
                }
            }

            //Console.Write("Peek at: {0}, Force = {1} |", maxIndex, currentE);           
            increaseFactor = (float)(max / (20 - maxIndex));
        }

        public double GetForce()
        {
            return currentE / 60;
        }        

        public void CreateWaveAnimation(ref List<double> currentWave, ref List<double> finalWave)
        {
            if (wavePoint - 1 <= maxIndex)
            {
                beatDetected = false;
                ResetWaveAnimation();
                return;
            }

            
            currentWave[wavePoint] = rep * increaseFactor;
            currentWave[wavePoint - 1] = currentWave[wavePoint] * 0.8;
            currentWave[wavePoint + 1] = currentWave[wavePoint];

            
            finalWave[wavePoint] = rep * increaseFactor;
            finalWave[wavePoint - 1] = finalWave[wavePoint] * 0.8;
            finalWave[wavePoint + 1] = finalWave[wavePoint];

            for (int i = 18; i > wavePoint + 2; i--)
            {
                currentWave[i] = 0;
                finalWave[i] = 0;
            }

            rep++;
            wavePoint--;
        }

        public void ResetWaveAnimation()
        {
            wavePoint = 18;
            rep = 1;
        }
    }
}
