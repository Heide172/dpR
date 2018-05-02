using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xmlSettings;
using System.Net;
namespace clSpec
{
    [Serializable]
    public class ClSpec
    {
        public XmlSet settings;
        public string responseState = "DEFAULT";
        public string ipAdr;
    }

    [Serializable]
    public class Cmd
    {
       public int cmdIndex;
       public string strCmd;
       public List<Guid> list;
       public Guid destGuid;
    }
  
}
