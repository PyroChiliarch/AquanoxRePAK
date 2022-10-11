using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AquanoxRePAK
{
    internal static class Pak
    {





        //Details for .pak file format were taken from AquanoxMarkUnPAK by jTommy



        ///////////////////// Keys //////////////////////
        // To be used for Pak file decryption

        internal static byte[] Aquanox1Key = {
            0x68, 0x3c, 0x61, 0x37, 0x4c, 0x6c, 0xc4, 0x4f, 0x6f, 0x72, 0x78, 0x48, 0x33, 0x4a, 0x2b, 0x78,
            0xdc, 0xdf, 0x61, 0x62, 0x4b, 0x6e, 0x29, 0x6a, 0x73, 0x6c, 0x6e, 0x44, 0x6f, 0x4a, 0x44, 0x66,
            0x68, 0x44, 0x33, 0x37, 0x66, 0x55, 0x67, 0x4f, 0x6f, 0xd6, 0x78, 0x48, 0x33, 0x58, 0x32, 0x78,
            0x35, 0x41, 0x61, 0x35, 0x51, 0x37, 0x6e, 0x2a, 0xf6, 0x6c, 0x2b, 0xfc, 0x6f, 0x4a, 0x23, 0x40
        };

        internal static byte[] Aquanox2Key = {
            0x68, 0x3c, 0x61, 0x37, 0x4c, 0x6c, 0xc4, 0x4f, 0x6f, 0x72, 0x78, 0x48, 0x33, 0x4a, 0x2b, 0x78,
            0xdc, 0xdf, 0x61, 0x62, 0x4b, 0x6e, 0x29, 0x6a, 0x73, 0x6c, 0x6e, 0x44, 0x6f, 0x4a, 0x44, 0x66,
            0x68, 0x44, 0x33, 0x37, 0x66, 0x55, 0x67, 0x4f, 0x6f, 0xd6, 0x78, 0x48, 0x33, 0x58, 0x32, 0x78,
            0x35, 0x41, 0x61, 0x35, 0x51, 0x37, 0x6e, 0x2a, 0xf6, 0x6c, 0x2b, 0xfc, 0x6f, 0x4a, 0x23, 0x40
        };

        internal static byte[] Aquamark3Key = {
            0x70, 0x4a, 0x4b, 0x6a, 0x33, 0x77, 0x34, 0x44, 0x6f, 0x4d, 0x3b, 0x27, 0x44, 0x27, 0x53, 0x44,
            0x36, 0x64, 0x6b, 0x6e, 0x6c, 0x3d, 0x36, 0x63, 0x41, 0x4a, 0x20, 0x53, 0x58, 0x63, 0x49, 0x41,
            0x21, 0x59, 0x33, 0x34, 0x35, 0x53, 0x6f, 0x67, 0x24, 0xa3, 0x25, 0x53, 0x4e, 0x64, 0x36, 0x40,
            0x20, 0x58, 0x30, 0x39, 0x38, 0x61, 0x73, 0x37, 0x78, 0x63, 0x41, 0x2a, 0x28, 0x53, 0x44, 0x7b
        };







        //////////////////// Structs ///////////////////////
        internal struct FileHeader
        {
            public string magicBytes;

            public ushort majorVersion;

            public ushort minorVersion;

            public uint fileCount;

            public string copyright;

            public string lpt;

            public FileHeader()
            {
                magicBytes = "nil";
                majorVersion = 0;
                minorVersion = 0;
                fileCount = 0;
                copyright = "nil";
                lpt = "nil";
            }

        }


        internal struct EncryptedFileDetails
        {
            public byte[] FileName;
            public byte[] FileSize;

            public EncryptedFileDetails()
            {
                FileName = new byte[128];
                FileSize = new byte[4];
            }
        }


        internal struct FileDetails
        {
            public string FileName;
            public uint FileSize;

            public FileDetails()
            {
                FileName = "";
                FileSize = 0;
            }
        }







        //////////////////// Main Pak file functions ///////////////////////



        /// <summary>
        /// Reads and returns Pak file header information
        /// Expects file stream to be at start of file
        /// </summary>
        /// <param name="_file">Filestream of pak file</param>
        /// <returns>PakFileHeader, struct with header information</returns>
        internal static FileHeader ReadHeader(FileStream _file)
        {
            FileHeader header = new FileHeader();

            //Read the Magic Bytes
            byte[] magicBytesData = new byte[12];
            int bytesRead = _file.Read(magicBytesData, 0, magicBytesData.Length);
            if (bytesRead != magicBytesData.Length) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check
            header.magicBytes = Encoding.ASCII.GetString(magicBytesData);
            if (header.magicBytes != ("MASSIVEFILE" + (char)0x00)) Utils.PrintError("File is not a MASSIVE pak file"); //Sanity Check

            //Read Major version
            byte[] fileMajorVersionData = new byte[2];
            bytesRead = _file.Read(fileMajorVersionData, 0, fileMajorVersionData.Length);
            if (bytesRead != fileMajorVersionData.Length) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check
            header.majorVersion = BitConverter.ToUInt16(fileMajorVersionData);

            //Read Minor version
            byte[] fileMinorVersionData = new byte[2];
            bytesRead = _file.Read(fileMinorVersionData, 0, fileMinorVersionData.Length);
            if (bytesRead != fileMinorVersionData.Length) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check
            header.minorVersion = BitConverter.ToUInt16(fileMinorVersionData);

            //Read File Count
            byte[] fileCountData = new byte[4];
            bytesRead = _file.Read(fileCountData, 0, fileCountData.Length);
            if (bytesRead != fileCountData.Length) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check
            header.fileCount = BitConverter.ToUInt32(fileCountData);

            //Read Copyright data
            byte[] copyrightData = new byte[60];
            bytesRead = _file.Read(copyrightData, 0, copyrightData.Length);
            if (bytesRead != copyrightData.Length) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check
            header.copyright = Encoding.ASCII.GetString(copyrightData);

            //Read Unknown String
            byte[] lptData = new byte[4];
            bytesRead = _file.Read(lptData, 0, lptData.Length);
            if (bytesRead != lptData.Length) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check
            header.lpt = Encoding.ASCII.GetString(lptData);

            return header;
        }


        /// <summary>
        /// Reads and returns the Pak File table, result is encrypted and needs to be processed
        /// Expects stream to be at start of file table (after header)
        /// </summary>
        /// <param name="_file">File stream of pak file, read position must be at start of file table</param>
        /// <returns></returns>
        internal static EncryptedFileDetails[] ReadFileTable(FileStream _file, FileHeader header)
        {
            EncryptedFileDetails[] fileTable = new EncryptedFileDetails[header.fileCount];

            //Read the pak's file table
            for (int i = 0; i < header.fileCount; i++)
            {

                fileTable[i] = new EncryptedFileDetails();

                int bytesRead = _file.Read(fileTable[i].FileName, 0, 128); //128 is the filename length
                if (bytesRead != 128) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check

                bytesRead = _file.Read(fileTable[i].FileSize, 0, 4); //128 is the fileSize length
                if (bytesRead != 4) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check


                
            }
            

            return fileTable;

        }






        internal static FileDetails[] DecryptFileTable(EncryptedFileDetails[] _encryptedFileTable, FileHeader _header)
        {
            //Iterates over all encrypted file details
            //Decrypts name and size of each entry in file details
            //Only difference between Aquanox 1 and Aquanox 2 is that Aquanox 2 has static offsets when reading bytes from the key

            
            FileDetails[] fileTable = new FileDetails[_encryptedFileTable.Length];
            Utils.Game game = Utils.GetGameFromHeader(_header); //Different gamea require different methods to decrypt


            



            //Iterate over all files in table
            for (int i = 0; i < _encryptedFileTable.Length; i++)
            {



                ///////////////////////Decrypt Filename
                //Iterate over characters in filename
                for (int c = 0; c < _encryptedFileTable[i].FileName.Length; c++)
                {

                    byte enc; //Encrypted Byte
                    byte nameKey; //Key byte to decrypt
                    int unencValue; //Unencrypted Value
                    char unencChar; //Final Unencrypted Character



                    //Get byte to decrypt
                    enc = _encryptedFileTable[i].FileName[c]; 
                    if (enc == 0x00) break; //Null values start when data ends, Means we can stop


                    //Get Key byte for decryption
                    if (game == Utils.Game.Aquanox1)
                    {
                        nameKey = Aquanox1Key[(c + i) % Aquanox1Key.Length];
                    }
                    else if (game == Utils.Game.Aquanox2)
                    {
                        nameKey = Aquanox2Key[(c + i + 61) % Aquanox2Key.Length]; //61 is the offset specific to getting the key for decypting Aquanox2 file names
                    }
                    else
                    {
                        nameKey = 0; //Dummy value
                        Utils.PrintError($"Cannot Decrypt file name in pak file of {Utils.GetGameName(game)}");
                    }


                    //Decrypt the byte
                    unencValue = ((int)enc - (int)nameKey);



                    //Wrap value around if negative so it can become a valid char (range 0-255)
                    if (unencValue < 0) 
                    {
                        unencChar = (char)(unencValue + 256);
                    }
                    else
                    {
                        unencChar = (char)unencValue;
                    }

                    //Add Unencrypted Character to filename
                    fileTable[i].FileName += unencChar; 

                }




                //////////////////Decypt Filesize
                //Get the key for decrypting file size
                uint fileSizeKey;
                if (game == Utils.Game.Aquanox1)
                {
                    fileSizeKey = BitConverter.ToUInt32(Aquanox1Key, i % (Aquanox1Key.Length - 4));
                }
                else if (game == Utils.Game.Aquanox2)
                {
                    fileSizeKey = BitConverter.ToUInt32(Aquanox2Key, (i + 41) % (Aquanox2Key.Length - 4)); //41 is the offset specific to getting the key for decypting Aquanox2 file sizes
                }
                else
                {
                    fileSizeKey = 0; //Dummy Value
                    Utils.PrintError($"Cannot Decrypt file size in pak file of {Utils.GetGameName(game)}");
                }

                //Decrypt file size
                fileTable[i].FileSize = BitConverter.ToUInt32(_encryptedFileTable[i].FileSize, 0) - fileSizeKey; //Encrypted file size is an array of bytes and must be converted to uint before decrypting

            }

            return fileTable;
        }





        internal static EncryptedFileDetails[] EncryptFileTable(FileDetails[] _fileTable, FileHeader _header)
        {

            EncryptedFileDetails[] encryptedFileTable = new EncryptedFileDetails[_fileTable.Length];
            Utils.Game game = Utils.GetGameFromHeader(_header);

            //Loop file details in filetable, and encrypt each value
            for (int i = 0; i < _fileTable.Length; i++)
            {

                encryptedFileTable[i] = new EncryptedFileDetails();

                //Encrypt filename
                for (int c = 0; c < _fileTable[i].FileName.Length; c++)
                {

                    byte enc; //Encrypted Byte
                    byte nameKey; //Key byte to decrypt
                    int encValue; //Unencrypted Value
                    char unencChar; //Final Unencrypted Character



                    //Get byte to decrypt
                    //enc = _fileTable[i].FileName[c];
                    //Get Char to encrypt
                    unencChar = _fileTable[i].FileName[c];

                    //if (enc == 0x00) break; //Null values start when data ends, Means we can stop
                    //Not needed, only for decryption

                    //Get Key byte for decryption
                    if (game == Utils.Game.Aquanox1)
                    {
                        nameKey = Aquanox1Key[(c + i) % Aquanox1Key.Length];
                    }
                    else if (game == Utils.Game.Aquanox2)
                    {
                        nameKey = Aquanox2Key[(c + i + 61) % Aquanox2Key.Length]; //61 is the offset specific to getting the key for decypting Aquanox2 file names
                    }
                    else
                    {
                        nameKey = 0; //Dummy value
                        Utils.PrintError($"Cannot Decrypt file name in pak file of {Utils.GetGameName(game)}");
                    }


                    //encypt the byte
                    encValue = ((int)unencChar + (int)nameKey);



                    //Wrap value around if out of byte range 0-255
                    if (encValue > 255)
                    {
                        enc = (byte)(encValue - 256);
                    }
                    else
                    {
                        enc = (byte)encValue;
                    }

                    //Add Unencrypted Character to filename
                    encryptedFileTable[i].FileName[c] = enc;
                }
                

                //Encrypt filelength
                uint fileSizeKey;
                if (game == Utils.Game.Aquanox1)
                {
                    fileSizeKey = BitConverter.ToUInt32(Aquanox1Key, i % (Aquanox1Key.Length - 4));
                }
                else if (game == Utils.Game.Aquanox2)
                {
                    fileSizeKey = BitConverter.ToUInt32(Aquanox2Key, (i + 41) % (Aquanox2Key.Length - 4)); //41 is the offset specific to getting the key for decypting Aquanox2 file sizes
                }
                else
                {
                    fileSizeKey = 0; //Dummy value
                    Utils.PrintError($"Cannot Decrypt file size in pak file of {Utils.GetGameName(game)}");
                }



                encryptedFileTable[i].FileSize = BitConverter.GetBytes(_fileTable[i].FileSize + fileSizeKey); //Reverse of decryption


            }

            return encryptedFileTable;
        }









        /// <summary>
        /// Extract file data and creates it at a new location in the target directory
        /// Expects filestream to be at start of the target file (files start after file table)
        /// </summary>
        /// <param name="_fileDetails"></param>
        /// <param name="_file"></param>
        internal static void ExtractFile(FileDetails _fileDetails, FileStream _file, string _targetDirectory)
        {

            ////////Read file contents
            byte[] fileContents = new byte[_fileDetails.FileSize];
            int bytesRead = _file.Read(fileContents, 0, (int)_fileDetails.FileSize);
            if (bytesRead != (int)_fileDetails.FileSize) Utils.PrintError("Unexpected number of bytes read! Did the file end too soon?"); //Sanity check


            ////////Write file to disk
            FileInfo fileInfo = new FileInfo(_targetDirectory + "\\" + _fileDetails.FileName); //Prepare full file name
            Directory.CreateDirectory(fileInfo.DirectoryName); //Create Directory
            FileStream newFile = File.Create(fileInfo.FullName); //Create File
            newFile.Write(fileContents);                        //Write file
            newFile.Close();

        }











        /// <summary>
        /// Packs a target folder
        /// </summary>
        /// <param name="_sourceDirectory"></param>
        /// <param name="_targetFileName"></param>
        /// <param name="_targetGame"></param>
        internal static uint PackFolder(string _sourceDirectory, string _targetFileName, int _targetGame)
        {
            //Function Variables
            DirectoryInfo sourceDirectory = new DirectoryInfo(_sourceDirectory);
            FileInfo targetFile = new FileInfo(_targetFileName);
            Utils.Game targetGame = (Utils.Game)_targetGame;
            byte[] nullByte = new byte[1] { 0x00 };

            //Variables for packing
            FileDetails[] fileDetails;
            EncryptedFileDetails[] encryptedFileDetails;
            FileHeader header;

            //Get all files that need packing
            FileInfo[] filesToPack = sourceDirectory.GetFiles("*", SearchOption.AllDirectories);





            //////// Create header
            header = new FileHeader();
            header.magicBytes = "MASSIVEFILE";
            header.fileCount = (uint)filesToPack.Length;
            header.copyright = "This is a modded pak - Repacking done with AquanoxRePak.exe";
            header.lpt = "LPT";
            if (targetGame == Utils.Game.Aquanox1)
            {

                header.majorVersion = 3;
                header.minorVersion = 0;
            } else if (targetGame == Utils.Game.Aquanox2)
            {
                header.majorVersion = 3;
                header.minorVersion = 2;
            } else
            {
                Utils.PrintError($"Cannot pack files for {Utils.GetGameName(targetGame)}");
            }






            //////// Calculate fileNames and sizes to use for FileTable
            fileDetails = new FileDetails[filesToPack.Length];
            for (int i = 0; i < filesToPack.Length; i++)
            {
                fileDetails[i].FileName = filesToPack[i].FullName.Replace(sourceDirectory.FullName + "\\", "");
                fileDetails[i].FileSize = (uint)filesToPack[i].Length;
            }


            //////// Encrypt fileDetails
            encryptedFileDetails = EncryptFileTable(fileDetails, header);




            ////////// Create file
            FileStream newFile = File.Create(targetFile.FullName);
            Encoding enc = Encoding.ASCII;


            //Write header to file
            newFile.Write(enc.GetBytes(header.magicBytes));
            newFile.Write(nullByte);
            newFile.Write(BitConverter.GetBytes(header.majorVersion));
            newFile.Write(BitConverter.GetBytes(header.minorVersion));
            newFile.Write(BitConverter.GetBytes(header.fileCount));
            newFile.Write(enc.GetBytes(header.copyright));
            newFile.Write(nullByte);
            newFile.Write(enc.GetBytes(header.lpt));
            newFile.Write(nullByte);


            //Write File Table
            for (int i = 0; i < filesToPack.Length;i++)
            {
                byte[] fileNameBytes = new byte[128];
                encryptedFileDetails[i].FileName.CopyTo(fileNameBytes, 0);

                newFile.Write(fileNameBytes);
                newFile.Write(encryptedFileDetails[i].FileSize);
            }


            //Write file contents to new pak file
            for (int i = 0; i < filesToPack.Length; i++)
            {
                FileStream oldFile = filesToPack[i].Open(FileMode.Open);
                byte[] oldFileContents = new byte[oldFile.Length];
                oldFile.Read(oldFileContents);
                newFile.Write(oldFileContents);
                oldFile.Close();
            }

            //Close file
            newFile.Close();


            //Return file count
            return header.fileCount;


        }














        internal static DirectoryInfo GetPakFolder(string _installFolder)
        {
            DirectoryInfo aquanox1PakFolder = new DirectoryInfo(_installFolder + "\\dat\\pak");
            DirectoryInfo aquanox2PakFolder = new DirectoryInfo(_installFolder + "\\pak");

            //These values are set depending on detected folders below
            DirectoryInfo pakFolder = new DirectoryInfo("null");



            if (aquanox1PakFolder.Exists)
            {
                pakFolder = aquanox1PakFolder;
            }
            else if (aquanox2PakFolder.Exists)
            {
                pakFolder = aquanox2PakFolder;
            }


            return pakFolder;
        }



    }









    

}
