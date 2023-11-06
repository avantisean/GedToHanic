namespace GedToHanic;

public class GedLine
{
    // public static GedLine MakeGedLine(string[]? fields)
    // {
    //     if (null == fields || fields.Length < 2)
    //     {
    //         return null;
    //     }
    //     if ()
    // }
    public GedLine(string[]? fields)
    {
        Fields = fields;
    }

    public bool IsValid => Fields != null && Fields.Length > 0;

    public string ShapeName
    {
        get => this["R"];
        set => this["R"] = value;
    }

    public string MirrorFlag
    {
        get => this["S"];
        set => this["S"] = value;
    }

    private string[]? Fields { get; set; }

    public string? GetColumnValue(int col)
    {
        if (null == Fields || col >= Fields.Length)
            return null;
        return Fields[col];
    }
    public string? GetColumnValue(string col)
    {
        return Fields.GetExcelColumn(col);
    }
    public string this[string index]
    {
        get
        {
            var colNum = Utility.GetExcelColumnNum(index);
            return colNum >= Fields!.Length ? string.Empty : Fields[(int)colNum!];
        }
        set
        {
            var colNum = Utility.GetExcelColumnNum(index);
            if (null == colNum)
                return;
            if (colNum >= Fields!.Length)
                return;
            Fields[(int)colNum] = value;
        }
    }
    public string this[int index]
    {
        get => index >= Fields!.Length ? string.Empty : Fields[index];
        set
        {
            if (index >= Fields!.Length)
                return;
            Fields[index] = value;
        }
    }
}

/*
public class GedHeaderLine : GedLine
{
    public GedHeaderLine(string[]? fields) : base(fields)
    {
    }
}

public class GedDataLine : GedLine
{
    public GedDataLine(string[]? fields) : base(fields)
    {
    }
}

public class GedVorHLine : GedLine
{
    public GedVorHLine(string[]? fields) : base(fields)
    {
    }
}
*/
