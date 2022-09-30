using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Net.Sockets;

namespace Updater
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length is < 2 or 4)
            {
                Console.WriteLine(Properties.Resources.InvalidParametersErrorMessage);
                _ = Console.ReadKey(true);
            }
            else if (args.Length == 2)
            {
                if (Version.TryParse(args[1], out _))
                {
                    try
                    {
                        TcpClient Connection = new("2.235.201.25", 35000);
                        if (Connection.Connected)
                        {
                            NetworkStream Stream = Connection.GetStream();
                            using StreamReader sr = new(Stream);
                            using StreamWriter sw = new(Stream);
                            sw.WriteLine("AppVersion");
                            string NewestVersion = sr.ReadLine();
                            if (NewestVersion != args[0])
                            {
                                sw.WriteLine("Download");
                                byte[] NewVersionSizeBytes = new byte[4];
                                _ = Stream.Read(NewVersionSizeBytes, 0, 4);
                                int NewVersionSize = BitConverter.ToInt32(NewVersionSizeBytes, 0);
                                byte[] NewVersionDataBytes = new byte[NewVersionSize];
                                _ = Stream.Read(NewVersionDataBytes, 0, NewVersionSize);
                                InstallNewVersion(args[0], NewVersionDataBytes);
                            }
                            else
                            {
                                Console.WriteLine(Properties.Resources.NoNewVersionMessage);
                                _ = Console.ReadKey(true);
                            }
                        }
                        else
                        {
                            Console.WriteLine(Properties.Resources.ConnectionErrorMessage);
                            _ = Console.ReadKey(true);
                        }
                    }
                    catch (Exception ex) when (ex is SocketException or IOException)
                    {
                        Console.WriteLine(Properties.Resources.GenericErrorMessage);
                        _ = Console.ReadKey(true);
                    }
                }
                else
                {
                    Console.WriteLine(Properties.Resources.InvalidVersionErrorMessage);
                    _ = Console.ReadKey(true);
                }
            }
            else if (args.Length is 5 or 6)
            {
                AnonymousPipeClientStream OutPipe = new(PipeDirection.Out, args[2]);
                AnonymousPipeClientStream InPipe = new(PipeDirection.In, args[3]);
                using (StreamWriter PipeWriter = new(OutPipe))
                {
                    using (StreamReader PipeReader = new(InPipe))
                    {
                        try
                        {
                            if (Version.TryParse(args[1], out _))
                            {
                                using TcpClient Connection = new("2.235.201.25", 35000);
                                if (Connection.Connected)
                                {
                                    NetworkStream Stream = Connection.GetStream();
                                    using StreamReader sr = new(Stream);
                                    using StreamWriter sw = new(Stream);
                                    sw.WriteLine("AppVersion");
                                    string NewestVersion = sr.ReadLine();
                                    if (NewestVersion != args[1])
                                    {
                                        PipeWriter.WriteLine("NewVersion");
                                        string Response = PipeReader.ReadLine();
                                        while (Response is not "Download" or "Terminate")
                                        {
                                            Response = PipeReader.ReadLine();
                                        }
                                        if (Response is "Download")
                                        {
                                            Process AppProcess = Process.GetProcessById(Convert.ToInt32(args[4]));
                                            sw.WriteLine("Download");
                                            byte[] NewVersionSizeBytes = new byte[4];
                                            _ = Stream.Read(NewVersionSizeBytes, 0, 4);
                                            int NewVersionSize = BitConverter.ToInt32(NewVersionSizeBytes, 0);
                                            byte[] NewVersionDataBytes = new byte[NewVersionSize];
                                            _ = Stream.Read(NewVersionDataBytes, 0, NewVersionSize);
                                            PipeWriter.WriteLine("InstallReady");
                                            Response = PipeReader.ReadLine();
                                            if (Response is "Confirmed")
                                            {
                                                try
                                                {
                                                    AppProcess.WaitForExit();
                                                    AppProcess.Dispose();
                                                    InstallNewVersion(args[0], NewVersionDataBytes);
                                                    if (args.Length is 6 && args[5] is "restart")
                                                    {
                                                        _ = AppProcess.Start();
                                                    }
                                                }
                                                catch (Exception ex) when (ex is Win32Exception or SystemException)
                                                {
                                                    DirectoryInfo ExtractedDataDirectory = Directory.CreateDirectory(args[0] + "\\Updates\\Extraction");
                                                    using FileStream fs = new(args[0] + "\\Updates\\Archive.zip", FileMode.Create, FileAccess.Write, FileShare.None);
                                                    fs.Write(NewVersionDataBytes, 0, NewVersionDataBytes.Length);
                                                }
                                            }
                                            else
                                            {
                                                DirectoryInfo ExtractedDataDirectory = Directory.CreateDirectory(args[0] + "\\Updates\\Extraction");
                                                using FileStream fs = new(args[0] + "\\Updates\\Archive.zip", FileMode.Create, FileAccess.Write, FileShare.None);
                                                fs.Write(NewVersionDataBytes, 0, NewVersionDataBytes.Length);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        PipeWriter.WriteLine("NoNewVersion");
                                    }
                                }
                                else
                                {
                                    PipeWriter.WriteLine("ConnectionFailed");
                                }
                            }
                            else
                            {
                                PipeWriter.WriteLine("InvalidVersion");
                            }
                        }
                        catch (Exception ex) when (ex is SocketException or IOException)
                        {
                            PipeWriter.WriteLine("ConnectionError");
                        }
                        catch (FormatException)
                        {
                            PipeWriter.WriteLine("GenericError");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Installa la nuova versione.
        /// </summary>
        /// <param name="Path">Percorso di installazione.</param>
        /// <param name="NewVersionData">Array di byte che contiene i file della nuova versione.</param>
        private static void InstallNewVersion(string Path, byte[] NewVersionData)
        {
            DirectoryInfo AppFolder = new(Path);
            IEnumerable<FileInfo> Files = AppFolder.EnumerateFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in Files)
            {
                if (file.Name is not "AppLog")
                {
                    file.Delete();
                }
            }
            IEnumerable<DirectoryInfo> Directories = AppFolder.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
            foreach (DirectoryInfo directory in Directories)
            {
                if (directory.Name is not "Old logs")
                {
                    directory.Delete(true);
                }
            }
            DirectoryInfo ExtractedDataDirectory = Directory.CreateDirectory(Path + "\\Updates\\Extraction");
            using (FileStream fs = new(Path + "\\Updates\\Archive.zip", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.Write(NewVersionData, 0, NewVersionData.Length);
                _ = fs.Seek(0, SeekOrigin.Begin);
                ZipArchive Archive = new(fs, ZipArchiveMode.Read);
                FileStream EntryStream;
                foreach (ZipArchiveEntry entry in Archive.Entries)
                {
                    EntryStream = new(Path + "\\Updates\\Extraction\\" + entry.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
                    entry.Open().CopyTo(EntryStream);
                }
            }
            foreach (DirectoryInfo directory in ExtractedDataDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                directory.MoveTo(Path + "\\" + directory.Name);
            }
            foreach (FileInfo file in ExtractedDataDirectory.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                file.MoveTo(Path + "\\" + file.Name);
            }
            File.Delete(Path + "\\Updates\\Archive.zip");
        }
    }
}