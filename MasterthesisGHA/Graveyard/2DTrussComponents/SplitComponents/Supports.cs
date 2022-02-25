using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components._2DTrussComponents
{
    public class Supports : GH_Component
    {
        public Supports()
          : base("Nodal support points 2D Truss", "Supports",
              "All support points are threated like pinned supports",
              "Master", "2DTruss")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list, new Point3d());
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Supports", "Supports", "Supports", GH_ParamAccess.list);
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
            get { return new Guid("561D4713-197E-419C-BFFA-D5D98C77B48D"); }
        }
    }
}