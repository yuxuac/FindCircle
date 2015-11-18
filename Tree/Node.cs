using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree
{
    [Serializable]
    public class Node
    {
        public Claim ClaimInfo { get; set; }

        public Node Parent { get; set; }

        private List<Node> children;
        public List<Node> Children
        {
            get
            {
                if (children == null)
                {
                    children = new List<Node>();
                }
                return children;
            }

            set
            {
                if (children == null) children = new List<Node>();
                foreach (var node in value)
                {
                    node.Parent = this;
                    children.Add(node);
                }
            }
        }

        public bool HasChildren()
        {
            if (this.Children == null || this.Children.Count <= 0)
                return false;
            return true;
        }

        public bool HasParent()
        {
            if (this.Parent == null)
                return false;
            return true;
        }

        //private void AddChild(Node child)
        //{
        //    if (children == null) throw new ArgumentNullException("Parameter: 'child' can't be null.");
        //    child.Parent = this;
        //    children.Add(child);
        //}

        private StringBuilder sb = new StringBuilder();

        public string GetTraceString()
        {
            if (sb == null || sb.Length <= 0)
            {
                GetTraceTranverse(this);
            }
            return sb.ToString();
        }

        public void GetTraceTranverse(Node currentNode)
        {
            if (currentNode.HasParent())
            {
                sb.AppendLine(currentNode.ClaimInfo.ToString() + "=>");
                GetTraceTranverse(currentNode.Parent);
            }
            else
            {
                sb.AppendLine(currentNode.ClaimInfo.ToString());
            }
        }
    }
}
