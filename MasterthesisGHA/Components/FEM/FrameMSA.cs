using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA
{
    public class FrameMSA : GH_Component
    {
        // Stored Variables
        public double structureSize;
        public double maxLoad;
        public double maxDisplacement;

        //Test
        public bool firstRun;
        public Point3d startNode;
        public int returnCount;

        public FrameMSA()
          : base("Frame Matrix Structural Analysis", "Frame MSA",
              "Matrix Structural Analysis Tool for 2D and 3D Frame Systems",
              "Master", "FEM")
        {
            structureSize = -1;
            maxLoad = -1;
            maxDisplacement = -1;

            firstRun = true;
            startNode = new Point3d();
            returnCount = 0;
        }
        public FrameMSA(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("3D/2D", "3D/2D", "3D/2D", GH_ParamAccess.item, true);
            pManager.AddLineParameter("Line Geometry", "Geometry", "Geometry as list of lines", GH_ParamAccess.list);
            pManager.AddTextParameter("Bar Profiles", "Profiles", "Profile of each geometry line member as list", GH_ParamAccess.list);
            pManager.AddPointParameter("Support Points", "Supports", "Pinned support points restricted from translation but free to rotate", GH_ParamAccess.list);
            pManager.AddNumberParameter("List Loading [N]", "ListLoad", "Nodal loads by numeric values (x1, y1, x2, y2, ..)", GH_ParamAccess.list, new List<double> { 0 });
            pManager.AddVectorParameter("Vector Loading [N]", "VectorLoad", "Nodal loads by vector input", GH_ParamAccess.list, new Vector3d(0, 0, 0));
            pManager.AddNumberParameter("Line Load Value", "LL Value", "", GH_ParamAccess.item, 0);
            pManager.AddVectorParameter("Line Load Direction", "LL Direction", "", GH_ParamAccess.item, new Vector3d(0, 0, -1));
            pManager.AddVectorParameter("Line Load Distribution Direction", "LL Distribution Direction", "", GH_ParamAccess.item, new Vector3d(1, 0, 0));
            pManager.AddLineParameter("Line Load Members", "LL Members", "", GH_ParamAccess.list, new Line());
            pManager.AddBooleanParameter("Apply Self Weight", "Self Weigth", "", GH_ParamAccess.item, false);


        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Free Nodes", "Nodes", "Free nodes as list of points", GH_ParamAccess.list);
            pManager.AddMatrixParameter("Stiffness Matrix", "K", "Stiffness matrix as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Displacement Vector", "r", "Displacement vector as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Load Vector", "R", "Load vector as matrix", GH_ParamAccess.item);
            pManager.AddNumberParameter("Axial Forces X", "Nx", "Member axial forces as list of values", GH_ParamAccess.list);
            pManager.AddNumberParameter("Shear Forces Y", "Vy", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Shear Forces Z", "Vz", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Torsions X", "Tx", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Moments Y", "My", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Moments Z", "Mz", "", GH_ParamAccess.list);
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
            SpatialFrame frame;
            switch (is3d)
            {
                default:
                    frame = new SpatialFrame(iLines, iProfiles, iSupports);
                    break;
                case false:
                    throw new NotImplementedException();
            }

            frame.ApplyNodalLoads(iLoad, iLoadVecs);
            frame.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            if (applySelfWeight) frame.ApplySelfWeight();

            double rank = frame.GetStiffnessMartrixRank();
            double nullity = frame.GetStiffnessMartrixNullity();
            double determinant = frame.GetStiffnessMatrixDeterminant();
            frame.Solve();
            frame.Retracking();


            // OUTPUT
            DA.SetDataList(0, frame.FreeNodes);
            DA.SetData(1, frame.GetStiffnessMatrix());
            DA.SetData(2, frame.GetDisplacementVector());
            DA.SetData(3, frame.GetLoadVector());
            DA.SetDataList(4, frame.ElementAxialForcesX);
            DA.SetDataList(5, frame.ElementShearForcesY);
            DA.SetDataList(6, frame.ElementShearForcesZ);
            DA.SetDataList(7, frame.ElementTorsionsX);
            DA.SetDataList(8, frame.ElementMomentsY);
            DA.SetDataList(9, frame.ElementMomentsZ);
            DA.SetData(10, frame);
            DA.SetData(11, rank);
            DA.SetData(12, nullity);
            DA.SetData(13, determinant);

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
            get { return new Guid("78BD548E-A6B1-4C40-B9BE-DC638802CF6B"); }
        }
    }
}