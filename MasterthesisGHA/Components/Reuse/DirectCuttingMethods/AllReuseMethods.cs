using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA.Components.MethodOne
{
    public class AllReuseMethods : GH_Component
    {
        // Stored Variables
        public bool firstRun;
        public double trussSize;
        public double maxLoad;
        public double maxDisplacement;

        public AllReuseMethods()
          : base("All Reuse Methods", "Reuse",
              "",
              "Master", "Reuse Combi")
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
            pManager.AddIntegerParameter("Method", "Method", "", GH_ParamAccess.item, 0);
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
            pManager.AddNumberParameter("TotalMass", "TotalMass", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("ReusedMass", "ReusedMass", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("NewMass", "NewMass", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Structure Data", "Structure", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("MaterialBank Data", "MaterialBank", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Insertion Matrix", "IM", "", GH_ParamAccess.item);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            bool is3D = false;
            bool insertMaterialBank = false;
            bool insertNewElements = false;
            int optimizationMethod = 0;
            MaterialBank iMaterialBank = new MaterialBank();
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
            DA.GetData(3, ref optimizationMethod);
            DA.GetData(4, ref iMaterialBank);
            DA.GetDataList(5, iNewElementsCatalog);
            DA.GetDataList(6, iGeometryLines);
            DA.GetDataList(7, iSupports);
            DA.GetDataList(8, iLinesToLoad);
            DA.GetData(9, ref iLineLoadValue);
            DA.GetData(10, ref iLineLoadDirection);
            DA.GetData(11, ref iLineLoadDistribution);


            // CODE
            List<string> initialProfiles = new List<string>();
            foreach (Line line in iGeometryLines)
                initialProfiles.Add("IPE600");

            TrussModel3D truss;
            if (!is3D)
                truss = new TrussModel2D(iGeometryLines, initialProfiles, iSupports);
            else
                truss = new TrussModel3D(iGeometryLines, initialProfiles, iSupports);

            truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss.Solve();
            truss.Retracking();

            Matrix<double> insertionMatrix = Matrix<double>.Build.Sparse(0, 0);
            MaterialBank inputMaterialBank = iMaterialBank.GetDeepCopy();
            MaterialBank outMaterialBank = iMaterialBank.GetDeepCopy();
            optimizationMethod = optimizationMethod % 5;
            string outputInfo = string.Empty;

            if (insertNewElements)
            {
                truss.InsertNewElements();
                outputInfo += "Structure is optimized with new elements.\n";
            }
            if (insertMaterialBank)
            {
                outputInfo += "Reusable material bank is inserted by ";
                switch (optimizationMethod)
                {
                    case 0: // Direct Cutting with No Optimization
                        truss.InsertMaterialBank(inputMaterialBank, out outMaterialBank);
                        outputInfo += "direct method with no optimization.\n";
                        break;
                    
                    case 1: // Insertion Matrix with No Optimization 
                        truss.InsertMaterialBank(inputMaterialBank, out insertionMatrix);
                        truss.InsertReuseElementsFromInsertionMatrix(insertionMatrix, inputMaterialBank, out outMaterialBank);
                        outputInfo += "insertion matrix method with no optimization.\n";
                        break;

                    case 2: // Direct Method with Priority Matrix Optimization
                        truss.InsertMaterialBankByPriorityMatrix(inputMaterialBank, out outMaterialBank, out _);
                        outputInfo += "direct method with priority matrix optimization.\n";
                        break;

                    case 3: // Insertion Matrix with Priority Matrix Optimization
                        truss.InsertMaterialBankByPriorityMatrix(out insertionMatrix, inputMaterialBank, out _);
                        truss.InsertReuseElementsFromInsertionMatrix(insertionMatrix, inputMaterialBank, out outMaterialBank);
                        outputInfo += "insertion matrix method with priority matrix optimization.\n";
                        break;

                    case 4: // Direct Cutting with Brute Force Optimization
                        if (truss.ElementsInStructure.Count < 7) // All Permutations
                        {
                            truss.InsertMaterialBankByAllPermutations(inputMaterialBank, out outMaterialBank, out _, out _);
                            outputInfo += "direct method with all brute force permutations optimization.\n";
                        } 
                        else // Pseudo Random List Shuffles
                        {
                            truss.InsertMaterialBankByRandomPermutations((int)1e3, inputMaterialBank, out outMaterialBank, out _, out _);
                            outputInfo += "direct method with 1000 pseudo random permutations optimization.\n";
                        } 
                        break;
                }
            }
            
            outMaterialBank.UpdateVisualsMaterialBank();
            truss.Solve();
            truss.Retracking();

            outputInfo += truss.PrintStructureInfo() + "\n\n" + outMaterialBank.GetMaterialBankInfo();

            // OUTPUTS
            DA.SetData(0, outputInfo);           
            DA.SetData(1, truss.GetTotalMass());
            DA.SetData(2, truss.GetReusedMass());
            DA.SetData(3, truss.GetNewMass());
            DA.SetData(4, truss);
            DA.SetData(5, outMaterialBank);
            DA.SetData(6, ElementCollection.MathnetToRhinoMatrix(insertionMatrix));


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
            get { return new Guid("7B751CA9-B009-44DE-B20F-50D5D7E5A138"); }
        }
    }
}