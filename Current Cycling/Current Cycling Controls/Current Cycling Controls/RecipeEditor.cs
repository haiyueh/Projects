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

        private List<Recipe<TType>> _recipeList;
        public Recipe<TType> CurrentRecipe { get; set; }
        private readonly Data _conn;
        private DataTable _recipeData;
        private readonly BackgroundWorker _dataWorker = new BackgroundWorker();
        private bool _loading, _update;
        private List<PropertyInfo> _recipeProperties;

        private Type ThisType => typeof(TType);

        private readonly string[] _excluded = { "Id", "Table", "Active", "Selected", "Updated", "Created", "Name" };
        private readonly string[] _top = { "Name", "Created", "Updated" };
        private readonly string[] _readonly = { "Updated", "Created" };

        public RecipeEditor(Data conn, Recipe<TType> recipe) {
            InitializeComponent();
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
        /// Update the recipe editor with new recipes Saved or initial recipes loaded. 
        /// Used to refresh comboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateData(object sender, ProgressChangedEventArgs e) {

            comboBoxSelect.Items.Clear();
            if (_recipeList == null || _recipeList.Count == 0) {
                InitializeTable();
                return;
            }
            _loading = true;
            comboBoxSelect.Items.Add("Select a Recipe to Modify");
            comboBoxSelect.SelectedIndex = 0;
            var index = 0;
            foreach (var r in _recipeList) {
                comboBoxSelect.Items.Add(r);
                index++;
                if (r.Selected) {
                    CurrentRecipe = r;
                    comboBoxSelect.SelectedIndex = index;
                }

            }


            comboBoxSelect.Items.Add("Create new recipe");
            _loading = false;

            InitializeTable();
        }

        /// <summary>
        /// Populates the Recipe Viewer with the CurrentRecipe properties
        /// </summary>
        private void InitializeTable() {
            _recipeData = new DataTable();
            _recipeData.Columns.Add("Parameter");
            _recipeData.Columns.Add("Value");
            dataViewer.DataSource = _recipeData;
            dataViewer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataViewer.Columns[0].ReadOnly = true;
            dataViewer.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataViewer.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataViewer.Columns[0].FillWeight = 200;

            _recipeProperties = (from p in CurrentRecipe.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) select p).ToList();

            DataGridViewCellStyle dvc;

            var row = 0;

            foreach (var t in _top) {
                var p = (from pr in _recipeProperties where pr.Name.Equals(t) select pr).First();

                dvc = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                };

                var val = p.GetValue(CurrentRecipe, null);
                _recipeData.Rows.Add(p.Name, val);
                if (_readonly.Any(x => x.Equals(p.Name))) {
                    dataViewer[1, row].ReadOnly = true;
                    dvc.ForeColor = Color.DimGray;
                }
                dataViewer[1, row].Style = dvc;
                row++;
            }

            foreach (var property in _recipeProperties) {
                if (_excluded.Any(x => x.Equals(property.Name))) continue;
                dvc = new DataGridViewCellStyle {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                };
                var val = property.GetValue(CurrentRecipe, null);
                _recipeData.Rows.Add(property.Name, val);

                if (val is bool) {
                    dvc.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataViewer[1, row] = new DataGridViewCheckBoxCell { Value = val, Style = dvc };
                }

                if (!property.CanWrite || _readonly.Any(x => x.Equals(property.Name))) {
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

        /// <summary>
        /// Selected new ComboBox index. Switch CurrentRecipe to the one selected.
        /// If Get new recipe then populate editor with default recipe. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxSelect_SelectedIndexChanged(object sender, EventArgs e) {
            if (_loading || comboBoxSelect.SelectedIndex <= 0) return;
            if (comboBoxSelect.SelectedIndex == comboBoxSelect.Items.Count - 1) {
                GetNewRecipe();

            }
            else {
                CurrentRecipe = (Recipe<TType>)comboBoxSelect.SelectedItem;
                _conn.SelectData(CurrentRecipe);
            }

            InitializeTable();
        }

        private void UpdateParameters() {
            foreach (DataRow r in _recipeData.Rows) {
                foreach (var p in _recipeProperties.Where(p => p.Name.Equals(r[0]) && p.CanWrite && p.PropertyType != typeof(DateTime))) {
                    if (!GetValueFromString(p.PropertyType, r[1].ToString(), out var newVal)) continue;

                    p.SetValue(CurrentRecipe, newVal);

                }
            }
        }

        private void GetNewRecipe() {
            if (ThisType == typeof(CCRecipe)) {
                object recipe = new CCRecipe();
                CurrentRecipe = (Recipe<TType>)recipe;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e) {
            UpdateParameters();
            _dataWorker.RunWorkerAsync(1);
        }

        private void buttonDelete_Click(object sender, EventArgs e) {
            _dataWorker.RunWorkerAsync(2);
        }

        private void RunDataWorker(object sender, DoWorkEventArgs e) {
            switch (e.Argument) {
                case 0:
                    break;
                case 1:
                    if (CurrentRecipe == null) return;
                    _conn.SaveData(CurrentRecipe);
                    _conn.SelectData(CurrentRecipe);
                    break;
                case 2:
                    if (CurrentRecipe == null) return;
                    _conn.DeleteData(CurrentRecipe);
                    break;
            }

            if (ThisType == typeof(CCRecipe)) {
                var list = _conn.GetRecipeList<CCRecipe>(CurrentRecipe.Table);
                _recipeList = new List<Recipe<TType>>();
                foreach (var l in list) {
                    _recipeList.Add(l as Recipe<TType>);
                }
            }

            //else _recipeList =  _conn.GetRecipeList<type>(CurrentRecipe.Table);
            _dataWorker.ReportProgress(int.Parse(e.Argument.ToString()));
        }

    }
}
