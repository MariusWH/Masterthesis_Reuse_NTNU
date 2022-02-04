using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterthesisGHA.Components
{
    public class ReusableDatabase : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ReusableDatabase class.
        /// </summary>
        public ReusableDatabase()
          : base("ReusableDatabase", "Nickname",
              "k",
              "Master", "FEA")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Add (Update)", "Add", "Toggle", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Read File", "RF", "RF", GH_ParamAccess.item, "noFile");
            pManager.AddTextParameter("Read String", "RS", "RS", GH_ParamAccess.item, "");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddBrepParameter("Brep", "Brep", "Brep", GH_ParamAccess.list);   
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>

        

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Inputs
            bool toggle = false;
            DA.GetData(0, ref toggle);

            string filepath = "";
            DA.GetData(1, ref filepath);

            

            // Initialize variables
            ReusableElement element = new ReusableElement();

            if (toggle)
            {

                // Read file

                if (filepath != "noFile")
                {
                    List<string> profileInfoList = File.ReadAllLines(filepath).ToList();
                    foreach (string profileInfo in profileInfoList)
                    {
                        element = new ReusableElement(profileInfo);
                    }
                }

                toggle = false;

            }



            // Read string





            // Outputs
            DA.SetData("Info", element.GetDatabaseInfo());






        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
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
            get { return new Guid("6819BAE0-2165-41A9-B4BA-8781406E79D2"); }
        }
    }
}