using System.IO.Compression;
using System.Text.RegularExpressions;

Console.WriteLine("Comic Book File Manager - start");

string cbzPath = "";
List<string> argsPaths = new List<string>();
List<string> argsFiles = new List<string>();
List<string> images = new List<string>();

bool flagCloseWindow = false;
bool flagAutoIncrement = false;

try
{
    // parse args
    if (ParseArgs())
    {

        // get images list
        foreach (var name in argsPaths)
        {
            PreorderTraversal(name);
        }
        images.AddRange(argsFiles);

        // create cbz
        CreateCBZ(cbzPath);

        Console.WriteLine("Comic Book File Manager - end");

    }
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}

if (!flagCloseWindow)
    Console.ReadKey(false);



/// <summary>Usage instrunctions</summary>
void Usage()
{
    var exeName = System.AppDomain.CurrentDomain.FriendlyName;
    Console.WriteLine("Creates a .cbz without folders of all files passed.");
    Console.WriteLine($"Usage: {exeName}.exe [/E] [/I] names");
    Console.WriteLine("  names   Specifies a list of one or more files or directories.");
}

/// <summary>Parse arguments. Ordered by natural number, get folders list and files list</summary>
bool ParseArgs()
{

    // parse parameters
    foreach (var arg in OrderByNaturalNumber(args))
    {
        // option
        if (arg.ToLower() == "/e")
            flagCloseWindow = true;
        else if (arg.ToLower() == "/i")
            flagAutoIncrement = true;
        // name
        else if (Directory.Exists(arg)) argsPaths.Add(arg);
        else if (File.Exists(arg)) argsFiles.Add(arg);
        // invalid 
        else
        {
            Usage();
            return false;
        }
    }

    // no names
    var names = argsPaths.Union(argsFiles);
    if (!names.Any())
    {
        Usage();
        return false;
    }

    // cbz path 
    var parentFolder = Directory.GetParent(names.FirstOrDefault());
    if (argsPaths.Count == 1)
        cbzPath = Path.Combine(parentFolder.FullName, Path.GetFileName(argsPaths[0]));
    else 
        cbzPath = Path.Combine(parentFolder.FullName, parentFolder.Name);

    if (flagAutoIncrement)
    {
        var files = Directory.GetFiles(Path.GetDirectoryName(cbzPath), Path.GetFileName(cbzPath) + "*.cbz");
        string pattern = $@"{Regex.Escape(cbzPath)} (\d+)\.cbz";
        var suffixes = files.Where(x => Regex.IsMatch(x, pattern)).Select(x => Convert.ToInt16(Regex.Replace(x, pattern, "$1")));
        var suffix = 1;
        if (suffixes.Any())
            suffix = 1+ suffixes.Max(x => x);
        cbzPath += $" {suffix:D3}";
    }

    cbzPath += ".cbz";

    //Console.WriteLine(cbzPath);
    //return false;

    return true;
}

/// <summary>Get subdirectories files first.</summary>
void PreorderTraversal(string path)
{
    var folders = OrderByNaturalNumber(Directory.GetDirectories(path));
    foreach (var folder in folders)
    {
        PreorderTraversal(folder);
    }
    var files = OrderByNaturalNumber(Directory.GetFiles(path));
    foreach (var file in files)
    {
        images.Add(file);
    }
}

/// <summary>Print all images found, with sequential padded numbering.</summary>
void PrintImages()
{
    var cnt = 0;
    int padWidth = images.Count.ToString().Length;
    foreach (var image in images)
    {
        Console.WriteLine($"{(++cnt).ToString().PadLeft(padWidth, '0')} - {image}");
    }
}

/// <summary>Create CBZ, with sequential padded numbering.</summary>
void CreateCBZ(string path)
{
    var cnt = 0;
    int padWidth = images.Count.ToString().Length;

    if (File.Exists(path))
        File.Delete(path);

    using var cbz = ZipFile.Open(path, ZipArchiveMode.Create);
    foreach (var image in images)
    {
        var entryName = $"{(++cnt).ToString().PadLeft(padWidth, '0')}.{Path.GetExtension(image)}";
        var entry = cbz.CreateEntryFromFile(image, entryName, CompressionLevel.Fastest);
    }
}

/// <summary>Order by natural number. ie: "9.jpg" comes before "10.jpg"</summary>
IOrderedEnumerable<string> OrderByNaturalNumber(string[] list)
{
    return list.OrderBy(x => Regex.Replace(x, @"\d+", x => x.Value.PadLeft(9, '0')), StringComparer.OrdinalIgnoreCase);
}

