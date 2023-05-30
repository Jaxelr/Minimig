using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Minimig;

internal enum MigrateMode
{
    Skip,
    Run,
    Rename,
    HashMismatch,
}

public class Migration
{
    private static readonly MD5CryptoServiceProvider Md5Provider = new();
    private static readonly Regex LineEndings = new("\r\n|\n\r|\n|\r", RegexOptions.Compiled);

    public IEnumerable<string> SqlCommands { get; }
    public string Hash { get; }
    public string Filename { get; }
    public bool UseTransaction { get; }

    internal Migration(string filePath, Regex commandSplitter)
    {
        string sql = File.ReadAllText(filePath, Encoding.GetEncoding("iso-8859-1"));
        SqlCommands = commandSplitter.Split(sql).Where(s => s.Trim().Length > 0);
        Hash = GetHash(sql);
        Filename = Path.GetFileName(filePath);

        UseTransaction = !sql.StartsWith("-- no transaction --");
    }

    /// <summary>
    /// Classify migrations in order to determine action to take
    /// </summary>
    /// <param name="alreadyRan"></param>
    /// <returns>An enum with the migration mode to execute</returns>
    internal MigrateMode GetMigrateMode(AlreadyRan alreadyRan)
    {
        if (alreadyRan.ByFilename.TryGetValue(Filename, out MigrationRow row))
        {
            return row.Hash == Hash ? MigrateMode.Skip : MigrateMode.HashMismatch;
        }

        if (alreadyRan.ByHash.TryGetValue(Hash, out _))
        {
            return MigrateMode.Rename;
        }

        return MigrateMode.Run;
    }

    /// <summary>
    /// Get the md5 hash of the string provided
    /// </summary>
    /// <param name="str"></param>
    /// <returns>A new guid with the hashed bytes of the string provided</returns>
    private static string GetHash(string str)
    {
        string normalized = NormalizeLineEndings(str);
        byte[] inputBytes = Encoding.Unicode.GetBytes(normalized);

        byte[] hashBytes;
        lock (Md5Provider)
        {
            hashBytes = Md5Provider.ComputeHash(inputBytes);
        }

        return new Guid(hashBytes).ToString();
    }

    /// <summary>
    /// Standarize line endings with \n
    /// </summary>
    /// <param name="str"></param>
    /// <returns>A string standarized</returns>
    private static string NormalizeLineEndings(string str) => LineEndings.Replace(str, "\n");
}
