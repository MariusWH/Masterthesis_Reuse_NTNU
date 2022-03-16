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
        public int returnCount;

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
            returnCount = 0;
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
            pManager.AddGeometryParameter("Visuals", "Visuals", "", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Colors", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Circle", "Circle", "", GH_ParamAccess.item);

            pManager.AddGenericParameter("Debug1", "Debug1", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug2", "Debug2", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug3", "Debug3", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug4", "Debug4", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug5", "Debug5", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug6", "Debug6", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug7", "Debug7", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug8", "Debug8", "", GH_ParamAccess.item);
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




            // TEST

            //truss.getExposedMembers(iLineLoadDirection, out Point3d topPoint, out List<Line> exposedLines);
                     
            List<Point3d> firstPoints = truss.FindFirstPanel(iLineLoadDirection, 1500);
            truss.GiftWrapLoadPanels(iLineLoadDirection, 
                out List<Brep> liveVisuals, out List<System.Drawing.Color> liveColors,
                out List<Brep> newVisuals, out List<System.Drawing.Color> newColors,
                out List<Brep> visuals, out List<System.Drawing.Color> colors, 
                out Circle circle,
                out List<Line> edges,
                out List<Line> newEdges,
                returnCount++);





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
            DA.SetDataList(7, visuals);
            DA.SetDataList(8, colors);
            DA.SetData(9, circle);

            DA.SetDataList(10, edges);
            DA.SetDataList(11, newEdges);

            DA.SetDataList(12, liveVisuals);
            DA.SetDataList(13, liveColors);
            DA.SetDataList(14, newVisuals);
            DA.SetDataList(15, newColors);
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