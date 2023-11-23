namespace CookieCookbook.DataAccess;

public class StringsTextualRepository : StringsRepository
{
    private static readonly string Seperator = Environment.NewLine;


    protected override string StringsToText(List<string> strings)
    {
        return string.Join(Seperator, strings);
    }

    protected override List<string> TextToStrings(string fileContents)
    {
        return fileContents.Split(Seperator).ToList();
    }
}
