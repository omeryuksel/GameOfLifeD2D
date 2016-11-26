using SharpDX;
using System.Collections.Generic;

namespace GameOfLife
{
    public class Cell
    {
        public bool alive = false;
        public bool die = false;
        public bool born = false;

        public List<Cell> _neighborsCache = null;

        private static Color greenColor = new Color(255, 255, 0, 255);
        private static Color blackColor = new Color(0, 0, 0, 255);

        public Cell(bool isAlive)
        {
            this.alive = isAlive;
        }

        public Color ConvertToColor32()
        {
            if (alive) return greenColor;
            else return blackColor;
        }

        public void UpdateTime()
        {
            if (die)
            {
                alive = false;
            }
            else if (born)
            {
                alive = true;
                born = false;
            }
        }
    }
}