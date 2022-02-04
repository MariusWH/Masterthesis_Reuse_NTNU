/*

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics;

namespace MasterthesisGHA
{
    internal class Node
    {
        public Point3d NodePosition = new Point3d();
        public bool Anchored;

        private static int instanceCounter;
        private readonly int instanceID;

        public static List<Point3d> nodesPositions = new List<Point3d>();
        //public static List<Element> intersectingElements = new List<Element>();



        public Node(Point3d nodePosition, bool anchored)
        {
            if (!Node.nodesPositions.Contains(nodePosition))
            {
                this.instanceID = instanceCounter++;
                NodePosition = nodePosition;
                Anchored = anchored;
            }
 
            
        }



        public int GetID()
            { return instanceID; }



    }
}

*/