namespace InstallerVerificationLibrary.Tools
{
    using System;
    using System.Text;

    public static class FuzzTool
    {
        private static readonly Random Random = new Random();

        public static int RandomNumber(int maxValue)
        {
            return RandomNumber(0, maxValue);
        }

        public static int RandomNumber(int min, int max)
        {
            return Random.Next(min, max);
        }

        public static char UnicodeChar()
        {
            var bytes = new byte[2];
            Random.NextBytes(bytes);
            var characters = Encoding.Unicode.GetChars(bytes);
            return characters[0];
        }

        public static char AsciiCodeChar()
        {
            var number = RandomNumber(32, 127);
            var character = (char) number;
            return character;
        }

        public static string UnicodeString(int length)
        {
            if (length > 1000)
            {
                throw new ArgumentException("Length is bigger than 1000");
            }

            var bytes = new byte[length*2];
            Random.NextBytes(bytes);
            var characters = Encoding.Unicode.GetChars(bytes);
            return new string(characters);
        }

        public static string AsciiString(int length)
        {
            var myBuilder = new StringBuilder(length);
            var i = 0;

            for (; i < length;)
            {
                myBuilder.Append(AsciiCodeChar());
                i++;
            }

            return myBuilder.ToString();
        }
    }
}