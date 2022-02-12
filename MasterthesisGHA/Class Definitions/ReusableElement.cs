﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace MasterthesisGHA
{
    internal class ReusableElement
    {
        public string ProfileName;
        public double E;
        public double A;
        public double I;
        public double ReusableLength;

        public readonly int InstanceID;
        public static int InstanceCounter;
        public static List<ReusableElement> MaterialBank;

        
        
        // Static constructor
        static ReusableElement()
        {
            InstanceCounter = 0;
            MaterialBank = new List<ReusableElement>();
        }


        // Constructors
        public ReusableElement(string profileName, double length, double a, double i = 0, double e = 210e3)
        {
            ProfileName = profileName;
            ReusableLength = length;
            E = e;
            A = a;
            I = i;

            InstanceID = InstanceCounter++;
            UpdateDatabase();
        }

        public ReusableElement(string profileName, double length, double e = 210e3)
        {
            ProfileName = profileName;
            ReusableLength = length;

            E = e;
            //A = this.NameToA(profileName);
            //I = this.NameToI(profileName);
            A = 1;
            E = 1;

            InstanceID = InstanceCounter++;
            UpdateDatabase();
        }

        public ReusableElement(string profileInfo) // "ProfileName;ReusableLength;A;I;E"
        {
            string[] txtcol = profileInfo.Split(',');

            ProfileName = txtcol[0];
            ReusableLength = Convert.ToDouble(txtcol[1]);
            A = Convert.ToDouble(txtcol[2]);
            I = Convert.ToDouble(txtcol[3]);
            E = Convert.ToDouble(txtcol[4]);

            InstanceID = InstanceCounter++;
            UpdateDatabase();
        }




        // Methods
        public static void ResetStatic()
        {
            MaterialBank = new List<ReusableElement>();
            InstanceCounter = 0;
        }



        public void UpdateDatabase()
        {
            MaterialBank.Add(this);
        }




        public static string GetDatabaseInfo() // "ProfileName;ReusableLength;A;I;E"
        {
            string info = "";

            if (MaterialBank.Count == 0)
                throw new Exception("Close but no cigar..");

            foreach (ReusableElement element in MaterialBank)
            {
                info += element.ProfileName + ";";
                info += element.ReusableLength + ";";
                info += element.A + ";";
                info += element.I + ";";
                info += element.E + "\n";
            }
            return info;
        }

        public static List<Brep> VisualizeDatabase()
        {
            List<Brep> outList = new List<Brep>();

            double group = 0;
            double instance = 0;
            double spacing = 100;

            foreach (ReusableElement element in MaterialBank)
            {

                Plane basePlane = new Plane(new Point3d(instance, 0, group), new Vector3d(0, 1, 0));
                Circle baseCircle = new Circle(basePlane, Math.Sqrt(element.A) / Math.PI);

                Cylinder cylinder = new Cylinder(baseCircle, element.ReusableLength);
                outList.Add(cylinder.ToBrep(true, true));

                instance = instance + 2 * Math.Sqrt(element.A) / Math.PI + spacing;
            }
            return outList;

        }



        public double CheckUtilization(double N)
        {
            double f_dim = 355;
            return (N / (A * f_dim));
        }


    }
}
