using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components.ParametricOptimization
{
    public class CreateCSV : GH_Component
    {
        public CreateCSV()
          : base("Create CSV Dataset", "CSV Dataset",
              "Creates CSV Dataset of Structural Analysis Results",
              "Master", "ML")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("3D/2D", "3D/2D", "3D/2D", GH_ParamAccess.list, true);
            pManager.AddLineParameter("Line Geometry", "Geometry", "Geometry as list of lines", GH_ParamAccess.tree);
            pManager.AddTextParameter("Bar Profiles", "Profiles", "Profile of each geometry line member as list", GH_ParamAccess.tree);
            pManager.AddPointParameter("Support Points", "Supports", "Pinned support points restricted from translation but free to rotate", GH_ParamAccess.tree);
            pManager.AddNumberParameter("List Loading [N]", "ListLoad", "Nodal loads by numeric values (x1, y1, x2, y2, ..)", GH_ParamAccess.tree, new List<double> { 0 });
            pManager.AddVectorParameter("Vector Loading [N]", "VectorLoad", "Nodal loads by vector input", GH_ParamAccess.tree, new Vector3d(0, 0, 0));
            pManager.AddNumberParameter("Line Load Value", "LL Value", "", GH_ParamAccess.list, 0);
            pManager.AddVectorParameter("Line Load Direction", "LL Direction", "", GH_ParamAccess.list, new Vector3d(0, 0, -1));
            pManager.AddVectorParameter("Line Load Distribution Direction", "LL Distribution Direction", "", GH_ParamAccess.list, new Vector3d(1, 0, 0));
            pManager.AddLineParameter("Line Load Members", "LL Members", "", GH_ParamAccess.tree, new Line());
            pManager.AddBooleanParameter("Apply Self Weight", "Self Weigth", "", GH_ParamAccess.list, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            List<bool> is3d = new List<bool>();
            List<double> iLineLoadValue = new List<double>();
            List<Vector3d> iLineLoadDirection = new List<Vector3d>();
            List<Vector3d> iLineLoadDistribution = new List<Vector3d>();
            List<bool> applySelfWeight = new List<bool>();

            DA.GetDataList(0, is3d);
            DA.GetDataTree(1, out Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Line> iLinesTree);
            DA.GetDataTree(2, out Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_String> iProfilesTree);
            DA.GetDataTree(3, out Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Point> iSupportsTree);
            DA.GetDataTree(4, out Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> iLoadTree);
            DA.GetDataTree(5, out Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Vector> iLoadVecsTree);
            DA.GetDataList(6, iLineLoadValue);
            DA.GetDataList(7, iLineLoadDirection);
            DA.GetDataList(8, iLineLoadDistribution);
            DA.GetDataTree(9, out Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Line> iLinesToLoadTree);
            DA.GetDataList(10, applySelfWeight);


            // CODE

            // Grasshopper Trees to Nested Lists
            IList<List<Grasshopper.Kernel.Types.GH_Line>> iLinesNested = iLinesTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_String>> iProfilesNested = iProfilesTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_Point>> iSupportsNested = iSupportsTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_Number>> iLoadNested = iLoadTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_Vector>> iLoadVecsNested = iLoadVecsTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_Line>> iLinesToLoadNested = iLinesToLoadTree.Branches;



            TrussModel3D truss;
            for (int i = 0; i < iLinesNested.Count; i++)
            {
                List<Grasshopper.Kernel.Types.GH_Line> iLines = iLinesNested[i];
                List<Grasshopper.Kernel.Types.GH_String> iProfiles = iProfilesNested[i];
                List<Grasshopper.Kernel.Types.GH_Point> iSupports = iSupportsNested[i];
                List<Grasshopper.Kernel.Types.GH_Number> iLoad = iLoadNested[i];
                List<Grasshopper.Kernel.Types.GH_Vector> iLoadVecs = iLoadVecsNested[i];
                List<Grasshopper.Kernel.Types.GH_Line> iLinesToLoad = iLinesToLoadNested[i];

                
            }






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
            get { return new Guid("CE78D6BD-0F1B-4DE2-AA2D-D38BAFAB84A2"); }
        }
    }
}