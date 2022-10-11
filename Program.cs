

using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;


namespace AquanoxRePAK
{
    internal class Program
    {



        //      Structure of this file
        //
        // Main                  - Just Handles Command line args
        // Print Help            - Prints help
        // Print Error           - Prints error message
        // Arg Functions         - Functions to handle command line arguments, one for each command line option
        //


        //     Structure of pak file  {bytelength}
        //
        // char   {12}              - MagicBytes, tells us this is a MASSIVEFILE .pak (or not)
        // ushort {2}               - Major version
        // ushort {2}               - Minor Version
        // uint   {4}               - File Count
        // char   {60}              - Copyright String
        // char   {4}               - Marks start of file table? (Not found a use for this, always = "LPT<0x00>")
        //
        // char {132} * File Count  - File Table, Contents are as below, each are encrypted individually
        //      char {128}          - File Name (Encrypted)
        //      char {4}            - File Size (Encrypted)
        //
        // char {???} * File Count  - File Contents, Length varies and is determined by the file table
        //
        //
        //Note: Order of files does not matter to Aquanox when files are repacked


        static string version = "v1.0"; //This programs version




        ////////////////////////// Main ////////////////////////////


        /// <summary>
        /// Command line arguments are handled here
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine($"\n===== Pyro's AquaNoxRePAK Utility {version} =====");
            Console.WriteLine("Unpack and Repack Aquanox .pak files");

            //Print Help
            if (args.Contains("-h"))
            {
                PrintHelp();
            }


            //Install mods
            if (args.Contains("-i") || args.Length == 0)
            {
                InstallMods();
            }


            //Revert back to vanilla
            if (args.Contains("-r"))
            {
                RevertMods();
            }


            //Print contents of a pak file
            if (args.Contains("-l"))
            {
                if (args.Length == 2)
                {
                    ListContents(args[1]);
                }
                else
                {
                    Console.WriteLine("Wrong number of arguments");
                    PrintHelp();
                }
            }


            //Unpack single file
            if (args.Contains("-u"))
            {
                if (args.Length == 3)
                {
                    UnPackFile(args[1], args[2]);
                }
                else
                {
                    Console.WriteLine("Wrong number of arguments");
                    PrintHelp();
                }
                
            }

            //Unpack all .pak files in a directory
            if (args.Contains("-U"))
            {
                if (args.Length == 3)
                {
                    UnpackAllFiles(args[1], args[2]);
                }
                else
                {
                    Console.WriteLine("Wrong number of arguments");
                    PrintHelp();
                }
            }


            //Unpack all .pak files in a directory
            if (args.Contains("-p"))
            {
                if (args.Length == 4)
                {
                    PackFolder(args[1], args[2], args[3]);
                }
                else
                {
                    Console.WriteLine("Wrong number of arguments");
                    PrintHelp();
                }
            }

            //Pack all folders in a directory
            if (args.Contains("-P"))
            {
                if (args.Length == 4)
                {
                    PackAllFolders(args[1], args[2], args[3]);
                }
                else
                {
                    Console.WriteLine("Wrong number of arguments");
                    PrintHelp();
                }
            }
        }












        ////////////////////////////////// Print Help ////////////////////////////////


        /// <summary>
        /// Print help text
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine("\nSimple Options:");
            Console.WriteLine("-h          \tPrint this help message");
            Console.WriteLine("-i          \tInstalls mods placed in mod folder");
            Console.WriteLine("-r          \tRevert last mod install");
            Console.WriteLine();
            Console.WriteLine("            \tNote: For mod installs, this program must be placed in the");
            Console.WriteLine("            \tsame folder as your Aquanox 1 or 2 executable.");
            Console.WriteLine("            \tThe mods folder must also be placed in the same directory.");

            Console.WriteLine("\nAdvanced Options:");
            Console.WriteLine("-l source.pak                            \tList all files in target pak");
            Console.WriteLine("-u source.pak targetFolder               \tUnpack a file");
            Console.WriteLine("-U sourceFolder targetFolder             \tUnpack all .pak files to target directory");
            Console.WriteLine("-p gameVersion sourceFolder target.pak   \tPack a folder into a .pak file");
            Console.WriteLine("-P gameVersion sourceFolder targetFolder \tPack all folders in current directory");


