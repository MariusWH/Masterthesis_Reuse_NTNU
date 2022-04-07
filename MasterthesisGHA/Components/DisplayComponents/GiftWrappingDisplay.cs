using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA
{
    public class GiftWrappingDisplay : GH_Component
    {
        // Stored Variables
        public double trussSize;
        public double maxLoad;
        public double maxDisplacement;
        List<Triangle3d> loadPanels;

        //Test
        public bool firstRun;
        public Point3d startNode;
        public int returnCount;
        List<Brep> visuals;
        List<System.Drawing.Color> colors;
        Circle circle;
        List<Line> edges;
        List<Line> newEdges;
        List<Line> tempLines;
        List<Point3d> closePoints;
        List<Brep> liveVisuals;
        List<System.Drawing.Color> liveColors;


        public GiftWrappingDisplay()
          : base("Gift Wrapping Display", "Gift Wrapping",
              "",
              "Master", "FEM")
        {
            trussSize = -1;
            maxLoad = -1;
            maxDisplacement = -1;

            firstRun = true;
            startNode = new Point3d();
            returnCount = 0;
            
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
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
            pManager.AddBooleanParameter("Apply Snow Load", "Snow Load", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Apply Wind Load", "Wind Load", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Wind Load Direction", "Wind Direction", "", GH_ParamAccess.item, new Vector3d(0,-1,0));
            
            pManager.AddIntegerParameter("Debugging Steps", "Steps", "", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Free Nodes", "Nodes", "Free nodes as list of points", GH_ParamAccess.list);
            pManager.AddMatrixParameter("Stiffness Matrix", "K", "Stiffness matrix as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Displacement Vector", "r", "Displacement vector as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Load Vector", "R", "Load vector as matrix", GH_ParamAccess.item);
            pManager.AddNumberParameter("Axial Forces", "N", "Member axial forces as list of values", GH_ParamAccess.list);
            pManager.AddGenericParameter("Model Data", "Model", "", GH_ParamAccess.item);

            pManager.AddPointParameter("FirstPoints", "FirstPoints", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Circle", "Circle", "", GH_ParamAccess.item);

            pManager.AddGenericParameter("Debug1", "Debug1", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug2", "Debug2", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug3", "Debug3", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug4", "Debug4", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug5", "Debug5", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug6", "Debug6", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug7", "Debug7", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug8", "Debug8", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug9", "Debug9", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug10", "Debug10", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug11", "Debug11", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Debug12", "Debug12", "", GH_ParamAccess.item);
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
            bool applySnowLoad = false;
            bool applyWindLoad = false;
            Vector3d windLoadDirection = new Vector3d();


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
            DA.GetData(11, ref applySnowLoad);
            DA.GetData(12, ref applyWindLoad);
            DA.GetData(13, ref windLoadDirection);
            DA.GetData(14, ref returnCount);


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





            //if (firstRun)
            
                loadPanels = truss.GiftWrapLoadPanels(
                    iLineLoadDirection,
                    out visuals,
                    out colors,
                    out circle,
                    out edges,
                    out newEdges,
                    out tempLines,
                    out closePoints,
                    returnCount++,
                    out liveVisuals,
                    out liveColors);

                firstRun = false;
            
            

            truss.GlobalLoadVector.Clear();
            truss.ApplyNodalLoads(iLoad, iLoadVecs);
            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            
            if (applySelfWeight)
                truss.ApplySelfWeight();
            if (applySnowLoad)
                truss.ApplySnowLoadOnPanels(loadPanels);
            if (applyWindLoad)
                truss.ApplyWindLoadOnPanels(loadPanels, windLoadDirection);



            truss.Solve();
            truss.Retracking();



            // OUTPUT
            DA.SetDataList(0, truss.FreeNodes);
            DA.SetData(1, truss.GetStiffnessMatrix());
            DA.SetData(2, truss.GetDisplacementVector());
            DA.SetData(3, truss.GetLoadVector());
            DA.SetDataList(4, truss.ElementAxialForce);
            DA.SetData(5, truss);

            DA.SetData(7, circle);

            DA.SetDataList(8, edges);
            DA.SetDataList(9, newEdges);
            DA.SetDataList(10, tempLines);

            DA.SetDataList(15, visuals);
            DA.SetDataList(16, colors);
            DA.SetDataList(17, liveVisuals);
            DA.SetDataList(18, liveColors);

            DA.SetDataList(19, closePoints);
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
            get { return new Guid("3AD2ED4B-0211-4498-A6C4-C13D51A4CA3C"); }
        }
    }
}