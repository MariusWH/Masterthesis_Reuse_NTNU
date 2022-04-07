using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA.Components.MethodOne
{
    public class RankLCA : GH_Component
    {
        // Stored Variables
        public bool firstRun;
        public double trussSize;
        public double maxLoad;
        public double maxDisplacement;

        public RankLCA()
          : base("Rank LCA Reuse Method", "RankLCA",
              "A linear best-fit method for inserting a defined material bank into a pre-defined structur geometry",
              "Master", "Reuse")
        {
            firstRun = true;
            trussSize = -1;
            maxLoad = -1;
            maxDisplacement = -1;
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserMaterialBank", "InsertMB", "Insert Material Bank (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserNewElements", "InsertNew", "Insert New Elements (true)", GH_ParamAccess.item, false);

            pManager.AddGenericParameter("MaterialBank", "MB", "", GH_ParamAccess.item);
            pManager.AddTextParameter("NewElements", "New", "", GH_ParamAccess.list, "ALL");

            pManager.AddLineParameter("Structure Lines", "Lines", "", GH_ParamAccess.list, new Line());
            pManager.AddPointParameter("Structure Supports", "Supports", "", GH_ParamAccess.list, new Point3d());

            pManager.AddLineParameter("Load Lines", "LoadLines", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load Value", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Distribution Direction", "", "", GH_ParamAccess.item);

        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "", GH_ParamAccess.item);

            pManager.AddBrepParameter("StockVisuals", "StockVisuals", "", GH_ParamAccess.list);
            pManager.AddColourParameter("StockColour", "StockColour", "", GH_ParamAccess.list);

            pManager.AddNumberParameter("TotalMass", "TotalMass", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("ReusedMass", "ReusedMass", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("NewMass", "NewMass", "", GH_ParamAccess.item);

            pManager.AddGenericParameter("Model Data", "Model", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Rank Matrix", "Rank", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Optimum Order", "Order", "", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            bool is3D = false;
            bool insertMaterialBank = false;
            bool insertNewElements = false;
            MaterialBank iMaterialBankOriginal = new MaterialBank();
            List<string> iNewElementsCatalog = new List<string>();
            List<Line> iGeometryLines = new List<Line>();
            List<Point3d> iSupports = new List<Point3d>();
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();

            DA.GetData(0, ref is3D);
            DA.GetData(1, ref insertMaterialBank);
            DA.GetData(2, ref insertNewElements);
            DA.GetData(3, ref iMaterialBankOriginal);
            DA.GetDataList(4, iNewElementsCatalog);
            DA.GetDataList(5, iGeometryLines);
            DA.GetDataList(6, iSupports);
            DA.GetDataList(7, iLinesToLoad);
            DA.GetData(8, ref iLineLoadValue);
            DA.GetData(9, ref iLineLoadDirection);
            DA.GetData(10, ref iLineLoadDistribution);


            // CODE
            List<string> initialProfiles = new List<string>();
            foreach (Line line in iGeometryLines)
                initialProfiles.Add("IPE600");

            TrussModel3D truss;
            MaterialBank iMaterialBankCopy = iMaterialBankOriginal.GetDeepCopy();
            MaterialBank outMaterialBank;

            if (!is3D)
                truss = new TrussModel2D(iGeometryLines, initialProfiles, iSupports);
            else
                truss = new TrussModel3D(iGeometryLines, initialProfiles, iSupports);

            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss.Solve();
            truss.Retracking();


            IEnumerable<int> optimumOrder = Enumerable.Empty<int>();


            Matrix<double> rank = Matrix<double>.Build.SparseIdentity(0, 0);

            if (insertMaterialBank && insertNewElements)
            {
                truss.InsertNewElements();
                
                rank = truss.EmissionReductionRank(iMaterialBankCopy);
                
                truss.InsertMaterialBankByRankMatrix(
                    iMaterialBankCopy, out outMaterialBank, out optimumOrder);
            }
            else if (insertMaterialBank)
            {
                rank = truss.EmissionReductionRank(iMaterialBankCopy);

                truss.InsertMaterialBankByRankMatrix(
                    iMaterialBankCopy, out outMaterialBank, out optimumOrder);
            }
            else if (insertNewElements)
            {
                truss.InsertNewElements();

                rank = truss.EmissionReductionRank(iMaterialBankCopy);

                outMaterialBank = iMaterialBankOriginal.GetDeepCopy();
            }
            else
            {
                rank = truss.EmissionReductionRank(iMaterialBankCopy);

                outMaterialBank = iMaterialBankOriginal.GetDeepCopy();
            }

            outMaterialBank.UpdateVisualsMaterialBank();
            truss.Solve();
            truss.Retracking();



            // OUTPUTS
            DA.SetData("Info", truss.PrintStructureInfo() + "\n\n" + outMaterialBank.GetMaterialBankInfo());
            DA.SetData(1, outMaterialBank);
            DA.SetDataList(2, outMaterialBank.MaterialBankVisuals);
            DA.SetDataList(3, outMaterialBank.MaterialBankColors);
            DA.SetData(4, truss.GetTotalMass());
            DA.SetData(5, truss.GetReusedMass());
            DA.SetData(6, truss.GetNewMass());
            DA.SetData(7, truss);
            DA.SetData(8, ElementCollection.MathnetToRhinoMatrix(rank));
            DA.SetDataList(9, optimumOrder);

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
            get { return new Guid("FB7D4607-FE5D-4E08-B8D4-BB5200925C8E"); }
        }
    }
}