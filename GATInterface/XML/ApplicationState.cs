using System;
using System.Collections.Generic;
using System.Xml;
using GATUtils.Utilities;
using GATUtils.Utilities.FileUtils;

namespace GATInterface.XML
{
    public class ApplicationState
    {
        public enum FormView { OrderLog, Position }
        
        public static ApplicationState Instance { get { return _instance ?? (_instance = new ApplicationState()); } }

        public FormView CurrentFormView
        {
            get
            {
                if (string.IsNullOrEmpty(_currentView))
                {
                    XmlNode tempNode = _sSettingsDocument.GetElementsByTagName(StateSettingFields.View).Item(0);
                    _currentView = tempNode != null ? tempNode.InnerText : "Order";
                }

                if (_currentView.ToUpper().Contains("ORD"))
                    return FormView.OrderLog;

                return FormView.Position;
            }

            set 
            {
                switch (value)
                {
                    case FormView.OrderLog:
                        _currentView = "Order";
                        break;
                    case FormView.Position:
                        _currentView = "Position";
                        break;
                    default:
                        throw new Exception("Unhandled View type " + value.ToString());
                }

                _shouldUpdateFile = true;
            }
        }

        public DateTime DisplayDate
        {
            get
            {
                if (_displayDate == default(DateTime))
                {
                    XmlNode tempNode = _sSettingsDocument.GetElementsByTagName(StateSettingFields.DisplayDate).Item(0);
                    _displayDate = tempNode != null ? DateTime.Parse(tempNode.InnerText) : MyTime.Today;
                }

                return _displayDate;
            }

            set 
            {
                _displayDate = value;
                _shouldUpdateFile = true;
            }
        }

        public List<FilterItem> Filter
        {
            get
            { 
                if (_filter == null)
                {
                    XmlNodeList tempNodeList = _sSettingsDocument.GetElementsByTagName(StateSettingFields.FilterItem);
                    //_displayDate = tempNode != null ? DateTime.Parse(tempNode.InnerText) : MyTime.Today;
                    _filter = new List<FilterItem>();
                    foreach (XmlNode filterItem in tempNodeList)
                    {
                        FilterItem filter = new FilterItem();
                        if (filterItem.Attributes != null) 
                            filter.Column = filterItem.Attributes["name"].InnerText;
                        else
                            continue;

                        filter.Value = filterItem.InnerText;
                        _filter.Add(filter);
                    }
                }

                return _filter;
            }

            set 
            { 
                _filter = value;
                _shouldUpdateFile = true;
            }
        }
        
        public ApplicationState()
        {
            _stateDocumentPath = GatFile.Path(Dir.ApplicationSettings, ct_settingDocumentName);
            _LoadSettings();
        }

        private void _LoadSettings()
        {
            _sSettingsDocument = new XmlDocument(); 
            _sSettingsDocument.Load(_stateDocumentPath);
        }

        private XmlDocument _sSettingsDocument;
        private bool _shouldUpdateFile;

        private readonly string _stateDocumentPath;
        
        private const string ct_settingDocumentName = "LastState.xml";
        private static ApplicationState _instance;
        
        private DateTime _displayDate;
        private string _currentView;
        private List<FilterItem> _filter;

        private static class StateSettingFields
        {
            public static string DocumentName { get { return "AppState"; } }
            public static string View { get { return "CurrentView"; } }
            public static string DisplayDate { get { return "DisplayDate"; } }
            public static string FilterGroup { get { return "Filter"; } }
            public static string FilterItem { get { return "Filter/Column"; } }
        }
    }

    public class FilterItem
    {
        public string Column { get; set; }
        public string Value { get; set; }
    }
}
