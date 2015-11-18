using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree
{
    [Serializable]
    public class Tree
    {
        public Tree()
        {
            Root = new Node();
        }

        private Node root;
        public Node Root 
        {
            get 
            {
                return root;
            }
            set 
            {
                root = value;
            }
        }

        public static int GetDepth(Node node)
        {
            int depth = 0;
            GetDepthRecurively(node, ref depth);
            return depth;
        }

        private static void GetDepthRecurively(Node node, ref int depth)
        {
            if (node.Children.Count > 0)
            {
                depth += 1;
                GetDepthRecurively(node.Children.FirstOrDefault(), ref depth);
            }
        }

        private List<Node> circleNodes = new List<Node>();
        public List<Node> GetCircles()
        {
            if(circleNodes == null || circleNodes.Count<=0)
            {
                FindCircle(Root);
            }
            return circleNodes;
        }

        private void FindCircle(Node currentNode)
        {
            if (currentNode.HasChildren())
            {
                var nodes = currentNode.Children.Where(c => c.ClaimInfo.VehicleRegistrationNumber == Root.ClaimInfo.VehicleRegistrationNumber);
                if (nodes.Any())
                {
                    circleNodes.AddRange(nodes);
                }
                foreach (var child in currentNode.Children)
                {
                    FindCircle(child);
                }
            }
        }
        
        private void Print(Node currentNode, ref StringBuilder sb)
        {
            if (!currentNode.HasParent())
            {
                sb.AppendLine(currentNode.ClaimInfo.ToString());
            }

            if(currentNode.HasChildren())
            {
                foreach (var child in currentNode.Children)
                {
                    for (int i = 0; i < (Tree.GetDepth(Root) - Tree.GetDepth(child)); i++)
                    {
                        sb.Append("    ");
                    }
                    sb.AppendLine("|--" + child.ClaimInfo.ToString());
                    Print(child, ref sb);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Print(root, ref sb);
            return sb.ToString();
        }
    }
}
