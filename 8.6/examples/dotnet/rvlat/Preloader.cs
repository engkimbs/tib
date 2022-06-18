using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RVLat
{

    public class Preloader
    {

        public static void PreloadAssemblies()
        {
            List<string> references = new List<string>();
            Queue<AssemblyName> pending = new Queue<AssemblyName>();

            pending.Enqueue(Assembly.GetEntryAssembly().GetName());

            while (pending.Count > 0)
            {
                AssemblyName assemblyName = pending.Dequeue();

                string name = assemblyName.ToString();

                if (references.Contains(name))
                {

                    continue;
                }

                references.Add(name);

                try
                {
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (assembly != null)
                    {
                        foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                        {
                            pending.Enqueue(reference);
                        }

                        foreach (Type type in assembly.GetTypes())
                        {
                            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                            {
                                DllImportAttribute attribute = (DllImportAttribute)Attribute.GetCustomAttribute(method,
                                                                                                                typeof(DllImportAttribute));
                                if (attribute != null && !references.Contains(attribute.Value))
                                {
                                    references.Add(attribute.Value);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

        }

    }

}
