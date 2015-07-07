using System;
using System.Drawing;

namespace NuFridge.Shared.Server.Statistics.Design
{
    public class ColorGenerator
    {
        private readonly Random _random = new Random();

        public string NextColour()
        {
            Color c = Color.FromArgb(150, _random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255));

            return HexConverter(c);
        }

        private static String HexConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

    }
}
