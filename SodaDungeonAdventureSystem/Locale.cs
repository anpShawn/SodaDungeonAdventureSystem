

public class Locale
{
    /*
     * 
     * Normally this class would be responsible for retrieving a localized string depending on the user's language of choice
     * "formatParams" is used to inject numbers and other dynamic data into the target string
     * Here, Locale.Get() is stubbed out to simply return the same ID that was passed in
     * 
     */

    public static string Get(string id, params string[] formatParams)
    {
        string result = id;
        //curLangStrings.TryGetValue( id.ToUpper(), out result);

        if(result == null)
        {
            //throw an exception or implement a fallback
        }

        if(formatParams.Length > 0)
        {
            //result = string.Format(result, formatParams);
        }
        
        return result;
    }

}
