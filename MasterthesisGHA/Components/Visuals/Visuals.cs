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
        public int prevStructuresCount;
        public List<double> size;
        public List<double> maxLoad;
        public List<double> maxMoment;
        public List<double> maxDisplacement;
        public List<double> maxAngle;
        List<Brep> outGeometry;
        List<System.Drawing.Color> outColor;

        public Visuals()
          : base("Visuals", "Visuals",
               "Visualize Structural Model",
              "Master", "Visuals")
        {
            firstRun = true;
            //size = new List<double>();
            maxLoad = new List<double>();
            maxMoment = new List<double>();
            maxDisplacement = new List<double>();
            maxAngle = new List<double>();
            outGeometry = new List<Brep>();
            outColor = new List<System.Drawing.Color>();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Data", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Normalize Visuals", "Normalize", "Use button to normalize the visuals output", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Color Code", "Color Code", "", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Structure Size", "Size", "", GH_ParamAccess.item, 10000);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry Visuals", "Geometry", "", GH_ParamAccess.list);
            pManager.AddColourParameter("Color Visuals", "Color", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Color Code Info", "Color Code", "", GH_ParamAccess.item);
        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            List<ElementCollection> elementCollections = new List<ElementCollection>();
            bool normalize = false;
            int colorCode = 0;
            double size = 0;

            DA.GetDataList(0, elementCollections);
            DA.GetData(1, ref normalize);
            DA.GetData(2, ref colorCode);
            DA.GetData(3, ref size);

            // Color codes
            // 0 - Discrete Member Verifiation (Red/Green/Yellow)
            // 1 - Continuous Member Stresses (White-Blue)
            // 2 - Continuous Member Buckling (White-Blue)
            // 3 - New and Reuse Members (White-Blue)
            // 4 - Discrete Node Displacements (Red/Green)
            // 5 - Continuous Node Displacements (White-Blue)

            // CODE
            


            if (normalize || firstRun || (prevStructuresCount != elementCollections.Count))
            {
                firstRun = false;
                //size = new List<double>(elementCollections.Count);
                maxLoad = new List<double>(elementCollections.Count);
                maxMoment = new List<double>(elementCollections.Count);
                maxDisplacement = new List<double>(elementCollections.Count);
                maxAngle = new List<double>(elementCollections.Count);

                foreach (ElementCollection elementCollection in elementCollections)
                {
                    //size.Add(elementCollection.GetSize());
                    //size.Add(1e4);
                    maxLoad.Add(elementCollection.GetMaxLoad());
                    maxMoment.Add(elementCollection.GetMaxMoment());
                    maxDisplacement.Add(elementCollection.GetMaxDisplacement());
                    maxAngle.Add(elementCollection.GetMaxAngle());
                }
            }


            outGeometry.Clear();
            outColor.Clear();

            string codeInfo = "";
            for (int i = 0; i < elementCollections.Count; i++)
            {
                ElementCollection elementCollection = elementCollections[i];

                elementCollection.GetVisuals(
                    out List<Brep> geometry, out List<System.Drawing.Color> color, out codeInfo, 
                    colorCode, size, maxDisplacement[i], maxAngle[i], maxLoad[i], maxMoment[i]);

                outGeometry.AddRange(geometry);
                outColor.AddRange(color);
            }


            // OUTPUT            
            DA.SetDataList(0, outGeometry);
            DA.SetDataList(1, outColor);
            DA.SetData(2, codeInfo);

            prevStructuresCount = elementCollections.Count;

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("E748F785-C01A-44AC-BD2E-E33E0671BFBD"); }
        }
    }
}