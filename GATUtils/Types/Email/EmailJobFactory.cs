using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GATUtils.Logger;

namespace GATUtils.Types.Email
{
    internal class EmailJobFactory
    {
        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        public EmailJobFactory()
        {
            _LoadEmailJobs();
        }

        /// <summary>
        /// Collection of all classes that implements the IEmailJob interface.
        /// </summary>
        public Dictionary<string, Type> EmailJobCollection { get; private set; }

        /// <summary>
        /// Creates an instance of a emailJob that implements the IEmailJob interface.
        /// </summary>
        /// <param name="emailJob">the name of the emailJob type.</param>
        /// <returns>returns an IEmailJob object.</returns>
        public IEmailJob CreateInstanceOf(string emailJob)
        {
            Type t = _GetTypeToCreate(emailJob);

            if (t != null)
                return Activator.CreateInstance(t) as IEmailJob;
            return null;
        }

        /// <summary>
        /// Creates an instance of a EmailJob that implements the IemailJob interface.
        /// </summary>
        /// <param name="emailJob">the type of emailJob.</param>
        /// <returns>returns an IemailJob object.</returns>
        public IEmailJob CreateInstanceOf(Type emailJob)
        {
            if (emailJob != typeof(IEmailJob))
                return null;
            return Activator.CreateInstance(emailJob) as IEmailJob;
        }

        /// <summary>
        /// Returns a Type based on a given string.
        /// </summary>
        /// <param name="emailJobName">the name of the emailJob type.</param>
        /// <returns></returns>
        private Type _GetTypeToCreate(string emailJobName)
        {
            return (from item in EmailJobCollection
                    where item.Key.ToLower().Contains(emailJobName.ToLower()) && emailJobName != string.Empty
                    select EmailJobCollection[item.Key]).FirstOrDefault();
        }

        /// <summary>
        /// Get all classes that implements the IemailJob interface from the executing assembly.
        /// </summary>
        private void _LoadEmailJobs()
        {
            EmailJobCollection = new Dictionary<string, Type>();

            try
            {
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (type.GetInterface(typeof (IEmailJob).ToString()) != null)
                    {
                        EmailJobCollection.Add(type.ToString().ToLower(), type);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                GatLogger.Instance.AddMessage(string.Format("Exception Thrown {0} \n\t {2} \n\t {1}", errorMessage, ex.StackTrace, ex.Message));
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage(string.Format("Exception Thrown {0} \n\t {1}", e.Message, e.StackTrace));
                throw;
            }
        }
    }
}
