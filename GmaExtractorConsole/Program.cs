using GmaExtractorLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace GmaExtractorConsole
{
    class Program
    {
        private static Process GmadExe = null;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExitEvent);

            Extractor.InitConfig();
            Extractor.ParseDirectory();

            if (args.Length != 0)
            {
                foreach (string path in args)
                {
                    Console.WriteLine("Path - " + path);

                    FileAttributes attr = File.GetAttributes(path);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        string FileName = Path.GetFileName(path);
                        if (int.TryParse(FileName, out _))
                            GmadExe = Extractor.ExtractSingle(FileName);
                    }
                    else
                        GmadExe = Extractor.ExtractSingleFile(path);
                }

                Console.WriteLine("Press any button to close the application.");
                Console.ReadLine();
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

                    if (long.TryParse(command, out _))
                        switch (Convert.ToInt64(command))
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
            List<string[]> AddonWhiteList = new List<string[]>();

            while (IsBack == false)
            {
                long addonId = 1;
                foreach (var addon in extractDatas)
                {
                    Workshop.AddonData workshopData = Workshop.GetAddonData(addon.AddonUid);

                    if (workshopData.Uid == string.Empty)
                        Console.WriteLine($"  {addonId} - ({addon.AddonUid}) - " + addon.AddonFileName);
                    else
                        Console.WriteLine($"  {addonId} - ({addon.AddonUid}) - " + workshopData.Title);

                    AddonWhiteList.Add(new string[]
                    {
                        Convert.ToString(addonId), addon.AddonUid
                    });

                    addonId++;
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
                        List<string> duplicatesId = new List<string>();
                        command = command.Replace("  ", " ");
                        string[] identifiers = command.Trim().Split(' ');

                        foreach (string selectedId in identifiers)
                        {
                            string value = selectedId.Trim();

                            if (duplicatesId.Exists(x => x == value))
                                continue;

                            if (!long.TryParse(value, out _))
                            {
                                if (new Regex(@"^(http|http(s)?://)?([\w-]+\.)+[\w-]+[.com|.in|.org]+(\[\?%&=]*)?").IsMatch(value))
                                {
                                    string url = value;
                                    NameValueCollection parameters = HttpUtility.ParseQueryString(new Uri(url).Query);
                                    string parseAddonUid = parameters.Get("id");

                                    string[] GetAddongLink = AddonWhiteList.Find(x => x[1] == parseAddonUid);
                                    if (GetAddongLink != null && GetAddongLink.Length != 0)
                                        value = GetAddongLink[0];
                                    else
                                        continue;
                                }
                                else
                                {
                                    long localAddonId = 1;
                                    foreach (var addon in extractDatas)
                                    {
                                        Workshop.AddonData workshopData = Workshop.GetAddonData(addon.AddonUid);
                                        string addonName = addon.AddonFileName;

                                        if (workshopData.Uid != string.Empty)
                                            addonName = workshopData.Title;

                                        string checkValue = value.ToLower().Replace("*", @"(.*)");
                                        if (new Regex(@$"(.*){checkValue}(.*)", RegexOptions.IgnoreCase).IsMatch(addonName.ToLower()))
                                        {
                                            value = Convert.ToString(localAddonId);
                                            break;
                                        }

                                        localAddonId++;
                                    }
                                }
                            }

                            duplicatesId.Add(value);

                            Extractor.ExtractData extractData = extractDatas[Convert.ToInt32(value) - 1];
                            GmadExe = Extractor.ExtractSingle(extractData.AddonUid);
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

        private static void ProcessExitEvent(object sender, EventArgs e)
        {
            if (GmadExe != null)
                GmadExe.Close();
        }
    }
}
