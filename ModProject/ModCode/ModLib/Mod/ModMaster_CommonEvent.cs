using System.IO;
using System.Reflection;
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
            //copy new configs to debug folder
            var orgFolder = $"{ConfHelper.GetConfFolderPath()}\\..\\..\\..\\ModProject\\ModConf\\";
            if (Directory.Exists(orgFolder))
            {
                Directory.CreateDirectory(ConfHelper.GetConfFolderPath());
                foreach (var orgFile in Directory.GetFiles(orgFolder))
                {
                    File.Copy(orgFile, ConfHelper.GetConfFilePath(Path.GetFileName(orgFile)), true);
                }
            }
            //load configs
            ConfHelper.LoadCustomConf();
        }

        public virtual void OnInitEObj()
        {
            GameHelper.LoadEnumObj(Assembly.GetAssembly(typeof(ModMaster)));
            GameHelper.LoadEnumObj(Assembly.GetAssembly(ModObj.GetType()));
        }
        #endregion
    }
}