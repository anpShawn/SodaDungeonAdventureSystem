using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


public static class Utils
{
    public static string PrintList<T>( List<T> inList)
    {
        return PrintArray(inList.ToArray());
    }

    public static string PrintArray<T>( T[] inArr)
    {
        StringBuilder sb = new StringBuilder();
        //sb.Append("printing array with " + inArr.Length + " elements");
        sb.AppendLine();
        for(int i=0; i<inArr.Length; i++)
        {
            sb.Append(inArr[i].ToString() + ", ");
        }

        Console.WriteLine(sb.ToString());
        return sb.ToString();
    }

    public static int GetRandomIndex<T>(ICollection<T> c)
    {
        return RandomInRange(0, c.Count);
    }

    public static bool FlipCoin()
    {
        return RandomInRange(0, 100) < 50;
    }

    public static int RandomSign()
    {
        return FlipCoin() ? 1 : -1;
    }

    public static bool OneIn(int inChance)
    {
        return RandomInRange(1, inChance) == 1;
    }

    public static bool PercentageChance(int inPercent)
    {
        return RandomInRange(1, 100) < inPercent;
    }

    public static int AddWithOverflowCheck(int a, int b)
    {
        try
        {
            checked
            {
                int result = a + b;
                return result;
            }
        }
        catch
        {
            return int.MaxValue;
        }
    }

    public static int MultiplyWithOverflowCheck(int a, int b)
    {
        try
        {
            checked
            {
                int result = a * b;
                return result;
            }
        }
        catch
        {
            return int.MaxValue;
        }
    }

    public static Vector2 GetRandomInsideOf(Vector2 inOrigin, float inXRange, float inYRange)
    {
        float xOffset = RandomInRange(-inXRange/2, inXRange/2);
        float yOffset = RandomInRange(-inYRange/2, inYRange/2);

        return inOrigin + new Vector2(xOffset, yOffset);
    }

    public static T ParseStringToFlagEnum<T>(string inFlags, char inDelimiter) where T: struct, IConvertible
    {
        int temp = 0x0;
        Type enumType = typeof(T);

        string[] flagList = inFlags.Split(inDelimiter);
        string strFlag;
        for(int j=0; j<flagList.Length; j++)
        {
            strFlag = flagList[j].Trim().ToUpper();
            if(strFlag.Length > 0)
            {
                T parsedFlag = (T)Enum.Parse(enumType, strFlag);
                temp |= Convert.ToInt32(parsedFlag);
            }
        }

        return (T)Enum.Parse(enumType, temp.ToString());
    }

    public static string CommaFormatStringNumber(string inStringNumber)
    {
        if(inStringNumber.Length > 3)
        {
            int precedingDigits = inStringNumber.Length % 3;
            int commaPos = precedingDigits == 0 ? 2 : (precedingDigits - 1);
            StringBuilder output = new StringBuilder();

            for(int i=0; i<inStringNumber.Length; i++)
            {
                output.Append(inStringNumber[i]);

                if (i == commaPos && i != (inStringNumber.Length-1))
                {
                    output.Append(",");
                    commaPos += 3;
                }    
            }

            return output.ToString();
        }
        else
        {
            return inStringNumber;
        }
    }

