using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubiksSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * 0: Green
             * 1: Orange
             * 2: Blue
             * 3: Red
             * 4: Yellow
             * 5: White
             */

            OptimalVsQuickSolveUI();
            // OptimalAndQuickSolveUI();
            // TestFrontTurns();
            // TestBackTurns();
        }

        private static void OptimalVsQuickSolveUI()
        {
            while (true)
            {
                Console.Clear();
                Cube cube = new Cube();

                Console.WriteLine(Cube.ToFriendlyString(Cube.Scramble(20, 30, cube)));

                Console.WriteLine(cube.ToString());
                Console.WriteLine("\nPress any key to begin.");
                Console.ReadKey();

                DateTime beginTimeOptimal = DateTime.Now;

                Console.WriteLine("\n" + beginTimeOptimal.ToLongTimeString() + " - Optimal Solve starting now...");

                Cube.Move[] optimalMoves = Cube.OptimalSolve(cube, false, true);

                DateTime beginTimeQuick = DateTime.Now;
                int[] durationOptimal = GetTimeDiff(beginTimeOptimal, beginTimeQuick);

                Console.WriteLine("\n" + beginTimeQuick.ToLongTimeString() + " - Quick Solve starting now...");

                Cube.Move[] quickMoves = Cube.QuickSolve(cube, true);

                DateTime endTime = DateTime.Now;
                int[] durationQuick = GetTimeDiff(beginTimeQuick, endTime);

                Console.WriteLine("\nOptimal solved in:\t" + durationOptimal[0] + " minute(s), " + durationOptimal[1] + " second(s) and " + durationOptimal[2] + " millisecond(s)");
                Console.WriteLine("Quick solved in:\t" + durationQuick[0] + " minute(s), " + durationQuick[1] + " second(s) and " + durationQuick[2] + " millisecond(s)");

                Console.WriteLine("\nSolutions:");
                Console.WriteLine("Optimal Solve:\t" + Cube.ToFriendlyString(optimalMoves));
                Console.WriteLine("Quick Solve:\t" + Cube.ToFriendlyString(quickMoves));

                Console.ReadKey();
            }
        }

        private static int[] GetTimeDiff(DateTime low, DateTime high)
        {
            int[] timeDiff = new int[3];
            double subTotMilSec = (high - low).TotalMilliseconds;
            timeDiff[0] = (int)Math.Floor(subTotMilSec / 60000);
            subTotMilSec = subTotMilSec - (timeDiff[0] * 60000);
            timeDiff[1] = (int)Math.Floor(subTotMilSec / 1000);
            timeDiff[2] = (int)Math.Floor(subTotMilSec - (timeDiff[1] * 1000));
            return timeDiff;
        }

        private static void OptimalAndQuickSolveUI()
        {
            while (true)
            {
                Cube cube = new Cube();

                Console.WriteLine(Cube.ToFriendlyString(Cube.Scramble(12, 12, cube)));

                Console.WriteLine(cube.ToString());

                char key;
                do
                {
                    Console.WriteLine("\nPress 'S' for Optimal Solve or 'Q' for Quick Solve.");
                    key = Console.ReadKey().KeyChar;
                } while ((key != 's') && (key != 'q'));

                DateTime now = DateTime.Now;
                Console.WriteLine("\nSolve started at: " + now.ToShortTimeString());

                if (key == 's')
                {
                    Console.WriteLine(Cube.ToFriendlyString(Cube.OptimalSolve(cube)));
                }
                else
                {
                    Cube.Move[] moves = Cube.QuickSolve(cube, false);
                    Console.WriteLine(Cube.ApplyMoves(cube, moves));
                    Console.WriteLine(Cube.ToFriendlyString(moves));
                }

                double subTotMilSec = (DateTime.Now - now).TotalMilliseconds;
                int subMin = (int)Math.Floor(subTotMilSec / 60000);
                subTotMilSec = subTotMilSec - (subMin * 60000);
                int subSec = (int)Math.Floor(subTotMilSec / 1000);
                int subMilSec = (int)Math.Floor(subTotMilSec - (subSec * 1000));

                Console.WriteLine("Solved in " + subMin + " minute(s), " + subSec + " second(s) and " + subMilSec + " millisecond(s)");
                Console.ReadKey();
            }
        }

        private static void TestFrontTurns()
        {
            Cube cube = new Cube();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateUClock();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateFClock();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateUCounterClock();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateFCounterClock();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateFHalf();

            Console.WriteLine(cube);
            Console.ReadKey();
        }

        private static void TestBackTurns()
        {
            Cube cube = new Cube();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateUClock();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateBClock();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateUCounterClock();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateBCounterClock();

            Console.WriteLine(cube);
            Console.ReadKey();

            cube.RotateBHalf();

            Console.WriteLine(cube);
            Console.ReadKey();
        }
    }
}
