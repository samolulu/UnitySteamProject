using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


/// <summary>
/// 字符串拓展
/// </summary>
public static partial class Extensions 
{
	public static System.Data.DataTable DataTable = new();
 

    /// <summary>
    /// 匹配中文  Chinese
    /// </summary>
    static string patternChinese = @"[\u4e00-\u9fa5]+";
    /// <summary>
    /// 匹配字母
    /// </summary>
    static string patternZiMu = @"[a-zA-Z]+";//\w  https://www.runoob.com/csharp/csharp-regular-expressions.html
    /// <summary>
    /// 匹配运算符号 加减乘除
    /// </summary>
    static string patternYunSuanFuHao = @"[+\-*/]";//匹配 + - * /  匹配减号- 需要加转移符
    /// <summary>
    /// 匹配数字
    /// </summary>
    static string patternNumber = @"\d+";

    /// <summary>
    /// 精准匹配英文 要求前后没有英文字母
    /// </summary>
    public static string ReplacePatternStr = "(?<![a-zA-Z]){0}(?![a-zA-Z])";


	#region 
	/*
正则表达式中的元字符
编号	字符	描述
1	.	匹配除换行符以外的所有字符
2	\w	匹配字母、数字、下画线
3	\s	匹配空白符（空格）
4	\d	匹配数字
5	\b	匹配表达式的开始或结束
6	^	匹配表达式的开始
7	$	匹配表达式的结束
     */

	/*
     正则表达式中表示重复的字符
编 号	字 符	描 述
1	*	0次或多次字符
2	?	0次或1次字符
3	+	1次或多次字符
4	{n}	n次字符
5	{n,M} 	n到M次字符
6	{n, }	n次以上字符
     * */

	/*
     在 Regex 类中还提供了很多方法来操作正则表达式
字符	描述
\	转义字符，将一个具有特殊功能的字符转义为一个普通字符，或反过来
(pattern)	匹配 pattern 并获取这一匹配
(?:pattern)	匹配 pattern 但不获取匹配结果
(?=pattern) 	正向预查，在任何匹配 pattern 的字符串开始处匹配查找字符串
(?!pattern)	负向预查，在任何不匹配 pattern 的字符串开始处匹配查找字符串
x|y	匹配x或y。例如，‘z|food’能匹配“z”或“food”。‘(z|f)ood’则匹配“zood”或“food”
[xyz]	字符集合。匹配所包含的任意一个字符。例如，‘[abc]’可以匹配“plain”中的‘a’
[^xyz] 	负值字符集合。匹配未包含的任意字符。例如，‘[^abc]’可以匹配“plain”中的‘p’
[a-z]	匹配指定范围内的任意字符。例如，‘[a-z]’可以匹配'a'到'z'范围内的任意小写字母字符
[^a-z]	匹配不在指定范围内的任意字符。例如，‘[^a-z]’可以匹配不在‘a’～‘z’'内的任意字符
\B	匹配非单词边界
\D	匹配一个非数字字符，等价于 [^0-9]
\f 	匹配一个换页符
\n	匹配一个换行符
\r	匹配一个回车符
\S	匹配任何非空白字符
\t	匹配一个制表符
\v	匹配一个垂直制表符。等价于 \x0b 和 \cK
\W	匹配任何非单词字符。等价于‘[^A-Za-z0-9_]’
     */
	#endregion

    public static string Compress(this string rawString)
    {
        byte[] strBytes = Encoding.Unicode.GetBytes(rawString);
        byte[] strBytesCompressed = Deflate.CompressBytes(strBytes);
        
        return Convert.ToBase64String(strBytesCompressed);
    }
    
    public static string DeCompress(this string zipString)
    {
        byte[] strBytesCompressed = Convert.FromBase64String(zipString);
        byte[] strBytes = Deflate.DecompressBytes(strBytesCompressed);
        
        return Encoding.Unicode.GetString(strBytes);
    }

 

