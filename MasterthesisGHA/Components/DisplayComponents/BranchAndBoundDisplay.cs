using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA.Components
{
    public class BranchAndBoundDisplay : GH_Component
    {
        public BranchAndBoundDisplay()
          : base("Branch&Bound Cost Martrix Reduction", "BnB",
              "Description",
              "Master", "Display")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserMaterialBank", "InsertMB", "Insert Material Bank (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserNewElements", "InsertNew", "Insert New Elements (true)", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Method", "Method", "", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("MaterialBank", "MB", "", GH_ParamAccess.item);
            pManager.AddTextParameter("NewElements", "New", "", GH_ParamAccess.list, "ALL");
            pManager.AddLineParameter("Structure Lines", "Lines", "", GH_ParamAccess.list, new Line());
            pManager.AddPointParameter("Structure Supports", "Supports", "", GH_ParamAccess.list, new Point3d());
            pManager.AddLineParameter("Load Lines", "LoadLines", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load Value", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Distribution Direction", "", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMatrixParameter("Initial Priority Matrix", "Priority Matrix", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Initial Cost Matrix", "Cost Matrix", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Solution Cost", "Cost", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Solution Path", "Path", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            bool is3D = false;
            bool insertMaterialBank = false;
            bool insertNewElements = false;
            int optimizationMethod = 0;
            MaterialBank iMaterialBank = new MaterialBank();
            List<string> iNewElementsCatalog = new List<string>();
            List<Line> iGeometryLines = new List<Line>();
            List<Point3d> iSupports = new List<Point3d>();
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();

            DA.GetData(0, ref is3D);
            DA.GetData(1, ref insertMaterialBank);
            DA.GetData(2, ref insertNewElements);
            DA.GetData(3, ref optimizationMethod);
            DA.GetData(4, ref iMaterialBank);
            DA.GetDataList(5, iNewElementsCatalog);
            DA.GetDataList(6, iGeometryLines);
            DA.GetDataList(7, iSupports);
            DA.GetDataList(8, iLinesToLoad);
            DA.GetData(9, ref iLineLoadValue);
            DA.GetData(10, ref iLineLoadDirection);
            DA.GetData(11, ref iLineLoadDistribution);



            // CODE
            List<string> initialProfiles = new List<string>();
            foreach (Line line in iGeometryLines)
                initialProfiles.Add("IPE600");

            SpatialTruss truss;
            if (!is3D)
                truss = new PlanarTruss(iGeometryLines, initialProfiles, iSupports);
            else
                truss = new SpatialTruss(iGeometryLines, initialProfiles, iSupports);

            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss.Solve();
            truss.Retracking();


            Matrix<double> priorityMatrix = truss.getPriorityMatrix(iMaterialBank);
            Node node = new Node();
            Matrix<double> costMatrix = node.getCostMatrix(priorityMatrix);
            Node solutionNode = node.Solve(costMatrix);

            List<Tuple<int,int>> solutionPath = solutionNode.path;
            double solutionCost = solutionNode.lowerBoundCost;

            string solutionPathString = "";
            foreach(Tuple<int,int> coordinate in solutionPath) solutionPathString += "["+coordinate.Item1.ToString()+","+coordinate.Item2.ToString()+"]\n";

            DA.SetData(0, ElementCollection.MathnetToRhinoMatrix(priorityMatrix));
            DA.SetData(1, ElementCollection.MathnetToRhinoMatrix(costMatrix));
            DA.SetData(2, solutionCost);
            DA.SetData(3, solutionPathString);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("D32C3A3A-07F3-48D7-B5D5-9F8ED8C24143"); }
        }
    }
}