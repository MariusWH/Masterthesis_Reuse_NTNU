using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components.Visuals
{
    public class Visuals : GH_Component
    {
        // Stored Variables
        public bool firstRun;
        public List<double> trussSize;
        public List<double> maxLoad;
        public List<double> maxDisplacement;
        List<Brep> outGeometry;
        List<System.Drawing.Color> outColor;

        public Visuals()
          : base("Visuals", "Visuals",
               "Visualize Structural Model",
              "Master", "Visuals")
        {
            firstRun = true;
            trussSize = new List<double>();
            maxLoad = new List<double>();
            maxDisplacement = new List<double>();
            outGeometry = new List<Brep>();
            outColor = new List<System.Drawing.Color>();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structural Model", "Model", "Data", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Normalize Geometry", "NormalizeGeometry", "Use button to normalize the visuals output", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Normalize Loads", "NormalizeLoads", "Use button to normalize the visuals output", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry Visuals", "Geometry", "", GH_ParamAccess.list);
            pManager.AddColourParameter("Color Visuals", "Color", "", GH_ParamAccess.list);
        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            List<TrussModel3D> structures = new List<TrussModel3D>();
            bool normalizeGeometry = false;
            bool normalizeColor = false;

            DA.GetDataList(0, structures);
            DA.GetData(1, ref normalizeGeometry);
            DA.GetData(2, ref normalizeColor);



            // CODE
            if (normalizeGeometry || firstRun)
            {
                firstRun = false;
                trussSize.Clear();
                maxLoad.Clear();
                maxDisplacement.Clear();

                foreach (TrussModel3D truss in structures)
                {                   
                    trussSize.Add( truss.GetStructureSize() );
                    maxLoad.Add( truss.GetMaxLoad() );
                    maxDisplacement.Add( truss.GetMaxDisplacement() );
                }                   
                return;
            }



            outGeometry.Clear();
            outColor.Clear();

            for (int i = 0; i < structures.Count; i++)
            {
                TrussModel3D truss = structures[i];

                truss.GetResultVisuals(0, trussSize[i], maxDisplacement[i]);
                truss.GetLoadVisuals(trussSize[i], maxLoad[i], maxDisplacement[i]);

                outGeometry.AddRange(truss.StructureVisuals);
                outColor.AddRange(truss.StructureColors);

            }


            // OUTPUT            
            DA.SetDataList(0, outGeometry);
            DA.SetDataList(1, outColor);

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
            get { return new Guid("01956385-CD76-4044-92E0-B32694AEC4C9"); }
        }
    }
}