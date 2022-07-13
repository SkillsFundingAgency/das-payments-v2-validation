using System;
using System.Collections.Generic;
using Azure.Storage.Files.Shares;

namespace SFA.DAS.Payments.FM36Tool.Submission
{
    public class AzureFileStorageFm36FilesProvider
    {
        private readonly ShareClient _shareClient;
        public AzureFileStorageFm36FilesProvider(ShareClient shareClient)
        {
            _shareClient = shareClient ?? throw new ArgumentNullException(nameof(shareClient));
        }

        public List<FileGroup> GetGroups()
        {
            var root = _shareClient.GetRootDirectoryClient();

            var items = root.GetFilesAndDirectories();

            var fileGroups = new List<FileGroup>();

            foreach (var item in items)
            {
                if (!item.IsDirectory)
                    continue;

                var fm36Folder = root.GetSubdirectoryClient(item.Name);

                var fileGroup = new FileGroup { Name = fm36Folder.Name };

                var fm36Files = fm36Folder.GetFilesAndDirectories();

                foreach (var fm36File in fm36Files)
                {
                    if (fm36File.IsDirectory)
                        continue;

                    fileGroup.Files.Add(Fm36File.Parse(fm36File.Name));
                }

                fileGroups.Add(fileGroup);
            }

            return fileGroups;
        }

        public FileGroup GetGroup(string groupName)
        {
            var root = _shareClient.GetRootDirectoryClient();

            var fm36Folder = root.GetSubdirectoryClient(groupName);

            var fileGroup = new FileGroup { Name = fm36Folder.Name };

            var fm36Files = fm36Folder.GetFilesAndDirectories();

            foreach (var fm36File in fm36Files)
            {
                if (fm36File.IsDirectory)
                    continue;

                fileGroup.Files.Add(Fm36File.Parse(fm36File.Name));
            }

            return fileGroup;
        }
    }


    public class FileGroup
    {
        public string Name { get; set; }
        public List<Fm36File> Files { get; } = new List<Fm36File>();
    }

    public class Fm36File
    {
        public string Name { get; }
        public long Ukprn { get; }

        private Fm36File(string name, long ukprn)
        {
            Name = name;
            Ukprn = ukprn;
        }
        public static Fm36File Parse(string filename)
        {
            var parts = filename.Split('-');

            long.TryParse(parts[1].Trim(), out var ukprn);

            return new Fm36File(filename, ukprn);
        }
    }
}