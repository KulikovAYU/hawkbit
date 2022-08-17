using System.Collections.Generic;
using System.IO;
using ForteConfigurationLoader.InnerCommandLayer;

namespace ForteConfigurationLoader.FBootParcerLayer
{
    public interface IFBootParser
    {
        void Parse(string sFilePath);
        void Parse(FileStream fs);
        bool IsValid { get; }
        List<CommandsFbSet> GetExecTasks();
    }
}