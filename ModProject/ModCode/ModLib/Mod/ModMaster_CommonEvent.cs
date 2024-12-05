using UnityEngine;

namespace ModLib.Mod
{
    public abstract partial class ModMaster : MonoBehaviour
    {
        #region ModLib - Events
        public virtual void OnInitMod()
        {
        }

        public virtual void OnInitConf()
        {
            //load configs
            ConfHelper.LoadCustomConf();
        }

        public virtual void OnInitEObj()
        {
            GameHelper.LoadEnumObj(GameHelper.GetModMasterAssembly());
            GameHelper.LoadEnumObj(GameHelper.GetModMainAssembly());
        }
        #endregion
    }
}