using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Object
{
    public class UIItemComposite : UIItemBase
    {
        public UIItemText Prefix { get; set; }
        public UIItemBase MainComponent { get; set; }
        public UIItemText Postfix { get; set; }
        public bool HidePostfixIfDisabled { get; set; } = false;

        public override UICustomBase UI => MainComponent.UI;
        public override UIItemBase Parent => MainComponent.Parent;
        public override Component Component => MainComponent.Component;
        public override UIItemData ItemData
        {
            get => MainComponent.ItemData;
            set
            {
                Prefix.ItemData = value;
                MainComponent.ItemData = value;
                Postfix.ItemData = value;
            }
        }
        public override UIItemWork ItemWork
        {
            get => MainComponent.ItemWork;
            set
            {
                Prefix.ItemWork = value;
                MainComponent.ItemWork = value;
                Postfix.ItemWork = value;
            }
        }
        public override bool Enable { get => MainComponent.Enable; set => MainComponent.Enable = value; }
        public override object Tag { get => MainComponent.Tag; set => MainComponent.Tag = value; }

        private UIItemComposite(UICustomBase ui) : base(ui) { }

        public static UIItemComposite CreateSlider(UICustomBase ui, float x, float y, string prefix, float min, float max, float def, string postfix = null, Slider copySource = null)
        {
            var rs = new UIItemComposite(ui);
            rs.Prefix = new UIItemText(ui, x, y, prefix);
            rs.Prefix.Item.Align(TextAnchor.MiddleRight);
            rs.Prefix.Parent = rs;
            rs.MainComponent = new UIItemSlider(ui, x + UIHelper.UICUSTOM_DELTA_X * 4, y, min, max, def, copySource);
            rs.MainComponent.Parent = rs;
            rs.Postfix = new UIItemText(ui, x + UIHelper.UICUSTOM_DELTA_X * 8, y, postfix);
            rs.Postfix.Item.Align(TextAnchor.MiddleLeft);
            rs.Postfix.Parent = rs;
            return rs;
        }

        public static UIItemComposite CreateToggle(UICustomBase ui, float x, float y, string prefix, bool def, string postfix = null, Toggle copySource = null)
        {
            var rs = new UIItemComposite(ui);
            rs.Prefix = new UIItemText(ui, x, y, prefix);
            rs.Prefix.Item.Align(TextAnchor.MiddleRight);
            rs.Prefix.Parent = rs;
            rs.MainComponent = new UIItemToggle(ui, x + UIHelper.UICUSTOM_DELTA_X * 2, y, def, copySource);
            rs.MainComponent.Parent = rs;
            rs.Postfix = new UIItemText(ui, x + UIHelper.UICUSTOM_DELTA_X * 4, y, postfix);
            rs.Postfix.Item.Align(TextAnchor.MiddleLeft);
            rs.Postfix.Parent = rs;
            return rs;
        }

        public static UIItemComposite CreateSelect(UICustomBase ui, float x, float y, string prefix, string[] selections, int def, string postfix = null, Toggle copySource = null)
        {
            var rs = new UIItemComposite(ui);
            rs.Prefix = new UIItemText(ui, x, y, prefix);
            rs.Prefix.Item.Align(TextAnchor.MiddleRight);
            rs.Prefix.Parent = rs;
            rs.MainComponent = new UIItemSelect(ui, x + UIHelper.UICUSTOM_DELTA_X * 2, y, selections, def, copySource);
            rs.MainComponent.Parent = rs;
            rs.Postfix = new UIItemText(ui, x + UIHelper.UICUSTOM_DELTA_X * 8, y, postfix);
            rs.Postfix.Item.Align(TextAnchor.MiddleLeft);
            rs.Postfix.Parent = rs;
            return rs;
        }

        public static UIItemComposite CreateInput(UICustomBase ui, float x, float y, string prefix, string def, string postfix = null, InputField copySource = null)
        {
            var rs = new UIItemComposite(ui);
            rs.Prefix = new UIItemText(ui, x, y, prefix);
            rs.Prefix.Item.Align(TextAnchor.MiddleRight);
            rs.Prefix.Parent = rs;
            rs.MainComponent = new UIItemInput(ui, x + UIHelper.UICUSTOM_DELTA_X * 4, y, def, copySource);
            rs.MainComponent.Parent = rs;
            rs.Postfix = new UIItemText(ui, x + UIHelper.UICUSTOM_DELTA_X * 8, y, postfix);
            rs.Postfix.Item.Align(TextAnchor.MiddleLeft);
            rs.Postfix.Parent = rs;
            return rs;
        }

        public override object Get()
        {
            return MainComponent.Get();
        }

        public override void Set(object input)
        {
            MainComponent.Set(input);
        }

        public void SetPercent(float percent, float min = float.MinValue, float max = float.MaxValue)
        {
            (MainComponent as UIItemSlider)?.SetPercent(percent, min, max);
        }

        public override void Update()
        {
            base.Update();
            Prefix.Update();
            MainComponent.Update();
            Postfix.Update();

            if (HidePostfixIfDisabled)
                Postfix.Item.gameObject.SetActive(IsEnable());
        }

        public override void Dispose()
        {
            UI.Items.Remove(this);

            Prefix.Dispose();
            MainComponent.Dispose();
            Postfix.Dispose();
        }

        public override bool IsActive()
        {
            return MainComponent.IsActive();
        }

        public override bool IsEnable()
        {
            return MainComponent.IsEnable();
        }

        public override void SetEnable(bool value)
        {
            Prefix.SetEnable(value);
            MainComponent.SetEnable(value);
            Postfix.SetEnable(value);
        }

        public override void Pos(float x, float y)
        {
            var oldMainPos = MainComponent.Pos();
            base.Pos(x, y); //move main
            var moveDis = MainComponent.Pos() - oldMainPos;

            Prefix.Pos(MainComponent, moveDis.x, moveDis.y);
            Postfix.Pos(MainComponent, moveDis.x, moveDis.y);
        }

        public override void Pos(UIItemBase org, float x, float y)
        {
            var oldMainPos = MainComponent.Pos();
            base.Pos(org, x, y); //move main
            var moveDis = MainComponent.Pos() - oldMainPos;

            Prefix.Pos(MainComponent, moveDis.x, moveDis.y);
            Postfix.Pos(MainComponent, moveDis.x, moveDis.y);
        }
    }
}