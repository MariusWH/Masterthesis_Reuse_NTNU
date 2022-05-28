using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA
{
    public class TrussMSA : GH_Component
    {
        // Stored Variables
        public double trussSize;
        public double maxLoad;
        public double maxDisplacement;

        //Test
        public bool firstRun;
        public Point3d startNode;
        public int returnCount;

        public TrussMSA()
          : base("Truss Matrix Structural Analysis", "Truss MSA",
              "Matrix Structural Analysis Tool for 2D and 3D Truss Systems",
              "Master", "Structural Analysis")
        {
            trussSize = -1;
            maxLoad = -1;
            maxDisplacement = -1;

            firstRun = true;
            startNode = new Point3d();
            returnCount = 0;
        }
        public TrussMSA(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {

        }
       
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("3D/2D", "3D/2D", "3D/2D", GH_ParamAccess.item, true);
            pManager.AddLineParameter("Line Geometry", "Geometry", "Geometry as list of lines", GH_ParamAccess.list);
            pManager.AddTextParameter("Bar Profiles", "Profiles", "Profile of each geometry line member as list", GH_ParamAccess.list);
            pManager.AddPointParameter("Pinned Support Points", "Supports", "Pinned support points restricted from translation but free to rotate", GH_ParamAccess.list);
            pManager.AddNumberParameter("List Loading [N]", "ListLoad", "Nodal loads by numeric values (x1, y1, x2, y2, ..)", GH_ParamAccess.list, new List<double> { 0 });
            pManager.AddVectorParameter("Vector Loading [N]", "VectorLoad", "Nodal loads by vector input", GH_ParamAccess.list, new Vector3d(0, 0, 0));
            pManager.AddNumberParameter("Line Load Value", "LL Value", "", GH_ParamAccess.item, 0);
            pManager.AddVectorParameter("Line Load Direction", "LL Direction", "", GH_ParamAccess.item, new Vector3d(0, 0, -1));
            pManager.AddVectorParameter("Line Load Distribution Direction", "LL Distribution Direction", "", GH_ParamAccess.item, new Vector3d(1, 0, 0));
            pManager.AddLineParameter("Line Load Members", "LL Members", "", GH_ParamAccess.list, new Line());
            pManager.AddBooleanParameter("Apply Self Weight", "Self Weigth", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Apply Snow Load (beta)", "Snow Load", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Apply Wind Load (beta)", "Wind Load", "", GH_ParamAccess.item, false);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Free Nodes", "Nodes", "Free nodes as list of points", GH_ParamAccess.list);
            pManager.AddMatrixParameter("Stiffness Matrix", "K", "Stiffness matrix as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Displacement Vector", "r", "Displacement vector as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Load Vector", "R", "Load vector as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Axial Forces", "Nx", "Member axial forces as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Total Utilization", "Util", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Axial Stress Utilization", "Util Nx", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Axial Buckling Utilization", "Util Buckling", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Model Data", "Model", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rank", "Rank", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Nullity", "Nullity", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Determinant", "Determinant", "", GH_ParamAccess.item);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            bool is3d = true;
            List<Line> iLines = new List<Line>();
            List<string> iProfiles = new List<string>();
            List<Point3d> iSupports = new List<Point3d>();
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
            DA.GetDataList(3, iSupports);
            DA.GetDataList(4, iLoad);
            DA.GetDataList(5, iLoadVecs);
            DA.GetData(6, ref iLineLoadValue);
            DA.GetData(7, ref iLineLoadDirection);
            DA.GetData(8, ref iLineLoadDistribution);
            DA.GetDataList(9, iLinesToLoad);
            DA.GetData(10, ref applySelfWeight);

            // CODE
            SpatialTruss truss;
            switch (is3d)
            {
                default:
                    truss = new SpatialTruss(iLines, iProfiles, iSupports);
                    break;
                case false:
                    truss = new PlanarTruss(iLines, iProfiles, iSupports);
                    break;
            }
            
            truss.ApplyNodalLoads(iLoad, iLoadVecs);
            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            if (applySelfWeight)
            {
                truss.ApplySelfWeight();
            }

            double rank = truss.GetStiffnessMartrixRank();
            double nullity = truss.GetStiffnessMartrixNullity();
            double determinant = truss.GetStiffnessMatrixDeterminant();
            truss.Solve();
            truss.Retracking();
                        

            // OUTPUT
            DA.SetDataList(0, truss.FreeNodes);
            DA.SetData(1, truss.GetStiffnessMatrix());
            DA.SetData(2, truss.GetDisplacementVector());
            DA.SetData(3, truss.GetLoadVector());
            DA.SetData(4, ElementCollection.ListToRhinoMatrix(truss.ElementAxialForcesX));
            DA.SetData(5, truss.GetTotalUtilization());
            DA.SetData(6, truss.GetAxialForceUtilization());
            DA.SetData(7, truss.GetAxialBucklingUtilization());
            DA.SetData(8, truss);
            DA.SetData(9, rank);
            DA.SetData(10, nullity);
            DA.SetData(11, determinant);

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.TrussMSA;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("DC35EDA8-72CC-45ED-A755-DF28A9EFB877"); }
        }
    }
}