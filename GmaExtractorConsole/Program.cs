using GmaExtractorLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading;

namespace GmaExtractorConsole
{
    class Program
    {
      
        public class Config
        {
            public string BinPath;
            public string ContentPath;
            //public string SevenZipExePath;
            public string ExtractPath;
        }

        private static string CurrentDirectoryPath = null;

        static void Main(string[] args)
        {
            CurrentDirectoryPath = System.AppDomain.CurrentDomain.BaseDirectory;

            if (File.Exists(CurrentDirectoryPath + "\\windowsdesktop-runtime-3.1.1-win-x64.exe"))
                File.Delete(CurrentDirectoryPath + "\\windowsdesktop-runtime-3.1.1-win-x64.exe");

            InitConfig();

            Extractor.ParseDirectory();

            if (args.Length != 0)
            {
                foreach (string path in args)
                {
                    Console.WriteLine("Path - " + path);

                    FileAttributes attr = File.GetAttributes(path);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        if (int.TryParse(Path.GetFileName(path), out _))
                        {
                            Extractor.ExtractSingle(Convert.ToInt32(Path.GetFileName(path)));
                        }
                    }
                    else
                        Extractor.ExtractSingleFile(path);
                }
            }
            else
            {
                bool IsExit = false;
                while (IsExit == false)
                {
                    Console.WriteLine("\n  Garry's Mod addons extractor 2020 - Console version\n  by. Shark_vil\r\n");

                    Console.WriteLine("  1 - View addon list");
                    Console.WriteLine("  2 - Extract all");
                    Console.WriteLine("  3 - Exit");

                    Console.Write("> ");
                    string command = Console.ReadLine();

                    Console.Clear();

                    if (int.TryParse(command, out _))
                        switch (Convert.ToInt32(command))
                        {
                            case 1:
                                ViewAddons();
                                break;
                            case 2:
                                Extractor.ExtractAll();
                                break;
                            case 3:
                                IsExit = true;
                                break;
                        }

                    Console.Clear();
                }
            }
        }

        private static void ViewAddons()
        {
            bool IsBack = false;

            List<Extractor.ExtractData> extractDatas = Extractor.GetContentData();

            while (IsBack == false)
            {
                int addon_id = 1;
                foreach (var addon in extractDatas)
                {
                    Workshop.AddonData workshop_data = Workshop.GetAddonData(addon.AddonUid);

                    if (workshop_data.Uid == 0)
                        Console.WriteLine($"  {addon_id} - ({addon.AddonUid}) - " + addon.AddonFileName);
                    else
                        Console.WriteLine($"  {addon_id} - ({addon.AddonUid}) - " + workshop_data.Title);

                    addon_id++;
                }

                Console.WriteLine("  0 - Back");

                Console.Write("> ");
                string command = Console.ReadLine();

                if (command.Trim() == "0")
                    IsBack = true;
                else
                {
                    try
                    {
                        List<string> bad_ids = new List<string>();
                        command = command.Replace("  ", " ");
                        string[] ids = command.Trim().Split(' ');

                        foreach (string id in ids)
                        {
                            string _id = id.Trim();

                            if (bad_ids.Exists(x => x == _id))
                                continue;
                            
                            bad_ids.Add(_id);

                            Extractor.ExtractData extractData = extractDatas[Convert.ToInt32(_id) - 1];
                            Extractor.ExtractSingle(extractData.AddonUid);
                        }

                        Console.Clear();
                    }
                    catch
                    {
                        Console.WriteLine("  Invalid list id selected! Press any key to continue.");
                        Console.ReadLine();
                        Console.Clear();
                    }
                }
            }
        }

        private static void InitConfig()
        {
            Config config = new Config();

            if (!Directory.Exists("Config"))
                Directory.CreateDirectory("Config");

            string configJsonPath = CurrentDirectoryPath + @"\Config\config.json";

            if (!File.Exists(configJsonPath))
            {
                config.BinPath = @"C:\SteamLibrary\steamapps\common\GarrysMod\bin";
                config.ContentPath = @"C:\SteamLibrary\steamapps\workshop\content\4000";
                config.ExtractPath = CurrentDirectoryPath + "\\Extract";

                if (!Directory.Exists(config.ExtractPath))
                    Directory.CreateDirectory(config.ExtractPath);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configJsonPath, jsonString);
            }
            else
            {
                string fileJson = File.ReadAllText(configJsonPath);
                config = JsonConvert.DeserializeObject<Config>(fileJson);
            }

            Extractor.BinPath = config.BinPath;
            Extractor.ContentPath = config.ContentPath;
            Extractor.ExtractPath = config.ExtractPath;
            Extractor.SevenZipExePath = CurrentDirectoryPath + @"\7-ZipPortable\App\7-Zip64\7z.exe";
        }
    }
}
