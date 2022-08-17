using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ForteConfigurationLoader.InteropLayer;
using ForteConfigurationLoader.Options;
using ForteConfigurationLoader.Utilities;
using Microsoft.Extensions.Options;

namespace ForteConfigurationLoader.DataAccessLayer
{
    public class FilesDataStore : IFilesDataStore
    {
        private readonly List<FileDataModel> _fileDataModels = new();
        private readonly string _filePath;
        private readonly string _successfullyCfgFilePath;


        public FilesDataStore(IOptions<HawkbitSettings> hawkbitSettings)
        {
            _filePath = hawkbitSettings.Value.DownloadFilePath;
            _successfullyCfgFilePath =  Path.Combine(_filePath, "SuccessfullyCfg");
            Directory.CreateDirectory(_filePath);
            Directory.CreateDirectory(_successfullyCfgFilePath);
        }

        public void FromDeploymentData(NativeHawkbitDeploymentData deploymentData)
        {
            var fileDataModel = new FileDataModel();

            var fileName = deploymentData.payload.fileName;
            fileDataModel.Name = fileName;
            fileDataModel.FullName = Path.Combine(_filePath, fileName);

            var fileExtn = Path.GetExtension(fileName);
            fileDataModel.FileType = fileExtn.Equals(".fboot") ? FileDataModel.Type.eFboot : FileDataModel.Type.eForte;

            var hashes = deploymentData.payload.hashes;
            fileDataModel.Hashes.Md5 = hashes.md5;
            fileDataModel.Hashes.Sha1 = hashes.sha1;
            fileDataModel.Hashes.Sha256 = hashes.sha256;
            _fileDataModels.Add(fileDataModel);
        }

        public void StartTransaction()
        {
            _fileDataModels.Clear();
            // FileUtils.CopyFilesFromDirectory(_filePath, _successfullyCfgFilePath);
        }
        
        public void CommitTransaction()
        {
            FileUtils.ClearFilesInDirectory(_successfullyCfgFilePath);
            FileUtils.MoveFilesFromDirectory(_filePath, _successfullyCfgFilePath);
            _fileDataModels.Clear();
        }
        
        public void RollBackTransaction()
        {
            FileUtils.ClearFilesInDirectory(_filePath);
            FileUtils.CopyFilesFromDirectory(_successfullyCfgFilePath, _filePath);
            _fileDataModels.Clear();

            var files= Directory.GetFiles(_filePath);
            foreach (var fileName in files)
            {
                var fileExtn = Path.GetExtension(fileName);
             
                var fileDataModel = new FileDataModel
                {
                    Name = Path.GetFileName(fileName),
                    FullName = fileName,
                    FileType = fileExtn.Equals(".fboot") ? FileDataModel.Type.eFboot : FileDataModel.Type.eForte
                };

                _fileDataModels.Add(fileDataModel);
            }
        }

        public IEnumerable<FileDataModel> QueryFiles(FileDataModel.Type type)
        {
            return _fileDataModels.Where(dm => dm.FileType == type).ToList();
        }
        
        public IEnumerator<FileDataModel> GetEnumerator()
        {
            yield return _fileDataModels.GetEnumerator().Current;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}