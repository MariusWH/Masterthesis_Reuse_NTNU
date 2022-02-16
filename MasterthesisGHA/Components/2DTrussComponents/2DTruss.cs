﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;


namespace MasterthesisGHA.Components
{
    public class Truss2D : GH_Component
    {

        public Truss2D()
          : base("2D Truss Analysis", "2D Truss",
              "Minimal Finite Element Analysis of 2D Truss",
              "Master", "FEA")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Input Lines", "Lines", "Lines for creating truss geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Input Profiles", "Profiles", "Profiles of members", GH_ParamAccess.list);
            pManager.AddPointParameter("Input Anchored Points", "Anchored", "Support points restricted from translation but free to rotate", GH_ParamAccess.list);           
            pManager.AddNumberParameter("Cross Section Area [mm^2]", "A", "Cross Section Area by indivial member values (list) or constant value", GH_ParamAccess.list, 10000);
            pManager.AddNumberParameter("Young's Modulus [N/mm^2]", "E", "Young's Modulus for all members", GH_ParamAccess.item, 210e3);           
            pManager.AddNumberParameter("Nodal Loading [N]", "Load", "Nodal loads by numeric values (x1, y1, x2, y2, ..)", GH_ParamAccess.list, new List<double> { 0 });
            pManager.AddVectorParameter("Nodal Loading [N]", "vecLoad", "Nodal loads by vector input", GH_ParamAccess.list, new Vector3d(0,0,0));
            pManager.AddNumberParameter("Load Value","","",GH_ParamAccess.item);
            pManager.AddVectorParameter("Load Direction","","",GH_ParamAccess.item);
            pManager.AddVectorParameter("Distribution Direction","","",GH_ParamAccess.item);
            pManager.AddLineParameter("Elements","","",GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("#Elements", "#Elements", "#Elements", GH_ParamAccess.item);
            pManager.AddNumberParameter("#Nodes", "#Nodes", "#Nodes", GH_ParamAccess.item);
            pManager.AddMatrixParameter("K", "K", "K", GH_ParamAccess.item);
            pManager.AddMatrixParameter("r", "r", "r", GH_ParamAccess.item);
            pManager.AddPointParameter("Nodes", "Nodes", "Nodes", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddNumberParameter("N", "N", "N", GH_ParamAccess.list);
            pManager.AddColourParameter("Util", "Util", "Util", GH_ParamAccess.list);
            pManager.AddBrepParameter("Geometry", "Geometry", "Geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("ElementOutput", "ElementOutput", "ElementOutput", GH_ParamAccess.item);

        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT

            List<Line> iLines = new List<Line>();
            List<string> iProfiles = new List<string>();
            List<Point3d> iAnchoredPoints = new List<Point3d>();
            List<double> iA = new List<double>();
            double iE = 0;
            List<double> iLoad = new List<double>();
            List<Vector3d> iLoadVecs = new List<Vector3d>();
                      
            DA.GetDataList(0, iLines);
            DA.GetDataList(1, iProfiles);
            DA.GetDataList(2, iAnchoredPoints);
            DA.GetDataList(3, iA);
            DA.GetData(4, ref iE);
            DA.GetDataList(5, iLoad);
            DA.GetDataList(6, iLoadVecs);


            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            List<Line> iLinesToLoad = new List<Line>();

            DA.GetData(7, ref iLineLoadValue);
            DA.GetData(8, ref iLineLoadDirection);
            DA.GetData(9, ref iLineLoadDistribution);
            DA.GetDataList(10, iLinesToLoad);


 
            
            
            // CODE

            //TrussModel2D truss2D = new TrussModel2D(iLines, iA, iAnchoredPoints, iLoad, iLoadVecs, iE);
            TrussModel2D truss2D = new TrussModel2D(iLines, iProfiles, iAnchoredPoints);

            truss2D.Assemble();
            truss2D.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss2D.Solve();
            truss2D.Retracking();
            truss2D.GetResultVisuals();
            truss2D.GetLoadVisuals();
            
            
            
            // OUTPUT

            DA.SetData("#Elements", truss2D.ElementsInStructure.Count);
            DA.SetData("#Nodes",truss2D.FreeNodes.Count); 
            DA.SetData("K", truss2D.K_out);
            DA.SetData("r", truss2D.r_out);
            DA.SetDataList("Nodes", truss2D.FreeNodes);
            DA.SetData("Info", truss2D.PrintInfo());
            DA.SetDataList("N", truss2D.N_out);
            DA.SetDataList("Util", truss2D.BrepColors);
            DA.SetDataList("Geometry", truss2D.BrepVisuals );
            DA.SetData("ElementOutput", truss2D.writeElementOutput());
        }






        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources._2D_Truss_Icon;
                //return null;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("1AB4E987-0B6A-4EFA-BD27-8418F7C24308"); }
        }

    }
}






