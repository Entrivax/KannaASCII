using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KannaASCII
{
    public static class CommandLineHelper
    {
        public static T Parse<T>(string[] arguments) where T : new()
        {
            var obj = new T();
            var type = typeof(T);
            for (int i = 0; i < arguments.Length; i++)
            {
                var parsed = false;
                if (arguments[i].StartsWith("-"))
                {
                    foreach (var property in type.GetProperties())
                    {
                        var attrs = property.GetCustomAttributes(false);

                        ArgumentAttribute argAttr = null;
                        var isShortName = false;
                        foreach (var attr in attrs)
                        {
                            var tmpArgAttr = attr as ArgumentAttribute;
                            if (tmpArgAttr != null
                                && ((tmpArgAttr.Name != null && "--" + tmpArgAttr.Name == arguments[i])
                                || (tmpArgAttr.ShortName != null && "-" + tmpArgAttr.ShortName == arguments[i])))
                            {
                                isShortName = !arguments[i].StartsWith("--");
                                argAttr = tmpArgAttr;
                                break;
                            }
                        }

                        if (argAttr != null)
                        {
                            var typeCode = Type.GetTypeCode(property.PropertyType);
                            var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                            if (underlyingType != null)
                            {
                                typeCode = Type.GetTypeCode(underlyingType);
                            }
                            switch (typeCode)
                            {
                                case TypeCode.String:
                                    i++;
                                    CheckMissingArgument(i, arguments.Length, isShortName, argAttr);
                                    property.SetValue(obj, arguments[i]);
                                    break;

                                case TypeCode.Int16:
                                    i++;
                                    CheckMissingArgument(i, arguments.Length, isShortName, argAttr);
                                    try
                                    {
                                        property.SetValue(obj, Convert.ToInt16(arguments[i], CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine($"\t Error: {(isShortName ? ("-" + argAttr.ShortName) : ("--" + argAttr.Name))} value must be a 2 bytes number");
                                        throw;
                                    }
                                    break;

                                case TypeCode.Int32:
                                    i++;
                                    CheckMissingArgument(i, arguments.Length, isShortName, argAttr);
                                    try
                                    {
                                        property.SetValue(obj, Convert.ToInt32(arguments[i], CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine($"\t Error: {(isShortName ? ("-" + argAttr.ShortName) : ("--" + argAttr.Name))} value must be a 4 bytes number");
                                        throw;
                                    }
                                    break;

                                case TypeCode.Int64:
                                    i++;
                                    CheckMissingArgument(i, arguments.Length, isShortName, argAttr);
                                    try
                                    {
                                        property.SetValue(obj, Convert.ToInt64(arguments[i], CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine($"\t Error: {(isShortName ? ("-" + argAttr.ShortName) : ("--" + argAttr.Name))} value must be a 8 bytes number");
                                        throw;
                                    }
                                    break;

                                case TypeCode.Single:
                                    i++;
                                    CheckMissingArgument(i, arguments.Length, isShortName, argAttr);
                                    try
                                    {
                                        property.SetValue(obj, Convert.ToSingle(arguments[i], CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine($"\t Error: {(isShortName ? ("-" + argAttr.ShortName) : ("--" + argAttr.Name))} value must be a decimal number");
                                        throw;
                                    }
                                    break;

                                case TypeCode.Double:
                                    i++;
                                    CheckMissingArgument(i, arguments.Length, isShortName, argAttr);
                                    try
                                    {
                                        property.SetValue(obj, Convert.ToDouble(arguments[i], CultureInfo.InvariantCulture.NumberFormat));
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine($"\t Error: {(isShortName ? ("-" + argAttr.ShortName) : ("--" + argAttr.Name))} value must be a decimal number");
                                        throw;
                                    }
                                    break;

                                case TypeCode.Boolean:
                                    if (i + 1 >= arguments.Length || !arguments[i + 1].StartsWith("-"))
                                    {
                                        property.SetValue(obj, true);
                                    }
                                    else
                                    {
                                        i++;
                                        CheckMissingArgument(i, arguments.Length, isShortName, argAttr);
                                        try
                                        {
                                            if (arguments[i].ToLower() == "true")
                                            {
                                                property.SetValue(obj, true);
                                            }
                                            else if (arguments[i].ToLower() == "false")
                                            {
                                                property.SetValue(obj, false);
                                            }
                                            else
                                            {
                                                throw new FormatException();
                                            }
                                        }
                                        catch (FormatException)
                                        {
                                            Console.WriteLine($"\t Error: {(isShortName ? ("-" + argAttr.ShortName) : ("--" + argAttr.Name))} value must be a boolean (true/false)");
                                            throw;
                                        }
                                    }
                                    break;

                                default:
                                    throw new UnsupportedTypeException("This type is not supported");
                            }
                            parsed = true;
                            break;
                        }
                    }
                }
                if (parsed == false)
                {
                    Console.WriteLine($"Unknown argument {arguments[i]}");
                    throw new ArgumentException($"Unknown argument {arguments[i]}");
                }
            }

            return obj;
        }

        private static void CheckMissingArgument(int i, int argsLength, bool isShortName, ArgumentAttribute argAttr)
        {
            if (i < argsLength) return;
            Console.WriteLine($"Missing value for argument {(isShortName ? ("-" + argAttr.ShortName) : ("--" + argAttr.Name))}");
            throw new ArgumentException("Unable to parse arguments");
        }

        public static void WriteHelp<T>()
        {
            Console.WriteLine("Usable arguments:");
            var type = typeof(T);
            var argsAttrs = new List<ArgumentAttribute>();
            foreach (var property in type.GetProperties())
            {
                var attrs = property.GetCustomAttributes(false);
                foreach (var attr in attrs)
                {
                    var argAttr = attr as ArgumentAttribute;
                    if (argAttr != null)
                    {
                        argsAttrs.Add(argAttr);
                    }
                }
            }
            var longestLongName = argsAttrs.Select(at => at.Name != null ? at.Name.Length + 2 : 0).Max();
            var longestShortName = argsAttrs.Select(at => at.ShortName != null ? at.ShortName.Length + 1 : 0).Max();
            var neededTabsToSkipLongName = longestLongName / 8;
            var neededTabsToSkipShortName = longestShortName / 8;
            foreach (var arg in argsAttrs)
            {
                var builder = new StringBuilder();
                builder.Append("\t");
                if (arg.Name == null)
                {
                    builder.Append('\t', neededTabsToSkipLongName);
                }
                else
                {
                    builder.Append("--" + arg.Name);
                    var tabs = neededTabsToSkipLongName - (arg.Name.Length / 8) + (arg.Name.Length % 8 != 0 ? 1 : 0);
                    if (tabs > 0)
                        builder.Append('\t', tabs);
                }
                builder.Append("\t");
                if (arg.ShortName == null)
                {
                    builder.Append('\t', neededTabsToSkipShortName);
                }
                else
                {
                    builder.Append("-" + arg.ShortName);
                    var tabs = neededTabsToSkipShortName - (arg.ShortName.Length / 8) + (arg.ShortName.Length % 8 != 0 ? 1 : 0);
                    if (tabs > 0)
                        builder.Append('\t', tabs);
                }
                builder.Append("\t");
                builder.Append(arg.HelpMessage);
                
                Console.WriteLine(builder.ToString());
            }
        }
    }

    public class UnsupportedTypeException : Exception
    {
        public UnsupportedTypeException(string message) : base(message) { }
    }

    public class ArgumentAttribute : Attribute
    {
        public string Name { get; }
        public string ShortName { get; }
        public string HelpMessage { get; }

        public ArgumentAttribute(string name, string helpMessage)
        {
            if (name == null)
                throw new ArgumentException("Name and ShortName cannot be null at the same time");
            Name = name;
            ShortName = null;
            HelpMessage = helpMessage;
        }

        public ArgumentAttribute(string name, char shortName, string helpMessage)
        {
            Name = name;
            ShortName = shortName.ToString();
            HelpMessage = helpMessage;
        }
    }
}
