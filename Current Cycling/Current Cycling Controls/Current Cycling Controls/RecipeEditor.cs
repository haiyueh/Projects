using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Current_Cycling_Controls {
    public partial class RecipeEditor<TType> : Form {

        /// <summary>
        /// Used for populating comboBox
        /// </summary>
        private List<string> _recipeList;
        public CCRecipe CurrentRecipe { get; set; }
        private readonly Data _conn;
        private DataTable _recipeData;
        private readonly BackgroundWorker _dataWorker = new BackgroundWorker();
        private bool _loading, _update;
        private List<PropertyInfo> _recipeProperties;
        private string _selectedSample;

        private readonly string[] _excluded = { "Id", "Table", "Active", "Selected", "Updated", "Created", "Name", "CycleNumber" };
        private readonly string[] _readonly = { "Updated", "Created", "SampleName" };

        public RecipeEditor(Data conn, CCRecipe recipe, string sample) {
            InitializeComponent();
            _recipeList = new List<string>();
            _selectedSample = sample;
            _conn = conn;
            CurrentRecipe = recipe;
            _dataWorker.DoWork += RunDataWorker;
            _dataWorker.WorkerReportsProgress = true;
            _dataWorker.ProgressChanged += UpdateData;

            Shown += UpdateUi;
        }

        /// <summary>
        /// Initializes Recipe editor in the form with the correct Recipe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateUi(object sender, EventArgs e) {
            _dataWorker.RunWorkerAsync(0);
        }

        /// <summary>
        /// Updates comboBox with newest Samples and starts the InitializeTable function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateData(object sender, ProgressChangedEventArgs e) {

            comboBoxSelect.Items.Clear();
            _loading = true;
            comboBoxSelect.Items.Add("Select a Recipe to Update or Load");
            comboBoxSelect.Items.Add("Create new recipe");
            comboBoxSelect.SelectedIndex = 0;
            if (_recipeList == null || _recipeList.Count == 0) {
                InitializeTable(false);
                return;
            }
            var index = 1;
            foreach (var r in _recipeList) {
                comboBoxSelect.Items.Add(r);
                index++;
                if (r == _selectedSample) {
                    CurrentRecipe = _conn.GetCurrentRecipe(_selectedSample);
                    comboBoxSelect.SelectedIndex = index;
                }

            }


            _loading = false;
            InitializeTable(false);
        }

        private void InitializeTable(bool readOnly) {
            _recipeData = new DataTable();
            _recipeData.Columns.Add("Parameter");
            _recipeData.Columns.Add("Value");
            dataViewer.DataSource = _recipeData;
            dataViewer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataViewer.Columns[0].ReadOnly = true;
            dataViewer.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataViewer.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataViewer.Columns[0].FillWeight = 50;

            _recipeProperties = (from p in CurrentRecipe.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) select p).ToList();

            DataGridViewCellStyle dvc;

            var row = 0;

            foreach (var property in _recipeProperties) {
                if (_excluded.Any(x => x.Equals(property.Name))) continue;
                dvc = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                };
                var val = property.GetValue(CurrentRecipe, null);
                _recipeData.Rows.Add(property.Name, val);

                // if bool change to checkbox
                if (val is bool) {
                    dvc.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataViewer[1, row] = new DataGridViewCheckBoxCell { Value = val, Style = dvc };
                }

                if (!property.CanWrite || _readonly.Any(x => x.Equals(property.Name))
                    && readOnly) {
                    dataViewer[1, row].ReadOnly = true;
                    dvc.ForeColor = Color.DimGray;
                }

                dataViewer[1, row].Style = dvc;

                row++;
            }

        }

        private bool GetValueFromString(object o, string value, out object newvalue) {
            newvalue = null;
            try {
                if (o == null) throw new Exception("Incompatible type!");

                if (o.Equals(typeof(double))) {
                    newvalue = double.Parse(value);
                }
                else if (o.Equals(typeof(int))) {
                    newvalue = int.Parse(value);
                }
                else if (o.Equals(typeof(long))) {
                    newvalue = long.Parse(value);
                }
                else if (o.Equals(typeof(string))) {
                    newvalue = value;
                }
                else if (o.Equals(typeof(bool))) {
                    newvalue = Convert.ToBoolean(value);
                }
                else if (o.Equals(typeof(DateTime))) {
                    newvalue = DateTime.Parse(value);
                }
            }
            catch (Exception exc) {
                Debug.WriteLine(exc.ToString());
                return false;
            }

            return newvalue != null;
        }

        private void comboBoxSelect_SelectedIndexChanged(object sender, EventArgs e) {
            if (_loading || comboBoxSelect.SelectedIndex <= 0) return;

            // creates new default recipe and lets user write to it
            if (comboBoxSelect.SelectedIndex == 1) {
                CurrentRecipe = new CCRecipe();
                InitializeTable(false);
            }
            // select recipe already there
            else {
                _selectedSample = comboBoxSelect.SelectedItem.ToString();
                CurrentRecipe = _conn.GetCurrentRecipe(_selectedSample);
                InitializeTable(true);
            }


        }

        /// <summary>
        /// Saves the dataTable to the CurrentRecipe
        /// </summary>
        private void UpdateRecipe() {
            foreach (DataRow r in _recipeData.Rows) {
                foreach (var p in _recipeProperties.Where(p => p.Name.Equals(r[0]) && p.CanWrite && p.PropertyType != typeof(DateTime))) {
                    if (!GetValueFromString(p.PropertyType, r[1].ToString(), out var newVal)) continue;
                    if (newVal is string) {
                        if ((string)newVal == "") return;
                    }
                    p.SetValue(CurrentRecipe, newVal);
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e) {
            UpdateRecipe();
            Close();
            //_dataWorker.RunWorkerAsync(1);
        }

        private void RunDataWorker(object sender, DoWorkEventArgs e) {
            switch (e.Argument) {
                case 0:
                    break;
                case 1:
                    if (CurrentRecipe == null) return;
                    _selectedSample = CurrentRecipe.SampleName;
                    CurrentRecipe.Selected = true;
                    break;
            }
            _recipeList = _conn.GetRecipeList<CCRecipe>();
            _dataWorker.ReportProgress(int.Parse(e.Argument.ToString()));
        }




    }
}
