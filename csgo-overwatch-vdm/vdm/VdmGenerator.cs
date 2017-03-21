using System;
using System.Collections.Generic;
using System.IO;

namespace csgo_overwatch_vdm.vdm
{
    public static class VdmGenerator
    {
        public static List<DemoActionBase> _demoactions = new List<DemoActionBase>();

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
                for (var i = 0; i < _demoactions.Count; i++)
                {
                    var action = _demoactions[i];
                    file.Write(action.ToString(i));
                }
            }
        }
    }
}