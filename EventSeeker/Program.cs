using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using static EventSeeker.F;

namespace EventSeeker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var types = FindAllDerivedTypes<Control>();
            string contents = FinalEnclosing(string.Join(Environment.NewLine + Environment.NewLine,
                types
                    .Select(type => new
                    {
                        TypeName = type.Name,
                        Events = type.GetEvents()
                    })
                    .ToList()
                    .Select(a =>
                        GenerateEnclosingText(a.TypeName, a.Events.Select(e => e.Name)))));

            string path = Path.Combine(Environment.CurrentDirectory, "Gen.cs");

            File.WriteAllText(path, contents);

            //Process.Start(path ?? throw new InvalidOperationException());
            //Process.Start(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
            Console.ReadKey(true);
        }
    }

    public static class F
    {
        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                ).ToList();
        }

        public static string FinalEnclosing(string value, string className = "WinFormsEvents")
        {
            //value += "";
            return $"public class {className} \n{{\n{value}\n}}";
        }

        public static string GenerateEnclosingText(string name, IEnumerable<string> col, string separator = ",\n", Func<string, bool, string> headerFunc = null)
        {
            string value = headerFunc?.Invoke(name, false) ?? GetHeader(name, false);

            value += string.Join(separator, col.Select(i => "\t\t" + i));
            value += headerFunc?.Invoke(name, true) ?? GetHeader(name, true);

            return value;
        }

        private static string GetHeader(string name, bool end)
        {
            if (!end)
                return $"\tpublic enum {name}Events\n\t{{\n";

            return "\n\t}";
        }

        private static string IndentCode(string csCode)
        {
            if (string.IsNullOrEmpty(csCode))
                throw new ArgumentNullException(nameof(csCode));

            var tree = CSharpSyntaxTree.ParseText(csCode);
            var root = tree.GetRoot().NormalizeWhitespace();
            var ret = root.ToFullString();

            return ret;
        }
    }
}