using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components.MethodOne
{
    public class HeuristicReplacement : GH_Component
    {
        // Stored Variables
        public double trussSize;
        public double maxLoad;
        public double maxDisplacement;

        public HeuristicReplacement()
          : base("HeuristicReplacement", "Heuristic",
              "A Best-Fit Heuristic Method for inserting a defined Material Bank into a pre-defined structur geometry",
              "Master", "MethodOne")
        {
            trussSize = -1;
            maxLoad = -1;
            maxDisplacement = -1;
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserMaterialBank", "InsertMB", "Insert Material Bank (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserNewElements", "InsertNew", "Insert New Elements (true)", GH_ParamAccess.item, false);

            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);
            pManager.AddTextParameter("NewElements", "NewElements", "NewElements", GH_ParamAccess.list, "ALL");

            pManager.AddLineParameter("Structure Lines", "GeometryLines", "GeometryLines", GH_ParamAccess.list, new Line());
            pManager.AddPointParameter("Structure Supports", "Supports", "Supports", GH_ParamAccess.list, new Point3d());

            pManager.AddLineParameter("Load Lines", "LoadLines", "LoadLines", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load Value", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Distribution Direction", "", "", GH_ParamAccess.item);

            pManager.AddBooleanParameter("NormalizeVisuals", "NormalizeVisuals", "Use button to normalize the visuals output", GH_ParamAccess.item, false);

        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);

            pManager.AddBrepParameter("Visuals", "Visuals", "Visuals", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour", "Colour", "Colour", GH_ParamAccess.list);          

            pManager.AddBrepParameter("StockVisuals", "StockVisuals", "StockVisuals", GH_ParamAccess.list);
            pManager.AddColourParameter("StockColour", "StockColour", "StockColour", GH_ParamAccess.list);

            pManager.AddNumberParameter("TotalMass", "TotalMass", "TotalMass", GH_ParamAccess.item);
            pManager.AddNumberParameter("ReusedMass", "ReusedMass", "ReusedMass", GH_ParamAccess.item);
            pManager.AddNumberParameter("NewMass", "NewMass", "NewMass", GH_ParamAccess.item);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            bool is3D = false;
            bool insertMaterialBank = false;
            bool insertNewElements = false;
            MaterialBank iMaterialBank = new MaterialBank();
            List<string> iNewElementsCatalog = new List<string>();
            List<Line> iGeometryLines = new List<Line>();
            List<Point3d> iSupports = new List<Point3d>();
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();
            bool normalizeVisuals = false;

            DA.GetData(0, ref is3D);
            DA.GetData(1, ref insertMaterialBank);
            DA.GetData(2, ref insertNewElements);
            DA.GetData(3, ref iMaterialBank);
            DA.GetDataList(4, iNewElementsCatalog);
            DA.GetDataList(5, iGeometryLines);
            DA.GetDataList(6, iSupports);
            DA.GetDataList(7, iLinesToLoad);
            DA.GetData(8, ref iLineLoadValue);
            DA.GetData(9, ref iLineLoadDirection);
            DA.GetData(10, ref iLineLoadDistribution);
            DA.GetData(11, ref normalizeVisuals);
            



            // CODE
            List<string> initialProfiles = new List<string>();
            foreach (Line line in iGeometryLines)
                initialProfiles.Add("IPE600");

            TrussModel3D truss;
            MaterialBank inputMaterialBank = iMaterialBank.DeepCopy();
            MaterialBank outMaterialBank;

            if (!is3D )
                truss = new TrussModel2D(iGeometryLines, initialProfiles, iSupports);
            else
                truss = new TrussModel3D(iGeometryLines, initialProfiles, iSupports);

            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss.Solve();
            truss.Retracking();

            if (insertMaterialBank && insertNewElements)
                truss.InsertMaterialBankThenNewElements(inputMaterialBank, out outMaterialBank);
            else if (insertMaterialBank)
                truss.InsertMaterialBank(inputMaterialBank, out outMaterialBank);
            else if (insertNewElements)
            {
                truss.InsertNewElements();
                outMaterialBank = iMaterialBank.DeepCopy();
                outMaterialBank.ResetMaterialBank();
                outMaterialBank.UpdateVisuals();
            }         
            else
            {
                outMaterialBank = iMaterialBank.DeepCopy();
                outMaterialBank.ResetMaterialBank();
                outMaterialBank.UpdateVisuals();
            }

            truss.Solve();
            truss.Retracking();


            if (normalizeVisuals)
            {
                trussSize = truss.GetStructureSize();
                maxLoad = truss.GetMaxLoad();
                maxDisplacement = truss.GetMaxDisplacement();
            }

            truss.GetResultVisuals(0, trussSize, maxDisplacement);
            truss.GetLoadVisuals(trussSize, maxLoad, maxDisplacement);
        


            // OUTPUTS
            DA.SetData("Info", truss.PrintStructureInfo() + "\n\n" + outMaterialBank.GetMaterialBankInfo());
            DA.SetData(1, outMaterialBank);
            DA.SetDataList("Visuals", truss.StructureVisuals);
            DA.SetDataList("Colour", truss.StructureColors);           
            DA.SetDataList(4, outMaterialBank.MaterialBankVisuals);
            DA.SetDataList(5, outMaterialBank.MaterialBankColors);
            DA.SetData(6, truss.GetTotalMass());
            DA.SetData(7, truss.GetReusedMass());
            DA.SetData(8, truss.GetNewMass());


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