using EmbeddedDebugger.DebugProtocol;
using NLog;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void AddValueToBTree(Register register, double value, double timeStamp)
        {
            if (theTrees.ContainsKey(register))
            {
                this.theTrees[register].AddPoint(timeStamp, value);
            }
        }
    }

    // TODO Please make sure the classes are in different files, preferable in one folder in the model: BTree or some other nice name 

    public class Btree
    {
        Random randNum = new Random(); //test purposes

        public const int leafSize = 3; //amount of points in leafnode
        public const int nodeSize = 3; //amount of childnodes in node
        public const int displayPointSize = 150; //
        int rootNodeLevel = 2; //

        double testCounter = 1;

        Node rootNode;

        bool bTreeReady = false;

        public Btree()
        {
            //initialize btree
            rootNode = new IntermediateNode(0);
            Node new_leaf = new LeafNode();
            rootNode.AppendNode(new_leaf, 0);

            //demodata to test btree
            FillDemoData();
            bTreeReady = true;
        }

        /// <summary>
        /// fill btree with dummy data
        /// </summary>
        private void FillDemoData()
        {
            DateTime dateTimeNow = DateTime.Now;
            var dateTimeOffset = new DateTimeOffset(dateTimeNow);
            var unixDateTime = dateTimeOffset.ToUnixTimeMilliseconds();
            //long unixDateTime = 1;

            for (testCounter = unixDateTime; testCounter < unixDateTime + 100000; testCounter++)
            {
                AddPoint(testCounter, randNum.Next(-10, 40) + (Math.Cos(testCounter / 50) * 40) + (Math.Sin(testCounter / 1000) * 10) + 80);
            }
            bTreeReady = true;
        }


        public bool BTreeReady()
        {
            return bTreeReady;
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
                Debug.Write("Making new rootnode on level: ");
                Debug.WriteLine(rootNodeLevel.ToString());
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
            NodeVariables rootNodeValues = rootNode.GetValues();
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
        public List<NodeVariables> GetData(double minX, double maxX)
        {
            List<Node> results = new List<Node>();

            results.AddRange(rootNode.GetChildNodesInRange(minX, maxX));

            while (results.Count < displayPointSize && results.Count != 0 && results != null)
            {
                int j = results.Count;
                List<Node> tempList = new List<Node>();
                tempList.AddRange(results);
                results.Clear();
                NodeVariables values = tempList[0].GetValues();
                if (values.currentNodeLevel != -1)
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

            Debug.Write("Returning points: ");
            Debug.WriteLine(results.Count.ToString());

            List<NodeVariables> returnList = new List<NodeVariables>();
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
        abstract public NodeVariables GetValues();
    }

    public class NodeVariables
    {
        public double yAvg;
        public double yMin;
        public double yMax;
        public double xAvg;
        public double xMin;
        public double xMax;
        public int currentNodeLevel;
        public double totalPointCount; //total amount of points in this node, this includes points in childnodes
        public int nodeCount; //amount of nodes or points in this node
        public double[,] leafPoints;
    }

    public class IntermediateNode : Node
    {
        List<Node> nodeList = new List<Node>();

        NodeVariables localNodeVariables = new NodeVariables();

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

        public override NodeVariables GetValues()
        {
            return localNodeVariables;
        }



        /// <summary>
        /// calculates averages in for current node
        /// </summary>
        public void CalculateAverages()
        {
            NodeVariables recentNodeValues = nodeList[nodeList.Count - 1].GetValues();
            if (nodeList.Count == 1)
            {
                NodeVariables values = nodeList[0].GetValues();
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
                    NodeVariables values = nodeList[i].GetValues();
                    tempCountX = tempCountX + values.nodeCount;
                    tempX = tempX + (values.xAvg * values.nodeCount);
                }
                localNodeVariables.xAvg = tempX / tempCountX;


                //update y average
                double tempY = 0;
                double tempCountY = 0;
                for (int i = 0; i < nodeList.Count; i++)
                {
                    NodeVariables values = nodeList[i].GetValues();
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
                NodeVariables tempValues = nodeList[i].GetValues();
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
        NodeVariables localNodeVariables = new NodeVariables();

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
            localNodeVariables.currentNodeLevel = -1;
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
            Debug.WriteLine("Individual points not implemented yet");
            return null;
        }

        public override NodeVariables GetValues()
        {
            return localNodeVariables;
        }
    }
}
