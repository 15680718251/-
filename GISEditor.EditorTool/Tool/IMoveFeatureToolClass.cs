namespace GISEditor.EditorTool.Tool
{
    interface IMoveFeatureToolClass
    {
        int Bitmap { get; }
        string Caption { get; }
        string Category { get; }
        bool Checked { get; }
        int Cursor { get; }
        bool Deactivate();
        bool Enabled { get; }
        int HelpContextID { get; }
        string HelpFile { get; }
        string Message { get; }
        string Name { get; }
        void OnClick();
        bool OnContextMenu(int x, int y);
        void OnCreate(object Hook);
        void OnDblClick();
        void OnKeyDown(int keyCode, int shift);
        void OnKeyUp(int keyCode, int shift);
        void OnMouseDown(int button, int shift, int x, int y);
        void OnMouseMove(int button, int shift, int x, int y);
        void OnMouseUp(int button, int shift, int x, int y);
        void Refresh(int hdc);
        string Tooltip { get; }
    }
}