	/// <summary>
	/// 获取文本串的md5
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
    public static string MD5(this string s)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] t = md5.ComputeHash(Encoding.GetEncoding("utf-8").GetBytes(s));
        StringBuilder sb = new StringBuilder(32);
        for (int i = 0; i < t.Length; i++)
            sb.Append(t[i].ToString("x").PadLeft(2, '0'));
        return sb.ToString();
    }
	
	/// <summary>
	/// 是否是全中文
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public static bool IsChinese(this string text)
	{

		if (string.IsNullOrEmpty(text))
			return false;

		string pattern = "^[\u4e00-\u9fa5]+$";
		return System.Text.RegularExpressions.Regex.IsMatch(text, pattern);
	}


    /// <summary>
    /// 是否包含中文
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    /// 
    public static bool ContainsChinese(this string text)
    {

        if (string.IsNullOrEmpty(text))
            return false;
        string pattern = "[\u4e00-\u9fa5]+";
        return System.Text.RegularExpressions.Regex.IsMatch(text, pattern);
    }


    /// <summary>
    /// 是否空或者“0”
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool IsNullOrZero(this string text)
    {
        if(text == null || text=="0" || text == "")
        {
            return true;
        }
        return false;
    }

	    
    /// <summary>
    /// 是否是空/空格或者为0
    /// </summary>
    /// <param name="CompareStr"></param>
    /// <param name="strList"></param>
    /// <returns></returns>
    public static bool IsNoNullOrEmptyOrZero(this string str)
    {
        return !string.IsNullOrEmpty(str) && (str != "0");
    }
	
    /// <summary>
	/// 根据字符串计算结果
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static object? ComputeString(this string str)
	{
		//var a = DataTable.Compute("1*2*3", "");
		object? result;
		try
		{
			result = DataTable.Compute(str, null);
		}
		catch (Exception )
		{
			result = null;
		}
		finally
		{

		}

		return result;
	}

    public static int ComputeInt(this string str)
    {
        var result = str.ComputeString();
        if (result == null) return 0;
        if (result is int) return (int)result;

        return result.ConvertTo<int>();
    }

    public static float ComputeFloat(this string str)
    {
        var result = str.ComputeString();
        if (result == null) return 0f;
        if (result is float) return (float)result;

        return result.ConvertTo<float>();
    }

    public static bool ComputeBool(this string str)
    {
        var result = str.ComputeString();
        if (result == null) return false;
        if (result is bool) return (bool)result;

        return result.ConvertTo<bool>();
    }
    
    public static double ComputeDouble(this string str)
    {
        var result = str.ComputeString();
        if (result == null) return 0;
        if (result is double) return (double)result;

        return result.ConvertTo<double>();
    }
 



    /// <summary>
    /// 匹配字母
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? RegexMatchZiMu(this string value)
    {
        return RegexMatch(value, patternZiMu);
    }


    /// <summary>
    /// 匹配中文
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? RegexMatchChinese(this string value)
    {
        return RegexMatch(value, patternChinese);
    }

    

    /// <summary>
    /// 匹配字母
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? RegexMatchYunSuanFuHao(this string value)
    {
        return RegexMatch(value, patternYunSuanFuHao);
    }


    /// <summary>
    /// 匹配所有符合条件的子字符串
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static MatchCollection RegexMatchs(this string value, string pattern)
    {

        MatchCollection matchs = Regex.Matches(value, pattern, RegexOptions.IgnoreCase);
        if (matchs == null || matchs.Count == 0)
        {
            return null;
        }

        return matchs;
    }



    /// <summary>
    /// 匹配字母
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static MatchCollection RegexMatchsZiMu(this string value)
    {

        return value.RegexMatchs(patternZiMu);
    }


    /// <summary>
    /// 匹配中文
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static MatchCollection RegexMatchsChinese(this string value)
    {

        return value.RegexMatchs(patternChinese);
    }

    /// <summary>
    /// 匹配中文与字母
    /// </summary>
    public static MatchCollection RegexMatchChineseAndLetter(this string value)
    {
        return value.RegexMatchs($"{patternChinese}|{patternZiMu}");
    }
    
    /// <summary>
    /// 匹配数字
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static MatchCollection RegexMatchsNumber(this string value)
    {

        return value.RegexMatchs(patternNumber);
    }


    /// <summary>
    /// 匹配运算符号
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static MatchCollection RegexMatchsYunSuanFuHao(this string value)
    {

        return value.RegexMatchs(patternYunSuanFuHao);
    }



    /// <summary>
    /// 匹配并返回第一个满足正则表达式的字符串
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? RegexMatch(this string value, string pattern)
    {
        if (value == null || pattern == null)
        {
            return null;
        }
        value = value.Trim();
        Match match = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
        if (match == null)
        {
            return null;
        }
        return match.Value;
    }

    public static void TestMatch()
    {
        Debug.LogErrorFormat("TestMatch Start");

        // 三目表达式
        float intNum = ComputeFloat("iif(50=50,50,100)");
        Debug.LogErrorFormat("-2 ComputeInt {0}", intNum);

        //嵌套iif 三目表达式 2个三目表达式
        intNum = ComputeFloat("iif(1000>=5,1000,iif(100>100,4001,20))");
        Debug.LogErrorFormat("-1 ComputeInt {0}", intNum);

        intNum = ComputeFloat("(10*2)*0.1+2+(2*2.1)");
        Debug.LogErrorFormat("0 ComputeInt {0}", intNum);


        intNum = ComputeFloat("2+0.01");
        Debug.LogErrorFormat("2 ComputeInt {0}", intNum);

        intNum = ComputeFloat("2+14/2");
        Debug.LogErrorFormat("1 ComputeInt {0}", intNum);

        float num = ComputeFloat("2+1/100");
        Debug.LogErrorFormat("3 ComputeFloat {0}", num);
        //hurt,force*2+14;
        string textValue = "hurt,force*2+14;".ReplaceChineseStr();
        string pattern = @"[/+/-/*//]";
        pattern = @"[+\-*/]";//匹配 + - * /  匹配减号- 需要加转移符
        string pattern2 = @"[a-zA-Z]+";
        string[]? strArray = RegexMatchsStr(textValue, pattern);
        if (strArray != null)
        {
            for (int i = 0; i < strArray.Length; ++i)
            {
                Debug.LogErrorFormat("11 match {0}", strArray[i]);
            }

        }


        strArray = RegexMatchsStr(textValue, pattern2);
        if (strArray != null)
        {
            for (int i = 0; i < strArray.Length; ++i)
            {
                Debug.LogErrorFormat("22 match {0}", strArray[i]);
            }

        }

        Debug.LogErrorFormat("TestMatch End");
    }
  
    /// <summary>
	/// 匹配并返回所有满足正则表达式的字符串子串
	/// </summary>
	/// <param name="pattern"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static string[]? RegexMatchsStr(this string value, string pattern)
	{
		if (value == null || pattern == null)
		{
			return null;
		}
		value = value.Trim();
		MatchCollection matchs = Regex.Matches(value, pattern, RegexOptions.IgnoreCase);
		if (matchs == null || matchs.Count == 0)
		{
			return null;
		}

		string[] strArray = new string[matchs.Count];
		for (int i = 0; i < matchs.Count; ++i)
		{
			strArray[i] = matchs[i].Value;
		}

		return strArray;
	}


    /// <summary>
    /// string 转 int   如果失败就返回0
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int ToInt(this string str)
    {
        //int value = int.MinValue;
        int value = 0;
        int.TryParse(str, out value);
        return value;
    }


    /// <summary>
    /// 精准替换字符串,要求其前后没有英文字母
    /// </summary>
    public static string ReplaceTrueStr(this string str, string oldStr, string newStr)
    {
        if (str == null) return string.Empty;

        oldStr = string.Format(ReplacePatternStr, oldStr);
        return str.ReplaceRegex(oldStr, newStr);
    }


    /// <summary>
    /// 测试精准匹配level
    /// </summary>
    public static void TestStringReplaceRegex()
    {

        string tempStr = "levellevel00level00fflevel00";

        // System.Text.RegularExpressions.MatchCollection matchZiMuLevel = tempStr.RegexMatchs(@"\Blevel\B");//匹配等级

        //前面非字母 中间是level
        //System.Text.RegularExpressions.MatchCollection matchZiMuLevel = tempStr.RegexMatchs(@"(?<![\W])level(?![\W])");//匹配等级
        System.Text.RegularExpressions.MatchCollection matchZiMuLevel = tempStr.RegexMatchs(@"(?<![a-zA-Z])level(?![a-zA-Z])");//匹配等级
        //System.Text.RegularExpressions.MatchCollection matchZiMuLevel = tempStr.RegexMatchsZiMu();
        string regexStr = "(?<![a-zA-Z])level(?![a-zA-Z])";
        //string regexStr = @"(?<![\W])level(?![\W])";
        if (matchZiMuLevel != null)
        {

            //计算等级
            //tempStr = tempStr.Replace(matchZiMuLevel[0].Value, levelStr);
            for (int f = 0; f < matchZiMuLevel.Count; ++f)
            {
                //tempStr = tempStr.Replace(matchZiMuLevel[f].Value, levelStr);
                Debug.LogErrorFormat("matchLevel {0}", matchZiMuLevel[f].Value);
                //matchZiMuLevel[f].Index
            }

            Debug.LogErrorFormat("tempStr {0}", tempStr);
            tempStr = tempStr.ReplaceRegex(regexStr, "666");
            Debug.LogErrorFormat("tempStr ReplaceRegex {0}", tempStr);

        }
    }


    /// <summary>
    /// string 转 float   如果失败就返回0f
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static float ToFloat(this string str)
    {
        //int value = int.MinValue;
        float value = 0f;
        float.TryParse(str, out value);
        return value;
    }
 
    /// <summary>
	/// Regex 正则表达式 替换所有
	/// </summary>
	/// <param name="str"></param>
	/// <param name="regexStr"></param>
	/// <param name="replaceRegex"></param>
	/// <returns></returns>
	public static string ReplaceRegex(this string str, string regexStr, string replaceRegex)
	{
		RegexOptions ops = RegexOptions.Multiline;
		// Regex r = new Regex(@"\[(.+?)\]", ops);
		Regex r = new Regex(regexStr, ops);
		if (r.IsMatch(str))
		{
			str = r.Replace(str, replaceRegex);
		}

		return str;
	}

    /// <summary>
    /// 替换正则表达式一次
    /// </summary>
    /// <param name="str"></param>
    /// <param name="regexStr"></param>
    /// <param name="replaceRegex"></param>
    /// <returns></returns>
    public static string ReplaceRegexOnece(this string str, string regexStr, string replaceRegex)
    {
        RegexOptions ops = RegexOptions.Multiline;
        // Regex r = new Regex(@"\[(.+?)\]", ops);
        Regex r = new Regex(regexStr, ops);
        if (r.IsMatch(str))
        {
            //r.Matches
            str = r.Replace(str, replaceRegex, 1);
        }
        return str;
    }


    /// <summary>
    /// 替换掉中文 逗号  分号
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ReplaceChineseStr(this string str)
    {
        if (str == null)
        {
            return string.Empty;
        }
        return str.Replace('；', ';').Replace('，', ',').Trim();
    }


    /// <summary>
    /// 替换掉中文 括号
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ReplaceChineseStrKuoHao(this string str)
    {
        if (str == null)
        {
            return string.Empty;
        }
        return str.Replace("（", "(", StringComparison.Ordinal).Replace("）", ")", StringComparison.Ordinal).Trim();
    }

    /// <summary>
    /// 替换掉中文 冒号
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ReplaceChineseStrMaoHao(this string str)
    {
        if (str == null)
        {
            return string.Empty;
        }
        return str.Replace("：", ":", StringComparison.Ordinal);
    }
 
    /// <summary>
    /// 去掉换行
    /// </summary>
    /// <returns></returns>
    public static string ReplaceLineBreaks(this string str)
    {
        var newStr = str.Replace("\n", "", StringComparison.Ordinal); 
        newStr = newStr.Replace("\\n", "", StringComparison.Ordinal);
        return newStr;
    }

    /// <summary>
    /// string 转 long  如果失败就返回0
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static long ToLong(this string str)
    {
        long value = 0;
        long.TryParse(str, out value);
        return value;
    }
    public static ulong ToUlong(this string str)
    {
        ulong value = 0;
        ulong.TryParse(str, out value);
        return value;
    }

 
    public static string JoinToString(this List<(int, int)> listValueTuple, char splite = ',', char splite2 = ';')
    {
        if (listValueTuple == null || listValueTuple.Count == 0)
        {
            return string.Empty;
        }

        string value = string.Empty;
        for (int i = 0; i < listValueTuple.Count; ++i)
        {
            value += $"{listValueTuple[i].Item1}{splite}{listValueTuple[i].Item2}{splite2}";
        }

        return value;
    }

    /// <summary>
    /// 得到int元组 2位
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static (int, int) ToIntValueTuple(this string str, char splite = '_')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new(0, 0);
        }
        string[] strs = str.ToStringArray(splite);
        if (strs != null)
        {
            return new(strs.Length > 0 ?strs[0].ToInt():0, strs.Length > 1 ?strs[1].ToInt():0);
        }

        return new(0, 0);
    }

    /// <summary>
    /// 得到int元组 3位
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static (int, int, int) ToIntValueTuple3(this string str, char splite = '_')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new(-1, -1, -1);
        }
        string[] strs = str.ToStringArray(splite);
        if (strs != null && strs.Length > 2)
        {
            return new(strs[0].ToInt(), strs[1].ToInt(), strs[2].ToInt());
        }

        return new(-1, -1, -1);
    }
 

    /// <summary>
    /// 得到int元组 4位
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static (int, int, int, int) ToIntValueTuple4(this string str, char splite = '_')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new(-1, -1, -1, -1);
        }
        string[] strs = str.ToStringArray(splite);
        if (strs != null && strs.Length > 1)
        {
            return new(strs[0].ToInt(), strs[1].ToInt(), strs[2].ToInt(), strs[3].ToInt());
        }

        return new(-1, -1, -1, -1);
    }
    public static string[] ToStringArray(this string str, char splite = '_')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new string[] { };
        }

        return str.Split(splite, StringSplitOptions.RemoveEmptyEntries);
    }


    /// <summary>
    /// 转字符数组
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splite">分割字符串 如:"||"</param>
    /// <returns></returns>
    public static string[] ToStringArray(this string str, string splite)//= ","
    {
        if (string.IsNullOrEmpty(str))
        {
            return new string[] { };
        }

        return str.Split(splite, StringSplitOptions.RemoveEmptyEntries);
    }

    public static List<string> ToStringList(this string str, char splite = ',', StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<string>();
        }


        return new List<string>(str.Split(splite, options));
        //return new List<string>(str.ToStringArray(splite));
    }

 
    public static List<string> ToStringList(this string str, string splite, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<string>();
        }

        return new List<string>(str.Split(splite, options));
    }

    public static List<int> ToIntList(this string str, char splite = '_')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<int>();
        }
        List<int> listInt = str.Trim().Split(splite, StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(s => s.ToInt());
        return listInt;
    }
  

    public static List<long> ToLongList(this string str, char splite = '_')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new List<long>();
        }

        List<long> listInt = str.Trim().Split(new char[] { splite }).ToList().ConvertAll(s => s.ToLong());
        return listInt;
    }

 
    /// <summary>
    ///     Returns an enumerable collection of the specified type containing the substrings in this instance that are
    ///     delimited by elements of a specified Char array
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="separator">
    ///     An array of Unicode characters that delimit the substrings in this instance, an empty array containing no
    ///     delimiters, or null.
    /// </param>
    /// <typeparam name="T">
    ///     The type of the element to return in the collection, this type must implement IConvertible.
    /// </typeparam>
    /// <returns>
    ///     An enumerable collection whose elements contain the substrings in this instance that are delimited by one or more
    ///     characters in separator.
    /// </returns>
    public static IEnumerable<T> SplitTo<T>(this string str, params char[] separator) where T : IConvertible
    {
        return str.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(s => (T)Convert.ChangeType(s, typeof(T)));
    }


    public static List<List<int>> ToIntList2(this string str, char splite1 = '_', char splite2 = '|')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new();
        }
        List<List<int>> list  =  new();
        List<string> list1 =  str.ToStringList(splite2);
        for (int i = 0; i < list1.Count; i++)
        {
            list.Add( list1[i].ToIntList(splite1) );
        }
        return list;
    }

    public static List<List<List<int>>> ToIntList3(this string str, char splite1 = '_', char splite2 = '|', char splite3 = '#')
    {
        if (string.IsNullOrEmpty(str))
        {
            return new();
        }
        List<List<List<int>>> list  =  new();
        List<string> list1 =  str.ToStringList(splite3);
        for (int i = 0; i < list1.Count; i++)
        {
            list.Add( list1[i].ToIntList2(splite1, splite2) );
        }
        return list;
    }


    public static void PrintCharArray(this string str)
    {
        if (str == null) return;
        char[] charLen = str.ToCharArray();
        Debug.LogError(string.Format("str Length {0}", charLen.Length));
        for (int i = 0; i < charLen.Length; i++)
        {
            Debug.LogError(string.Format("char {0}", charLen[i]));
        }
    }

    /// <summary>
    /// 计算utf8字符 占用字节数
    /// </summary>
    /// <param name="_char"></param>
    /// <returns></returns>
    public static int GetCharLength(this char _char)
    {
        if (_char > 240)
        {
            return 4;
        }
        else if (_char > 225)
        {
            return 3;
        }
        else if (_char > 192)
        {
            return 2;
        }

        return 1;
    }
}
