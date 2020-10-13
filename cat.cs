using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

class Program {
    private static void ERROR (string err) {
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write ("ERROR:");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write (err + "\n");
    }
    public static void Main (string[] args) {
        bool isNumber = false;
        try {
            int vo = 0;
            vo = int.Parse (args[1]);
            isNumber = true;
        } catch {
            isNumber = false;
        }
        if (args.Length > 0) {
            if (File.Exists (args[0])) {
                if (args.Length >= 2) {
                    if (args[1] == "-open" || args[1] == "-o" || isNumber) {
                        if (args.Length >= 3) {
                            bool isNumber_2 = false;
                            try {
                                int vo = 0;
                                vo = int.Parse (args[2]);
                                isNumber_2 = true;
                            } catch {
                                isNumber_2 = false;
                            }
                            if (args[1] == "-open" || args[1] == "-o") {
                                FileManager fm = new FileManager (args[0], true, args[2]);
                            } else if (isNumber_2) {
                                FileManager fm = new FileManager (args[0], false, args[2]);
                            }
                        } else {
                            if (args[1] == "-open" || args[1] == "-o") {
                                FileManager fm = new FileManager (args[0], true, args[1]);
                            } else if (isNumber) {
                                FileManager fm = new FileManager (args[0], false, args[1]);
                            }
                        }
                    } else {
                        ERROR (" No found flag \"" + args[1] + "\"\n");
                        Console.WriteLine ("cat <filepath> [-o|-open]");
                    }
                } else {
                    FileManager fm = new FileManager (args[0], false);
                }
            } else {
                if (Path.GetDirectoryName (args[0]) != null && Path.GetDirectoryName (args[0]) != String.Empty) {
                    ERROR (" File not found in \"" + Path.GetDirectoryName (args[0]) + "\"\n");
                } else {
                    ERROR (" File not found in \"" + args[0] + "\"\n");
                }
            }
        } else {
            ERROR (" Not input file !\n");
            Console.WriteLine ("cat <filepath> [-o|-open]");
        }
    }
}

