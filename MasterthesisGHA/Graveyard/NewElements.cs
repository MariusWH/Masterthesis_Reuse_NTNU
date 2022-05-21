/*using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using System.Reflection;

namespace MasterthesisGHA.Components
{
    public class NewElements : GH_Component
    {
        public NewElements()
          : base("New Elements Catalog", "NewElements",
              "Description",
              "Master", "Member Replacement")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Section Names", "Sections", "Sections", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Profiles", "Profiles", "Profiles", GH_ParamAccess.list);
        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            List<string> sectionNames = new List<string>();
            DA.GetDataList(0, sectionNames);
                       

            
            
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
            
            foreach (string sectionName in sectionNames)
            {
                foreach (string line in profilesArray)
                {
                    if (line.Contains(sectionName))
                        profiles.Add(line);
                }
            }
            
                



            // OUTPUT
            DA.SetDataList(0, profiles);


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
            get { return new Guid("1B996CB4-86D3-47DC-B9EB-C20617DAAD02"); }
        }
    }
}*/