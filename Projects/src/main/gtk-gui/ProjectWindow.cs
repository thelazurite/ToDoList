﻿using System;
using Gdk;
using Gtk;
using Projects.main.backend;
using Action = Gtk.Action;
using Label = Gtk.Label;
using Menu = Gtk.Menu;
using MenuItem = Gtk.MenuItem;
using ProgressBar = Gtk.ProgressBar;
using TreeView = Gtk.TreeView;

namespace Projects.main
{
    public sealed partial class ProjectWindow
    {
        private Toolbar _categoryToolbar;

        private Action _addCategoryAction;
        private Action _removeCategoryAction;

        private Action _addTaskItemAction;
        private Action _removeTaskItemAction;

        private TreeView _categoryTreeView;
        private TreeViewColumn _categoryItemId;
        private TreeViewColumn _categoryItemName;
        private TreeViewColumn _categoryItemToggle;

        private Label _categoryDescriptionLabel;
        private TextView _categoryDescription;

        private TreeView _mainView;
        private TreeViewColumn _taskId;
        private TreeViewColumn _taskName;
        private TreeViewColumn _taskPriority;
        private TreeViewColumn _taskCategory;
        private TreeViewColumn _taskStartDate;
        private TreeViewColumn _taskDueDate;

        private Notebook _noteBook;

        private VBox _categorySidebar;
        public ListStore CategoryStore;
        public ListStore TaskStore;
        public Calendar Calendar;

        private ProgressBar _fileActionProgBar;

