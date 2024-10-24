﻿using System.Globalization;

namespace Game_Engine.Core;

public static class JSONSerializer
{
    private static readonly CultureInfo _cultureInfo = new("en-US");

    public static Dictionary<string, object?> JSONToObj(string data)
    {
        Dictionary<string, object?>? result = null;
        var symIndex = 0;

        while (symIndex < data.Length)
        {
            if (data[symIndex] == '{' && result is null)
                result = ReadObject(data, ref symIndex);
            else if (data[symIndex] is not ' ' or '\t' or '\n')
                throw new ArgumentException("Syntax error. JSON file is invalid");

            symIndex++;
        }

        return result ?? [];
    }

    private static Dictionary<string, object?> ReadObject(string data, ref int symIndex)
    {
        Dictionary<string, object?> result = [];

        string currentKey = string.Empty;
        object? currentValue = null;

        var isKeyInitialized = false;
        var isValueInitialized = false;
        var wasThereColon = false;

        var numberOfCommas = 0;

        do
        {
            symIndex++;

            if (data[symIndex] == '"' && ((isKeyInitialized == false) || (isValueInitialized == false)))
            {
                if (isKeyInitialized == false)
                {
                    currentKey = ReadString(data, ref symIndex);
                    isKeyInitialized = true;
                }
                else if (wasThereColon)
                {
                    currentValue = ReadString(data, ref symIndex);
                    isValueInitialized = true;
                }
            }
            else if (data[symIndex] == ':' && (wasThereColon == false) && isKeyInitialized)
            {
                wasThereColon = true;
            }
            else if (data[symIndex] == ',' && isKeyInitialized && isValueInitialized)
            {
                if (result.TryAdd(currentKey, currentValue))
                {
                    isKeyInitialized = false;
                    isValueInitialized = false;
                    wasThereColon = false;
                    numberOfCommas++;
                }
                else
                    throw new ArgumentException("Error. JSON file is invalid, there are pairs with matching keys");
            }
            else if (data[symIndex] == '{' && wasThereColon && (isValueInitialized == false))
            {
                currentValue = ReadObject(data, ref symIndex);
                isValueInitialized = true;
            }
            else if (data[symIndex] == '}' && (isKeyInitialized == isValueInitialized))
            {
                if (isKeyInitialized)
                {
                    if (result.TryAdd(currentKey, currentValue))
                        return result;
                    else
                        throw new ArgumentException("Error. JSON file is invalid, there are pairs with matching keys");
                }

                return result;

            }
            else if (data[symIndex] == '[' && wasThereColon && (isValueInitialized == false))
            {
                currentValue = ReadArray(data, ref symIndex);
                isValueInitialized = true;
            }
            else if (data[symIndex] is ' ' or '\t' or '\n' or '\r')
            {
                continue;
            }
            else if ((char.IsAsciiLetter(data[symIndex]) || char.IsDigit(data[symIndex]) || data[symIndex] == '-')
                      && wasThereColon && (isValueInitialized == false))
            {
                currentValue = ReadLiteral(data, ref symIndex);
                isValueInitialized = true;
            }
            else
            {
                throw new ArgumentException("Syntax error. JSON file is invalid");
            }
        }
        while (symIndex < data.Length - 1);

        throw new ArgumentException("Syntax error. JSON file is invalid");
    }

    private static object?[] ReadArray(string data, ref int symIndex)
    {
        List<object?> elements = [];
        var isElementItitialized = false;

        do
        {
            symIndex++;

            if (data[symIndex] == ',')
            {
                if (isElementItitialized == false)
                    throw new ArgumentException("Syntax error. JSON file is invalid");
                else
                    isElementItitialized = false;
            }
            else if (data[symIndex] == '"' && isElementItitialized == false)
            {
                elements.Add(ReadString(data, ref symIndex));
                isElementItitialized = true;
            }
            else if (data[symIndex] == '{' && isElementItitialized == false)
            {
                elements.Add(ReadObject(data, ref symIndex));
                isElementItitialized = true;
            }
            else if (data[symIndex] == '[' && isElementItitialized == false)
            {
                elements.Add(ReadArray(data, ref symIndex));
                isElementItitialized = true;
            }
            else if (data[symIndex] == ']')
            {
                return [.. elements];
            }
            else if ((char.IsAsciiLetter(data[symIndex]) || char.IsDigit(data[symIndex]) || data[symIndex] == '-')
                    && isElementItitialized == false)
            {
                elements.Add(ReadLiteral(data, ref symIndex));
                isElementItitialized = true;
            }
            else if (data[symIndex] is ' ' or '\t' or '\n' or '\r')
            {
                continue;
            }
            else
            {
                throw new ArgumentException("Syntax error. JSON file is invalid");
            }
        }
        while (symIndex < data.Length - 1);

        throw new ArgumentException("Syntax error. JSON file is invalid");
    }

    private static object? ReadLiteral(string data, ref int symIndex)
    {
        var startIndex = symIndex;

        while ((char.IsAsciiLetter(data[symIndex]) || char.IsDigit(data[symIndex]) || data[symIndex] is '.' or '-' or '+')
                && symIndex < data.Length) { symIndex++; }

        var length = symIndex-- - startIndex;
        var resultStr = new string(data.ToCharArray(startIndex, length));

        if (resultStr.Length <= 5)
        {
            if (resultStr == "null")
                return null;
            else if (resultStr == "true")
                return true;
            else if (resultStr == "false")
                return false;
        }

        if (int.TryParse(resultStr, out int int_result))
            return int_result;
        else if (float.TryParse(resultStr, _cultureInfo, out float float_result))
            return float_result;
        else
            throw new ArgumentException("Syntax error. JSON file is invalid");
    }

    private static string ReadString(string data, ref int symIndex)
    {
        var startIndex = symIndex + 1;

        do { symIndex++; }
        while (symIndex < data.Length && data[symIndex] != '"');

        var length = symIndex - startIndex;

        if (symIndex == data.Length)
            throw new ArgumentException("Syntax error. JSON file is invalid");
        else if (length == 0)
            return string.Empty;
        else
            return new string(data.ToCharArray(startIndex, length));
    }
}
