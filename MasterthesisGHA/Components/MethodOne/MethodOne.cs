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
            pManager.AddBooleanParameter("InserMaterialBank", "Insert", "Insert", GH_ParamAccess.item);

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

            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddBrepParameter("StockVisuals", "StockVisuals", "StockVisuals", GH_ParamAccess.list);
            pManager.AddColourParameter("StockColour", "StockColour", "StockColour", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            bool insert = false;
            MaterialBank iMaterialBank = new MaterialBank();
            List<string> iNewElementsCatalog = new List<string>();
            List<Line> iGeometryLines = new List<Line>();
            List<Point3d> iSupports = new List<Point3d>();
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();

            DA.GetData(0, ref insert);
            DA.GetData(1, ref iMaterialBank);
            DA.GetDataList(2, iNewElementsCatalog);
            DA.GetDataList(3, iGeometryLines);
            DA.GetDataList(4, iSupports);
            DA.GetData(5, ref iLineLoadValue);
            DA.GetData(6, ref iLineLoadDirection);
            DA.GetData(7, ref iLineLoadDistribution);
            DA.GetDataList(8, iLinesToLoad);

            

            // CODE
            List<string> initialProfiles = new List<string>();
            foreach (Line line in iGeometryLines)
                initialProfiles.Add("IPE100");

            TrussModel2D truss2D = new TrussModel2D(iGeometryLines, initialProfiles, iSupports);
            truss2D.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss2D.Solve();
            truss2D.Retracking();


            MaterialBank inputMaterialBank = iMaterialBank.DeepCopy();
            MaterialBank outMaterialBank;
            
            if (insert)
                truss2D.InsertMaterialBank( inputMaterialBank, out outMaterialBank);
            else
            {
                outMaterialBank = iMaterialBank.DeepCopy();
                outMaterialBank.ResetMaterialBank();
                outMaterialBank.UpdateVisuals();
            }
                

            truss2D.Solve();
            truss2D.Retracking();
            truss2D.GetResultVisuals();
            truss2D.GetLoadVisuals();

            
            



            // OUTPUTS
            DA.SetData("Info", truss2D.PrintStructureInfo());
            DA.SetDataList("N", truss2D.ElementUtilization);
            DA.SetDataList("Visuals", truss2D.StructureVisuals);
            DA.SetDataList("Colour", truss2D.StructureColors);           
            //DA.SetDataTree(4, ElementCollection.GetOutputDataTree(reusablesSuggestionTree));


            DA.SetData(5, outMaterialBank);
            DA.SetData(6, outMaterialBank.GetMaterialBankInfo());
            DA.SetDataList(7, outMaterialBank.MaterialBankVisuals);
            DA.SetDataList(8, outMaterialBank.MaterialBankColors);


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