using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WangSql
{
    internal static class AssemblyHelper
    {
        private static IList<Type> types = new List<Type>();

        public static IList<Type> GetAssemblyTypes()
        {
            if (types == null || !types.Any())
            {
                var assemblyTypes = new List<Type>();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x =>
                {
                    var name = x.GetName().Name;
                    return
                    !name.StartsWith("Microsoft") &&
                    !name.StartsWith("System") &&
                    !name.StartsWith("runtime") &&
                    !name.StartsWith("Newtonsoft") &&
                    !name.StartsWith("Oracle") &&
                    !name.StartsWith("Npgsql") &&
                    !name.StartsWith("MySql")
                    ;
                }).ToList();
                System.IO.Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                     .Where(
                         x =>
                         {
                             var f = new FileInfo(x);

                             return
                              !f.Name.StartsWith("Microsoft") &&
                              !f.Name.StartsWith("System") &&
                              !f.Name.StartsWith("runtime") &&
                              !f.Name.StartsWith("Newtonsoft") &&
                              !f.Name.StartsWith("Oracle") &&
                              !f.Name.StartsWith("Npgsql") &&
                              !f.Name.StartsWith("MySql") &&
                              !assemblies.Any(y => y.GetName().Name == f.Name)
                              ;
                         })
                     .ToList()
                     .ForEach(x =>
                     {
                         assemblies.Add(Assembly.LoadFile(x));
                     });


                assemblies
                .ForEach(item =>
                {
                    try
                    {
                        assemblyTypes.AddRange(
                            item
                            .GetTypes()
                            .Select(x => x.AssemblyQualifiedName)
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Select(x => Type.GetType(x))
                            .Where(type => type != null && ((type.IsClass && !type.IsAbstract) || type.IsInterface))
                            .ToList()
                        );
                    }
                    catch { }
                });

                types = assemblyTypes;
            }
            return types;
        }

    }
}
