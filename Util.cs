using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IsochronDrafter
{
    public class Util
    {
        public static readonly int version = 3;
        public static string imageDirectory;
        public static Random random = new Random();

        public static int[] PickN(int max, int count)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < count; i++)
            {
                int newNumber = random.Next(max);
                while (output.Contains(newNumber))
                    newNumber = random.Next(max);
                output.Add(newNumber);
            }
            return output.ToArray();
        }

        public static int Clamp(int min, int value, int max)
        {
            return Math.Min(max, Math.Max(min, value));
        }
    }
}
