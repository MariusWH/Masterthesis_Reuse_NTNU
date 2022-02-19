using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components.MethodOne
{ 

    public class MethodOne : GH_Component
    {

        public MethodOne()
          : base("MethodOne", "MethodOne",
              "Description",
              "Master", "MethodOne")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);
            pManager.AddTextParameter("NewElements", "NewElements", "NewElements", GH_ParamAccess.list, "ALL");

            pManager.AddLineParameter("GeometryLines", "GeometryLines", "GeometryLines", GH_ParamAccess.list, new Line());
            pManager.AddPointParameter("Supports", "Supports", "Supports", GH_ParamAccess.list, new Point3d());
            pManager.AddNumberParameter("Load Value", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Distribution Direction", "", "", GH_ParamAccess.item);
            pManager.AddLineParameter("LoadLines", "LoadLines", "LoadLines", GH_ParamAccess.list);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddNumberParameter("N", "N", "N", GH_ParamAccess.list);
            pManager.AddBrepParameter("Visuals", "Visuals", "Visuals", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour", "Colour", "Colour", GH_ParamAccess.list);
            

            pManager.AddGenericParameter("ReplacementSuggestions", "", "", GH_ParamAccess.tree);

        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            MaterialBank iMaterialBank = new MaterialBank();
            List<string> iNewElementsCatalog = new List<string>();
            List<Line> iGeometryLines = new List<Line>();
            List<Point3d> iSupports = new List<Point3d>();
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();

            DA.GetData(0, ref iMaterialBank);
            DA.GetDataList(1, iNewElementsCatalog);
            DA.GetDataList(2, iGeometryLines);
            DA.GetDataList(3, iSupports);
            DA.GetData(4, ref iLineLoadValue);
            DA.GetData(5, ref iLineLoadDirection);
            DA.GetData(6, ref iLineLoadDistribution);
            DA.GetDataList(7, iLinesToLoad);

            

            // CODE
            List<string> initialProfiles = new List<string>();
            foreach (Line line in iGeometryLines)
                initialProfiles.Add("IPE100");

            TrussModel2D truss2D = new TrussModel2D(iGeometryLines, initialProfiles, iSupports);
            truss2D.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss2D.Solve();
            truss2D.Retracking();
            truss2D.GetResultVisuals();
            truss2D.GetLoadVisuals();

            List<List<StockElement>> reusablesSuggestionTree = 
                truss2D.PossibleStockElementForEachInPlaceElement(iMaterialBank);

          



            // OUTPUTS
            DA.SetData("Info", truss2D.PrintInfo());
            DA.SetDataList("N", truss2D.N_out);
            DA.SetDataList("Visuals", truss2D.StructureVisuals);
            DA.SetDataList("Colour", truss2D.StructureColors);           
            DA.SetDataTree(4, ElementCollection.GetOutputDataTree(reusablesSuggestionTree));


            

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
            get { return new Guid("DAA23F40-0F0C-4E67-ADD5-2AE99E9AFC20"); }
        }
    }
}