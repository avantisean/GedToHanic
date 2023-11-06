using System.Text;

namespace GedToHanic;

public class HanicLine
{
    public HanicLine()
    {
        Fields[0] = "2000";
    }
    private string[] Fields { get; set; } = new string[(int)Utility.GetExcelColumnNum("AH")!];
    public static string FileHeader => "FILEVERSION100";
    public static string FileFooter => "END100";

    public string this[string index]
    {
        get
        {
            var colNum = Utility.GetExcelColumnNum(index);
            return colNum >= Fields.Length ? string.Empty : Fields[(int)colNum!];
        }
        set
        {
            var colNum = Utility.GetExcelColumnNum(index);
            if (null == colNum)
                return;
            if (colNum >= Fields.Length)
                return;
            Fields[(int)colNum] = value;
        }
    }
    // public void SetValue(string columnName, string value)
    // {
    //     var colNum = Utility.GetExcelColumnNum(columnName);
    //     if (null == colNum)
    //         return;
    //     if (colNum >= Fields.Length)
    //         return;
    //     Fields[(int)colNum] = value;
    // }
    // public void SetValue(int colNum, string value)
    // {
    //     if (colNum >= Fields.Length)
    //         return;
    //     Fields[(int)colNum] = value;
    // }
    public string FileLine
    {
        get
        {
            var prefix = "";
            var result = new StringBuilder();
            foreach (var field in Fields)
            {
                result.AppendFormat($"{prefix}{field}");
                prefix = ";";
            }
            return result.ToString();
        }
    }
}
