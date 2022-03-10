using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA
{
    public class TrussSolver : GH_Component
    {
        // Stored Variables
        public double trussSize;
        public double maxLoad;
        public double maxDisplacement;

        //Test
        public bool firstRun;
        public Point3d startNode;

        public TrussSolver()
          : base("Truss Matrix Structural Analysis", "Truss MSA",
              "Matrix Structural Analysis Tool for 2D and 3D Truss Systems",
              "Master", "FEM")
        {
            trussSize = -1;
            maxLoad = -1;
            maxDisplacement = -1;

            firstRun = true;
            startNode = new Point3d();
        }
        public TrussSolver(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {

        }
       
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("3D/2D", "3D/2D", "3D/2D", GH_ParamAccess.item, true);
            pManager.AddLineParameter("Line Geometry", "Geometry", "Geometry as list of lines", GH_ParamAccess.list);
            pManager.AddTextParameter("Bar Profiles", "Profiles", "Profile of each geometry line member as list", GH_ParamAccess.list);
            pManager.AddPointParameter("Support Points", "Supports", "Pinned support points restricted from translation but free to rotate", GH_ParamAccess.list);
            pManager.AddNumberParameter("List Loading [N]", "ListLoad", "Nodal loads by numeric values (x1, y1, x2, y2, ..)", GH_ParamAccess.list, new List<double> { 0 });
            pManager.AddVectorParameter("Vector Loading [N]", "VectorLoad", "Nodal loads by vector input", GH_ParamAccess.list, new Vector3d(0, 0, 0));
            pManager.AddNumberParameter("Line Load Value", "LL Value", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Line Load Direction", "LL Direction", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Line Load Distribution Direction", "LL Distribution Direction", "", GH_ParamAccess.item);
            pManager.AddLineParameter("Line Load Members", "LL Members", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Apply Self Weight", "Self Weigth", "", GH_ParamAccess.item);

            
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Free Nodes", "Nodes", "Free nodes as list of points", GH_ParamAccess.list);
            pManager.AddMatrixParameter("Stiffness Matrix", "K", "Stiffness matrix as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Displacement Vector", "r", "Displacement vector as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Load Vector", "R", "Load vector as matrix", GH_ParamAccess.item);
            pManager.AddNumberParameter("Axial Forces", "N", "Member axial forces as list of values", GH_ParamAccess.list);           
            pManager.AddGenericParameter("Model Data", "Model", "", GH_ParamAccess.item);

            pManager.AddPointParameter("FirstPoints", "FirstPoints", "", GH_ParamAccess.list);
            pManager.AddLineParameter("Exposed", "Exposed", "", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            bool is3d = true;
            List<Line> iLines = new List<Line>();
            List<string> iProfiles = new List<string>();
            List<Point3d> iAnchoredPoints = new List<Point3d>();
            List<double> iLoad = new List<double>();
            List<Vector3d> iLoadVecs = new List<Vector3d>();
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();
            bool applySelfWeight = false;

            DA.GetData(0, ref is3d);
            DA.GetDataList(1, iLines);
            DA.GetDataList(2, iProfiles);
            DA.GetDataList(3, iAnchoredPoints);
            DA.GetDataList(4, iLoad);
            DA.GetDataList(5, iLoadVecs);
            DA.GetData(6, ref iLineLoadValue);
            DA.GetData(7, ref iLineLoadDirection);
            DA.GetData(8, ref iLineLoadDistribution);
            DA.GetDataList(9, iLinesToLoad);
            DA.GetData(10, ref applySelfWeight);


            // CODE
            TrussModel3D truss;
            switch (is3d)
            {
                default:
                    truss = new TrussModel3D(iLines, iProfiles, iAnchoredPoints);
                    break;
                case false:
                    truss = new TrussModel2D(iLines, iProfiles, iAnchoredPoints);
                    break;
            }
            
            truss.ApplyNodalLoads(iLoad, iLoadVecs);
            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            if (applySelfWeight)
            {
                truss.ApplySelfWeight();
            }




            /*

            // --- Test ---
            Vector3d loadDirection = iLineLoadDirection;
            List<Point3d> nodes = truss.FreeNodes;
            Point3d topPoint = nodes[0];


            // Find top-point
            if (firstRun)
            {
                firstRun = false;

                Plane zeroPlane = new Plane(new Point3d(0, 0, 0), loadDirection);                
                foreach (Point3d node in nodes)
                {
                    if (zeroPlane.DistanceTo(topPoint) < zeroPlane.DistanceTo(node))
                        topPoint = node;
                }
                
                Point3d startNode = topPoint;
            }


            

            // Exposed Lines
            List<Line> exposedLines = new List<Line>();

            // Initialize
            IEnumerable<InPlaceElement> allElements = truss.ElementsInStructure.ToList();
            List<Line> exposedLinesCopy = exposedLines.ToList();

            // Find neighboors
            IEnumerable<InPlaceElement> neighbors = allElements
                .Where(o => o.StartPoint == startNode || o.EndPoint == startNode)
                .ToList();
            
            List<Line> newNeighborLines = neighbors
                .Select(o => new Line(o.StartPoint, o.EndPoint))
                .Where(o => !exposedLinesCopy.Contains(new Line(o.PointAt(0), o.PointAt(1))))
                .Where(o => !exposedLinesCopy.Contains(new Line(o.PointAt(1), o.PointAt(0))))
                .ToList();

            // Delete hidden from previous line
            Point3d nextNode;
            for (int i = 0; i < newNeighborLines.Count; i++)
            {
                Line line = newNeighborLines[i];

                if (line.PointAt(0) == startNode)
                    nextNode = line.PointAt(1);
                else if (line.PointAt(1) == startNode)
                    nextNode = line.PointAt(0);
                else
                    throw new Exception("Line " + line.ToString() + " is not connected to node " + startNode.ToString());

                Line prevLine = new Line(prevNode, startNode);
                Line thisLine = new Line(startNode, nextNode);


                if (!firstRun)
                {
                    Plane projectionPlane = new Plane(startNode, loadDirection, new Vector3d(startNode - prevNode));
                    Point3d projectedNextNode = projectionPlane.ClosestPoint(nextNode);

                    double anglePreviousMember = Vector3d.VectorAngle(loadDirection, new Vector3d(startNode - prevNode));
                    double angleThisMember = Vector3d.VectorAngle(loadDirection, new Vector3d(projectedNextNode - startNode));

                    if (anglePreviousMember <= Math.PI && angleThisMember >= Math.PI ||
                        anglePreviousMember >= Math.PI && angleThisMember <= Math.PI)
                    {
                        newNeighborLines.RemoveAt(i);
                    }
                }
            }


            exposedLines.AddRange(newNeighborLines);

            prevNode = startNode;
            if (newNeighborLines.Count > 0)
            {
                foreach (Line line in newNeighborLines)
                {
                    if (line.PointAt(0) != startNode)
                    {
                        FindExposedNodes(prevNode, line.PointAt(0), loadDirection, ref exposedLines, false);
                    }
                    else if (line.PointAt(1) != startNode)
                    {
                        FindExposedNodes(prevNode, line.PointAt(1), loadDirection, ref exposedLines, false);
                    }
                }
            }
            else
            {
                return;
            }

            */


            //truss.getExposedMembers(iLineLoadDirection, out Point3d topPoint, out List<Line> exposedLines);
            
            
            List<Point3d> firstPoints = truss.FindFirstPanel(iLineLoadDirection, 1500);










            
            truss.Solve();
            truss.Retracking();
            


            // OUTPUT
            DA.SetDataList(0, truss.FreeNodes);
            DA.SetData(1, truss.GetStiffnessMatrix());
            DA.SetData(2, truss.GetDisplacementVector());
            DA.SetData(3, truss.GetLoadVector());
            DA.SetDataList(4, truss.ElementAxialForce);
            DA.SetData(5, truss);

            DA.SetDataList(6, firstPoints);
            //DA.SetDataList(7, exposedLines);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources._3D_Truss_Icon;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("DC35EDA8-72CC-45ED-A755-DF28A9EFB877"); }
        }
    }
}