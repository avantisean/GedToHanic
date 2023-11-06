namespace GedToHanic;

public static class Utility
{
    public static string? GetExcelColumn(this string[]? cols, string excelName)
    {
        if (null == cols)
            return null;
        var col = GetExcelColumnNum(excelName);
        if (null == col || col >= cols.Length)
            return null;
        return cols[(int)col];
    }

    public static int? GetExcelColumnNum(string excelName)
    {
        excelName = excelName.ToUpper();
        var col = 0;
        if (string.IsNullOrEmpty(excelName))
            return null;
        for (var i = 0; i < excelName.Length; ++i)
        {
            if (excelName[i] < 'A' || excelName[i] > 'Z')
                return null;
            if (i > 0)
                col = (col + 1) * 26;
            col += (excelName[i] - 'A');
        }

        return col;
    }
}