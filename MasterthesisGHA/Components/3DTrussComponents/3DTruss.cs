using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA
{
    public class Truss3D : GH_Component
    {
        public Truss3D()
          : base("3D Truss Analysis", "3D Truss",
              "Minimal Finite Element Analysis of 3D Truss",
              "Master", "FEA")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Input Lines", "Lines", "Lines for creating truss geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Input Profiles", "Profiles", "Profiles of members", GH_ParamAccess.list);
            pManager.AddPointParameter("Input Anchored Points", "Anchored", "Support points restricted from translation but free to rotate", GH_ParamAccess.list);
            pManager.AddNumberParameter("Nodal Loading [N]", "Load", "Nodal loads by numeric values (x1, y1, x2, y2, ..)", GH_ParamAccess.list, new List<double> { 0 });
            pManager.AddVectorParameter("Nodal Loading [N]", "vecLoad", "Nodal loads by vector input", GH_ParamAccess.list, new Vector3d(0, 0, 0));
            pManager.AddNumberParameter("Load Value", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Distribution Direction", "", "", GH_ParamAccess.item);
            pManager.AddLineParameter("Elements", "", "", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("#Elements", "#Elements", "#Elements", GH_ParamAccess.item);
            pManager.AddNumberParameter("#Nodes", "#Nodes", "#Nodes", GH_ParamAccess.item);
            pManager.AddMatrixParameter("K", "K", "K", GH_ParamAccess.item);
            pManager.AddMatrixParameter("r", "r", "r", GH_ParamAccess.item);
            pManager.AddMatrixParameter("R", "R", "R", GH_ParamAccess.item);
            pManager.AddNumberParameter("N", "N", "N", GH_ParamAccess.list);
            pManager.AddPointParameter("Nodes", "Nodes", "Nodes", GH_ParamAccess.list);
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
            TrussModel3D truss3D = new TrussModel3D(iLines, iProfiles, iAnchoredPoints);
            truss3D.ApplyNodalLoads(iLoad, iLoadVecs);
            truss3D.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss3D.Solve();
            truss3D.Retracking();
            truss3D.GetResultVisuals();
            truss3D.GetLoadVisuals();


            // OUTPUT
            DA.SetData("#Elements", truss3D.ElementsInStructure.Count);
            DA.SetData("#Nodes", truss3D.FreeNodes.Count);
            DA.SetData("K", truss3D.GetStiffnessMatrix());
            DA.SetData("r", truss3D.GetDisplacementVector());
            DA.SetData("R", truss3D.GetLoadVector());
            DA.SetDataList("N", truss3D.AxialElementLoad);
            DA.SetDataList("Nodes", truss3D.FreeNodes);
            DA.SetDataList("Geometry", truss3D.StructureVisuals);
            DA.SetDataList("Util", truss3D.StructureColors);
            DA.SetDataList("Elements", truss3D.ElementsInStructure);
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