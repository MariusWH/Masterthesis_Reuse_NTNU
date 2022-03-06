using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components.ParametricOptimization
{
    public class ParametricOptimization : GH_Component
    {
        public ParametricOptimization()
          : base("ParametricOptimization", "ParametricOptimization",
              "Description",
              "Master", "MethodTwo")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserMaterialBank", "Insert", "Insert Material Bank (true)", GH_ParamAccess.item, false);

            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);
            pManager.AddTextParameter("NewElements", "NewElements", "NewElements", GH_ParamAccess.list, "ALL");

            pManager.AddLineParameter("Structure Lines", "GeometryLines", "GeometryLines", GH_ParamAccess.list, new Line());
            pManager.AddPointParameter("Structure Supports", "Supports", "Supports", GH_ParamAccess.list, new Point3d());

            pManager.AddLineParameter("Load Lines", "LoadLines", "LoadLines", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load Value", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Distribution Direction", "", "", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);

            pManager.AddBrepParameter("Visuals", "Visuals", "Visuals", GH_ParamAccess.list);
            pManager.AddColourParameter("Colour", "Colour", "Colour", GH_ParamAccess.list);

            pManager.AddTextParameter("MaterialBankInfo", "Info", "Info", GH_ParamAccess.item);
            pManager.AddBrepParameter("StockVisuals", "StockVisuals", "StockVisuals", GH_ParamAccess.list);
            pManager.AddColourParameter("StockColour", "StockColour", "StockColour", GH_ParamAccess.list);
        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            bool is3D = false;
            bool insert = false;
            MaterialBank iMaterialBank = new MaterialBank();
            List<string> iNewElementsCatalog = new List<string>();
            List<Line> iGeometryLines = new List<Line>();
            List<Point3d> iSupports = new List<Point3d>();
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();

            DA.GetData(0, ref is3D);
            DA.GetData(1, ref insert);
            DA.GetData(2, ref iMaterialBank);
            DA.GetDataList(3, iNewElementsCatalog);
            DA.GetDataList(4, iGeometryLines);
            DA.GetDataList(5, iSupports);
            DA.GetDataList(6, iLinesToLoad);
            DA.GetData(7, ref iLineLoadValue);
            DA.GetData(8, ref iLineLoadDirection);
            DA.GetData(9, ref iLineLoadDistribution);




            // CODE
            List<string> initialProfiles = new List<string>();
            foreach (Line line in iGeometryLines)
                initialProfiles.Add("IPE100");

            TrussModel3D truss;
            MaterialBank inputMaterialBank = iMaterialBank.DeepCopy();
            MaterialBank outMaterialBank;

            if (!is3D)
                truss = new TrussModel2D(iGeometryLines, initialProfiles, iSupports);
            else
                truss = new TrussModel3D(iGeometryLines, initialProfiles, iSupports);

            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss.Solve();
            truss.Retracking();

            if (insert)
                truss.InsertMaterialBank(inputMaterialBank, out outMaterialBank);
            else
            {
                outMaterialBank = iMaterialBank.DeepCopy();
                outMaterialBank.UpdateVisuals();
            }


            truss.Solve();
            truss.Retracking();




            // OUTPUTS
            DA.SetData("Info", truss.PrintStructureInfo() + "\n\n" + outMaterialBank.GetMaterialBankInfo());
            DA.SetDataList("Visuals", truss.StructureVisuals);
            DA.SetDataList("Colour", truss.StructureColors);

            DA.SetData(5, outMaterialBank);
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
            get { return new Guid("4CD0D13F-BEA8-4FEB-8C85-ABF21D0CA584"); }
        }
    }
}