using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquanoxRePAK.Pak;

namespace AquanoxRePAK
{
    internal class Utils
    {


        //////////////////// Enums ///////////////////////

        public enum Game
        {
            Unknown,
            Aquanox1,
            Aquanox2,
            AquaMark3
        }




        /// <summary>
        /// Prints errors and waits for user to press any key to continue
        /// </summary>
        internal static void PrintError(string msg)
        {
            Console.WriteLine("-------------------------------");
            Console.WriteLine("------------ Error ------------");
            Console.WriteLine("-------------------------------");
            Console.WriteLine(msg);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Environment.Exit(1);
        }





        /// <summary>
        /// Gets the name of the game as a string
        /// </summary>
        /// <param name="_ver">Game Type</param>
        /// <returns>String of game name</returns>
        public static string GetGameName(Utils.Game _ver)
        {

            if (_ver == Utils.Game.Unknown) return "Unknown Game";
            if (_ver == Utils.Game.Aquanox1) return "Aquanox 1";
            if (_ver == Utils.Game.Aquanox2) return "Aquanox 2: Revelation";
            if (_ver == Utils.Game.AquaMark3) return "AquaMark 3";

            Utils.PrintError("Code should never get here?, Could not get Game Version string");
            return "Null";
        }



        /// <summary>
        /// Takes the pak file header, returns which game the file is from
        /// </summary>
        /// <param name="_header">Header of pak file</param>
        /// <returns>Game of pak file</returns>
        public static Utils.Game GetGameFromHeader(FileHeader _header)
        {
            if (_header.majorVersion == 3 && _header.minorVersion == 0) return Utils.Game.Aquanox1;
            if (_header.majorVersion == 3 && _header.minorVersion == 2) return Utils.Game.Aquanox2;
            if (_header.majorVersion == 3 && _header.minorVersion == 3) return Utils.Game.AquaMark3;
            return Utils.Game.Unknown;
        }



        /// <summary>
        /// Get game type by looking at the Game Installation folder
        /// </summary>
        /// <param name="_installFolder">Location where Aquanox is installed</param>
        public static Utils.Game GetGameFromInstallFolder(string _installFolder)
        {
            DirectoryInfo aquanox1PakFolder = new DirectoryInfo(_installFolder + "\\dat\\pak");
            DirectoryInfo aquanox2PakFolder = new DirectoryInfo(_installFolder + "\\pak");

            //These values are set depending on detected folders below
            Utils.Game game = Utils.Game.Unknown;



            if (aquanox1PakFolder.Exists)
            {
                game = Utils.Game.Aquanox1;
            }
            else if (aquanox2PakFolder.Exists)
            {
                game = Utils.Game.Aquanox2;
            }

            return game;
        }


    }
}
