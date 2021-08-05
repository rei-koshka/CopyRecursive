using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentArgs;

namespace CopyRecursive
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args);
        }

        private static void Run(params string[] args)
        {
            FluentArgsBuilder.New()
                .DefaultConfigsWithAppDescription("Copy files recursive.")
                .RegisterHelpFlag("-h", "--help")
                .Parameter("-l", "--list")
                    .WithDescription("File to get list from.")
                    .WithExamples("list_of_files.txt")
                    .WithValidation(File.Exists, "Given file with list of files does not exists.")
                    .IsRequired()
                .Parameter("-d", "--directory")
                    .WithDescription("Target directory.")
                    .WithExamples("D:\\work\\logs\\errors")
                    .WithValidation(Directory.Exists, "Given directory does not exists.")
                    .IsOptionalWithDefault(Environment.CurrentDirectory)
                .Parameter("-r", "--root")
                    .WithDescription("Take recursion starting from given folder name.")
                    .WithExamples("logs")
                    .IsOptional()
                .Call(
                    rootFolderName =>
                    targetDirectory =>
                    listFilePath =>
                    {
                        Execute(
                            listFilePath,
                            rootFolderName,
                            targetDirectory);
                    })
                .Parse(args);
        }

        private static void Execute(string listFilePath, string rootFolderName, string targetDirectory)
        {
            var lines = File.ReadAllLines(listFilePath);

            foreach (var line in lines)
            {
                var sourceFilePath = line;
                var directoryName = Path.GetDirectoryName(sourceFilePath);
                var fileName = Path.GetFileName(line);
                var directoryInfo = new DirectoryInfo(directoryName);
                var folderHierarchy = GetFolderHierarchy(directoryInfo).ToArray();

                var directoryToCopy = GetDirectoryToCopy(folderHierarchy, rootFolderName);

                var newDirectory = Path.GetFullPath(
                    Path.Combine(
                        targetDirectory,
                        directoryToCopy));

                if (!Directory.Exists(newDirectory))
                {
                    Directory.CreateDirectory(newDirectory);
                }

                var newFilePath = Path.Combine(newDirectory, fileName);

                File.Copy(sourceFilePath, newFilePath, true);
            }
        }

        private static IEnumerable<string> GetFolderHierarchy(DirectoryInfo directoryInfo)
        {
            var result = new Stack<string>();

            DirectoryInfo current = directoryInfo;

            do
            {
                result.Push(current.Name);
                current = current.Parent;
            }
            while (current != null);

            return result.Skip(1);
        }

        private static string GetDirectoryToCopy(string[] folderHierarchy, string rootFolderName)
        {
            if (string.IsNullOrEmpty(rootFolderName))
            {
                return Path.Combine(folderHierarchy);
            }

            var remainingFolderNames = folderHierarchy.SkipWhile(folderName => folderName != rootFolderName)
                                                      .Skip(1)
                                                      .ToArray();

            return Path.Combine(remainingFolderNames);
        }
    }
}
