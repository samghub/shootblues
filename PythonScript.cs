﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using System.IO;
using System.Text.RegularExpressions;

namespace ShootBlues {
    public class PythonScript : ManagedScript {
        private string ScriptText = null;

        public PythonScript (ScriptName name)
            : base(name) {
        }

        public override IEnumerator<object> Initialize () {
            yield return Reload();
        }

        public override IEnumerator<object> Reload () {
            ScriptText = null;

            var filename = Program.FindScript(Name);

            if (File.Exists(filename.FullPath)) {
                var fText = Future.RunInThread(
                    () => File.ReadAllText(filename.FullPath)
                );
                yield return fText;

                ScriptText = fText.Result;
            } else {
                throw new FileNotFoundException("Python script not found", filename.Name);
            }
        }

        public string ModuleName {
            get {
                return Regex.Replace(
                    Name, @"(\.script\.py|\.py)", 
                    "", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
                );
            }
        }

        public override IEnumerator<object> LoadInto (ProcessInfo process) {
            if (ScriptText == null)
                yield return Reload();

            yield return Program.LoadPythonScript(
                process, ModuleName, ScriptText
            );
        }

        public override IEnumerator<object> UnloadFrom (ProcessInfo process) {
            yield return Program.UnloadPythonScript(
                process, ModuleName
            );
        }
    }
}
