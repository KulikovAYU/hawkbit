using System;
using System.Collections.Generic;
using System.Xml;

namespace ForteConfigurationLoader.InnerCommandLayer
{

    public abstract class CommandBase : IRequestFb
    {
        public string Action { get;  protected set;}= "";
        public int Id { get;protected set; } = 0;
        public bool HasErrors { get; protected set; } = false;
        public string Context { get;  protected set;}
        public string Tag { get; protected set; } = "";
    }

    public class SubRequestFbCommand : CommandBase
    {
        public SubRequestFbCommand(string xmlString, string tag = "")
        {
            try
            {
                Tag = tag;
                XmlDocument doc= new XmlDocument();
                doc.LoadXml(xmlString);
              
                var idAttr = doc.DocumentElement?.SelectSingleNode("//Request[@ID]")?.Attributes?["ID"]?.Value ?? "";
                var actionAttr = doc.DocumentElement?.SelectSingleNode("//Request[@Action]")?.Attributes?["Action"]?.Value ?? "";
                
                HasErrors = string.IsNullOrEmpty(idAttr) ||
                            string.IsNullOrEmpty(actionAttr);

                int id = -1;
                if (!int.TryParse(idAttr, out id))
                {
                    HasErrors = true;
                    return;
                }

                Id = id;
                Action = actionAttr;
                Context = xmlString;
            }
            catch (Exception)
            {
                HasErrors = true;
            }
        }
    }
    
    public class QueryFbCommand : CommandBase
    {
        public QueryFbCommand(FunctionBlock fb, int id = 1)
        {
            Id = id;
            Action = "QUERY";
            Context = $"<Request ID=\"{Id}\" Action=\"{Action}\"><FB Name=\"{fb.Name}\" Type=\"{fb.Type}\"/></Request>";
        }
    }
    
    public class KillFbCommand : CommandBase
    {
        public KillFbCommand(FunctionBlock fb, int id = 1)
        {
            Id = id;
            Action = "KILL";
            Context = $"<Request ID=\"{Id}\" Action=\"{Action}\"><FB Name=\"{fb.Name}\" Type=\"{fb.Type}\"/></Request>";
        }
    }
    
    public class DeleteFbCommand : CommandBase
    {
        public DeleteFbCommand(FunctionBlock fb, int id = 1)
        {
            Id = id;
            Action = "DELETE";
            Context = $"<Request ID=\"{Id}\" Action=\"{Action}\"><FB Name=\"{fb.Name}\" Type=\"{fb.Type}\"/></Request>";
        }
    }
    
    public class StartFbCommand : CommandBase
    {
        public StartFbCommand(int id)
        {
            Id = id;
            Action = "START";
            Context = $"<Request ID=\"{Id}\" Action=\"{Action}\"/>";
        }
    }

   

    public class CommandsFbSet
    {
        public string Name { get; set; } = "";
        public List<IRequestFb> Commands { get; } = new();
    }
}