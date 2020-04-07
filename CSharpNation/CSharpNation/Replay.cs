using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace CSharpNation
{
    class Replay
    {
        public List<List<Vector2>> catmullRomPoints = new List<List<Vector2>>();

        public void Push(List<Vector2> newCatmullRomPoints)
        {
            List<Vector2> NewList = new List<Vector2>();

            for (int i = 0; i < newCatmullRomPoints.Count; i++)
            {
                NewList.Add(newCatmullRomPoints[i]);
            }

            catmullRomPoints.Add(NewList);

            if (catmullRomPoints.Count > 15)
            {
                catmullRomPoints.RemoveAt(0);
            }
        }

        public List<Vector2> GetCatmullRomPoints(int Index)
        {
            if (catmullRomPoints.Count > Index)
            {
                return catmullRomPoints[Index];
            }

            return null;
        }
    }
}
