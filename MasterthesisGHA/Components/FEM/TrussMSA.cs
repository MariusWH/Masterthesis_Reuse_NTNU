using System;
using System.Collections.Generic;

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

        public TrussSolver()
          : base("Truss Matrix Structural Analysis", "Truss MSA",
              "Matrix Structural Analysis Tool for 2D and 3D Truss Systems",
              "Master", "FEM")
        {
            trussSize = -1;
            maxLoad = -1;
            maxDisplacement = -1;
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
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Free Nodes", "Nodes", "Free nodes as list of points", GH_ParamAccess.list);
            pManager.AddMatrixParameter("Stiffness Matrix", "K", "Stiffness matrix as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Displacement Vector", "r", "Displacement vector as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Load Vector", "R", "Load vector as matrix", GH_ParamAccess.item);
            pManager.AddNumberParameter("Axial Forces", "N", "Member axial forces as list of values", GH_ParamAccess.list);           
            pManager.AddGenericParameter("Model Data", "Model", "", GH_ParamAccess.item);

        }
        protected virtual void SetInputs(IGH_DataAccess DA, out bool is3d, out List<Line> iLines, out List<string> iProfiles, out List<Point3d> iAnchoredPoints, out List<double> iLoad,
           out List<Vector3d> iLoadVecs, out double iLineLoadValue, out Vector3d iLineLoadDirection, out Vector3d iLineLoadDistribution, out List<Line> iLinesToLoad)
        {
            is3d = true;
            iLines = new List<Line>();
            iProfiles = new List<string>();
            iAnchoredPoints = new List<Point3d>();
            iLoad = new List<double>();
            iLoadVecs = new List<Vector3d>();
            iLineLoadValue = 0;
            iLineLoadDirection = new Vector3d();
            iLineLoadDistribution = new Vector3d();
            iLinesToLoad = new List<Line>();

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
        }
        protected virtual void SetOutputs(IGH_DataAccess DA, Structure truss)
        {
            
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            SetInputs(DA, out bool is3d, out List<Line> iLines, out List<string> iProfiles, out List<Point3d> iAnchoredPoints, out List<double> iLoad,
            out List<Vector3d> iLoadVecs, out double iLineLoadValue, out Vector3d iLineLoadDirection, out Vector3d iLineLoadDistribution, 
            out List<Line> iLinesToLoad);


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
            truss.Solve();
            truss.Retracking();



            // OUTPUT
            DA.SetDataList(0, truss.FreeNodes);
            DA.SetData(1, truss.GetStiffnessMatrix());
            DA.SetData(2, truss.GetDisplacementVector());
            DA.SetData(3, truss.GetLoadVector());
            DA.SetDataList(4, truss.ElementAxialForce);
            DA.SetData(5, truss);
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