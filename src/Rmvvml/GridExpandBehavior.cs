using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace Rmvvml
{

    public class GridExpandBehavior : Behavior<Grid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_OnLoaded;
        }

        #region Column
        public int Column
        {
            get { return (int)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Column.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register("Column", typeof(int), typeof(GridExpandBehavior), new PropertyMetadata(-1));
        #endregion

        #region Row
        public int Row
        {
            get { return (int)GetValue(RowProperty); }
            set { SetValue(RowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Row.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowProperty =
            DependencyProperty.Register("Row", typeof(int), typeof(GridExpandBehavior), new PropertyMetadata(-1));
        #endregion

        #region Duration
        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Duration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(GridExpandBehavior), new PropertyMetadata(new Duration(TimeSpan.Zero)));
        #endregion

        #region Fade
        public bool Fade
        {
            get { return (bool)GetValue(FadeProperty); }
            set { SetValue(FadeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fade.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FadeProperty =
            DependencyProperty.Register("Fade", typeof(bool), typeof(GridExpandBehavior), new PropertyMetadata(false));
        #endregion

        #region Dock
        public Dock Dock
        {
            get { return (Dock)GetValue(DockProperty); }
            set { SetValue(DockProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Dock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DockProperty =
            DependencyProperty.Register("Dock", typeof(Dock), typeof(GridExpandBehavior), new PropertyMetadata(Dock.Left));
        #endregion

        GridLength Backup_Size { get; set; }
        double Backup_ActualSize { get; set; }
        double Backup_MinSize { get; set; }
        double Backup_MaxSize { get; set; }
        GridDefinitionWrapper AssociatedDefinition { get; set; }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(GridExpandBehavior), new PropertyMetadata(true, OnIsExpandedChanged));

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridExpandBehavior)d).OnIsExpandedChanged(e);
        }

        private void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject == null)
            {
                return;
            }
            if (!AssociatedObject.IsLoaded)
            {
                return;
            }
            if (IsExpanded)
            {
                ShowWithAnimation();
            }
            else
            {
                HideWithAnimation();
            }
        }

        private void AssociatedObject_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsExpanded)
            {
                HideWithoutAnimation();
            }
        }

        bool IsHorizontal
        {
            get
            {
                if (Column != -1)
                {
                    return true;
                }
                if (Row != -1)
                {
                    return false;
                }
                throw new NotImplementedException();
            }
        }

        GridDefinitionWrapper GetAssociatedDefinition()
        {
            if (IsHorizontal)
            {
                return new GridDefinitionWrapper(AssociatedObject.ColumnDefinitions[Column]);
            }
            else
            {
                return new GridDefinitionWrapper(AssociatedObject.RowDefinitions[Row]);
            }
        }

        bool IsExpandingChild(FrameworkElement fe)
        {
            if (IsHorizontal)
            {
                var ret = Grid.GetColumn(fe) == Column && Grid.GetColumnSpan(fe) == 1;
                return ret;
            }
            else
            {
                var ret = Grid.GetRow(fe) == Row && Grid.GetRowSpan(fe) == 1;
                return ret;
            }
        }

        void HideBeforeAnimation()
        {
            // アニメーション前
            foreach (var obj in AssociatedObject.Children)
            {
                var child = obj as FrameworkElement;
                if (IsExpandingChild(child))
                {
                    var childWrapper = new GridElementWrapper(child, IsHorizontal, Dock);
                    childWrapper.PreHide();
                }
            }

            AssociatedDefinition = GetAssociatedDefinition();
            Backup_Size = AssociatedDefinition.Size;
            Backup_ActualSize = AssociatedDefinition.ActualSize;
            Backup_MinSize = AssociatedDefinition.MinSize;
            Backup_MaxSize = AssociatedDefinition.MaxSize;

            AssociatedDefinition.Size = new GridLength(Backup_ActualSize, GridUnitType.Pixel);
            AssociatedDefinition.MinSize = 0;
        }

        void HideAfterAnimation()
        {
            // アニメーション後
            AssociatedDefinition.Size = new GridLength(0, GridUnitType.Pixel);
            AssociatedDefinition.MaxSize = 0;

            foreach (var obj in AssociatedObject.Children)
            {
                var child = obj as FrameworkElement;
                if (IsExpandingChild(child))
                {
                    var childWrapper = new GridElementWrapper(child, IsHorizontal, Dock);
                    childWrapper.PostHide();
                }
            }
        }

        void HideWithoutAnimation()
        {
            HideBeforeAnimation();

            // アニメーションの代わり
            AssociatedDefinition.Size = new GridLength(0, GridUnitType.Pixel);

            HideAfterAnimation();
        }

        void HideWithAnimation()
        {
            if (Duration.TimeSpan.TotalSeconds < 0.001)
            {
                // アニメーションなしで実行
                HideWithoutAnimation();
                return;
            }

            HideBeforeAnimation();

            // アニメーション
            var story = new Storyboard();
            var animation = new GridLengthAnimation();
            Storyboard.SetTarget(animation, AssociatedDefinition.Definition);
            Storyboard.SetTargetProperty(animation, new PropertyPath(AssociatedDefinition.SizePropertyName));
            animation.To = new GridLength(0, GridUnitType.Pixel);
            animation.EasingFunction = new PowerEase();
            animation.Duration = Duration;
            story.Children.Add(animation);

            // おまけ
            if (Fade)
            {
                foreach (var obj in AssociatedObject.Children)
                {
                    var child = obj as FrameworkElement;
                    if (IsExpandingChild(child))
                    {
                        var fade = new DoubleAnimation(0.5, Duration);
                        Storyboard.SetTarget(fade, child);
                        Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
                        fade.EasingFunction = new PowerEase();
                        story.Children.Add(fade);
                    }
                }
            }

            story.Completed += (sender, e) =>
            {
                story.Remove(AssociatedDefinition.Definition);
                // animationを外さないとなぜかちゃんと解除されない？
                AssociatedDefinition.Definition.BeginAnimation(AssociatedDefinition.SizeProperty, null);

                // おまけ解除
                if (Fade)
                {
                    foreach (var obj in AssociatedObject.Children)
                    {
                        var child = obj as FrameworkElement;
                        if (IsExpandingChild(child))
                        {
                            child.BeginAnimation(FrameworkElement.OpacityProperty, null);
                        }
                    }
                }

                HideAfterAnimation();
            };

            story.Begin(AssociatedDefinition.Definition);

        }

        void ShowBeforeAnimation()
        {
            // アニメーション前
            foreach (var obj in AssociatedObject.Children)
            {
                var child = obj as FrameworkElement;
                if (IsExpandingChild(child))
                {
                    var childWrapper = new GridElementWrapper(child, IsHorizontal, Dock);
                    childWrapper.PreShow();
                }
            }

            AssociatedDefinition.MaxSize = Backup_MaxSize;
        }

        void ShowAfterAnimation()
        {
            // アニメーション後
            AssociatedDefinition.MinSize = Backup_MinSize;
            AssociatedDefinition.Size = Backup_Size;
            // TODO: starの場合は相当する重みを計算する処理が必要
            foreach (var obj in AssociatedObject.Children)
            {
                var child = obj as FrameworkElement;
                if (IsExpandingChild(child))
                {
                    var childWrapper = new GridElementWrapper(child, IsHorizontal, Dock);
                    childWrapper.PostShow();
                }
            }
        }

        void ShowWithoutAnimation()
        {
            ShowBeforeAnimation();

            // アニメーションの代わり
            AssociatedDefinition.Size = new GridLength(Backup_ActualSize, GridUnitType.Pixel);

            ShowAfterAnimation();
        }

        void ShowWithAnimation()
        {
            if (Duration.TimeSpan.TotalSeconds < 0.001)
            {
                // アニメーションなしで実行
                ShowWithoutAnimation();
                return;
            }

            ShowBeforeAnimation();

            // アニメーション
            var story = new Storyboard();
            var animation = new GridLengthAnimation();
            Storyboard.SetTarget(animation, AssociatedDefinition.Definition);
            Storyboard.SetTargetProperty(animation, new PropertyPath(AssociatedDefinition.SizePropertyName));
            animation.To = new GridLength(Backup_ActualSize, GridUnitType.Pixel);
            animation.EasingFunction = new PowerEase();
            animation.Duration = Duration;
            story.Children.Add(animation);

            // おまけ
            if (Fade)
            {
                foreach (var obj in AssociatedObject.Children)
                {
                    var child = obj as FrameworkElement;
                    if (IsExpandingChild(child))
                    {
                        var fade = new DoubleAnimation(0.5, child.Opacity, Duration);
                        Storyboard.SetTarget(fade, child);
                        Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));
                        fade.EasingFunction = new PowerEase();
                        story.Children.Add(fade);
                    }
                }
            }

            story.Completed += (sender, e) =>
            {
                story.Remove(AssociatedDefinition.Definition);
                // animationを外さないとなぜかちゃんと解除されない？
                AssociatedDefinition.Definition.BeginAnimation(AssociatedDefinition.SizeProperty, null);

                // おまけ解除
                if (Fade)
                {
                    foreach (var obj in AssociatedObject.Children)
                    {
                        var child = obj as FrameworkElement;
                        if (IsExpandingChild(child))
                        {
                            child.BeginAnimation(FrameworkElement.OpacityProperty, null);
                        }
                    }
                }

                ShowAfterAnimation();
            };

            story.Begin(AssociatedDefinition.Definition);
        }
    }

    // TODO: 名前変更
    public static class FrameworkElementAtt
    {

        public static GridExpandBehaviorBackupInfo GetGridExpanderBehavior_Backup(DependencyObject obj)
        {
            return (GridExpandBehaviorBackupInfo)obj.GetValue(GridExpanderBehavior_BackupProperty);
        }

        public static void SetGridExpanderBehavior_Backup(DependencyObject obj, GridExpandBehaviorBackupInfo value)
        {
            obj.SetValue(GridExpanderBehavior_BackupProperty, value);
        }

        // Using a DependencyProperty as the backing store for GridExpanderBehavior_Backup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridExpanderBehavior_BackupProperty =
            DependencyProperty.RegisterAttached("GridExpanderBehavior_Backup", typeof(GridExpandBehaviorBackupInfo), typeof(FrameworkElementAtt), new PropertyMetadata(null));


    }


    class GridDefinitionWrapper
    {
        ColumnDefinition Column { get; set; }
        RowDefinition Row { get; set; }

        public GridDefinitionWrapper(ColumnDefinition column)
        {
            Column = column;
        }

        public GridDefinitionWrapper(RowDefinition row)
        {
            Row = row;
        }

        public DefinitionBase Definition
        {
            get
            {
                if (Column != null)
                {
                    return Column;
                }
                if (Row != null)
                {
                    return Row;
                }
                throw new NotImplementedException();
            }
        }

        public GridLength Size
        {
            get
            {
                if (Column != null)
                {
                    return Column.Width;
                }
                if (Row != null)
                {
                    return Row.Height;
                }
                throw new NotImplementedException();
            }
            set
            {
                if (Column != null)
                {
                    Column.Width = value;
                }
                if (Row != null)
                {
                    Row.Height = value;
                }
            }
        }

        public double ActualSize
        {
            get
            {
                if (Column != null)
                {
                    return Column.ActualWidth;
                }
                if (Row != null)
                {
                    return Row.ActualHeight;
                }
                throw new NotImplementedException();
            }
        }

        public double MaxSize
        {
            get
            {
                if (Column != null)
                {
                    return Column.MaxWidth;
                }
                if (Row != null)
                {
                    return Row.MaxHeight;
                }
                throw new NotImplementedException();
            }
            set
            {
                if (Column != null)
                {
                    Column.MaxWidth = value;
                }
                if (Row != null)
                {
                    Row.MaxHeight = value;
                }
            }
        }

        public double MinSize
        {
            get
            {
                if (Column != null)
                {
                    return Column.MinWidth;
                }
                if (Row != null)
                {
                    return Row.MinHeight;
                }
                throw new NotImplementedException();
            }
            set
            {
                if (Column != null)
                {
                    Column.MinWidth = value;
                }
                if (Row != null)
                {
                    Row.MinHeight = value;
                }
            }
        }

        public DependencyProperty SizeProperty
        {
            get
            {
                if (Column != null)
                {
                    return ColumnDefinition.WidthProperty;
                }
                if (Row != null)
                {
                    return RowDefinition.HeightProperty;
                }
                throw new NotImplementedException();
            }
        }

        public string SizePropertyName
        {
            get
            {
                if (Column != null)
                {
                    return "Width";
                }
                if (Row != null)
                {
                    return "Height";
                }
                throw new NotImplementedException();
            }
        }
    }

    class GridElementWrapper
    {
        FrameworkElement Element { get; set; }
        Dock Dock { get; set; }
        bool IsHorizontal { get; set; }

        public GridElementWrapper(FrameworkElement fe, bool isHorizontal, Dock dock)
        {
            Element = fe;
            IsHorizontal = isHorizontal;
            Dock = dock;
        }

        public void PreHide()
        {
            var backup = new GridExpandBehaviorBackupInfo(Element);
            FrameworkElementAtt.SetGridExpanderBehavior_Backup(Element, backup);

            if (IsHorizontal)
            {
                Element.Width = Element.ActualWidth;
                Element.HorizontalAlignment = Dock == Dock.Right ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            else
            {
                Element.Height = Element.ActualHeight;
                Element.VerticalAlignment = Dock == Dock.Bottom ? VerticalAlignment.Bottom : VerticalAlignment.Top;
            }
        }

        public void PostHide()
        {
            Element.Visibility = Visibility.Collapsed;
        }

        public void PreShow()
        {
            Element.Visibility = FrameworkElementAtt.GetGridExpanderBehavior_Backup(Element).Visibility;
        }

        public void PostShow()
        {
            var backup = FrameworkElementAtt.GetGridExpanderBehavior_Backup(Element);
            backup.ApplyTo(Element);
        }
    }

    public class GridExpandBehaviorBackupInfo
    {
        public double Width { get; set; }
        public double ActualWidth { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public double Height { get; set; }
        public double ActualHeight { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public Visibility Visibility { get; set; }

        public GridExpandBehaviorBackupInfo(FrameworkElement fe)
        {
            Width = fe.Width;
            ActualWidth = fe.ActualWidth;
            HorizontalAlignment = fe.HorizontalAlignment;
            Height = fe.Height;
            ActualHeight = fe.ActualHeight;
            VerticalAlignment = fe.VerticalAlignment;
            Visibility = fe.Visibility;
        }

        public void ApplyTo(FrameworkElement fe)
        {
            fe.Width = Width;
            fe.HorizontalAlignment = HorizontalAlignment;
            fe.Height = Height;
            fe.VerticalAlignment = VerticalAlignment;
            fe.Visibility = Visibility;
        }
    }
}
