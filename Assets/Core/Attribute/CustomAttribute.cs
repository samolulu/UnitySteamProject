using System;

/// <summary>
/// 定义一个特性，用于 外部通过 策划配置的途径（反射）对该属性赋值时 约束其范围 
/// </summary>
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
public class ExtraIntRangeAttribute : Attribute
{
    public int minNum;
    public int maxNum;

    public ExtraIntRangeAttribute(int minNum, int maxNum)
    {
        this.minNum = minNum;
        this.maxNum = maxNum;
    }
 
 
} 

/// <summary>
/// 定义一个特性，用于标记属性或字段不属于Excel中定义的
/// </summary>
// [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
// public class ExcelIgnoreAttribute : Attribute
// {
 
//     public ExcelIgnoreAttribute()
//     {
 
//     }
 
 
// } 