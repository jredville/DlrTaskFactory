using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace DlrTaskFactory {
    /// <summary>
    /// A task that executes a custom script.
    /// </summary>
    /// <remarks>
    /// This task can implement <see cref="IGeneratedTask"/> to support task properties
    /// that are defined in the script itself and not known at compile-time of this task factory.
    /// </remarks>
    internal class DlrTask : Task, IDisposable, IGeneratedTask {
        /// <summary>
        /// The task factory that generated this task.
        /// </summary>
        private readonly DlrTaskFactory taskFactory;
        private XElement xElement;
        private IBuildEngine taskFactoryLoggingHost;
        private string language;
        private ScriptEngine engine;
        private dynamic scope;


        private static string GetLanguage(XElement taskXml) {
            return taskXml.Attribute("Language").Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DlrTask"/> class.
        /// </summary>
        internal DlrTask(DlrTaskFactory factory, XElement xElement, IBuildEngine taskFactoryLoggingHost) {
            Contract.Requires(factory != null);
            Contract.Requires(xElement != null);
            Contract.Requires(taskFactoryLoggingHost != null);

            this.taskFactory = factory;
            this.xElement = xElement;
            this.language = GetLanguage(xElement);
            this.taskFactoryLoggingHost = taskFactoryLoggingHost;

            var srs = new ScriptRuntimeSetup();
            srs.LanguageSetups.Add(IronRuby.Ruby.CreateRubySetup());
            srs.LanguageSetups.Add(IronPython.Hosting.Python.CreateLanguageSetup(null));
            var runtime = new ScriptRuntime(srs);
            engine = runtime.GetEngineByFileExtension(language);
            scope = engine.CreateScope();
            scope.log = this.Log;
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute() {
            try {
                engine.Execute(xElement.Value, scope);
            } catch {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The value of the property.</returns>
        public object GetPropertyValue(TaskPropertyInfo property) {
            Contract.Requires(property != null);
            dynamic variable;
            dynamic res;
            if (RubyUtils.HasMangledName(property.Name)) {
                variable =  scope.GetVariable(RubyUtils.TryMangleName(property.Name));
            } else {
                variable = scope.GetVariable(property.Name);
            }
            variable.TryGetValue(out res);
            return res;
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value to set.</param>
        public void SetPropertyValue(TaskPropertyInfo property, object value) {
            Contract.Requires(property != null);

            ((ScriptScope)this.scope).SetVariable(property.Name, value);
        }

        #region Dispose pattern

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // Dispose of referenced objects implementing IDisposable here.
            }
        }

        #endregion

    }
}
