using System;
using System.Collections.Generic;
using System.IO;

namespace csgo_overwatch_vdm.vdm
{
    public static class VdmGenerator
    {
        private static readonly List<DemoActionBase> _demoactions = new List<DemoActionBase>();

        public static void Add(DemoActionBase demoaction)
        {
            _demoactions.Add(demoaction);
        }

        public static void Generate(string path)
        {
            if (_demoactions.Count < 1)
            {
                Console.WriteLine("[WARNING] The list of actions is empty, add some before generating it.");
                return;
            }

            using (var file = new StreamWriter(path))
            {
                file.Write("demoactions\n{"); // Header
                for (var i = 0; i < _demoactions.Count; i++)
                {
                    file.Write(_demoactions[i].ToString(i)); // Content
                }
                file.Write("\n}"); // Footer
            }
        }
    }
}