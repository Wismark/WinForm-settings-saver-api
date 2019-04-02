using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace SettingsSaver
{

    public class SettingsSaver
    {
        private static SettingsSaver _instance;
        public static SettingsSaver Instance => _instance ?? (_instance = new SettingsSaver());

        private Dictionary<string, FormInfo> _formData = new Dictionary<string, FormInfo>(); 
        private string _settingsFileName;
        private bool _indent;

        public void Init(string settingFile="FormSettings.xml", bool indent=false)
        {
            _settingsFileName = settingFile;
            _indent = indent;
            _formData = LoadFromXml(_settingsFileName);
        }

        public void SaveData()
        {
            var serializer = new XmlSerializer(typeof(List<FormInfo>));

            Stream fs = new FileStream(_settingsFileName, FileMode.Create);
            XmlWriter writer = XmlWriter.Create(fs,
                new XmlWriterSettings { Indent = _indent, Encoding = Encoding.Unicode });
            serializer.Serialize(writer, _formData.Values.ToList());
            writer.Close();
            fs.Close();
            fs.Dispose();
        }

        public void LookAfterForm(Form form)
        {
            form.Load += Form_Load;
            form.FormClosed += FormClosed;         
        }

        private void FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveFormSettings((Form)sender);
        }

        private void Form_Load(object sender, EventArgs e)
        {
            RestoreFormSettings((Form)sender);
        }

        private void SaveFormSettings(Form form)
        {
            var grids = GetAllControlsByType(form, typeof(DataGridView));
            Size formSize;
            Point formLocation;
            if (form.WindowState == FormWindowState.Maximized)
            {
                formSize = form.RestoreBounds.Size;
                formLocation = form.RestoreBounds.Location;
            }
            else
            {
                formSize = form.Size;
                formLocation = form.Location;
            }

            var gridsInfo = GetGridsWidthInfo(grids);

            var info = new FormInfo
            {
                Key = form.GetType().Namespace + '.' + form.Name,
                FormWidth = formSize.Width,
                FormHeight = formSize.Height,
                LocationX = formLocation.X,
                LocationY = formLocation.Y,
                GridsInfos = gridsInfo,
                Maximized = (form.WindowState == FormWindowState.Maximized)
            };

            _formData?.Remove(info.Key);
            if(_formData!=null) _formData.Add(info.Key,info);
            else
            {
                _formData = new Dictionary<string, FormInfo>(); 
                _formData.Add(info.Key, info);
            }
        }

        private void RestoreFormSettings(Form form)
        {
            FormInfo formInfo = null;
            if (_formData!=null)
            formInfo = _formData.SingleOrDefault(f => f.Value.Key == form.GetType().Namespace+'.'+form.Name).Value;

            if (formInfo!=null)
            {
                form.WindowState = formInfo.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
                form.Size = new Size(formInfo.FormWidth, formInfo.FormHeight);
                form.Location = new Point(formInfo.LocationX, formInfo.LocationY);
                SetGridsWidthInfo(form, formInfo.GridsInfos);
            }
        }

        private static Dictionary<string, FormInfo> LoadFromXml(string fileName)
        {
            if (File.Exists(fileName))
            {
                var serializer = new XmlSerializer(typeof(List<FormInfo>));
                StreamReader reader = new StreamReader(fileName);
                var tempInfo = (List<FormInfo>) serializer.Deserialize(reader);
                reader.Close();

                var formsInfo = new Dictionary<string, FormInfo>();

                foreach (var formInfo in tempInfo)
                {
                    formsInfo.Add(formInfo.Key,formInfo);
                }

                return formsInfo;
            }
            return null;
        }

        private static List<GridsInfo> GetGridsWidthInfo(IEnumerable<Control> grids)
        {
            var gridInfo = new List<GridsInfo>();
            foreach (var control in grids)
            {
                var widthList = new List<SomeInfo<int>>();
                var temp = (DataGridView)control;

                for (var i = 0; i < temp.Columns.Count; i++)
                {
                    widthList.Add(new SomeInfo<int>() { Value = temp.Columns[i].Width });
                }
                gridInfo.Add(new GridsInfo()
                {
                    GridName = temp.Name,
                    GridWidths = widthList
                });
            }
            return gridInfo;
        }

        private static void SetGridsWidthInfo(Form form, List<GridsInfo> gridsInfos)
        {
            var grids = GetAllControlsByType(form, typeof(DataGridView));

            foreach (var control in grids)
            {

                var temp = (DataGridView)control;
                var tempGridWidths = gridsInfos.Single(g => g.GridName == temp.Name).GridWidths;

                if(tempGridWidths.Count>0)
                for (var i = 0; i < temp.Columns.Count; i++)
                {
                    temp.Columns[i].Width = tempGridWidths.First().Value;
                    tempGridWidths.RemoveAt(0);
                }
            }
        }

        private static IEnumerable<Control> GetAllControlsByType(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            var second = controls.ToList();
            return second.SelectMany(ctrl => GetAllControlsByType(ctrl, type))
                .Concat(second)
                .Where(c => c.GetType() == type);
        }

    }

}
