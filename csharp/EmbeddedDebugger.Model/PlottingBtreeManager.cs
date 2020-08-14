using EmbeddedDebugger.DebugProtocol;
using NLog;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedDebugger.Model
{
    public class PlottingBtreeManager
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<Register, Btree> theTrees;

        public PlottingBtreeManager()
        {
            this.theTrees = new Dictionary<Register, Btree>();
        }

        public void AddBTree(Register register)
        {
            if (!theTrees.ContainsKey(register))
            {
                this.theTrees.Add(register, new Btree());
                Logger.Trace($"Added BTree for register: {register}");
            }
        }

        public void RemoveBTree(Register register)
        {
            if (theTrees.ContainsKey(register))
            {
                this.theTrees.Remove(register);
                Logger.Trace($"Removed BTree for register: {register}");
            }
        }

        public void AddValueToBTree(Register register, object value, double timeStamp)
        {
            if (this.theTrees.ContainsKey(register))
            {
                this.theTrees[register].AddPoint(timeStamp, Convert.ToDouble(value));
            }
        }

        /// <summary>
        /// Gets min and max x value of all btree's, used to reset the plotview
        /// </summary>
        /// <returns>Min and max x value in all btrees</returns>
        public double[] getMinMax()
        {
            double[] returnMinMax = new double[2];
            int i = 0;
            foreach (Btree entry in theTrees.Values)
            {
                double[] temp = entry.GetBtreeMinMax();

                if (returnMinMax[0] > temp[0] || i == 0)
                {
                    returnMinMax[0] = temp[0];
                }
                if (returnMinMax[1] < temp[1])
                {
                    returnMinMax[1] = temp[1];
                }
                i++;
            }
            return returnMinMax;
        }

        /// <summary>
        /// Get data of all btrees in a specific window
        /// </summary>
        /// <param name="minX">x axis minimum</param>
        /// <param name="maxX">x axis maximum</param>
        /// <returns>dictorinary of </returns>
        public Dictionary<Register, List<NodeStatistics>> GetData(double minX, double maxX)
        {
            Dictionary<Register, List<NodeStatistics>> returnData = new Dictionary<Register, List<NodeStatistics>>();
            foreach (KeyValuePair<Register, Btree> entry in theTrees)
            {
                List<NodeStatistics> templist = entry.Value.GetData(minX, maxX);
                Register tempregister = entry.Key;
                returnData.Add(entry.Key, templist);
            }
            return returnData;
        }
    }

    // TODO Please make sure the classes are in different files, preferable in one folder in the model: BTree or some other nice name 

    public class Btree
    {
        public const int leafSize = 4; //amount of points in leafnode
        public const int nodeSize = 4; //amount of childnodes in node
        public const int displayPointSize = 100; //how many points to display at least
        int rootNodeLevel = 2; //

        Node rootNode;

        public Btree()
        {
            //initialize btree
            rootNode = new IntermediateNode(1);
            Node new_leaf = new LeafNode();
            rootNode.AppendNode(new_leaf, 1);
        }

        /// <summary>
        /// Add a single data point to the Btree
        /// </summary>
        /// <param name="timestamp">current timestamp</param>
        /// <param name="yValue"></param>
        public void AddPoint(double timestamp, double yValue)
        {

            var root_sibling = rootNode.AppendPoint(timestamp, yValue);
            if (root_sibling != null)
            {
                rootNodeLevel++;
                Node new_root = new IntermediateNode(rootNodeLevel);
                new_root.AppendNode(rootNode, rootNodeLevel - 1); //Adds old rootnode to the new rootnode as a childnode. 
                new_root.AppendNode(root_sibling, rootNodeLevel - 1);
                rootNode = new_root;
            }
        }

        /// <summary>
        /// Gets the x min and max value of a btree, used for resetting the view/zoom fit
        /// </summary>
        /// <returns>X min max of the current btree | 0 = Min | 1 = Max</returns>
        public double[] GetBtreeMinMax()
        {
            NodeStatistics rootNodeValues = rootNode.GetValues();
            double[] returnValues = new double[2];
            returnValues[0] = rootNodeValues.xMin;
            returnValues[1] = rootNodeValues.xMax;
            return returnValues;
        }

        /// <summary>
        /// Gets data from the btree in a specific x min and max. First it gets the rootnode, and then goes down the btree until it has enough points according to "displayPointSize"
        /// </summary>
        /// <param name="minX">min display value</param>
        /// <param name="maxX">max display value</param>
        /// <returns>List of NodeVariables, this also includes leafPoint at the leafNode level</returns>
        public List<NodeStatistics> GetData(double minX, double maxX)
        {
            List<Node> results = new List<Node>();

            results.AddRange(rootNode.GetChildNodesInRange(minX, maxX));

            while (results.Count < displayPointSize && results.Count != 0 && results != null)
            {
                int j = results.Count;
                List<Node> tempList = new List<Node>();
                tempList.AddRange(results);
                results.Clear();
                NodeStatistics values = tempList[0].GetValues();
                if (values.currentNodeLevel != 0)
                {
                    for (int i = 0; i < j; i++)
                    {
                        results.AddRange(tempList[i].GetChildNodesInRange(minX, maxX));
                    }
                }
                else
                {
                    break;
                }
            }
            if (results.Count == 0)
            {
                return null;
            }

            List<NodeStatistics> returnList = new List<NodeStatistics>();
            for (int i = 0; i < results.Count; i++)
            {
                returnList.Add(results[i].GetValues());
            }

            return returnList;
        }

    }


    public abstract class Node
    {
        abstract public Node AppendPoint(double timestamp, double yValue);
        abstract public void AppendNode(Node node, int rootNodeLevel);
        abstract public List<Node> GetChildNodesInRange(double minX, double maxX);
        abstract public NodeStatistics GetValues();
    }

    public class NodeStatistics
    {
        public double yAvg;
        public double yMin;
        public double yMax;
        public double xAvg;
        public double xMin;
        public double xMax;
        public int currentNodeLevel;
        public int totalPointCount; //total amount of points in this node, this includes points in childnodes
        public int nodeCount; //amount of nodes or points in this node
        public double[,] leafPoints;
    }

    public class IntermediateNode : Node
    {
        List<Node> nodeList = new List<Node>();

        NodeStatistics localNodeVariables = new NodeStatistics();

        public IntermediateNode(int nodeLevel)
        {
            localNodeVariables.currentNodeLevel = nodeLevel;
        }

        /// <summary>
        /// append nodes to the new current rootnode
        /// </summary>
        /// <param name="node">old node</param>
        /// <param name="rootNodeLevel">new level of rootnode</param>
        public override void AppendNode(Node node, int rootNodeLevel)
        {
            localNodeVariables.currentNodeLevel = rootNodeLevel; //sets nodelevel to the correct level

            nodeList.Add(node); //adds node to the current node
            localNodeVariables.nodeCount++;

            CalculateAverages();
        }

        /// <summary>
        /// append point
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="yValue"></param>
        /// <returns>node if full</returns>
        public override Node AppendPoint(double timestamp, double yValue)
        {
            var new_child_node = nodeList[nodeList.Count - 1].AppendPoint(timestamp, yValue);
            localNodeVariables.totalPointCount++;
            if (new_child_node != null)
            {
                if (nodeList.Count < Btree.nodeSize - 1)
                {
                    localNodeVariables.nodeCount++;
                    nodeList.Add(new_child_node);
                    CalculateAverages();
                    return null;
                }
                else
                {
                    //goes to node above this node
                    var new_sibling_node = new IntermediateNode(localNodeVariables.currentNodeLevel);
                    new_sibling_node.nodeList.Add(new_child_node);
                    new_sibling_node.CalculateAverages();
                    return new_sibling_node;
                }
            }
            CalculateAverages();
            return null;
        }

        public override NodeStatistics GetValues()
        {
            return localNodeVariables;
        }

        /// <summary>
        /// calculates averages in for current node
        /// </summary>
        public void CalculateAverages()
        {
            NodeStatistics recentNodeValues = nodeList[nodeList.Count - 1].GetValues();
            if (nodeList.Count == 1)
            {
                NodeStatistics values = nodeList[0].GetValues();
                localNodeVariables.yAvg = values.yAvg;
                localNodeVariables.yMin = values.yMin;
                localNodeVariables.yMax = values.yMax;
                localNodeVariables.xAvg = values.xAvg;
                localNodeVariables.xMin = values.xMin;
                localNodeVariables.xMax = values.xMax;
                localNodeVariables.totalPointCount = values.nodeCount;
            }
            else
            {
                if (localNodeVariables.yMax < recentNodeValues.yMax) //update max value
                {
                    localNodeVariables.yMax = recentNodeValues.yMax;
                }
                if (localNodeVariables.yMin > recentNodeValues.yMin) //update min value
                {
                    localNodeVariables.yMin = recentNodeValues.yMin;
                }
                localNodeVariables.xMax = recentNodeValues.xMax; //x value will always be max

                //update x average
                double tempX = 0;
                double tempCountX = 0;
                for (int i = 0; i < nodeList.Count; i++)
                {
                    NodeStatistics values = nodeList[i].GetValues();
                    tempCountX = tempCountX + values.nodeCount;
                    tempX = tempX + (values.xAvg * values.nodeCount);
                }
                localNodeVariables.xAvg = tempX / tempCountX;


                //update y average
                double tempY = 0;
                double tempCountY = 0;
                for (int i = 0; i < nodeList.Count; i++)
                {
                    NodeStatistics values = nodeList[i].GetValues();
                    tempCountY = tempCountY + values.nodeCount;
                    tempY = tempY + (values.yAvg * values.nodeCount);
                }
                localNodeVariables.yAvg = tempY / tempCountY;
            }
        }

        public override List<Node> GetChildNodesInRange(double minX, double maxX)
        {
            List<Node> returnNode = new List<Node>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                NodeStatistics tempValues = nodeList[i].GetValues();
                if (tempValues.xMin < maxX && tempValues.xMax > minX)
                {
                    returnNode.Add(nodeList[i]);
                }
            }
            return returnNode;
        }
    }

    /// <summary>
    /// class with the seperate points, level 0
    /// </summary>
    public class LeafNode : Node
    {
        NodeStatistics localNodeVariables = new NodeStatistics();

        public LeafNode()
        {
            localNodeVariables.leafPoints = new double[Btree.leafSize - 1, 2];
        }

        /// <summary>
        /// Adds point to leafNode
        /// </summary>
        /// <param name="timestamp">current timestamp</param>
        /// <param name="yValue"></param>
        /// <returns>Returns null when leafnode is not full, returns new leafnode when leafnode is full</returns>
        public override Node AppendPoint(double timestamp, double yValue)
        {
            localNodeVariables.currentNodeLevel = 0;
            if (localNodeVariables.nodeCount < Btree.leafSize - 1)
            {
                //add point to array
                localNodeVariables.leafPoints[localNodeVariables.nodeCount, 0] = timestamp;
                localNodeVariables.leafPoints[localNodeVariables.nodeCount, 1] = yValue;

                localNodeVariables.nodeCount++;

                #region calculate averages
                if (localNodeVariables.nodeCount == 1)
                {
                    //update min max avg for all values when theres just one value in the array
                    localNodeVariables.yAvg = localNodeVariables.yMax = localNodeVariables.yMin = yValue;
                    localNodeVariables.xAvg = localNodeVariables.xMax = localNodeVariables.xMin = timestamp;
                }
                else
                {
                    if (localNodeVariables.yMax < yValue) //update max value
                    {
                        localNodeVariables.yMax = yValue;
                    }
                    if (localNodeVariables.yMin > yValue) //update min value
                    {
                        localNodeVariables.yMin = yValue;
                    }
                    localNodeVariables.xMax = timestamp;

                    //update x average
                    double tempX = 0;
                    for (int i = 0; i < localNodeVariables.nodeCount; i++)
                    {
                        tempX = tempX + localNodeVariables.leafPoints[i, 0];
                    }
                    localNodeVariables.xAvg = tempX / localNodeVariables.nodeCount;

                    //update y average
                    double tempY = 0;
                    for (int i = 0; i < localNodeVariables.nodeCount; i++)
                    {
                        tempY = tempY + localNodeVariables.leafPoints[i, 1];
                    }
                    localNodeVariables.yAvg = tempY / localNodeVariables.nodeCount;
                }
                #endregion

                return null;
            }
            else
            {
                var new_leaf = new LeafNode();
                new_leaf.AppendPoint(timestamp, yValue);
                return new_leaf;
            }
        }

        public override void AppendNode(Node node, int rootNodeLevel)
        {
            //Can't append node to a LeafNode, as LeafNodes don't have nodes in them
            throw new NotImplementedException();
        }

        public override List<Node> GetChildNodesInRange(double minX, double maxX)
        {
            //No childnodes in Leafnode
            return null;
        }

        public override NodeStatistics GetValues()
        {
            return localNodeVariables;
        }
    }
}