using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using FieldType = Microsoft.VisualBasic.FileIO.FieldType;
using System.Collections.Generic;
namespace GedToHanic;
public class Worker : BackgroundService
{
    public class PathInstance
    {
        public string? SourcePath { get; init; }
        public string? DonePath { get; init; }
        public string? ErrorPath { get; init; }
        public string? DestPath { get; init; }
        public string? DestExtension { get; init; }
    }

    public class ShapeXRef
    {
        public required string GedName { get; set; }
        public int? HanicShape { get; init; }
        public int? HanicMirrorShape { get; init; }
    }
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;
    private List<PathInstance>? _paths;
    private Dictionary<string, ShapeXRef>? _shapes;
    private Dictionary<string, int> _errorFiles = new Dictionary<string, int>();
    
    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    private void LoadShapes()
    {
        try
        {
            _shapes = new Dictionary<string, ShapeXRef>();
            var section = _config.GetSection("ShapeXRef");
            foreach (var child in section.GetChildren())
            {
                var source = child["GedName"] ?? throw new ArgumentNullException($"child[\"GedName\"]");
                var shape = child.GetValue<int?>("HanicShape");
                var mirrorShape = child.GetValue<int?>("HanicMirrorShape");
//                var kids = child.GetChildren();
//                Console.WriteLine($"key: {kids.}");
//                var val = child.Value;
                _shapes.Add(source, new ShapeXRef {GedName = source, HanicShape = shape, HanicMirrorShape = mirrorShape});
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    private void LoadPaths()
    {
        try
        {
            var section = _config.GetSection("Directories");
            _paths = new List<PathInstance>();
            foreach (var child in section.GetChildren())
            {
                var source = child["SourcePath"];
                if (null == source)
                {
                    _logger.LogError($"Missing SourcePath Value");
                    return;
                }
                var done = child["DonePath"];
                if (null == done)
                {
                    _logger.LogError($"Missing DonePath Value");
                    return;
                }
                var error = child["ErrorPath"];
                if (null == error)
                {
                    _logger.LogError($"Missing ErrorPath Value");
                    return;
                }
                var dest = child["DestPath"];
                if (null == dest)
                {
                    _logger.LogError($"Missing DestPath Value");
                    return;
                }
                var destExt = child["DestExtension"];
                if (null == destExt)
                {
                    _logger.LogError($"Missing DestExt Value");
                    return;
                }

                Directory.CreateDirectory(source);
                Directory.CreateDirectory(done);
                Directory.CreateDirectory(error);
                Directory.CreateDirectory(dest);
                _paths.Add(new PathInstance
                {
                    SourcePath = source, DonePath = done, DestPath = dest, ErrorPath = error, DestExtension = destExt
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void ProcessPath(PathInstance path, CancellationToken token)
    {
        Debug.Assert(path.SourcePath != null, "path.SourceDir != null");
        var files = Directory.GetFiles(path.SourcePath);
        foreach (var file in files)
        {
            if (token.IsCancellationRequested)
                return;
            Debug.Assert(path.DestPath != null, "path.DestDir != null");
            if (ProcessFile(file, path))
            {
                Debug.Assert(path.DonePath != null, "path.DoneDir != null");
                File.Move(file, Path.Combine(path.DonePath, Path.GetFileName(file)), overwrite: true);
            }
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        LoadShapes();
        LoadPaths();
//        ProcessFile("sample.dat", "sample.csv");
//        ProcessFile("10165004.dat", "sample.csv");
        while (!stoppingToken.IsCancellationRequested)
        {
//            _logger.LogInformation("Worker running at: {Now}", DateTimeOffset.Now);
            foreach (var path in _paths!)
            {
                ProcessPath(path, stoppingToken);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
    // public override async Task StopAsync(CancellationToken cancellationToken)
    // {
    //     // Code here runs when the service stops.
    //     // This is a good place for cleanup code.
    //     _logger.LogInformation("Service Stopping");
    //     await base.StopAsync(cancellationToken);
    // }

    private string CalcHanicShape(GedLine line) 
    {
        if (line.ShapeName == "8-STD" && line["W"] != "0")
            line.ShapeName = "15-STD";
        if (!_shapes!.TryGetValue(line.ShapeName, out var shape))
            return string.Empty;
        if (line.MirrorFlag == "T" && shape.HanicMirrorShape != null)
            return shape.HanicMirrorShape.ToString() ?? string.Empty;
        return shape.HanicShape.ToString() ?? string.Empty;
    }

    private HanicLine MapGedLine(GedLine gedLine, string? orderNumber)
    {
//        var newOrderNum = orderNumber?.Substring(0, 4) + orderNumber?.Substring(6, 4);
        var hanicLine = new HanicLine
        {
            ["B"] = orderNumber!,
            ["C"] = gedLine["A"],
            ["E"] = gedLine["Q"],
            ["F"] = gedLine["B"],
            ["G"] = gedLine["E"],
            ["H"] = gedLine["F"],
            ["J"] = gedLine["D"],
//            ["O"] = new[] { "1", "T" }.Contains(gedLine["S"]) ? "1" : ""
            ["O"] = gedLine["R"] == "1-STD" ? "" : "1"
        };
//        hanicLine["Q"] = hanicLine["O"];
        hanicLine["Q"] = "1";
        var hanicShape = CalcHanicShape(gedLine);
        hanicLine["R"] = hanicShape;
#if false
        if ("81" == hanicShape)
            hanicLine["AA"] = gedLine["W"];
        else if (new[] { "83", "84" }.Contains(hanicShape))
        {
            hanicLine["AA"] = gedLine["V"];
            hanicLine["W"] = gedLine["W"];
        }
        else if (new[] { "82", "85", "86" }.Contains(hanicShape))
        {
            hanicLine["AA"] = gedLine["V"];
        }
        else if (!string.IsNullOrEmpty(hanicShape))
            hanicLine["W"] = gedLine["V"];
#else
        switch (hanicShape)
        {
            case "81":
                hanicLine["AA"] = gedLine["W"];
                break;
            case "83":
            case "84":
                hanicLine["AA"] = gedLine["V"];
                hanicLine["W"] = gedLine["W"];
                break;
            case "82":
            case "85":
            case "86":
                hanicLine["AA"] = gedLine["V"];
                break;
            default:
                if (!string.IsNullOrEmpty(hanicShape))
                    hanicLine["W"] = gedLine["V"];
                break;
        }
#endif
        return hanicLine;
    }

    private string NextAvailableName(string filename)
    {
        var suffix = 0;
        var dir = Path.GetDirectoryName(filename);
        var baseName = Path.GetFileNameWithoutExtension(filename);
        var ext = Path.GetExtension(filename);
        
        while (true)
        {
            var tryName = Path.Combine(dir!, baseName) + (suffix == 0 ? "" : $"_{suffix}") + ext;
            if (!File.Exists(tryName))
                return tryName;
            suffix++;
        }
    }
    private void HandleErroredFile(string file, PathInstance paths)
    {
        if (_errorFiles.TryGetValue(file, out var count))
        {
            if (count > 3)
            {
                
                File.Move(file, NextAvailableName( Path.Combine(paths.ErrorPath!,
                    Path.GetFileName(file))));
                _errorFiles.Remove(file);
                _logger.LogInformation("Deleted file {File}", file);
                return;
            }
            else
            {
                _errorFiles[file] = count + 1;
            }
        }
        else
        {
            _errorFiles.Add(file, 1);
        }
        _logger.LogInformation("Skipped file {File}", file);
    }
    private bool ProcessFile(string inFileName, PathInstance paths)
    {
        var hadEof = false;
        try
        {
            using var reader = new StreamReader(inFileName);
            string orderNumber = string.Empty;
            var hanicLines = new List<HanicLine>();
            while (reader.ReadLine() is { } line)
            {
                using var parser = new TextFieldParser(new StringReader(line));
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true; // This ensures qualifiers are considered

                var gedLine = new GedLine(parser.ReadFields());
                if (!gedLine.IsValid)
                    continue;
                switch (gedLine[0])
                {
                    case "#":
                        hadEof = true;
                        continue;
                    case "*":
                        orderNumber = gedLine[1];
                        continue;
                }

                if (!int.TryParse(gedLine[0], out _))
                {
                    continue;
                }

                var hanicLine = MapGedLine(gedLine, orderNumber: Path.GetFileNameWithoutExtension(inFileName));
                hanicLines.Add(hanicLine);
            }
            reader.Dispose();

            if (!hadEof)
            {
                HandleErroredFile(inFileName, paths);
                return false;
            }
            {
                var dest = Path.Combine(paths.DestPath!, Path.GetFileNameWithoutExtension(inFileName)) + '.' + paths.DestExtension;
                var writer = new StreamWriter(dest);
                writer.WriteLine(HanicLine.FileHeader);
                foreach (var hanicLine in hanicLines)
                {
                    writer.WriteLine(hanicLine.FileLine);
                }
                writer.WriteLine(HanicLine.FileFooter);
                writer.Dispose();
                _logger.LogInformation("Processed file {File}", inFileName);
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}