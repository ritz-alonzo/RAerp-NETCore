using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Models.NavigationModel
{
    public class PluginNode
    {
        public PluginNode()
        {
            RelatedNodes = new List<PluginNode>();
        }
        public string MenuTitle { get; set; }
        public string SystemName { get; set; }
        public string Url { get; set; }
        public string IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public bool Visible { get; set; }
        // will keep this in case there's major change in navigation display
        public bool IsParentNode { get; set; }
        public string NodeId { get; set; }
        public List<PluginNode> RelatedNodes { get; set; }
    }
}
