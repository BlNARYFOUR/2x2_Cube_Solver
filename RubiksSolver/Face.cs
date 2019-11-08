using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubiksSolver
{
    class Face
    {
        /*
         * Face layout:
         * 
         * ---------
         * | 0 | 1 | < ShiftY : inner
         * ---------
         * | 2 | 3 |
         * ---------
         *   ^
         * ShiftX : inner
         * 
         */

        public int[] Tiles { get; }

        public Face(int color)
        {
            this.Tiles = Enumerable.Repeat(color, 4).ToArray();
        }

        public Face(Face face)
        {
            this.Tiles = new int[face.Tiles.Length];
            Array.Copy(face.Tiles, this.Tiles, face.Tiles.Length);
        }

        public Face(int[] tiles)
        {
            this.Tiles = tiles;
        }

        public void Rotate(bool clockWise)
        {
            if(clockWise)
            {
                int buf = this.Tiles[0];
                this.Tiles[0] = this.Tiles[2];
                this.Tiles[2] = this.Tiles[3];
                this.Tiles[3] = this.Tiles[1];
                this.Tiles[1] = buf;
            } else
            {
                int buf = this.Tiles[1];
                this.Tiles[1] = this.Tiles[3];
                this.Tiles[3] = this.Tiles[2];
                this.Tiles[2] = this.Tiles[0];
                this.Tiles[0] = buf;
            }
        }

        public int[] ShiftX(bool isInner, int[] tilesIn, bool flipOutput = false)
        {
            int[] tilesOut;

            if (isInner)
            {
                tilesOut = new int[] { this.Tiles[0], this.Tiles[2] };

                this.Tiles[0] = tilesIn[0];
                this.Tiles[2] = tilesIn[1];
            } else
            {
                tilesOut = new int[] { this.Tiles[1], this.Tiles[3] };

                this.Tiles[1] = tilesIn[0];
                this.Tiles[3] = tilesIn[1];
            }

            if(flipOutput)
            {
                int buf = tilesOut[0];
                tilesOut[0] = tilesOut[1];
                tilesOut[1] = buf;
            }

            return tilesOut;
        }

        public int[] ShiftY(bool isInner, int[] tilesIn, bool flipOutput = false)
        {
            int[] tilesOut;

            if (isInner)
            {
                tilesOut = new int[] { this.Tiles[0], this.Tiles[1] };

                this.Tiles[0] = tilesIn[0];
                this.Tiles[1] = tilesIn[1];
            } else
            {
                tilesOut = new int[] { this.Tiles[2], this.Tiles[3] };

                this.Tiles[2] = tilesIn[0];
                this.Tiles[3] = tilesIn[1];

            }

            if (flipOutput)
            {
                int buf = tilesOut[0];
                tilesOut[0] = tilesOut[1];
                tilesOut[1] = buf;
            }

            return tilesOut;
        }

        public int IsOneColor()
        {
            bool isOneColor = true;

            int prevColor = this.Tiles[0];
            //Console.Write(" " + this.Tiles.Length + " : " + "0" + prevColor);
            for(int i=1; i<this.Tiles.Length; i++)
            {
                //Console.Write(" " + i.ToString() + this.Tiles[i]);
                if (prevColor != this.Tiles[i])
                {
                    isOneColor = false;
                    break;
                }
            }

            return isOneColor ? prevColor : -1;
        }
    }
}
