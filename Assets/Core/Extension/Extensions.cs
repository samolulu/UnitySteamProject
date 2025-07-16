using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text.Json;
using MiniExcelLibs.Attributes;
using System.Text.Json.Nodes;
 


/// <summary>
///  object
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// 对象所有属性均是初始值
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsDefault(this object obj, bool justExcelProperty = false)
    {
        foreach(PropertyInfo pi in obj.GetType().GetProperties())
        {
            if(justExcelProperty)
            {
                foreach (var item in pi.GetCustomAttributes())
                {
                    if(item.GetType()  == typeof(ExcelIgnoreAttribute))
                    {
                        continue;
                    } 
                }
            }

            var value =  pi.GetValue(obj);
            var type = pi.PropertyType;
            //var _default = type.IsValueType ? Activator.CreateInstance(type) : null;
            if(type.IsValueType ){
                if(!value.Equals(Activator.CreateInstance(type)))  return false;
            } else{
                if(null != value && !value.Equals(default)) return false;
            }
 
        }
        return true;
    }

    /// <summary>
    /// 转换类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T ConvertTo<T>(this object obj)
    {

        return (T)Convert.ChangeType(obj, typeof(T));
    }

    public static T? JsonToObject<T>(this string str)
    {
        return JsonSerializer.Deserialize<T>(str);
    }

    public static string ToJsonString<T>(this T obj,  JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options);
    }

    public static JsonNode? ToJObject(this string str)
    {
        return JsonNode.Parse(str);
    }
   

    /// <summary>
    /// 反射对象的方法 //NeedDo 把方法的所有重载缓存到对象身上
    /// </summary>
    public static MethodInfo? GetMethod(this object obj, string methodName, object[]? parameters = null)
    {
        if(null == obj) 
        {
            UnityEngine.Debug.LogError($"GetMethod error:obj is null, methodName:{methodName}");
            return null;
        }
        if(string.IsNullOrEmpty(methodName)) return null;
        methodName = methodName.Trim();
        int paramLength = parameters?.Length ?? 0;
        var argTypes = new Type[paramLength] ;
        for (int l = 0; l < paramLength; l++)
        {
                argTypes[l] = parameters![l].GetType();
        }
        var type = obj.GetType();
        var method = type.GetMethod(methodName, argTypes);   
        if(method == null)
        {
            UnityEngine.Debug.LogError($"GetMethod not match: type:{type}, methodName:{methodName}, paramLength: {paramLength} ");
        }
        return method;
    }

    /// <summary>
    /// 反射属性
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propName"></param>
    /// <param name="allowNull">允许不存在该属性（仅用于检查）</param>
    /// <returns></returns>
    public static PropertyInfo? GetProperty(this object obj, string propName, bool allowNull = false )
    {
        if(null == obj) 
        {
            UnityEngine.Debug.LogError($"GetProperty error:obj is null, propName:{propName}");
            return null;
        }
        if(string.IsNullOrEmpty(propName)) 
        {
            UnityEngine.Debug.LogError($"GetProperty error:propName is null or empty: type:{obj.GetType()}, propName:{propName}");
            return null;
        }
        propName = propName.Trim();
        var type = obj.GetType();
        var prop = type.GetProperty(propName);   
        if(prop == null)
        {
           if(!allowNull) UnityEngine.Debug.LogError($"GetProperty error: type:{type}, propName:{propName} ");
        }
        return prop;
    }

    /// <summary>
    /// 反射 获取属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static object? GetPropertyValue<T>(this T model, string propertyName) where T : new()
    {
        PropertyInfo? property = model!.GetProperty(propertyName);
        if (property == null) return null;
        object value = property.GetValue(model, null);

        return value;
    }

    /// <summary>
    /// 反射 设置属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public static void SetPropertyValue<T>(this T model, string propertyName, object value) where T : new()
    {
        PropertyInfo? property = model!.GetProperty(propertyName);
        if (property == null) return;
 
        //支持int属性的赋值范围约束
        if(property.PropertyType == typeof(int))
        {
            if(value is long) //需处理long2int
            {
               value = int.Parse(value.ToString());
            }
            foreach (var item in property.GetCustomAttributes())
            {
                if(item.GetType()  == typeof(ExtraIntRangeAttribute))
                {
                    var attr = (ExtraIntRangeAttribute)item;
                    value = Math.Clamp((int)value, attr.minNum ,attr.maxNum);
                }
                
            }
        }

 
        property.SetValue(model, value);

    }
 
    private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

    public static bool IsPrimitive(this Type type)
    {
        if (type == typeof(string)) return true;
        return type.IsValueType & type.IsPrimitive;
    }

    public static object? Copy(this object originalObject)
    {
        return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
    }
	
    private static object? InternalCopy(object originalObject, IDictionary<object, object> visited)
	{
		if (originalObject == null) return null;
		var typeToReflect = originalObject.GetType();
		if (IsPrimitive(typeToReflect)) return originalObject;
		if (visited.ContainsKey(originalObject)) return visited[originalObject];
		if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
		var cloneObject = CloneMethod.Invoke(originalObject, null);
		if (typeToReflect.IsArray)
		{
			var arrayType = typeToReflect.GetElementType();
			if (IsPrimitive(arrayType) == false)
			{
				Array clonedArray = (Array)cloneObject;
				clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
			}

		}
		visited.Add(originalObject, cloneObject);
		CopyFields(originalObject, visited, cloneObject, typeToReflect);
		RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
		return cloneObject;
	}

    private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
    {
        if (typeToReflect.BaseType != null)
        {
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
            CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        }
    }

    private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
    {
        foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
        {
            if (filter != null && filter(fieldInfo) == false) continue;
            if (IsPrimitive(fieldInfo.FieldType)) continue;
            var originalFieldValue = fieldInfo.GetValue(originalObject);
            var clonedFieldValue = InternalCopy(originalFieldValue, visited);
            fieldInfo.SetValue(cloneObject, clonedFieldValue);
        }
    }
	
    public static T? Copy<T>(this T original)
	{
		return (T?)Copy((object)original!);
	}
}

internal class ReferenceEqualityComparer : EqualityComparer<object>
{
    public override bool Equals(object x, object y)
    {
        return ReferenceEquals(x, y);
    }
    public override int GetHashCode(object obj)
    {
        if (obj == null) return 0;
        return obj.GetHashCode();
    }
}