            Console.WriteLine("\nExample: \"AquanoxRePAK.exe -i \"              \tInstalls for mods in /mods folder, Auto-Detect game version");
            Console.WriteLine("Example: \"AquanoxRePAK.exe -u aquanox0.pak .\\\"\tUnpacks the file to the current directory");
            Console.WriteLine("Example: \"AquanoxRePAK.exe 1 -p folder .\\\"    \tPacks the target folder for Aquanox 1 and places it in the current directory\n");
            Console.WriteLine("Source Code can be found on github: https://github.com/PyroChiliarch/AquanoxRePAK\n");
        }



        







        ////////////////////////////////// Main Methods ////////////////////////////////


        /// <summary>
        /// Install mods automagically
        /// Takes zip files from the Mod folder in the Aquanox install directory
        /// </summary>
        private static void InstallMods ()
        {
            Console.WriteLine ("--- Installing Mods ---");

            //Get all folders and variables that we can at the start
            DirectoryInfo pwd = new DirectoryInfo(Directory.GetCurrentDirectory());
            Utils.Game game = Pak.GetGameFromInstallFolder(pwd.FullName);

            DirectoryInfo pakFolder = Pak.GetPakFolder(pwd.FullName);
            if (!pakFolder.Exists) Utils.PrintError("Could not find pak folder!, is AquanoxRePAK in the install directory?");

            DirectoryInfo modFolder = new DirectoryInfo("mods");
            if (!modFolder.Exists) Utils.PrintError("Could not find mods folder!, please create mod folder and place mods in it.");

            
            FileInfo[] modFiles = modFolder.GetFiles();
            FileInfo[] pakFiles = pakFolder.GetFiles("*.pak");


            
            
            //Tell user which game we found
            if (game == Utils.Game.Aquanox1)
            {
                Console.WriteLine($"Found dat\\pak\\ folder! Assuming {Pak.GetGameName(game)}");
            }
            else if (game == Utils.Game.Aquanox2)
            {
                Console.WriteLine($"Found pak\\ folder! Assuming {Pak.GetGameName(game)}");
            }




            //Lots of error checking, The install command is a basic command and the user may not be familiar with the tool or command lines
            //Want to make sure we give them lots of information to figure things out
            if (game == Utils.Game.Unknown) Utils.PrintError("Unknown game install folder! Did you place the tool in an install folder?");
            if (!modFolder.Exists) Utils.PrintError("Mods folder does not exits! Please create a mods folder or check you spelling!");
            if (modFiles.Length == 0) Utils.PrintError("No mods in mod folder, check mods folder is not empty!");
            if (pakFiles.Length == 0) Utils.PrintError("Cannot find pak files, this is bad, try option -r to revert or verify integrity of game files in steam");


            //Unpack pak files
            Console.WriteLine("Unpacking pak files");
            Console.WriteLine(pakFolder.FullName);
            UnpackAllFiles(pakFolder.FullName, pakFolder.FullName);



            //Create bak files for revert
            Console.WriteLine("Creating bak files for restore");
            for (int i = 0; i < pakFiles.Length; i++)
            {
                try {pakFiles[i].MoveTo(pakFiles[i].FullName + ".bak", false);} catch { } //Fails silently, means there is already a backup
            }


            //Unpack mods to a temp directory
            Console.WriteLine("Unpacking Mods");
            DirectoryInfo tempDir = Directory.CreateDirectory(modFolder + "\\temp");
            for (int i = 0; i < modFiles.Length; i++)
            {
                Console.WriteLine($"Unpacking: {modFiles[i].Name}");
                ZipFile.ExtractToDirectory(modFiles[i].FullName, tempDir.FullName, true);
            }
            DirectoryInfo[] unpackedPaks = pakFolder.GetDirectories();


            //Insert mods, these will be merged with the currently unpacked pak folders
            Console.WriteLine("Copying mods to unpacked pak folders");
            DirectoryInfo[] tempModDirs = tempDir.GetDirectories();
            for (int i = 0; i < tempModDirs.Length; i++)
            {
                FileSystem.MoveDirectory(tempModDirs[i].FullName, unpackedPaks[i].Parent.FullName + "\\" + tempModDirs[i].Name, true);
            }


            //Repack pak files
            Console.WriteLine("Repacking files");
            PackAllFolders(((int)game).ToString(), pakFolder.FullName, pakFolder.FullName);


            //Clean up
            Console.WriteLine("Cleaning up");
            tempDir.Delete(true);
            foreach (DirectoryInfo folder in unpackedPaks)
            {
                folder.Delete(true);
            }


            Console.WriteLine("--- Mod Install Complete ---");
        }












        /// <summary>
        /// Revert back to vanilla, uses .bak files from a previous install
        /// </summary>
        private static void RevertMods ()
        {
            Console.WriteLine("--- Reverting to vanilla ---");


            //Get all folders and variables that we can at the start
            DirectoryInfo pwd = new DirectoryInfo(Directory.GetCurrentDirectory());
            Utils.Game game = Pak.GetGameFromInstallFolder(pwd.FullName);
            DirectoryInfo pakFolder = Pak.GetPakFolder(pwd.FullName);
            FileInfo[] pakFiles = pakFolder.GetFiles("*.pak");
            FileInfo[] bakFiles = pakFolder.GetFiles("*.bak");

            Console.WriteLine("--- Removing current .pak files");
            for (int i = 0; i < pakFiles.Length; i++)
            {
                Console.WriteLine($"Deleting {pakFiles[i].Name}");
                pakFiles[i].Delete();
            }

            Console.WriteLine("--- Replacing pak files from backup");
            for (int i = 0; i < bakFiles.Length; i++)
            {
                Console.WriteLine($"Restoring {bakFiles[i].Name}");
                bakFiles[i].MoveTo(bakFiles[i].FullName.Substring(0, bakFiles[i].FullName.Length - 4)); //Moves bak file to same spot and name, just removes last 4 chars ".bak"
            }

            Console.WriteLine("--- Revert Complete ---");
        }









        private static void ListContents (string _targetPakFile)
        {
            Console.WriteLine("--- Listing pak contents ---");



            //Variables used
            FileInfo pakFileInfo = new FileInfo(_targetPakFile);           //Target .pak file
            FileStream pakFile = new FileStream(pakFileInfo.Name, FileMode.Open, FileAccess.Read); //Open target pack file for reading
            Pak.FileHeader pakFileHeader = new Pak.FileHeader();    //Holds variables found in the header after its read
            Pak.EncryptedFileDetails[] encryptedFileTable;          //The is read after the header, needs to be decrypted for use
            Pak.FileDetails[] fileTable;                            //After decryption, can be used to extract files from .pak




            ////////////////////////// Read the head of the file ////////////////////////////

            pakFileHeader = Pak.ReadHeader(pakFile);
            Console.WriteLine("MagicBytes: " + pakFileHeader.magicBytes);
            Console.WriteLine($"File Version: v{pakFileHeader.majorVersion}.{pakFileHeader.minorVersion} - {Pak.GetGameName(Pak.GetGameFromHeader(pakFileHeader))}");
            Console.WriteLine("File Count: " + pakFileHeader.fileCount);
            Console.WriteLine("Copyright: " + pakFileHeader.copyright);


            ///////////////////////////////// Read File Table //////////////////////////////

            Console.Write("Grabbing File Table: ");
            //Read The Data
            encryptedFileTable = Pak.ReadFileTable(pakFile, pakFileHeader);
            //filetable read complete
            Console.WriteLine("Done");




            //////////////////////////////// Decrpyt File Table ///////////////////////////

            Console.Write("Decrypting File Table: ");
            fileTable = Pak.DecryptFileTable(encryptedFileTable, pakFileHeader);
            //Decryption Complete
            Console.WriteLine("Done");


            ////////////////////////////// Print Results ///////////////////////////
            foreach (Pak.FileDetails file in fileTable)
            {
                Console.WriteLine($"Filename: {file.FileName} \t\t Bytes:{file.FileSize}");
            }

            Console.WriteLine("Total Files: " + fileTable.Length);


            Console.WriteLine("--- Listing Complete ---");


        }




        //Takes a folder and turns it into a .pak file
        private static void PackFolder(string _targetGame, string _sourceDirectory, string _targetFileName)
        {
            Console.WriteLine("--- Packing Folder ---");
            int targetGame = int.Parse(_targetGame);


            Console.WriteLine($"New file name: {_targetFileName}");

            Console.Write($"Packing Folder: ");
            uint fileCount = Pak.PackFolder(_sourceDirectory, _targetFileName, targetGame);
            Console.WriteLine("Done");

            Console.WriteLine($"Packed files count: {fileCount}");

            

            Console.WriteLine("--- Folder packing complete ---");
        }



        //Packs all folders in a directory and places the .pak files in another
        private static void PackAllFolders(string _targetGame, string _sourceDirectory, string _targetDirectory)
        {
            Console.WriteLine("--- Packing Multiple Directories ---");

            DirectoryInfo sourceDirectory = new DirectoryInfo(_sourceDirectory);
            DirectoryInfo[] packTargets = sourceDirectory.GetDirectories();

            for (int i = 0; i < packTargets.Length; i++)
            {
                PackFolder(_targetGame, packTargets[i].FullName, packTargets[i].FullName + ".pak");
            }

            Console.WriteLine($"{packTargets.Length} folders packed");
            Console.WriteLine("--- All Directories Complete ---");
        }



        //Unpack single file and place in target directory
        private static void UnPackFile(string _targetPakFile, string _targetDirectory)
        {
            Console.WriteLine("--- Unpacking file ---");


            //Variables used
            FileInfo pakFileInfo = new FileInfo(_targetPakFile);           //Target .pak file
            FileStream pakFile = new FileStream(pakFileInfo.FullName, FileMode.Open, FileAccess.Read); //Open target pack file for reading
            Pak.FileHeader pakFileHeader = new Pak.FileHeader();    //Holds variables found in the header after its read
            Pak.EncryptedFileDetails[] encryptedFileTable;          //The is read after the header, needs to be decrypted for use
            Pak.FileDetails[] fileTable;                            //After decryption, can be used to extract files from .pak




            ////////////////////////// Read the head of the file ////////////////////////////

            pakFileHeader = Pak.ReadHeader(pakFile);
            Console.WriteLine("MagicBytes: " + pakFileHeader.magicBytes);
            Console.WriteLine($"File Version: v{pakFileHeader.majorVersion}.{pakFileHeader.minorVersion} - {Pak.GetGameName(Pak.GetGameFromHeader(pakFileHeader))}");
            Console.WriteLine("File Count: " + pakFileHeader.fileCount);
            Console.WriteLine("Copyright: " + pakFileHeader.copyright);


            ///////////////////////////////// Read File Table //////////////////////////////

            Console.Write("Grabbing File Table: ");
            //Read The Data
            encryptedFileTable = Pak.ReadFileTable(pakFile, pakFileHeader);
            //filetable read complete
            Console.WriteLine("Done");




            //////////////////////////////// Decrpyt File Table ///////////////////////////

            Console.Write("Decrypting File Table: ");
            fileTable = Pak.DecryptFileTable(encryptedFileTable, pakFileHeader);
            //Decryption Complete
            Console.WriteLine("Done");



            //////////////////////////////// Extract files from pak ///////////////////////////

            Console.Write("Extracting files: ");
            string parentFolder = pakFileInfo.Name.Split('.')[0]; //use pak file name as parent folder for all files, this will help with repacking
            string extractFilesDirectory = _targetDirectory + "/" + parentFolder;

            //Loop over all files and extract them
            for (int i = 0; i < pakFileHeader.fileCount; i++)
            {
                Pak.ExtractFile(fileTable[i], pakFile, extractFilesDirectory);
            }
            Console.WriteLine("Done");


            pakFile.Close();



            Console.WriteLine("--- Unpack Complete ---");

        }


        //Unpack all files in source directory, place in target directory
        private static void UnpackAllFiles (string _sourceDirectory, string _targetDirectory)
        {
            Console.WriteLine("--- Unpacking Directory ---");
            


            DirectoryInfo sourceDirectory = new DirectoryInfo(_sourceDirectory);
            DirectoryInfo targetDirectory = new DirectoryInfo(_targetDirectory);

            FileInfo[] files = sourceDirectory.GetFiles("*.pak");

            foreach (FileInfo file in files)
            {
                UnPackFile(file.FullName, targetDirectory.FullName);
            }


            Console.WriteLine($"{files.Length} unpacked");
            Console.WriteLine("--- All files complete ---");
        }
        







        






        





    }
}