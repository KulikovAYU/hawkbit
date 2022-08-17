using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ForteConfigurationLoader.InnerCommandLayer;

namespace ForteConfigurationLoader.FBootParcerLayer
{
    public class FBootParser : IFBootParser
    {
        public bool IsValid { get; private set; } = true;
        private string _content;

        public void Parse(string sFilePath)
        {
            try
            {
                using FileStream fileStream =
                    new FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                ReadFileContent(fileStream);
            }
            catch (Exception)
            {
                IsValid = false;
            }
        }
        
        public void Parse(FileStream fs)
        {
            ReadFileContent(fs);
        }

        public List<CommandsFbSet> GetExecTasks()
        {
            List<CommandsFbSet> resTasksBuff = new List<CommandsFbSet>();

            if (string.IsNullOrEmpty(_content))
                return resTasksBuff;
            
            var regex = new Regex(@"\B^*;",RegexOptions.Compiled );
            var fbCreationGroups  = regex.Split(_content).ToList();
            fbCreationGroups.RemoveAll(string.IsNullOrEmpty);
                
            foreach (string fbCreationGroup in fbCreationGroups)
            {
                var matches = Regex.Split(fbCreationGroup, @"[\r\n|\r|\n]").ToList();
                matches.RemoveAll(string.IsNullOrEmpty);

                try
                {
                    XmlDocument doc= new XmlDocument();
                    doc.LoadXml(matches[0]);
                   
                    var fbName = doc.DocumentElement?.SelectSingleNode("//FB[@Name]")?.Attributes?["Name"]?.Value ?? "";
                    if (string.IsNullOrEmpty(fbName))
                    {
                        //incorrect command
                        IsValid = false;
                        return new List<CommandsFbSet>();
                    }

                    var cmdGroup = new CommandsFbSet{Name = fbName};


                    for (int i = 0; i < matches.Count; i++)
                    {
                        var trimSubRequest = Regex.Replace(matches[i], $"{fbName};", "");
                        var tag = i == 0 ? "" : cmdGroup.Name;
                        var cmd = new SubRequestFbCommand(trimSubRequest, tag);
                         
                        if (cmd.HasErrors)
                        {
                            IsValid = false;
                            return new List<CommandsFbSet>();
                        }

                        cmdGroup.Commands.Add(cmd);
                    }

                    resTasksBuff.Add(cmdGroup);
                }
                catch (Exception)
                {
                    IsValid = false;
                }
            }
            return resTasksBuff;
        }

        
        private void ReadFileContent(FileStream fileStream)
        {
            try
            {
                var stringBuilder = new StringBuilder();
                using var streamReader = new StreamReader(fileStream);
                string line = streamReader.ReadLine();
                while (line != null)
                {
                    if (string.Empty != line)
                        stringBuilder.AppendLine(line);

                    line = streamReader.ReadLine();
                }

                _content = stringBuilder.ToString();
            }
            catch (Exception)
            {
                IsValid = false;
                // ignored
            }
        }
    }
}