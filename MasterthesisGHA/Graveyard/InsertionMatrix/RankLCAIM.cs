using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA.Components.MethodOne
{
    public class RankLCAIM : GH_Component
    {
        // Stored Variables
        public bool firstRun;
        public double trussSize;
        public double maxLoad;
        public double maxDisplacement;

        public RankLCAIM()
          : base("Rank LCA Reuse Method by IM", "RankLCA IM",
              "A linear best-fit method for inserting a defined material bank into a pre-defined structure geometry",
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
            pManager.AddBooleanParameter("Cutting", "Cutting", "", GH_ParamAccess.item, true);

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
            bool cutting = true;

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
            DA.GetData(11, ref cutting);


            // CODE
            List<string> initialProfiles = new List<string>();
            foreach (Line line in iGeometryLines)
                initialProfiles.Add("IPE600");

            SpatialTruss truss;
            MaterialBank iMaterialBankCopy = iMaterialBankOriginal.GetDeepCopy();
            MaterialBank outMaterialBank;

            if (!is3D)
                truss = new PlanarTruss(iGeometryLines, initialProfiles, iSupports);
            else
                truss = new SpatialTruss(iGeometryLines, initialProfiles, iSupports);

            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss.Solve();
            truss.Retracking();


            Matrix<double> rank = Matrix<double>.Build.SparseIdentity(0, 0);
            IEnumerable<int> optimumOrder = Enumerable.Empty<int>();
            Matrix<double> insertionMatrix = Matrix<double>.Build.Sparse(0, 0);
            

            if (insertMaterialBank && insertNewElements)
            {
                truss.InsertNewElements();
                rank = truss.getObjectiveMatrix(iMaterialBankCopy);
                truss.InsertMaterialBankByPriorityMatrix(out insertionMatrix,
                    iMaterialBankCopy, out optimumOrder, cutting);
            }
            else if (insertMaterialBank)
            {
                rank = truss.getObjectiveMatrix(iMaterialBankCopy);
                truss.InsertMaterialBankByPriorityMatrix(out insertionMatrix,
                    iMaterialBankCopy, out optimumOrder, cutting);
            }
            else if (insertNewElements)
            {
                truss.InsertNewElements();
                rank = truss.getObjectiveMatrix(iMaterialBankCopy);
                outMaterialBank = iMaterialBankOriginal.GetDeepCopy();
            }
            else
            {
                rank = truss.getObjectiveMatrix(iMaterialBankCopy);
                outMaterialBank = iMaterialBankOriginal.GetDeepCopy();
            }

            iMaterialBankCopy.InsertionMatrix = insertionMatrix;
            iMaterialBankCopy.UpdateVisualsInsertionMatrix(out List<Brep> geometry, out List<System.Drawing.Color> colors, out _);
            truss.Solve();
            truss.Retracking();



            // OUTPUTS
            DA.SetData("Info", truss.PrintStructureInfo() + "\n\n" + iMaterialBankCopy.GetMaterialBankInfo());
            DA.SetData(1, iMaterialBankCopy);
            DA.SetDataList(2, geometry);
            DA.SetDataList(3, colors);
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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C99BBC48-4C59-48A0-90BE-5A316A1EDD14"); }
        }
    }
}