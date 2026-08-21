using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Automaticks.Testing.Coverage;

/// <summary>
///     A Cobertura coverage report parsed into per-file and per-method counters.
/// </summary>
public sealed class CoverageReport
{
    private readonly Dictionary<string, List<FileCoverage>> _filesByName;

    /// <summary>
    ///     Gets a value indicating whether the report contained at least one file entry.
    /// </summary>
    public bool IsPopulated { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CoverageReport" /> class from report XML.
    /// </summary>
    /// <param name="reportXml">The Cobertura report contents.</param>
    public CoverageReport(string reportXml)
    {
        _filesByName = new Dictionary<string, List<FileCoverage>>(StringComparer.OrdinalIgnoreCase);
        Parse(reportXml);
    }

    /// <summary>
    ///     Finds the coverage recorded for a source file.
    /// </summary>
    /// <param name="documentPath">The absolute path of the document being analysed.</param>
    /// <returns>The matching entry, or <see langword="null" /> when the file is absent.</returns>
    public FileCoverage? FindFile(string documentPath)
    {
        if (string.IsNullOrEmpty(documentPath))
        {
            return null;
        }

        var normalized = NormalizePath(documentPath);
        if (!_filesByName.TryGetValue(GetFileName(normalized), out var candidates))
        {
            return null;
        }

        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        foreach (var candidate in candidates)
        {
            if (normalized.EndsWith(NormalizePath(candidate.ReportedPath), StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    ///     Merges a further Cobertura report into this one.
    /// </summary>
    /// <param name="reportXml">The Cobertura report contents to merge.</param>
    public void Include(string reportXml)
    {
        Parse(reportXml);
    }

    private string GetFileName(string normalizedPath)
    {
        var separatorIndex = normalizedPath.LastIndexOf('/');
        return separatorIndex < 0 ? normalizedPath : normalizedPath.Substring(separatorIndex + 1);
    }

    private FileCoverage GetOrAddFile(string reportedPath)
    {
        var fileName = GetFileName(NormalizePath(reportedPath));
        if (!_filesByName.TryGetValue(fileName, out var candidates))
        {
            candidates = new List<FileCoverage>();
            _filesByName.Add(fileName, candidates);
        }

        foreach (var candidate in candidates)
        {
            if (string.Equals(candidate.ReportedPath, reportedPath, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        var created = new FileCoverage(reportedPath);
        candidates.Add(created);
        IsPopulated = true;
        return created;
    }

    private string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private void Parse(string reportXml)
    {
        try
        {
            ParseCore(reportXml);
        }
        catch (XmlException)
        {
        }
    }

    private void ParseCore(string reportXml)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreWhitespace = true,
        };

        using var stringReader = new StringReader(reportXml);
        using var reader = XmlReader.Create(stringReader, settings);
        FileCoverage? currentFile = null;
        MethodCoverage? currentMethod = null;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "method")
            {
                currentMethod = null;
                continue;
            }

            if (reader.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (reader.Name == "class")
            {
                currentFile = ReadClass(reader, currentFile);
                currentMethod = null;
            }
            else if (reader.Name == "method")
            {
                currentMethod = ReadMethod(reader, currentFile);
            }
            else if (reader.Name == "line")
            {
                ReadLine(reader, currentFile, currentMethod);
            }
        }
    }

    private FileCoverage? ReadClass(XmlReader reader, FileCoverage? currentFile)
    {
        var fileName = reader.GetAttribute("filename");
        return string.IsNullOrEmpty(fileName) ? currentFile : GetOrAddFile(fileName!);
    }

    private void ReadLine(XmlReader reader, FileCoverage? currentFile, MethodCoverage? currentMethod)
    {
        if (currentFile is null)
        {
            return;
        }

        var hits = int.TryParse(reader.GetAttribute("hits"), out var parsed) ? parsed : 0;
        var lineNumber = int.TryParse(reader.GetAttribute("number"), out var number) ? number : 0;
        currentFile.AddLine(lineNumber, hits, reader.GetAttribute("condition-coverage"));
        currentMethod?.AddLine(lineNumber, hits, reader.GetAttribute("condition-coverage"));
    }

    private MethodCoverage? ReadMethod(XmlReader reader, FileCoverage? currentFile)
    {
        if (currentFile is null)
        {
            return null;
        }

        var name = reader.GetAttribute("name");
        return string.IsNullOrEmpty(name) ? null : currentFile.GetOrAddMethod(name!);
    }
}