    private static StringBuilder builder = new StringBuilder();
    private static string notationSymbols = "KMBTqQsSONd";
    public static string FormatBigInt(long inBig, bool inAllowFontDownsize=true, int inDownsizeAmt = 5)
    {
        //under 1000, return verbatim
        if (inBig < 1000) return inBig.ToString();

        //build a string that consists of the number's first x digits with a decimal point inserted
        builder.Length = 0;
        string strBig = inBig.ToString();
        int numDigits = strBig.Length;
        int divisionsBy3 = numDigits / 3;
        int modulusBy3 = numDigits % 3;

        int digitsToCapture = modulusBy3 + 1;
        if (modulusBy3 == 0) digitsToCapture = 4;
        string temp = strBig.Substring(0, digitsToCapture);
        temp = temp.Insert(temp.Length-1, ".");

        builder.Append(temp);
        bool lastDigitIsZero = temp[temp.Length-1] == '0';

        //over 1 decillion? return as scientific notation
        if (numDigits > 36)
        {
            builder.Append(" ");
            if(inAllowFontDownsize)builder.Append($"<size=-{inDownsizeAmt}>");
            builder.Append("e");
            builder.Append(numDigits - 1);
            if(inAllowFontDownsize)builder.Append("</size>");
        }
        else
        {
            //if the last digit is a 0, remove it AND the preceding decimal point
            if(lastDigitIsZero)
            {
                builder.Remove(builder.Length-2, 2);
            }

            //then append a custom symbol
            int notationIndex = modulusBy3 == 0 ? divisionsBy3 - 1 : divisionsBy3;
            notationIndex--; //decrement to base it on a zero-indexed array of characters
            builder.Append(" ");
            if(inAllowFontDownsize)builder.Append($"<size=-{inDownsizeAmt}>");
            builder.Append( notationSymbols[notationIndex] );
            if(inAllowFontDownsize)builder.Append("</size>");
        }

        return builder.ToString();

        /*Examples
         * 
         * 5
         * 10
         * 250
         * 1k               4 digits (3 places to move)
         * 1.3k
         * 10k
         * 100k
         * 1M               7 digits (6 places to move)
         * 1.6M
         * 20M
         * 500M
         * 1B
         * 10B
         * 345B
         * 1T
         * 
         * K, M, B, T
         * q quadrillion
         * Q quintillion
         * s Sextillion
         * S Septillion
         * O Octillion
         * N Nonillion
         * d Decillion       34 digits (33 places to move)
         * 
         * 
         * 1 eXX
         * */
    }

    public static int MinutesElapsedSince(DateTime inTime)
    {
        return DateTime.Now.Subtract(inTime).TotalMinutes.AsInt();
    }



    //extension methods
    public static void CopyTo(this Stream source, Stream destination, int bufferSize = 81920)
    {
        byte[] array = new byte[bufferSize];
        int count;
        while ((count = source.Read(array, 0, array.Length)) != 0)
        {
           destination.Write(array, 0, count);
        }
    }

    public static string GetName(this ItemType source)
    {
        return Locale.Get("ITEM_TYPE_" + source.ToString());
    }

    public static T GetRandomElement<T>(this IList<T> source)
    {
        return source[GetRandomIndex(source)];
    }

    public static void Shuffle<T> (this T[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public static void Shuffle<T> (this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
    }

    /*
    public static int AsInt(this BigInteger source)
    {
        if (source < int.MaxValue) return int.Parse(source.ToString());
        else return int.MaxValue;
    }

    public static ulong AsUlong(this BigInteger source)
    {
        if (source < 0) return 0;
        else if (source < ulong.MaxValue) return ulong.Parse(source.ToString());
        else return ulong.MaxValue;
    }*/

    public static int AsInt(this double source)
    {
        if (source < int.MaxValue) return (int)source;
        else return int.MaxValue;
    }

    public static int AsInt(this long source)
    {
        if (source < int.MaxValue) return (int)source;
        else return int.MaxValue;
    }

    private static ItemType slotTypes = ItemType.WEAPON | ItemType.SHIELD | ItemType.ARMOR;
    public static bool CanHaveGemSlots(this ItemType inType)
    {
        return  (slotTypes & inType) == inType;
    }

    public static string GetLocalizedName(this AdventureArea source)
    {
        return Locale.Get("AREA_" + source.ToString() + "_NAME");
    }

    public static string CapitalizeFirstLetter(this string source)
    {
        return source[0].ToString().ToUpper() + source.Substring(1);
    }


    //UNITY REPLACEMENT METHODS
    private static Random random;

    //replacement for Mathf.CeilToInt();
    public static int CeilToInt(float inValue)
    {
        return (int)MathF.Ceiling(inValue);
    }

    //replacement for Mathf.FloorToInt();
    public static int FloorToInt(float inValue)
    {
        return (int)MathF.Floor(inValue);
    }

    //replacement for Mathf.Floor()
    public static float Floor(float inValue)
    {
        return MathF.Floor(inValue);
    }

    //replacement for INTEGER UnityEngine.Random.Range(min[inclusive], max[exclusive]);
    public static int RandomInRange(int inMin, int inMax)
    {
        if (random == null) random = new Random();
        return random.Next(inMin, inMax);
    }

    //replacement for FLOAT UnityEngine.Random.Range(min[inclusive], max[inclusive]);
    public static float RandomInRange(float inMin, float inMax)
    {
        if (random == null) random = new Random();
        double result = inMin + random.NextDouble() * (inMax-inMin);
        return (float)result;
    }

}
