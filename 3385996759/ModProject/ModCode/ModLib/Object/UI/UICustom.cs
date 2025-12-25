using ModLib.Helper;
using System;
using UnityEngine;

namespace ModLib.Object
{
    /// <summary>
    /// Provides a customizable user interface container for a specific UI type, supporting navigation and lifecycle
    /// management.
    /// </summary>
    /// <remarks>The <c>UICustom&lt;T&gt;</c> class serves as a base for creating custom UI panels or windows
    /// that encapsulate a specific <typeparamref name="T"/> UI instance. It manages the creation, navigation controls,
    /// and disposal of the underlying UI, ensuring that only one active instance exists at a time. The most recently
    /// created <c>UICustom&lt;T&gt;</c> is accessible via <see cref="LastUICustom"/>.</remarks>
    /// <typeparam name="T">The type of UI to be managed by this container. Must inherit from <see cref="UIBase"/>.</typeparam>
    public abstract class UICustom<T> : UICustomBase where T : UIBase
    {
        /// <summary>
        /// Gets the most recently created <see cref="UICustom{T}"/> instance.
        /// </summary>
        public static UICustom<T> LastUICustom { get; private set; } = null;

        /// <summary>
        /// Gets the UI component associated with this instance.
        /// </summary>
        public T UI { get; private set; }

