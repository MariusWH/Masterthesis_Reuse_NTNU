/*using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA
{
    public class Elements : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Elements()
          : base("Elements with defined placement", "Elements",
              "",
              "Master", "2DTruss")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Input Lines", "Lines", "Lines for creating truss geometry", GH_ParamAccess.list, new Line());          
            pManager.AddNumberParameter("Cross Section Area [mm^2]", "A", "Cross Section Area by indivial member values (list) or constant value", GH_ParamAccess.list, 10000);
            pManager.AddNumberParameter("Young's Modulus [N/mm^2]", "E", "Young's Modulus by indivial member values (list) or constant value", GH_ParamAccess.list, 210e3);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements in geometry", "Elements", "Elements", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            List<Line> iLines = new List<Line>();
            List<double> iA = new List<double>();
            List<double> iE = new List<double>();

            DA.GetDataList(0, iLines);
            DA.GetDataList(1, iA);
            DA.GetDataList(2, iE);

            if (iLines.Count == 0)
                throw new Exception("Line Input is not valid!");

            if (iA.Count == 1)
            {
                List<double> A_constValue = new List<double>();
                for (int i = 0; i < iLines.Count; i++)
                    A_constValue.Add(iA[0]);
                iA = A_constValue;
            }
            else if (iA.Count != iLines.Count)
                throw new Exception("A is wrong size! Input list with same length as Lines or constant value!");

            if (iE.Count == 1)
            {
                List<double> E_constValue = new List<double>();
                for (int i = 0; i < iLines.Count; i++)
                    E_constValue.Add(iA[0]);
                iE = E_constValue;
            }
            else if (iE.Count != iLines.Count)
                throw new Exception("E is wrong size! Input list with same length as Lines or constant value!");

            foreach (double e in iE)
            {
                if (e < 0 || e > 1000e3)
                    throw new Exception("E-modulus can not be less than 0 or more than 1000 GPa!");
            }





            // CODE
            List<OLDInPlaceElement> elements = new List<OLDInPlaceElement>();
            for ( int i = 0; i < iLines.Count; i++ )
            {
                elements.Add(new OLDInPlaceElement(iLines[i], iE[i], iA[i]));
            }





            string info = "";
            foreach (OLDInPlaceElement element in elements)
                info += element.getElementInfo() + "\n";





            // OUTPUT
            DA.SetDataList(0, elements);
            DA.SetData(1, info);

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
            get { return new Guid("0195B5A8-4929-4A2A-B2EE-1573D496CB5D"); }
        }
    }
}
*/