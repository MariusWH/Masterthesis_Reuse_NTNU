using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;


namespace MasterthesisGHA.Components
{
    public class Truss2D : GH_Component
    {

        public Truss2D()
          : base("2D Truss Analysis", "2D Truss",
              "Minimal Finite Element Analysis of 2D Truss",
              "Master", "FEA")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Input Lines", "Lines", "Lines for creating truss geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Input Profiles", "Profiles", "Profiles of members", GH_ParamAccess.list);
            pManager.AddPointParameter("Input Anchored Points", "Anchored", "Support points restricted from translation but free to rotate", GH_ParamAccess.list);           
            pManager.AddNumberParameter("Nodal Loading [N]", "Load", "Nodal loads by numeric values (x1, y1, x2, y2, ..)", GH_ParamAccess.list, new List<double> { 0 });
            pManager.AddVectorParameter("Nodal Loading [N]", "vecLoad", "Nodal loads by vector input", GH_ParamAccess.list, new Vector3d(0,0,0));
            pManager.AddNumberParameter("Load Value","","",GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction","","",GH_ParamAccess.item);
            pManager.AddVectorParameter("Distribution Direction","","",GH_ParamAccess.item);
            pManager.AddLineParameter("Elements","","",GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("#Elements", "#Elements", "#Elements", GH_ParamAccess.item);
            pManager.AddNumberParameter("#Nodes", "#Nodes", "#Nodes", GH_ParamAccess.item);
            pManager.AddMatrixParameter("K", "K", "K", GH_ParamAccess.item);
            pManager.AddMatrixParameter("r", "r", "r", GH_ParamAccess.item);
            pManager.AddMatrixParameter("R", "R", "R", GH_ParamAccess.item);
            pManager.AddPointParameter("Nodes", "Nodes", "Nodes", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddNumberParameter("N", "N", "N", GH_ParamAccess.list);
            pManager.AddBrepParameter("Geometry", "Geometry", "Geometry", GH_ParamAccess.list);
            pManager.AddColourParameter("Util", "Util", "Util", GH_ParamAccess.list);
            pManager.AddGenericParameter("Elements", "Elememts", "Elements", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            List<Line> iLines = new List<Line>();
            List<string> iProfiles = new List<string>();
            List<Point3d> iAnchoredPoints = new List<Point3d>();
            List<double> iLoad = new List<double>();
            List<Vector3d> iLoadVecs = new List<Vector3d>();
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();

            DA.GetDataList(0, iLines);
            DA.GetDataList(1, iProfiles);
            DA.GetDataList(2, iAnchoredPoints);
            DA.GetDataList(3, iLoad);
            DA.GetDataList(4, iLoadVecs);
            DA.GetData(5, ref iLineLoadValue);
            DA.GetData(6, ref iLineLoadDirection);
            DA.GetData(7, ref iLineLoadDistribution);
            DA.GetDataList(8, iLinesToLoad);

           
            // CODE
            TrussModel2D truss2D = new TrussModel2D(iLines, iProfiles, iAnchoredPoints);
            truss2D.ApplyNodalLoads(iLoad, iLoadVecs);
            truss2D.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss2D.Solve();
            truss2D.Retracking();
            truss2D.GetResultVisuals();
            truss2D.GetLoadVisuals();
                       
            
            // OUTPUT
            DA.SetData("#Elements", truss2D.ElementsInStructure.Count);
            DA.SetData("#Nodes",truss2D.FreeNodes.Count);
            DA.SetData("K", truss2D.GetStiffnessMatrix());
            DA.SetData("r", truss2D.GetDisplacementVector());
            DA.SetData("R", truss2D.GetLoadVector());
            DA.SetDataList("Nodes", truss2D.FreeNodes);
            DA.SetDataList("N", truss2D.N_out);
            DA.SetDataList("Geometry", truss2D.StructureVisuals );
            DA.SetDataList("Util", truss2D.StructureColors);
            DA.SetDataList("Elements", truss2D.ElementsInStructure);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources._2D_Truss_Icon;
                //return null;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("1AB4E987-0B6A-4EFA-BD27-8418F7C24308"); }
        }

    }
}






