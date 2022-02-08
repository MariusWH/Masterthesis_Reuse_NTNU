using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components._2DTrussComponents
{
    public class LineLoad : GH_Component
    {

        public LineLoad()
          : base("Line Loading on 2D Truss", "Line Load",
              "",
              "Master", "2DTruss")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Load Value", "", "", GH_ParamAccess.item, 0);
            pManager.AddVectorParameter("Load Direction", "", "", GH_ParamAccess.item, new Vector3d(0,-1,0));
            pManager.AddVectorParameter("Distribution Direction", "", "", GH_ParamAccess.item, new Vector3d(1,0,0));
            pManager.AddLineParameter("Elements", "", "", GH_ParamAccess.list, new Line());
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Load", "Load", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("211EF806-8A1D-43DB-A4A9-DB87BDB4EA5F"); }
        }
    }
}