class FileManager {
    [DllImport ("Shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern uint AssocQueryString (
        AssocF flags,
        AssocStr str,
        string pszAssoc,
        string pszExtra, [Out] StringBuilder pszOut,
        ref uint pcchOut);

    [Flags]
    private enum AssocF {
        None = 0,
        Init_NoRemapCLSID = 0x1,
        Init_ByExeName = 0x2,
        Open_ByExeName = 0x2,
        Init_DefaultToStar = 0x4,
        Init_DefaultToFolder = 0x8,
        NoUserSettings = 0x10,
        NoTruncate = 0x20,
        Verify = 0x40,
        RemapRunDll = 0x80,
        NoFixUps = 0x100,
        IgnoreBaseClass = 0x200,
        Init_IgnoreUnknown = 0x400,
        Init_Fixed_ProgId = 0x800,
        Is_Protocol = 0x1000,
        Init_For_File = 0x2000
    }

    public enum AssocStr {
        Command = 1,
        Executable,
        FriendlyDocName,
        FriendlyAppName,
        NoOpen,
        ShellNewValue,
        DDECommand,
        DDEIfExec,
        DDEApplication,
        DDETopic,
        InfoTip,
        QuickTip,
        TileInfo,
        ContentType,
        DefaultIcon,
        ShellExtension,
        DropTarget,
        DelegateExecute,
        Supported_Uri_Protocols,
        ProgID,
        AppID,
        AppPublisher,
        AppIconReference,
        Max
    }
    private string AssocQueryString (AssocStr association, string extension) {
        const int S_OK = 0;
        const int S_FALSE = 1;

        uint length = 0;
        uint ret = AssocQueryString (AssocF.None, association, extension, null, null, ref length);
        if (ret != S_FALSE) {
            throw new InvalidOperationException ("Could not determine associated string");
        }

        var sb = new StringBuilder ((int) length);
        ret = AssocQueryString (AssocF.None, association, extension, null, sb, ref length);
        if (ret != S_OK) {
            throw new InvalidOperationException ("Could not determine associated string");
        }

        return sb.ToString ();
    }

    private string GetProgamDefault (string path) {
        try {
            Process process = new Process ();
            ProcessStartInfo startInfo = new ProcessStartInfo ();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = path;
            process.StartInfo = startInfo;
            process.Start ();
            return process.ProcessName;
        } catch { }
        return String.Empty;
    }
    private int Width = 0;
    private int Height = 0;
    private void FileInfoGesture (string path, int lines, bool isImage = false) {
        //Console.WriteLine("IdentityReference: "+ File.GetAccessControl(path).GetAccessRules(true,true,typeof(System.Security.Principal.NTAccount))[0].IdentityReference);
        //Console.WriteLine("Default Program: " + GetProgamDefault(path));
        Console.WriteLine (String.Empty);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        FileInfo finfo = new FileInfo (path);
        Console.WriteLine (String.Empty);
        Console.WriteLine ("FileName: " + finfo.Name);
        Console.WriteLine ("DirectoryName: " + finfo.DirectoryName);
        Console.WriteLine ("Extension: " + finfo.Extension);
        Console.WriteLine ("Size: " + finfo.Length + "o");
        if (isImage) {
            Console.WriteLine ("Image Width: " + Width + "px");
            Console.WriteLine ("Image Height: " + Height + "px");
        } else {
            Console.WriteLine ("Lines Count: " + lines);
        }
        try {
            Console.WriteLine ("Created At: " + File.GetCreationTime (path));
            Console.WriteLine ("Last Access At: " + File.GetLastAccessTime (path));
            Console.WriteLine ("Last Edit At: " + File.GetLastWriteTime (path));
            Console.WriteLine ("Attributes: " + File.GetAttributes (path));
            Console.WriteLine ("Author: " + File.GetAccessControl (path).GetOwner (typeof (System.Security.Principal.NTAccount)));
            Console.WriteLine ("Content Type: " + AssocQueryString (AssocStr.ContentType, Path.GetExtension (path)));
            Console.WriteLine ("Default Program Path: " + AssocQueryString (AssocStr.Executable, Path.GetExtension (path)));
            Console.WriteLine ("Default Program Name: " + Path.GetFileName (AssocQueryString (AssocStr.Executable, Path.GetExtension (path))));
        } catch { }

        try {
            string fiversion = FileVersionInfo.GetVersionInfo (Path.Combine (finfo.DirectoryName, finfo.Name)).ToString ();
            string[] fiv = fiversion.Split (new [] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string linv in fiv) {
                if (linv.Split (':').Length == 2 && linv.Split (':') [1].Trim () != String.Empty) {
                    Console.WriteLine (linv.Split (':') [0].Trim () + ": " + linv.Split (':') [1].Trim ());
                }
            }
        } catch {
            Console.WriteLine ("Fail to get file version inforamtion !");
        }

        Console.WriteLine (String.Empty);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private void TxtFile (string path) {
        int i = 0;
        foreach (string line in File.ReadAllLines (path)) {
            i++;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine ("[" + i + "] " + line);
            Console.ForegroundColor = ConsoleColor.White;
        }
        FileInfoGesture (path, i);
    }
    private void ProgramFile (string path) {
        int i = 0;
        foreach (string line in File.ReadAllLines (path)) {
            i++;
        }
        FileInfoGesture (path, i);
    }
    private void ImageRender (string path) {
        Bitmap image = new Bitmap (path);
        Width = image.Width;
        Height = image.Height;
        int h = IMGS;
        int w = (image.Height * h) / image.Width;
        image = new Bitmap (image, new Size (w, h));
        int i = 0;
        int x, y;

        for (x = 0; x < w; x++) {
            Console.WriteLine ("");
            for (y = 0; y < h; y++) {
                Color c = image.GetPixel (x, y);
                // int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
                // index |= (c.R > 64) ? 4 : 0;
                // index |= (c.G > 64) ? 2 : 0;
                // index |= (c.B > 64) ? 1 : 0;
                // Console.ForegroundColor = (System.ConsoleColor) index;
                Console.ForegroundColor = RGBToConsoleColor (c);
                Console.Write ("██");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        FileInfoGesture (path, i, true);
    }
    public ConsoleColor RGBToConsoleColor (Color color) {
        if (color.GetSaturation () < 0.5)
            switch ((int) (color.GetBrightness () * 3.5)) {
                case 0:
                    return ConsoleColor.Black;
                case 1:
                    return ConsoleColor.DarkGray;
                case 2:
                    return ConsoleColor.Gray;
                default:
                    return ConsoleColor.White;
            }
        var hue = (int) Math.Round (color.GetHue () / 60, MidpointRounding.AwayFromZero);
        if (color.GetBrightness () < 0.4)
            switch (hue) {
                case 1:
                    return ConsoleColor.DarkYellow;
                case 2:
                    return ConsoleColor.DarkGreen;
                case 3:
                    return ConsoleColor.DarkCyan;
                case 4:
                    return ConsoleColor.DarkBlue;
                case 5:
                    return ConsoleColor.DarkMagenta;
                default:
                    return ConsoleColor.DarkRed;
            }
        switch (hue) {
            case 1:
                return ConsoleColor.Yellow;
            case 2:
                return ConsoleColor.Green;
            case 3:
                return ConsoleColor.Cyan;
            case 4:
                return ConsoleColor.Blue;
            case 5:
                return ConsoleColor.Magenta;
            default:
                return ConsoleColor.Red;
        }
    }
    private int IMGS = 70;

    public FileManager (string path, bool open, string size = "70") {
        try {
            IMGS = int.Parse (size);
            if (IMGS <= 0) {
                IMGS = 70;
            }

        } catch {
            IMGS = 70;
        }
        if (File.Exists (path)) {
            switch (Path.GetExtension (path)) {
                case ".exe":
                    ProgramFile (path);
                    break;
                case ".jar":
                    ProgramFile (path);
                    break;
                case ".jpg":
                    ImageRender (path);
                    break;
                case ".png":
                    ImageRender (path);
                    break;
                default:
                    TxtFile (path);
                    break;
            }
            if (open) {
                try {
                    Process.Start (path);
                } catch { }
            }
        }
    }
}