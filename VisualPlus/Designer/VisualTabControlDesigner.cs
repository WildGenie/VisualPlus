﻿namespace VisualPlus.Designer
{
    #region Namespace

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    using VisualPlus.EventArgs;
    using VisualPlus.Native;
    using VisualPlus.Toolkit.Child;
    using VisualPlus.Toolkit.Controls.Navigation;

    #endregion

    internal class VisualTabControlDesigner : ParentControlDesigner
    {
        #region Variables

        private IDesignerHost _designerHost;
        private DesignerVerbCollection _designerVerbCollection;
        private ISelectionService _selectionService;

        #endregion

        #region Constructors

        public VisualTabControlDesigner()
        {
            DesignerVerb verb1 = new DesignerVerb("Add Tab", OnAddPage);
            DesignerVerb verb2 = new DesignerVerb("Insert Tab", OnInsertPage);
            DesignerVerb verb3 = new DesignerVerb("Remove Tab", OnRemovePage);

            _designerVerbCollection = new DesignerVerbCollection();
            _designerVerbCollection.AddRange(new[] { verb1, verb2, verb3 });
        }

        public delegate void VisualTabControlEventHandler(object sender, VisualTabControlEventArgs e);

        #endregion

        #region Properties

        public IDesignerHost DesignerHost
        {
            get
            {
                if (_designerHost == null)
                {
                    _designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
                }

                return _designerHost;
            }
        }

        /// <summary>Fix: the AllSizable selection rule on DockStyle.Fill</summary>
        public override SelectionRules SelectionRules
        {
            get
            {
                if (Control.Dock == DockStyle.Fill)
                {
                    return SelectionRules.Visible;
                }

                return base.SelectionRules;
            }
        }

        public ISelectionService SelectionService
        {
            get
            {
                if (_selectionService == null)
                {
                    _selectionService = (ISelectionService)GetService(typeof(ISelectionService));
                }

                return _selectionService;
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (_designerVerbCollection.Count != 3)
                {
                    return _designerVerbCollection;
                }

                VisualTabControl _tabControl = (VisualTabControl)Control;
                if (_tabControl.TabCount > 0)
                {
                    _designerVerbCollection[1].Enabled = true;
                    _designerVerbCollection[2].Enabled = true;
                }
                else
                {
                    _designerVerbCollection[1].Enabled = false;
                    _designerVerbCollection[2].Enabled = false;
                }

                return _designerVerbCollection;
            }
        }

        #endregion

        #region Events

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);

            VisualTabPage _tabPage1 = (VisualTabPage)DesignerHost.CreateComponent(typeof(VisualTabPage));
            VisualTabPage _tabPage2 = (VisualTabPage)DesignerHost.CreateComponent(typeof(VisualTabPage));

            _tabPage1.Text = _tabPage1.Name;
            _tabPage2.Text = _tabPage2.Name;

            Control.Controls.Add(_tabPage1);
            Control.Controls.Add(_tabPage2);
        }

        protected override bool GetHitTest(Point point)
        {
            if (SelectionService.PrimarySelection == Control)
            {
                TCHITTESTINFO _tabControlHitTestInfo = new TCHITTESTINFO
                    {
                        Point = Control.PointToClient(point),
                        Flags = 0
                    };

                Message _message = new Message
                    {
                        HWnd = Control.Handle,
                        Msg = Constants.TCM_HITTEST
                    };

                IntPtr _longIntParameter = Marshal.AllocHGlobal(Marshal.SizeOf(_tabControlHitTestInfo));
                Marshal.StructureToPtr(_tabControlHitTestInfo, _longIntParameter, false);
                _message.LParam = _longIntParameter;

                base.WndProc(ref _message);
                Marshal.FreeHGlobal(_longIntParameter);

                if (_message.Result.ToInt32() != -1)
                {
                    return _tabControlHitTestInfo.Flags != TabControlHitTest.TCHT_NOWHERE;
                }
            }

            return false;
        }

        protected override void OnPaintAdornments(PaintEventArgs paintEventArgs)
        {
            // Don't want DrawGrid dots.
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            properties.Remove("ImeMode");
            properties.Remove("Padding");
            properties.Remove("FlatAppearance");
            properties.Remove("FlatStyle");

            properties.Remove("AutoEllipsis");
            properties.Remove("UseCompatibleTextRendering");

            // properties.Remove("Image");
            // properties.Remove("ImageAlign");
            // properties.Remove("ImageIndex");
            // properties.Remove("ImageKey");
            // properties.Remove("ImageList");
            // properties.Remove("TextImageRelation");

            // properties.Remove("BackColor");
            // properties.Remove("BackgroundImage");
            // properties.Remove("BackgroundImageLayout");
            // properties.Remove("UseVisualStyleBackColor");

            // properties.Remove("ComponentFont");
            // properties.Remove("ForeColor");
            // properties.Remove("RightToLeft");
            base.PreFilterProperties(properties);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == Constants.WM_NCHITTEST)
            {
                // select tab control when Tab control clicked outside of TabItem.
                if (m.Result.ToInt32() == Constants.HTTRANSPARENT)
                {
                    m.Result = (IntPtr)Constants.HTCLIENT;
                }
            }
        }

        private void OnAddPage(object sender, EventArgs e)
        {
            VisualTabControl _parentalControl = (VisualTabControl)Control;
            Control.ControlCollection _controlCollection = _parentalControl.Controls;

            RaiseComponentChanging(TypeDescriptor.GetProperties(_parentalControl)["TabPages"]);

            VisualTabPage _tabPage = (VisualTabPage)DesignerHost.CreateComponent(typeof(VisualTabPage));
            _tabPage.Text = _tabPage.Name;
            _parentalControl.TabPages.Add(_tabPage);

            RaiseComponentChanged(TypeDescriptor.GetProperties(_parentalControl)["TabPages"], _controlCollection, _parentalControl.TabPages);
            _parentalControl.SelectedTab = _tabPage;

            SetVerbs();
        }

        private void OnInsertPage(object sender, EventArgs e)
        {
            VisualTabControl _parentalControl = (VisualTabControl)Control;
            Control.ControlCollection _controlCollection = _parentalControl.Controls;
            int _index = _parentalControl.SelectedIndex;

            RaiseComponentChanging(TypeDescriptor.GetProperties(_parentalControl)["TabPages"]);

            VisualTabPage _tabPage = (VisualTabPage)DesignerHost.CreateComponent(typeof(VisualTabPage));
            _tabPage.Text = _tabPage.Name;

            var _tabPageCollection = new VisualTabPage[_parentalControl.TabCount];

            // Starting at our Insert Position, store and remove all the tab pages.
            for (int i = _index; i <= _tabPageCollection.Length - 1; i++)
            {
                _tabPageCollection[i] = (VisualTabPage)_parentalControl.TabPages[_index];
                _parentalControl.TabPages.Remove(_parentalControl.TabPages[_index]);
            }

            // add the tab page to be inserted.
            _parentalControl.TabPages.Add(_tabPage);

            // then re-add the original tab pages.
            for (int i = _index; i <= _tabPageCollection.Length - 1; i++)
            {
                _parentalControl.TabPages.Add(_tabPageCollection[i]);
            }

            RaiseComponentChanged(TypeDescriptor.GetProperties(_parentalControl)["TabPages"], _controlCollection, _parentalControl.TabPages);
            _parentalControl.SelectedTab = _tabPage;

            SetVerbs();
        }

        private void OnRemovePage(object sender, EventArgs e)
        {
            VisualTabControl _parentalControl = (VisualTabControl)Control;
            Control.ControlCollection _controlCollection = _parentalControl.Controls;

            if (_parentalControl.SelectedIndex < 0)
            {
                return;
            }

            RaiseComponentChanging(TypeDescriptor.GetProperties(_parentalControl)["TabPages"]);

            DesignerHost.DestroyComponent(_parentalControl.TabPages[_parentalControl.SelectedIndex]);

            RaiseComponentChanged(TypeDescriptor.GetProperties(_parentalControl)["TabPages"], _controlCollection, _parentalControl.TabPages);

            SelectionService.SetSelectedComponents(new IComponent[] { _parentalControl }, SelectionTypes.Auto);

            SetVerbs();
        }

        private void SetVerbs()
        {
            VisualTabControl _parentalControl = (VisualTabControl)Control;

            switch (_parentalControl.TabPages.Count)
            {
                case 0:
                    {
                        Verbs[1].Enabled = false;
                        Verbs[2].Enabled = false;
                        break;
                    }

                case 1:
                    {
                        Verbs[1].Enabled = false;
                        Verbs[2].Enabled = true;
                        break;
                    }

                default:
                    {
                        Verbs[1].Enabled = true;
                        Verbs[2].Enabled = true;
                        break;
                    }
            }
        }

        #endregion

        #region Methods

        private enum TabControlHitTest
        {
            TCHT_NOWHERE = 1,
            TCHT_ONITEMICON = 2,
            TCHT_ONITEMLABEL = 4,
            TCHT_ONITEM = TCHT_ONITEMICON | TCHT_ONITEMLABEL
        }

        private struct TCHITTESTINFO
        {
            public Point Point;

            public TabControlHitTest Flags;
        }

        #endregion
    }
}