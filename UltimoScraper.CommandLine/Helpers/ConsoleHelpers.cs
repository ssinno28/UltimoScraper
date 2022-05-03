using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UltimoScraper.CommandLine.Helpers
{
    public static class ConsoleHelpers
    {
        public static string GetArgument(this List<string> args, string name)
        {
            try
            {
                int index = args.IndexOf(name);
                if (index == -1) return null;

                return args[index + 1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static List<string> MakeArgs(this string consoleInput)
        {
            return consoleInput.Split(' ').ToList();
        }

        public static string Unescape(this string arg)
        {
            return Regex.Replace(arg, @"(\\*)" + "\"", @"\$1$0");
        }
    }
}