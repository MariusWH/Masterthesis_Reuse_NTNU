using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

/*
namespace MasterthesisGHA.Components
{
    public class Truss2D : Truss3D
    {
        public Truss2D()
          : base("New 2D Truss Analysis", "2D Truss",
              "Minimal Finite Element Analysis of 2D Truss",
              "Master", "FEA")
        {

        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            SetInputs(DA, out bool is3d, out List<Line> iLines, out List<string> iProfiles, out List<Point3d> iAnchoredPoints, out List<double> iLoad,
            out List<Vector3d> iLoadVecs, out double iLineLoadValue, out Vector3d iLineLoadDirection, out Vector3d iLineLoadDistribution,
            out List<Line> iLinesToLoad, out bool normalizeVisuals);

            TrussModel2D truss2D = new TrussModel2D(iLines, iProfiles, iAnchoredPoints);
            truss2D.ApplyNodalLoads(iLoad, iLoadVecs);
            truss2D.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, iLinesToLoad);
            truss2D.Solve();
            truss2D.Retracking();
            if (normalizeVisuals)
            {
                trussSize = truss2D.StructureSize;
                maxLoad = truss2D.GlobalLoadVector.AbsoluteMaximum();
            }
            truss2D.GetResultVisuals();
            truss2D.GetLoadVisuals(trussSize, maxLoad);

            SetOutputs(DA, truss2D);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources._2D_Truss_Icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C4D3178B-002C-4EF5-AF5E-8D6B4547A1AB"); }
        }
    }
}
*/