using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubiksSolver
{
    class Cube
    {
        // only 2x2 will be implemented in this project (non-generic)

        /*
         * Cube layout:
         * Index of array given at 0 index of Face (see Face layout)
         * 
         *         ---------
         *         | 4     | 
         *         -Yellow -
         *         |       |
         * ---------------------------------
         * | 0     | 1     | 2     | 3     |
         * -Green  -Orange -Blue   -Red    -
         * |       |       |       |       |
         * ---------------------------------
         *         | 5     |
         *         -White  -
         *         |       |
         *         ---------
         * 
         *     Z
         *     ^
         *     |     ________
         *     |    / 4     /|
         *     |   /______ / |
         *     |   | 1    |2 |
         *     |   |      | /
         *     |   |______|/
         *     |
         *     +-----------------> Y
         *    /
         *   /
         *  /
         * X
         * 
         */

        public enum MoveOptimalSolve { R, RP, R2, U, UP, U2, F, FP, F2 }

        public enum Move { R, RP, R2, U, UP, U2, F, FP, F2, L, LP, L2, D, DP, D2, B, BP, B2 }

        public static string ToFriendlyString(Move code)
        {
            string name = Enum.GetName(code.GetType(), code);
            return name.Replace('P', '\'');
            // return ((int)code).ToString();
        }

        public static string ToFriendlyString(Move[] moves)
        {
            if(moves.Length == 0)
            {
                return "/";
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach(Move move in moves)
            {
                stringBuilder.Append(Cube.ToFriendlyString(move));
                stringBuilder.Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            return stringBuilder.ToString();
        }

        public static Move[] Scramble(int minMoves, int maxMoves, Cube cube)
        {
            int maxMoveIndex = Enum.GetValues(typeof(Move)).Cast<int>().Max();
            List<Move> moves = new List<Move>();

            Random rand = new Random();

            for (int i=0; i < rand.Next(minMoves, maxMoves); i++)
            {
                Move m;

                do
                {
                    m = (Move)rand.Next(0, maxMoveIndex);
                } while (0 < moves.Count && !IsDecentNextMove(moves.Last(), m));
                
                Cube.ApplyMove(cube, m);
                moves.Add(m);
            }

            return moves.ToArray();
        }

        public static Move[] OptimalSolve(Cube cube, bool onlyBottomFace = false, bool feedBack = true)
        {
            Move maxMoveIndex;
            if (onlyBottomFace)
            {
                maxMoveIndex = Enum.GetValues(typeof(Move)).Cast<Move>().Max();
            }
            else
            {
                maxMoveIndex = (Move)Enum.GetValues(typeof(MoveOptimalSolve)).Cast<MoveOptimalSolve>().Max();
            }
            //maxMoveIndex = (Move)Enum.GetValues(typeof(MoveOptimalSolve)).Cast<MoveOptimalSolve>().Max(); // TEST

            List<Move> moves = new List<Move>();

            int amountChecked = 0;
            List<List<Move>> movesChecked = new List<List<Move>>();

            //Console.WriteLine("Solved: " + cube.IsSolved());

            if ((!onlyBottomFace && cube.IsSolved()) || (onlyBottomFace && cube.Faces[5].IsOneColor() != -1))
            {
                return moves.ToArray();
            }

            for (int i = 0; i <= 11; i++)
            {
                moves.Add((Move)0);

                int j = 0;

                do
                {
                    Cube testCube = new Cube(cube);
                    Cube.ApplyMoves(testCube, moves.ToArray());

                    if ((!onlyBottomFace && testCube.IsSolved()) || (onlyBottomFace && testCube.Faces[5].IsOneColor() != -1))
                    {
                        return moves.ToArray();
                    }

                    amountChecked++;

                    //Console.WriteLine(testCube);
                    //Console.WriteLine(ToFriendlyString(moves.ToArray()) + " " + testCube.IsSolved());
                    //Console.Write(ToFriendlyString(moves.ToArray()) + "\t\t");

                    bool isDecentMove;
                    j = -1;
                    do
                    {
                        isDecentMove = true;
                        j++;
                        if (0 < j)
                        {
                            moves[j - 1] = (Move)0;
                        }
                        moves[j]++;

                        // Console.Write(j.ToString() + i.ToString()  + " ");

                        if(j < i)
                        {
                            isDecentMove = IsDecentNextMove(moves[j], moves[j+1]);
                        }

                        while (j < i && moves[j] < maxMoveIndex && !isDecentMove)
                        {
                            moves[j]++;
                            isDecentMove = IsDecentNextMove(moves[j], moves[j + 1]);
                        }

                        if(!isDecentMove)
                        {
                            //Console.WriteLine("Not decent: " + moves[j]);
                            moves[j]++;
                        }

                        //Console.Write(":" + moves[j] + " " + (j < i ? moves[j + 1].ToString() : "") + " " + isDecentMove + "\n");
                    } while (j < i && maxMoveIndex < moves[j]);
                    // Console.WriteLine("DAMN");
                } while (moves[j] <= maxMoveIndex);
                moves[j] = (Move)0;

                if (feedBack)
                {
                    Console.WriteLine("Amount Checked: " + amountChecked);
                    //Console.ReadKey();
                }    
            }

            return null;
        }

        public static Move[] QuickSolve(Cube cube, bool feedBack = true)
        {
            Cube testCube = new Cube(cube);
            List<Move> moves = new List<Move>();

            if (feedBack)
                Console.WriteLine("Solving up face...");

            moves.AddRange(Cube.OptimalSolve(cube, true, false));

            ApplyMoves(testCube, moves.ToArray());

            int colorBottom = testCube.Faces[5].Tiles[0];
            int colorTop = colorBottom < 4 ? (colorBottom - 2) : (colorBottom < 5 ? 5 : 4);
            colorTop = colorTop < 0 ? (4 + colorTop) : colorTop;

            if (feedBack)
            {
                Console.WriteLine("\nColor down: " + colorBottom);
                Console.WriteLine("Color up: " + colorTop);
                Console.WriteLine("\nSolving up face...");
            }

            DoOLL(testCube, moves, colorTop);
            DoPLL(testCube, moves);

            while (!testCube.IsSolved())
            {
                testCube.RotateUClock();
                moves.Add(Move.U);
            }

            List<Move> orginalMoves = new List<Move>(moves);
            FixSymmetry(moves, feedBack);

            return moves.ToArray();
        }

        public static void FixSymmetry(List<Move> moves, bool feedback = true)
        {
            for(int i=moves.Count-1; i > 0; i--)
            {
                if(!IsDecentNextMove(moves[i-1], moves[i]))
                {
                    if (feedback)
                        Console.WriteLine("Fixing symmetry...");
                    // R, RP, R2
                    int test = (int)moves[i - 1] - (int)moves[i];
                    if(test == 0)
                    {
                        moves.RemoveAt(i);
                        if(new Move[] { Move.B2, Move.D2, Move.L2, Move.F2, Move.U2, Move.R2 }.Contains(moves[i - 1]))
                        {
                            moves.RemoveAt(i - 1);
                            i--;
                        }
                        else
                        {
                            moves[i - 1] += (3 - (((int)moves[i - 1] + 1) % 3));
                        }
                    }
                    else if(test == 1 || test == -1)
                    {
                        if(((int)moves[i] + 1) % 3 == 0 || ((int)moves[i-1] + 1) % 3 == 0)
                        {
                            moves[i - 1] = moves[i - 1] < moves[i] ? moves[i] - 1 : moves[i - 1] - 1;
                        }
                        else
                        {
                            moves.RemoveAt(i - 1);
                            i--;
                        }
                        moves.RemoveAt(i);
                    }
                    else
                    {
                        moves[i - 1] = moves[i - 1] < moves[i] ? moves[i] - 1 : moves[i - 1] - 1;
                        moves.RemoveAt(i);
                    }
                }
            }
        }

        public static void DoPLL(Cube cube, List<Move> moves)
        {
            List<Move> pllMoves = new List<Move>();

            int doubleColorFaceTop = -1;
            int doubleColorFaceBottom = -1;
            for (int i=0; i<4; i++)
            {
                if(cube.Faces[i].Tiles[0] == cube.Faces[i].Tiles[1])
                {
                    doubleColorFaceTop = doubleColorFaceTop == -1 ? i : -2;
                    Console.WriteLine("TopFace: " + doubleColorFaceTop);
                }

                if (cube.Faces[i].Tiles[2] == cube.Faces[i].Tiles[3])
                {
                    doubleColorFaceBottom = doubleColorFaceBottom == -1 ? i : -2;
                    Console.WriteLine("BotFace: " + doubleColorFaceBottom);
                }
            }

            if(-1 < doubleColorFaceTop && 1 != doubleColorFaceTop)
            {
                pllMoves.Add(doubleColorFaceTop == 0 ? Move.UP : (doubleColorFaceTop == 2 ? Move.U : Move.U2));
            }

            if (-1 < doubleColorFaceBottom && 1 != doubleColorFaceBottom)
            {
                pllMoves.Add(doubleColorFaceBottom == 0 ? Move.D : (doubleColorFaceBottom == 2 ? Move.DP : Move.D2));
            }



            if (-1 < doubleColorFaceTop && -1 < doubleColorFaceBottom)
            {
                pllMoves.AddRange(new Move[] { Move.R2, Move.UP, Move.B2, Move.U2, Move.R2, Move.UP, Move.R2 });
            }

            if (-1 < doubleColorFaceTop && -1 == doubleColorFaceBottom)
            {
                pllMoves.AddRange(new Move[] { Move.R, Move.UP, Move.R, Move.F2, Move.RP, Move.U, Move.RP });
            }

            if (-1 < doubleColorFaceTop && -2 == doubleColorFaceBottom)
            {
                pllMoves.AddRange(new Move[] { Move.RP, Move.U, Move.RP, Move.F2, Move.R, Move.FP, Move.RP, Move.F2, Move.R2 });
            }

            if (-1 == doubleColorFaceTop && -1 < doubleColorFaceBottom)
            {
                pllMoves.AddRange(new Move[] { Move.L, Move.DP, Move.L, Move.F2, Move.LP, Move.D, Move.LP });
            }

            if (-1 == doubleColorFaceTop && -1 == doubleColorFaceBottom)
            {
                pllMoves.AddRange(new Move[] { Move.R2, Move.F2, Move.R2 });
            }

            if (-1 == doubleColorFaceTop && -2 == doubleColorFaceBottom)
            {
                pllMoves.AddRange(new Move[] { Move.R, Move.UP, Move.RP, Move.UP, Move.F2, Move.UP, Move.R, Move.U, Move.RP, Move.D, Move.R2 });
            }

            if (-2 == doubleColorFaceTop && -1 < doubleColorFaceBottom)
            {
                pllMoves.AddRange(new Move[] { Move.LP, Move.D, Move.LP, Move.F2, Move.L, Move.FP, Move.LP, Move.F2, Move.L2 });
            }

            if (-2 == doubleColorFaceTop && -1 == doubleColorFaceBottom)
            {
                pllMoves.AddRange(new Move[] { Move.L, Move.DP, Move.LP, Move.DP, Move.F2, Move.DP, Move.L, Move.D, Move.LP, Move.U, Move.L2 });
            }



            if (0 < pllMoves.Count)
                Console.WriteLine("PLL done\n");
            moves.AddRange(pllMoves);
            ApplyMoves(cube, pllMoves.ToArray());
        }

        public static void DoOLL(Cube cube, List<Move> moves, int colorTop)
        {
            Move[] ollMoves = new Move[0];

            if (CheckForPatternAndOrientUpFace(new bool[] { false, false, true, false, false, false, false, true, false, true, false, true }, colorTop, cube, moves))
            {
                ollMoves = new Move[] { Move.R, Move.U, Move.RP, Move.U, Move.R, Move.U2, Move.RP };
            }
            else if (CheckForPatternAndOrientUpFace(new bool[] { false, true, false, false, true, false, true, false, true, false, false, false }, colorTop, cube, moves))
            {
                ollMoves = new Move[] { Move.R, Move.U2, Move.RP, Move.UP, Move.R, Move.UP, Move.RP };
            }
            else if (CheckForPatternAndOrientUpFace(new bool[] { false, false, false, false, false, false, true, true, false, false, true, true }, colorTop, cube, moves))
            {
                ollMoves = new Move[] { Move.R2, Move.U2, Move.R, Move.U2, Move.R2 };
            }
            else if (CheckForPatternAndOrientUpFace(new bool[] { false, true, false, true, true, true, false ,false, false, false, false, false }, colorTop, cube, moves))
            {
                ollMoves = new Move[] { Move.F, Move.R, Move.U, Move.RP, Move.UP, Move.FP };
            }
            else if (CheckForPatternAndOrientUpFace(new bool[] { false, false, false, false, true, true, false, true, false, false, true, false }, colorTop, cube, moves))
            {
                ollMoves = new Move[] { Move.F, Move.R, Move.U, Move.RP, Move.UP, Move.R, Move.U, Move.RP, Move.UP, Move.FP };
            }
            else if (CheckForPatternAndOrientUpFace(new bool[] { true, false, false, true, false, false, true, false, false, true, false, false }, colorTop, cube, moves))
            {
                ollMoves = new Move[] { Move.F, Move.RP, Move.FP, Move.R, Move.U, Move.R, Move.UP, Move.RP };
            }
            else if (CheckForPatternAndOrientUpFace(new bool[] { false, true, false, true, false, false, true, false, false, false, false, true }, colorTop, cube, moves))
            {
                ollMoves = new Move[] { Move.R, Move.U, Move.RP, Move.UP, Move.RP, Move.F, Move.R, Move.FP };
            }

            moves.AddRange(ollMoves);
            ApplyMoves(cube, ollMoves);
        }

        /*
         * Pattern is a bool array containing: 1, color to match and 0, any other color.
         * pattern order as follows: up face (0, 1, 2, 3), left face (0, 1), front face (0, 1), right face (0, 1) and back face (0, 1)
         * example:
         * Well known 'fish' OLL: R U R' U R U2 R'
         *  - new bool[] { false, false, true, false, false, false, false, true, false, true, false, true }
         */
        public static bool CheckForPatternAndOrientUpFace(bool[] pattern, int color, Cube cube, List<Move> moves)
        {
            if (pattern.Length != 12)
                throw new Exception("Pattern of wrong length.");

            for(int i=0; i<4; i++)
            {
                if((pattern[0] == (cube.Faces[4].Tiles[0] == color))
                    && (pattern[1] == (cube.Faces[4].Tiles[1] == color))
                    && (pattern[2] == (cube.Faces[4].Tiles[2] == color))
                    && (pattern[3] == (cube.Faces[4].Tiles[3] == color))
                    && (pattern[4] == (cube.Faces[0].Tiles[0] == color))
                    && (pattern[5] == (cube.Faces[0].Tiles[1] == color))
                    && (pattern[6] == (cube.Faces[1].Tiles[0] == color))
                    && (pattern[7] == (cube.Faces[1].Tiles[1] == color))
                    && (pattern[8] == (cube.Faces[2].Tiles[0] == color))
                    && (pattern[9] == (cube.Faces[2].Tiles[1] == color))
                    && (pattern[10] == (cube.Faces[3].Tiles[0] == color))
                    && (pattern[11] == (cube.Faces[3].Tiles[1] == color)))
                {
                    if(0 < i)
                    {
                        moves.Add(i == 1 ? Move.U : (i == 2 ? Move.U2 : Move.UP));
                    }

                    return true;
                }

                cube.RotateUClock();
            }

            return false;
        }

        public static bool IsDecentNextMove(Move lastMove, Move newMove)
        {
            //Console.WriteLine(lastMove + " " + newMove);

            if(Cube.Contains(new Move[] { Move.F, Move.FP, Move.F2 }, lastMove) && Cube.Contains(new Move[] { Move.F, Move.FP, Move.F2 }, newMove))
            {
                return false;
            }

            if (Cube.Contains(new Move[] { Move.R, Move.RP, Move.R2 }, lastMove) && Cube.Contains(new Move[] { Move.R, Move.RP, Move.R2 }, newMove))
            {
                return false;
            }

            if (Cube.Contains(new Move[] { Move.U, Move.UP, Move.U2 }, lastMove) && Cube.Contains(new Move[] { Move.U, Move.UP, Move.U2 }, newMove))
            {
                return false;
            }

            if (Cube.Contains(new Move[] { Move.B, Move.BP, Move.B2 }, lastMove) && Cube.Contains(new Move[] { Move.B, Move.BP, Move.B2 }, newMove))
            {
                return false;
            }

            if (Cube.Contains(new Move[] { Move.L, Move.LP, Move.L2 }, lastMove) && Cube.Contains(new Move[] { Move.L, Move.LP, Move.L2 }, newMove))
            {
                return false;
            }

            if (Cube.Contains(new Move[] { Move.D, Move.DP, Move.D2 }, lastMove) && Cube.Contains(new Move[] { Move.D, Move.DP, Move.D2 }, newMove))
            {
                return false;
            }

            return true;
        }

        public static bool Contains(Move[] moves, Move test)
        {
            return moves.Contains(test);
        }

        public static Cube ApplyMoves(Cube cube, Move[] moves)
        {
            foreach (Move move in moves)
            {
                Cube.ApplyMove(cube, move);
            }

            return cube;
        }

        public static Cube ApplyMove(Cube cube, Move move)
        {
            switch (move)
            {
                // TODO: expaaannddd
                case Move.B:
                    cube.RotateBClock();
                    break;
                case Move.BP:
                    cube.RotateBCounterClock();
                    break;
                case Move.B2:
                    cube.RotateBHalf();
                    break;
                case Move.L:
                    cube.RotateLClock();
                    break;
                case Move.LP:
                    cube.RotateLCounterClock();
                    break;
                case Move.L2:
                    cube.RotateLHalf();
                    break;
                case Move.D:
                    cube.RotateDClock();
                    break;
                case Move.DP:
                    cube.RotateDCounterClock();
                    break;
                case Move.D2:
                    cube.RotateDHalf();
                    break;
                case Move.F:
                    cube.RotateFClock();
                    break;
                case Move.FP:
                    cube.RotateFCounterClock();
                    break;
                case Move.F2:
                    cube.RotateFHalf();
                    break;
                case Move.R:
                    cube.RotateRClock();
                    break;
                case Move.RP:
                    cube.RotateRCounterClock();
                    break;
                case Move.R2:
                    cube.RotateRHalf();
                    break;
                case Move.U:
                    cube.RotateUClock();
                    break;
                case Move.UP:
                    cube.RotateUCounterClock();
                    break;
                case Move.U2:
                    cube.RotateUHalf();
                    break;
                default:
                    break;
            }

            return cube;
        }

        public Face[] Faces { get; }

        public Cube()
        {
            this.Faces = new Face[6];

            for (int i=0; i<6; i++)
            {
                this.Faces[i] = new Face(i);
            }
        }

        public Cube(Cube cube)
        {
            this.Faces = new Face[cube.Faces.Length];
            for(int i=0; i< this.Faces.Length; i++)
            {
                this.Faces[i] = new Face(cube.Faces[i]);
            }
        }

        public Cube(Face[] faces)
        {
            this.Faces = faces;
        }

        public bool IsSolved()
        {
            bool isSolved = true;

            //Console.WriteLine(this);
            //Console.Write("IsOneColor:");

            List<int> prevColors = new List<int>();
            for(int i=0; i<this.Faces.Length; i++)
            {
                int res = this.Faces[i].IsOneColor();
                //Console.Write(" " + res);
                if(res == -1 || prevColors.Contains(res))
                {
                    isSolved = false;
                    break;
                }

                prevColors.Add(res);
            }

            //Console.Write("\n");

            return isSolved;
        }

        public void RotateBClock()
        {
            int[] buf;

            buf = this.Faces[0].ShiftX(true, new int[] { -1, -1 });
            buf = this.Faces[5].ShiftY(false, buf, true);
            buf = this.Faces[2].ShiftX(false, buf);
            buf = this.Faces[4].ShiftY(true, buf, true);
            this.Faces[0].ShiftX(true, buf);

            this.Faces[3].Rotate(true);
        }

        public void RotateBCounterClock()
        {
            int[] buf;

            buf = this.Faces[0].ShiftX(true, new int[] { -1, -1 }, true);
            buf = this.Faces[4].ShiftY(true, buf);
            buf = this.Faces[2].ShiftX(false, buf, true);
            buf = this.Faces[5].ShiftY(false, buf);
            this.Faces[0].ShiftX(true, buf);

            this.Faces[3].Rotate(false);
        }

        public void RotateBHalf()
        {
            int[] buf;

            buf = this.Faces[0].ShiftX(true, new int[] { -1, -1 }, true);
            buf = this.Faces[2].ShiftX(false, buf, true);
            this.Faces[0].ShiftX(true, buf);

            buf = this.Faces[4].ShiftY(true, new int[] { -1, -1 }, true);
            buf = this.Faces[5].ShiftY(false, buf, true);
            this.Faces[4].ShiftY(true, buf);

            this.Faces[3].Rotate(true);
            this.Faces[3].Rotate(true);
        }

        public void RotateFClock()
        {
            int[] buf;

            buf = this.Faces[0].ShiftX(false, new int[] { -1, -1 }, true);
            buf = this.Faces[4].ShiftY(false, buf);
            buf = this.Faces[2].ShiftX(true, buf, true);
            buf = this.Faces[5].ShiftY(true, buf);
            this.Faces[0].ShiftX(false, buf);

            this.Faces[1].Rotate(true);
        }

        public void RotateFCounterClock()
        {
            int[] buf;

            buf = this.Faces[0].ShiftX(false, new int[] { -1, -1 });
            buf = this.Faces[5].ShiftY(true, buf, true);
            buf = this.Faces[2].ShiftX(true, buf);
            buf = this.Faces[4].ShiftY(false, buf, true);
            this.Faces[0].ShiftX(false, buf);

            this.Faces[1].Rotate(false);
        }

        public void RotateFHalf()
        {
            int[] buf;

            buf = this.Faces[0].ShiftX(false, new int[] { -1, -1 }, true);
            buf = this.Faces[2].ShiftX(true, buf, true);
            this.Faces[0].ShiftX(false, buf);

            buf = this.Faces[4].ShiftY(false, new int[] { -1, -1 }, true);
            buf = this.Faces[5].ShiftY(true, buf, true);
            this.Faces[4].ShiftY(false, buf);

            this.Faces[1].Rotate(true);
            this.Faces[1].Rotate(true);
        }

        public void RotateLClock()
        {
            int[] buf;

            buf = this.Faces[1].ShiftX(true, new int[] { -1, -1 });
            buf = this.Faces[5].ShiftX(true, buf, true);
            buf = this.Faces[3].ShiftX(false, buf, true);
            buf = this.Faces[4].ShiftX(true, buf);
            this.Faces[1].ShiftX(true, buf);

            this.Faces[0].Rotate(true);
        }

        public void RotateLCounterClock()
        {
            int[] buf;

            buf = this.Faces[1].ShiftX(true, new int[] { -1, -1 });
            buf = this.Faces[4].ShiftX(true, buf, true);
            buf = this.Faces[3].ShiftX(false, buf, true);
            buf = this.Faces[5].ShiftX(true, buf);
            this.Faces[1].ShiftX(true, buf);

            this.Faces[0].Rotate(false);
        }

        public void RotateLHalf()
        {
            int[] buf;

            buf = this.Faces[1].ShiftX(true, new int[] { -1, -1 }, true);
            buf = this.Faces[3].ShiftX(false, buf, true);
            this.Faces[1].ShiftX(true, buf);

            buf = this.Faces[4].ShiftX(true, new int[] { -1, -1 });
            buf = this.Faces[5].ShiftX(true, buf);
            this.Faces[4].ShiftX(true, buf);

            this.Faces[0].Rotate(true);
            this.Faces[0].Rotate(true);
        }

        public void RotateRClock()
        {
            int[] buf;

            buf = this.Faces[1].ShiftX(false, new int[] { -1, -1 });
            buf = this.Faces[4].ShiftX(false, buf, true);
            buf = this.Faces[3].ShiftX(true, buf, true);
            buf = this.Faces[5].ShiftX(false, buf);
            this.Faces[1].ShiftX(false, buf);

            this.Faces[2].Rotate(true);
        }

        public void RotateRCounterClock()
        {
            int[] buf;

            buf = this.Faces[1].ShiftX(false, new int[] { -1, -1 });
            buf = this.Faces[5].ShiftX(false, buf, true);
            buf = this.Faces[3].ShiftX(true, buf, true);
            buf = this.Faces[4].ShiftX(false, buf);
            this.Faces[1].ShiftX(false, buf);

            this.Faces[2].Rotate(false);
        }

        public void RotateRHalf()
        {
            int[] buf;

            buf = this.Faces[1].ShiftX(false, new int[] { -1, -1 }, true);
            buf = this.Faces[3].ShiftX(true, buf, true);
            this.Faces[1].ShiftX(false, buf);

            buf = this.Faces[4].ShiftX(false, new int[] { -1, -1 });
            buf = this.Faces[5].ShiftX(false, buf);
            this.Faces[4].ShiftX(false, buf);

            this.Faces[2].Rotate(true);
            this.Faces[2].Rotate(true);
        }

        public void RotateDClock()
        {
            int[] buf;

            buf = this.Faces[0].ShiftY(false, new int[] { -1, -1 });
            buf = this.Faces[1].ShiftY(false, buf);
            buf = this.Faces[2].ShiftY(false, buf);
            buf = this.Faces[3].ShiftY(false, buf);
            this.Faces[0].ShiftY(false, buf);

            this.Faces[5].Rotate(true);
        }

        public void RotateDCounterClock()
        {
            int[] buf;

            buf = this.Faces[0].ShiftY(false, new int[] { -1, -1 });
            buf = this.Faces[3].ShiftY(false, buf);
            buf = this.Faces[2].ShiftY(false, buf);
            buf = this.Faces[1].ShiftY(false, buf);
            this.Faces[0].ShiftY(false, buf); 

            this.Faces[5].Rotate(false);
        }

        public void RotateDHalf()
        {
            int[] buf;

            buf = this.Faces[0].ShiftY(false, new int[] { -1, -1 });
            buf = this.Faces[2].ShiftY(false, buf);
            this.Faces[0].ShiftY(false, buf);

            buf = this.Faces[1].ShiftY(false, new int[] { -1, -1 });
            buf = this.Faces[3].ShiftY(false, buf);
            this.Faces[1].ShiftY(false, buf);

            this.Faces[5].Rotate(true);
            this.Faces[5].Rotate(true);
        }

        public void RotateUClock()
        {
            int[] buf;

            buf = this.Faces[0].ShiftY(true, new int[] { -1, -1 });
            buf = this.Faces[3].ShiftY(true, buf);
            buf = this.Faces[2].ShiftY(true, buf);
            buf = this.Faces[1].ShiftY(true, buf);
            this.Faces[0].ShiftY(true, buf);

            this.Faces[4].Rotate(true);
        }

        public void RotateUCounterClock()
        {
            int[] buf;

            buf = this.Faces[0].ShiftY(true, new int[] { -1, -1 });
            buf = this.Faces[1].ShiftY(true, buf);
            buf = this.Faces[2].ShiftY(true, buf);
            buf = this.Faces[3].ShiftY(true, buf);
            this.Faces[0].ShiftY(true, buf);

            this.Faces[4].Rotate(false);
        }

        public void RotateUHalf()
        {
            int[] buf;

            buf = this.Faces[0].ShiftY(true, new int[] { -1, -1 });
            buf = this.Faces[2].ShiftY(true, buf);
            this.Faces[0].ShiftY(true, buf);

            buf = this.Faces[1].ShiftY(true, new int[] { -1, -1 });
            buf = this.Faces[3].ShiftY(true, buf);
            this.Faces[1].ShiftY(true, buf);

            this.Faces[4].Rotate(true);
            this.Faces[4].Rotate(true);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("        ---------\n");
            stringBuilder.AppendFormat("        | {0}   {1} |\n", this.Faces[4].Tiles[0], this.Faces[4].Tiles[1]);
            stringBuilder.AppendFormat("        -       -\n");
            stringBuilder.AppendFormat("        | {0}   {1} |\n", this.Faces[4].Tiles[2], this.Faces[4].Tiles[3]);
            stringBuilder.AppendFormat("---------------------------------\n");
            stringBuilder.AppendFormat("| {0}   {1} | {2}   {3} | {4}   {5} | {6}   {7} |\n", this.Faces[0].Tiles[0], this.Faces[0].Tiles[1], this.Faces[1].Tiles[0], this.Faces[1].Tiles[1], this.Faces[2].Tiles[0], this.Faces[2].Tiles[1], this.Faces[3].Tiles[0], this.Faces[3].Tiles[1]);
            stringBuilder.AppendFormat("-       -       -       -       -\n");
            stringBuilder.AppendFormat("| {0}   {1} | {2}   {3} | {4}   {5} | {6}   {7} |\n", this.Faces[0].Tiles[2], this.Faces[0].Tiles[3], this.Faces[1].Tiles[2], this.Faces[1].Tiles[3], this.Faces[2].Tiles[2], this.Faces[2].Tiles[3], this.Faces[3].Tiles[2], this.Faces[3].Tiles[3]);
            stringBuilder.AppendFormat("---------------------------------\n");
            stringBuilder.AppendFormat("        | {0}   {1} |\n", this.Faces[5].Tiles[0], this.Faces[5].Tiles[1]);
            stringBuilder.AppendFormat("        -       -\n");
            stringBuilder.AppendFormat("        | {0}   {1} |\n", this.Faces[5].Tiles[2], this.Faces[5].Tiles[3]);
            stringBuilder.AppendFormat("        ---------\n");

            return stringBuilder.ToString();
        }
    }
}