        /// <summary>
        /// Initializes a new instance of the UICustom class and sets up the custom UI, including
        /// navigation buttons.
        /// </summary>
        /// <remarks>This constructor creates and displays a custom UI of the specified type, replacing
        /// any previously active custom UI. It also adds navigation buttons for paging functionality and registers the
        /// instance with the UI helper system.</remarks>
        public UICustom() : base()
        {
            //init
            DeleteLastUI();
            UI = g.ui.OpenUISafe<T>(UIType.GetUIType(UITypeName));
            LastUICustom = this;
            UIBase = UI;

            //navigation buttons
            UnPaging = true;
            {
                PrevButton = AddButton(FirstCol, LastRow, () => PrevPage(), GameTool.LS("libtxt999990008")).Format(Color.black, 13).Size(80, 40)
                    .Active(IsShowNavigationButtons).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((item) => new object[] { CurrentPageIndex + 0 })
                });
                NextButton = AddButton(LastCol, LastRow, () => NextPage(), GameTool.LS("libtxt999990008")).Format(Color.black, 13).Size(80, 40)
                    .Active(IsShowNavigationButtons).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>((item) => new object[] { CurrentPageIndex + 2 })
                });
            }
            UnPaging = false;

            //test
            //for (var c = 0; c < Columns.Count; c++)
            //    for (var r = 0; r < Rows.Count; r++)
            //        AddText(c, r, $"{c}/{r}");

            UIHelper.UIs.Add(this);
            DebugHelper.WriteLine($"Create a UICustom for {UI.uiType.uiName}");
        }

        /// <summary>
        /// Releases resources held by the last custom UI element and removes its reference.
        /// </summary>
        /// <remarks>—Call this method to dispose of the most recently used custom UI element and set its
        /// reference to <see langword="null"/>. If no custom UI element exists, the method has no effect.</remarks>
        private void DeleteLastUI()
        {
            if (LastUICustom != null)
            {
                LastUICustom.Dispose();
                LastUICustom = null;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the UICustom class.
        /// </summary>
        /// <remarks>Call this method when the UICustom instance is no longer needed to free associated
        /// resources and perform necessary cleanup. After calling <see cref="Dispose"/>, the instance should not be
        /// used.</remarks>
        public override void Dispose()
        {
            DebugHelper.WriteLine($"Dispose the UICustom of {UI.uiType.uiName}");
            UIHelper.UIs.Remove(this);
            Clear();
            if (this?.UI?.uiType != null && g.ui.HasUI(this.UI.uiType))
                g.ui.CloseUI(this.UI);
            GC.Collect();
        }
    }

    /// <summary>
    /// Represents a customizable UI dialog with a title, configurable button text, and optional actions for OK and
    /// Cancel buttons.
    /// </summary>
    /// <remarks><para> <see cref="UICustom1"/> provides a dialog interface that allows developers to specify
    /// the dialog title, the text for the OK button, and actions to execute when the OK or Cancel buttons are clicked.
    /// The Cancel button is displayed only if explicitly enabled. </para> <para> This class is typically used to prompt
    /// users for confirmation or to display informational messages with optional user responses. The dialog's size
    /// constraints are determined dynamically based on the current screen dimensions. </para></remarks>
    public class UICustom1 : UICustom<UITextInfoLong>
    {
        /// <summary>
        /// Gets the display name of the UI type associated with this object.
        /// </summary>
        public override string UITypeName => UIType.TextInfoLong.uiName;
        /// <summary>
        /// Gets the minimum width, in pixels, that the UI element can occupy.
        /// </summary>
        public override float MinWidth => -(UIHelper.GetUIScreenWidth() / 2) * 0.630f;
        /// <summary>
        /// Gets the maximum allowable width for the UI element, in pixels.
        /// </summary>
        public override float MaxWidth => +(UIHelper.GetUIScreenWidth() / 2) * 0.660f;
        /// <summary>
        /// Gets the minimum height, in pixels, that the UI element can occupy.
        /// </summary>
        public override float MinHeight => +(UIHelper.GetUIScreenHeight() / 2) * 0.640f;
        /// <summary>
        /// Gets the maximum vertical position, in pixels, that the UI element can occupy relative to the screen center.
        /// </summary>
        public override float MaxHeight => -(UIHelper.GetUIScreenHeight() / 2) * 0.660f;

        /// <summary>
        /// Initializes a new instance of the <see cref="UICustom1"/> class with the specified title, button text, and
        /// actions for the OK and Cancel buttons.
        /// </summary>
        /// <remarks>Use this constructor to create a customizable dialog with optional OK and Cancel
        /// actions. The Cancel button is only shown if <paramref name="showCancel"/> is <see
        /// langword="true"/>.</remarks>
        /// <param name="title">The title to display in the UI dialog. Cannot be <see langword="null"/>.</param>
        /// <param name="btnText">The text to display on the OK button. If not specified, the default text is used.</param>
        /// <param name="okAct">The action to execute when the OK button is clicked. If <see langword="null"/>, no action is performed.</param>
        /// <param name="showCancel"><see langword="true"/> to display the Cancel button; otherwise, <see langword="false"/>.</param>
        /// <param name="cancelAct">The action to execute when the Cancel button is clicked. If <see langword="null"/>, no action is performed.</param>
        public UICustom1(string title, string btnText = "", Action okAct = null, bool showCancel = false, Action cancelAct = null) : base()
        {
            UI.InitData(title, string.Empty, btnText, ActionHelper.TracedIl2Action(okAct), showCancel);
            if (cancelAct != null)
                UI.btnCancel.onClick.AddListener(ActionHelper.TracedUnityAction(cancelAct));
        }
    }

    /// <summary>
    /// Represents a custom UI component for displaying text information with configurable title and button text.
    /// </summary>
    /// <remarks><para> <see cref="UICustom2"/> provides a specialized user interface element for presenting
    /// text-based information, allowing customization of the title and button label. It derives from <see
    /// cref="UICustom{T}"/> with <see cref="UITextInfo"/> as the data type, and exposes properties for UI type
    /// identification and layout constraints. </para> <para> The minimum and maximum width and height properties are
    /// dynamically calculated based on the current screen size, ensuring responsive layout behavior. </para></remarks>
    public class UICustom2 : UICustom<UITextInfo>
    {
        /// <summary>
        /// Gets the display name of the UI type associated with this object.
        /// </summary>
        public override string UITypeName => UIType.TextInfo.uiName;
        /// <summary>
        /// Gets the minimum width, in pixels, that the UI element can occupy.
        /// </summary>
        public override float MinWidth => -(UIHelper.GetUIScreenWidth() / 2) * 0.270f;
        /// <summary>
        /// Gets the maximum allowable width for the UI element, in pixels.
        /// </summary>
        public override float MaxWidth => +(UIHelper.GetUIScreenWidth() / 2) * 0.300f;
        /// <summary>
        /// Gets the minimum height, in pixels, for the UI element.
        /// </summary>
        public override float MinHeight => +(UIHelper.GetUIScreenHeight() / 2) * 0.200f;
        /// <summary>
        /// Gets the maximum allowable height for the UI element, relative to the screen size.
        /// </summary>
        public override float MaxHeight => -(UIHelper.GetUIScreenHeight() / 2) * 0.200f;

        /// <summary>
        /// Initializes a new instance of the <see cref="UICustom2"/> class with the specified title, button text, and
        /// action to execute when the OK button is pressed.
        /// </summary>
        /// <param name="title">The title to display in the UI dialog. Cannot be <see langword="null"/>.</param>
        /// <param name="btnText">The text to display on the OK button. If not specified, the button will use a default label.</param>
        /// <param name="okAct">An action to execute when the OK button is pressed. If <see langword="null"/>, no action is performed.</param>
        public UICustom2(string title, string btnText = "", Action okAct = null) : base()
        {
            UI.InitData(title, string.Empty, btnText, ActionHelper.TracedIl2Action(okAct));
        }
    }
}