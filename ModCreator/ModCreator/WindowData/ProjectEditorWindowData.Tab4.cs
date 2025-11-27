using ModCreator.Helpers;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Project editor window data layer - Tab 4 (Global Variables)
    /// </summary>
    public partial class ProjectEditorWindowData : CWindowData
    {
        #region Tab 4 Properties

        /// <summary>
        /// Global variables collection
        /// </summary>
        public ObservableCollection<GlobalVariable> GlobalVariables { get; set; } = new ObservableCollection<GlobalVariable>();

        /// <summary>
        /// Available variable types loaded from var-types.json
        /// </summary>
        public List<VarType> VarTypes { get; set; } = ResourceHelper.ReadEmbeddedResource<List<VarType>>("ModCreator.Resources.var-types.json");

        #endregion

        #region Tab 4 Methods

        /// <summary>
        /// Load global variables from project
        /// </summary>
        public void LoadGlobalVariables()
        {
            GlobalVariables = new ObservableCollection<GlobalVariable>(Project.GlobalVariables);
        }

        /// <summary>
        /// Save global variables to file
        /// </summary>
        public void SaveGlobalVariables()
        {
            if (Project == null) return;

            // Sync GlobalVariables ObservableCollection back to Project.GlobalVariables
            Project.GlobalVariables = new List<GlobalVariable>(GlobalVariables);
        }

        #endregion
    }
}
