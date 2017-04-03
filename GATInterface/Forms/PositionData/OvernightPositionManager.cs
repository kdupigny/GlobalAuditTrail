using System.Collections.Generic;
using System.Threading;
using GATUtils.Connection.FileTransfer;
using System;
using GATUtils.Utilities.FileUtils;
using System.IO;
using GATUtils.Logger;

namespace GATInterface.Forms.PositionData
{
    public class OvernightPositionManager
    {
        public static OvernightPositionManager Instance
        {
            get { return _instance ?? (_instance = new OvernightPositionManager()); }
        }

        public OvernightPositionManager()
        {

            _overnightPosition = new Dictionary<string, Dictionary<string, OvernightPositionObject>>();
            _DownLoadAndProcessPositionFiles();
        }

        public void Initialize()
        { }

        public Dictionary<string, OvernightPositionObject> GetAccountPositions(string acc)
        {
            Dictionary<string, OvernightPositionObject> output = null;

            _overnightPosition.TryGetValue(acc, out output);
            return output;
        }

        private void _DownLoadAndProcessPositionFiles()
        {
            GatLogger.Instance.AddMessage("Pulling today's position files.", LogMode.LogAndScreen);

            Ftp ftpHandle = new Ftp("54.245.114.32", "usr_vtbullseye", "rod30cl0wn!");
            ftpHandle.ChangeRemoteWorkingDirectory("PositionFiles");

            string fileDateString = DateTime.Now.ToString("yyMMdd");
            List<string> downloadedList = new List<string>();

            foreach (IRemoteFile remoteFile in ftpHandle.GetWorkingDirContents())
            {
                if (remoteFile.Filename.Contains(fileDateString))
                {
                    GatLogger.Instance.AddMessage(string.Format("Found file {0}", remoteFile.Filename), LogMode.LogAndScreen);
                    
                    string downloadedFile = GatFile.Path(Dir.Temp, remoteFile.Filename);
                    ftpHandle.Pull(remoteFile.Filename, Path.GetDirectoryName(downloadedFile));

                    if (File.Exists(downloadedFile))
                    {
                        downloadedList.Add(downloadedFile);
                        _ProcessPositionFile(downloadedFile);
                    }
                }    
            }

            Thread.Sleep(3000);

            foreach (string file in downloadedList)
            {
                File.Delete(file);
            }
        }

        private void _ProcessPositionFile(string filename)
        {
            string line;
            using (TextReader tr = new StreamReader(filename))
            {
                tr.ReadLine();
                while ((line = tr.ReadLine()) != null)
                {
                    string[] fields = line.Split(',');

                    if (fields.Length >= 16)
                    {
                        int pos;
                        int.TryParse(fields[(int) PositionFileFields.CurrentPos], out pos);
                        decimal strikePx, closePx;
                        decimal.TryParse(fields[(int) PositionFileFields.StrikePx], out strikePx);
                        decimal.TryParse(fields[(int) PositionFileFields.CloseMark], out closePx);

                        var oPosition = new OvernightPositionObject(fields[(int) PositionFileFields.Account],
                                                                    fields[(int) PositionFileFields.SubAccount],
                                                                    fields[(int) PositionFileFields.Symbol],
                                                                    fields[(int) PositionFileFields.Product],
                                                                    pos,
                                                                    strikePx,
                                                                    closePx,
                                                                    fields[(int) PositionFileFields.Cusip]);

                        Dictionary<string, OvernightPositionObject> positions;
                        if (_overnightPosition.TryGetValue(oPosition.FullAccount, out positions))
                        {
                            if (!positions.ContainsKey(oPosition.Symbol))
                                positions.Add(oPosition.Symbol, oPosition);
                        }
                        else
                        {
                            positions = new Dictionary<string, OvernightPositionObject>();
                            positions.Add(oPosition.Symbol, oPosition);

                            _overnightPosition.Add(oPosition.FullAccount, positions);
                        }
                    }
                }
            }
        }

        private Dictionary<string, Dictionary<string, OvernightPositionObject>> _overnightPosition;

        private static OvernightPositionManager _instance;
        private enum PositionFileFields { TradeFirm, ClearFirm, Account, SubAccount, AccGroup, Symbol, Product, Class, Expiration, StrikePx, CurrentPos,TradePos,YesterdayPos, CloseMark,Currency,Usdfx, Cusip }
    }

    public class OvernightPositionObject
    {
        public string MainAccount { get; private set; }
        public string SubAccount { get; private set; }
        public string FullAccount { get { return string.Format("{0}{1}", MainAccount, SubAccount); } }
        public string Symbol { get; private set; }
        public string ProductType { get; private set; }
        public int Position { get; private set; }
        public decimal StrikePx { get; private set; }
        public decimal ClosePx { get; private set; }
        public string Cusip { get; private set; }

        public OvernightPositionObject(string account, string subAccount, string symbol, string product, int position, decimal strikepx, decimal price, string cusip)
        {
            MainAccount = account;
            SubAccount = subAccount;
            Symbol = symbol;
            ProductType = product;
            Position = position;
            StrikePx = strikepx;
            ClosePx = price;
            Cusip = cusip;
        }

        public void UpdatePosition(int position, decimal strikepx, decimal closePx)
        {
            
        }
    }
}