        private void BuildInterface()
        {
            Gui.Initialize(this);

            Name = "TodoList";
            Title = "Projects";
            WindowPosition = WindowPosition.Center;
            Icon = new Pixbuf(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Content\img\todo.png"));

            var uiManager = new UIManager();

            var actionGrp = new ActionGroup("Default");

            var mainMenu = new MenuBar
            {
                Name = "mainMenu"
            };

            var fileMenu = new Menu();

            var fileMenuItem = new MenuItem("_File")
            {
                Name = "fileMenuItem",
                Submenu = fileMenu,

            };

            #region Menubar items
            var fileNewMenuItem = new MenuItem("_New");
            fileNewMenuItem.Activated += FileNewMenuItem_OnActivated;
            fileMenu.Append(fileNewMenuItem);

            var fileOpenMenuItem = new MenuItem("_Open");
            fileOpenMenuItem.Activated += OpenActionOnActivated;
            fileMenu.Append(fileOpenMenuItem);

            var fileSaveMenuItem = new MenuItem("_Save");
            fileMenu.Append(fileSaveMenuItem);
            fileSaveMenuItem.Activated += SaveItem_OnActivated;

            var hsepfilemenu = new SeparatorMenuItem { Name = "FileMenuSeparator" };
            fileMenu.Append(hsepfilemenu);

            var fileExitMenuItem = new MenuItem("_Exit");
            fileExitMenuItem.Activated += fileExitMenuItem_Clicked;
            fileMenu.Append(fileExitMenuItem);

            var windowPane = new HPaned();

            mainMenu.Append(fileMenuItem);

            _addCategoryAction = new Action("addCategoryAction",
                "New _Category", "Add a new category", "CircledPlus");
            _addCategoryAction.Activated += AddCategory_Clicked;
            actionGrp.Add(_addCategoryAction, null);

            _removeCategoryAction = new Action("removeCategoryAction",
                "_Remove Category", "Remove the selected category", "CircledMinus");
            _removeCategoryAction.Activated += deleteCategory_Clicked;
            actionGrp.Add(_removeCategoryAction, null);

            _addTaskItemAction = new Action("addToDoItemAction",
                "New _Task", "Add a new task", "CircledPlus");
            _addTaskItemAction.Activated += AddTaskItem_Clicked;
            actionGrp.Add(_addTaskItemAction, null);

            _removeTaskItemAction = new Action("removeToDoItemAction",
                "Remove Task", "Removes the selected task", "CircledMinus");
            _removeTaskItemAction.Activated += DeleteTask_Clicked;
            actionGrp.Add(_removeTaskItemAction, null);

            uiManager.InsertActionGroup(actionGrp, 0);

            AddAccelGroup(uiManager.AccelGroup);

            #endregion

            var windowContainer = new VBox
            {
                Name = "mainWindowContainer",
                Spacing = 3
            };

            _categorySidebar = new VBox
            {
                Name = "categorySidebar",
                Spacing = 6,
                ResizeMode = ResizeMode.Parent,
                WidthRequest = 210
            };

            uiManager.AddUiFromString(
                "<ui>" +
                "<toolbar name='categoryToolbar'>" +
                "<toolitem name='addCategoryAction' action='addCategoryAction'/>" +
                "<toolitem name='removeCategoryAction' action='removeCategoryAction'/>" +
                "</toolbar>" +
                "</ui>");

            _categoryToolbar = (Toolbar)uiManager.GetWidget("/categoryToolbar");
            _categoryToolbar.Events = (EventMask)8992;
            _categoryToolbar.Name = "catToolbar";
            _categoryToolbar.ShowArrow = false;

            var calendarExpander = new Expander(null)
            {
                CanFocus = true,
                Name = "CategoryExpander",
                Expanded = true
            };

            Calendar = new Calendar
            {
                CanFocus = true,
                Name = "calendar",
                DisplayOptions = (CalendarDisplayOptions)35
            };
            Calendar.MonthChanged += Calendar_MonthChanged;

            calendarExpander.Add(Calendar);

            var calendarExpanderLabel = new Label
            {
                Name = "CalendarExpanderLabel",
                LabelProp = "Calendar",
                UseUnderline = true
            };

            calendarExpander.LabelWidget = calendarExpanderLabel;

            var categoryExpander = new Expander(null)
            {
                CanFocus = true,
                Name = "CategoryExpander",
                Expanded = true
            };

            var categoryExpanderContainer = new VBox
            {
                Name = "categoryExpanderContainer",
                Spacing = 3
            };

            var categoryPane = new VPaned();

            var categoryContainer = new ScrolledWindow()
            {
                Name = "CategoryContainer",
                ShadowType = ShadowType.None,
                HscrollbarPolicy = PolicyType.Automatic,
                VscrollbarPolicy = PolicyType.Automatic
            };

            categoryContainer.HScrollbar.Visible = true;
            categoryContainer.VScrollbar.Visible = true;

            var categoryContainerLabel = new Label()
            {
                Name = "CategoryContainerLabel"
            };

            var categoryDescriptionScroll = new ScrolledWindow
            {
                Name = "categoryDescriptionScroll",
                ShadowType = (ShadowType)1,
                VscrollbarPolicy = PolicyType.Automatic,
                HscrollbarPolicy = PolicyType.Automatic
            };

            var descriptionHBox = new VBox
            {
                Name = "descriptionHBox",
                HasWindow = false
            };

            var descriptionFixed = new Fixed
            {
                Name = "descriptionFixed",
                HasWindow = false
            };

            _categoryDescriptionLabel = new Label
            {
                Name = "categoryDescriptionLabel",
                Text = "Description"
            };

            _categoryDescription = new TextView
            {
                Name = "categoryDescription",
                BorderWidth = 2,
                Editable = false,
                WrapMode = WrapMode.WordChar,
                HscrollPolicy = ScrollablePolicy.Natural,
                Buffer = {Text = "No category selected"}
            };

            var noteBookContainer = new VBox
            {
                Name = "NotebookContainer",
                Spacing = 3
            };

            _noteBook = new Notebook
            {
                Scrollable = true
            };

            noteBookContainer.Add(_noteBook);
            var noteBookContainerChild = (Box.BoxChild) noteBookContainer[_noteBook];
            noteBookContainerChild.Expand = true;
            noteBookContainerChild.Fill = true;

            var taskViewContainer = new VBox
            {
                Name = "ToDoTableListContainer",
                Spacing = 3
            };
            
            uiManager.AddUiFromString(
                "<ui>" +
                "<toolbar name='todoToolbar'>" +
                "<toolitem name='addToDoItemAction' action='addToDoItemAction'/>" +
                "<toolitem name='removeToDoItemAction' action='removeToDoItemAction'/>" +
                "</toolbar>" +
                "</ui>");

            var todoToolbar = (Toolbar)uiManager.GetWidget("/todoToolbar");
            todoToolbar.Events = (EventMask)8992;
            todoToolbar.Name = "todoToolbar";
            todoToolbar.ShowArrow = false;

            var recordsWindow = new ScrolledWindow
            {
                Name = "recordsWindow",
                ShadowType = (ShadowType)1,
                VscrollbarPolicy = PolicyType.Automatic,
                HscrollbarPolicy = PolicyType.Automatic
            };


            #region Category table properties

            _categoryTreeView = new TreeView();

            _categoryItemId = new TreeViewColumn
            {
                Title = "ID",
                Resizable = true,
#if !DEBUG
                Visible = false
#endif
#if DEBUG
                Visible = true
#endif
            };

            var categoryIdCell = new CellRendererText();
            _categoryItemId.PackStart(categoryIdCell, false);
            _categoryItemId.SetCellDataFunc(categoryIdCell, RenderCategoryId);

            _categoryItemName = new TreeViewColumn {Title = "Category",Resizable = true};
            var categoryNameCell = new CellRendererText();
            _categoryItemName.PackStart(categoryNameCell, false);
            _categoryItemName.SetCellDataFunc(categoryNameCell, RenderCategoryName);
            categoryNameCell.Edited += categoryItemNameCell_Edited;

            _categoryItemToggle = new TreeViewColumn {Title = "Show", Resizable = true };
            var categoryToggleCell = new CellRendererToggle();
            _categoryItemToggle.PackStart(categoryToggleCell, false);
            _categoryItemToggle.SetCellDataFunc(categoryToggleCell, RenderCategoryToggle);
            categoryToggleCell.Toggled += CategoryItem_Toggled;

            CategoryStore = new ListStore(typeof (Category));
            _categoryTreeView.Model = CategoryStore;

            _categoryTreeView.AppendColumn(_categoryItemId);
            _categoryTreeView.AppendColumn(_categoryItemName);
            _categoryTreeView.AppendColumn(_categoryItemToggle);
            _categoryTreeView.RowActivated += CategoryTreeView_RowActivated;
            #endregion categoryTable_properties

            #region MainView properties

            _mainView = new TreeView();
            
            _taskId = new TreeViewColumn
            {
                Title = "ID",
                Resizable = true,
                #if !DEBUG
                Visible = false
                #endif
                #if DEBUG
                Visible = true
                #endif
            };

            var taslIdCell = new CellRendererText();
            _taskId.PackStart(taslIdCell, false);
            _taskId.SetCellDataFunc(taslIdCell, RenderTaskItemId);

            _taskName = new TreeViewColumn {Title = "Title", Resizable = true };
            var taskNameCell = new CellRendererText();
            _taskName.PackStart(taskNameCell, false);
            _taskName.SetCellDataFunc(taskNameCell, RenderTaskItemName);

            _taskCategory = new TreeViewColumn {Title = "Category", Resizable = true };
            var taskCategoryCell = new CellRendererText();
            _taskCategory.PackStart(taskCategoryCell, false);
            _taskCategory.SetCellDataFunc(taskCategoryCell, RenderTaskItemCategory);

            _taskPriority = new TreeViewColumn {Title = "Priority", Resizable = true };
            var taskPriorityCell = new CellRendererText();
            _taskPriority.PackStart(taskPriorityCell, false);
            _taskPriority.SetCellDataFunc(taskPriorityCell, RenderTaskItemPriority);

            _taskStartDate = new TreeViewColumn {Title = "Start Date", Resizable = true };
            var taskStartCell = new CellRendererText();
            _taskStartDate.PackStart(taskStartCell, false);
            _taskStartDate.SetCellDataFunc(taskStartCell, RenderTaskItemStart);

            _taskDueDate = new TreeViewColumn {Title = "Due Date",Resizable = true};
            var todoFinishCell = new CellRendererText();
            _taskDueDate.PackStart(todoFinishCell, true);
            _taskDueDate.SetCellDataFunc(todoFinishCell, RenderTaskItemFinish);

            TaskStore = new ListStore(typeof (Task));

            _mainView.Model = TaskStore;

            _mainView.AppendColumn(_taskId);
            _mainView.AppendColumn(_taskName);
            _mainView.AppendColumn(_taskCategory);
            _mainView.AppendColumn(_taskPriority);
            _mainView.AppendColumn(_taskStartDate);
            _mainView.AppendColumn(_taskDueDate);
            #endregion 

            _fileActionProgBar = new ProgressBar()
            {
                Name = "fileActionProgBar",
                Visible = false
            };

            windowContainer.Add(mainMenu);

            var menuChild = (Box.BoxChild)windowContainer[mainMenu];
            menuChild.Fill = false;
            menuChild.Expand = false;

            _categorySidebar.Add(calendarExpander);

            var sidebarChild = (Box.BoxChild)_categorySidebar[calendarExpander];
            sidebarChild.Position = 0;
            sidebarChild.Expand = false;
            sidebarChild.Fill = false;

            categoryExpanderContainer.Add(_categoryToolbar);
            categoryExpanderContainer.Add(_categoryTreeView);

            var toolbarChild = (Box.BoxChild)categoryExpanderContainer[_categoryToolbar];
            toolbarChild.Expand = false;
            toolbarChild.Fill = false;

            categoryExpander.Add(categoryPane);

            categoryPane.Add(categoryContainer);

            var paneContainerChild = (Paned.PanedChild)categoryPane[categoryContainer];
            paneContainerChild.Resize = true;

            categoryContainer.AddWithViewport(categoryExpanderContainer);
            categoryContainerLabel.LabelProp = "Categories";
            categoryContainerLabel.UseUnderline = true;
            categoryExpander.LabelWidget = categoryContainerLabel;

            categoryPane.Add(descriptionHBox);

            var descriptionContainerChild = (Paned.PanedChild)categoryPane[descriptionHBox];
            descriptionContainerChild.Resize = true;


            descriptionHBox.Add(descriptionFixed);

            var descriptionFixedChild = (Box.BoxChild)descriptionHBox[descriptionFixed];
            descriptionFixedChild.Expand = false;
            descriptionFixedChild.Padding = 5;

            descriptionFixed.Add(_categoryDescriptionLabel);

            var descriptionLabelChild = (Fixed.FixedChild)descriptionFixed[_categoryDescriptionLabel];
            descriptionLabelChild.X = 5;
            descriptionLabelChild.Y = 0;

            descriptionHBox.Add(categoryDescriptionScroll);

            var descriptionViewChild = (Box.BoxChild)descriptionHBox[categoryDescriptionScroll];
            descriptionViewChild.Expand = true;

            categoryDescriptionScroll.Add(_categoryDescription);

            categoryDescriptionScroll.AddWithViewport(_categoryDescription);

            _categorySidebar.Add(categoryExpander);

            var sidebarchild = (Box.BoxChild)_categorySidebar[categoryExpander];
            sidebarchild.Position = 1;

            windowPane.Add(_categorySidebar);

            taskViewContainer.Add(todoToolbar);

            var todobarChild = (Box.BoxChild)taskViewContainer[todoToolbar];
            todobarChild.Fill = false;
            todobarChild.Expand = false;

            recordsWindow.Add(_mainView);

            recordsWindow.AddWithViewport(_mainView);

            taskViewContainer.Add(recordsWindow);

            taskViewContainer.Add(_fileActionProgBar);

            var barChild = (Box.BoxChild)taskViewContainer[_fileActionProgBar];
            barChild.Expand = false;
            barChild.Fill = false;

            _noteBook.AppendPage(taskViewContainer, new Label("Task List"));
            var taskContainerNoteBookContainer = (Notebook.NotebookChild) _noteBook[taskViewContainer];
            taskContainerNoteBookContainer.TabFill = true;
            taskContainerNoteBookContainer.Detachable = false;

            windowPane.Add(noteBookContainer);

            windowContainer.Add(windowPane);

            Add(windowContainer);

            SetSizeRequest(800, 600);
            DefaultWidth = 800;
            DefaultHeight = 600;

            DestroyEvent += ProjectWindow_DestroyEvent;
            DeleteEvent += OnDeleteEvent;
            ShowAll();

            _fileActionProgBar.Visible = false;
        }

        

    }
}