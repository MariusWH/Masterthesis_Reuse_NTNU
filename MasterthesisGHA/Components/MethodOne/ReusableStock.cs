using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using System.Reflection;

namespace MasterthesisGHA
{
    public class ReusableStock : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public ReusableStock()
          : base("Reusable Stock of Elements", "ReusableElements",
              "Description",
              "Master", "MethodOne")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("ReusableStock", "Reusables", "Input as: Amount x SectionName x Length (ex.: 10 x IPE200 x 1000)", GH_ParamAccess.list, "10 x IPE200 x 1000");
        }

 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "Elements", "Elements", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            List<string> inputCommands = new List<string>();
            DA.GetDataList(0, inputCommands);






            // CODE
            string profilesFromFile = "";
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MasterthesisGHA.Resources.Profiles.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                profilesFromFile = reader.ReadToEnd();
            }
            var profilesArray = profilesFromFile.Split('\n');
            List<string> profiles = new List<string>();

            foreach (string line in profilesArray)
            {
                profiles.Add(line);
            }





            List<string> vs = new List<string>();
            ReusableElement element = new ReusableElement();
            List<ReusableElement> elements = new List<ReusableElement>();


            foreach ( string command in inputCommands )
            {
                string[] commandArray = command.Split('x');
                int amount = Convert.ToInt32(commandArray[0]);
                string sectionName = commandArray[1];
                double length = Convert.ToDouble(commandArray[2]);
               
                foreach (string line in profilesArray)
                {
                    if(line.Contains(sectionName))
                    {                       
                        string[] lineArray = line.Split(',');
                        double a = Convert.ToDouble(lineArray[1]); // Change file to not contain length
                        double i = Convert.ToDouble(lineArray[2]);
                        
                        for (int j = 0; j < amount; j++)
                        {
                            vs.Add("test");
                            element = new ReusableElement(sectionName, length);
                            elements.Add(element);

                        }
                        
                    }                    
                }  
            }



            //string outputInfo = "";
            
            //foreach (ReusableElement element in reusableElements)
            //    outputInfo += element.GetDatabaseInfo();




            // OUTPUTS
            //DA.SetDataList(0, reusableElements);
            DA.SetDataList(1, vs);







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
            get { return new Guid("EC30A7CE-C83A-4A4D-AA49-3D5B7AFF7A00"); }
        }
    }
}