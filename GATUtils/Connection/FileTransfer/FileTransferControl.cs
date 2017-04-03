using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using GATUtils.XML;

namespace GATUtils.Connection.FileTransfer
{
    public class FileTransferControl
    {
        public static FileTransferControl Instance { get { return _instance ?? (_instance = new FileTransferControl());  } }

        public FileTransferObject this[string name]
        {
            get 
            { 
                FileTransferObject requestedHandle = null;
                _transferObjects.TryGetValue(name, out requestedHandle);
                return requestedHandle;
            }
        }

        public bool IsActice { get; private set; }

        public FileTransferControl()
        {
            XmlDocument settingsDoc = XmlSettings.Instance.SettingDocument;
            XmlNode transferSources = settingsDoc.GetElementsByTagName("FileTransfer").Item(0);
            
            _transferObjects = new Dictionary<string, FileTransferObject>();
            foreach (XmlNode source in transferSources.ChildNodes)
            {
                if (source.Name.Equals("TransfersActive"))
                {
                    IsActice = XmlTools.ResolveBooleanValue(source.InnerText.Trim());
                    continue;
                }

                FileTransferObject transferObject = new FileTransferObject(source);
                _transferObjects.Add(transferObject.Name, transferObject);
            }
        }

        private Dictionary<string, FileTransferObject> _transferObjects;

        private static FileTransferControl _instance;
    }
}
