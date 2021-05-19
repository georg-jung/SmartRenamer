using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SmartRenamer.PluginAbstractions;

namespace SmartRenamer.Core.Plugins
{
    public class Path : IPropertyValueProvider
    {
        private const string FileName = nameof(FileName);
        private const string FileNameWithoutExtension = nameof(FileNameWithoutExtension);
        private const string FullPath = nameof(FullPath);
        private const string FoldersOnlyPath = nameof(FoldersOnlyPath);
        private const string Extension = nameof(Extension);
        private const string ParentFolderName = nameof(ParentFolderName);

        public string Namespace => nameof(Path);

        public IReadOnlyCollection<string> AvailableProperties => new[] { FileName, FileNameWithoutExtension, FullPath, FoldersOnlyPath, Extension, ParentFolderName };

        public Task<object?> GetValue(string filePath, string propertyName)
        {
            var res = propertyName switch
            {
                FileName => System.IO.Path.GetFileName(filePath),
                FileNameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(filePath),
                FullPath => filePath,
                FoldersOnlyPath => System.IO.Path.GetDirectoryName(filePath),
                Extension => System.IO.Path.GetExtension(filePath),
                ParentFolderName => System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(filePath)),
                _ => throw new NotImplementedException()
            };
            return Task.FromResult<object?>(res);
        }
    }
}
